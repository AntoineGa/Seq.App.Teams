using Newtonsoft.Json;
using Seq.Apps;
using Seq.Apps.LogEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Seq.App.Teams
{
    [SeqApp("Teams",
    Description = "Sends log events to Teams.")]
    public class TeamsReactor : Reactor, ISubscribeTo<LogEventData>
    {
        
        private static IDictionary<LogEventLevel, string> _levelColorMap = new Dictionary<LogEventLevel, string>
        {
            {LogEventLevel.Verbose, "808080"},
            {LogEventLevel.Debug, "808080"},
            {LogEventLevel.Information, "008000"},
            {LogEventLevel.Warning, "ffff00"},
            {LogEventLevel.Error, "ff0000"},
            {LogEventLevel.Fatal, "ff0000"}
        };

        [SeqAppSetting(
        DisplayName = "Seq Base URL",
        HelpText = "Used for generating perma links to events in Teams messages.",
        IsOptional = true)]
        public string BaseUrl { get; set; }
        
        [SeqAppSetting(
        DisplayName = "Teams WebHook URL",
        HelpText = "Used to send message to Teams")]
        public string TeamsBaseUrl { get; set; }

        [SeqAppSetting(
        DisplayName = "Trace All Messages",
        HelpText = "Used to show all messages to trace",
        IsOptional = true)]
        public bool TraceMessage { get; set; }

        [SeqAppSetting(
        DisplayName = "Exclude Properties",
        HelpText = "Exclude the Seq properties from the messages",
        IsOptional = true)]
        public bool ExcludeProperties { get; set; }

        [SeqAppSetting(
        DisplayName = "Color",
        HelpText = "Hex theme color for messages (ex. ff0000). (default: auto based on message level)",
        IsOptional = true)]
        public string Color { get; set; }
        

        public void On(Event<LogEventData> evt)
        {

            try
            {
                if (TraceMessage)
                {
                    Log
                        .ForContext("Uri", new Uri(TeamsBaseUrl))
                        .Information("Start Processing {Message}", evt.Data.RenderedMessage);
                }

                TeamsCard body = BuildBody(evt);

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(TeamsBaseUrl);

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                  
                    var response = client.PostAsync(
                        "", 
                        new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
                        ).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        Log
                            .ForContext("Uri", response.RequestMessage.RequestUri)
                            .Error("Could not send Teams message, server replied {StatusCode} {StatusMessage}: {Message}", Convert.ToInt32(response.StatusCode), response.StatusCode, response.Content.ReadAsStringAsync().Result);
                    }
                    else
                    {
                        if (TraceMessage)
                        {
                            string reponseResult = response.Content.ReadAsStringAsync().Result;
                            Log
                                .ForContext("Uri", response.RequestMessage.RequestUri)
                                .Information("Server replied {StatusCode} {StatusMessage}: {Message}", Convert.ToInt32(response.StatusCode), response.StatusCode, reponseResult);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Log
                    .ForContext("Uri", new Uri(TeamsBaseUrl))
                    .Error(ex, "An error occured while constructing request.");
            }
        }

        private TeamsCard BuildBody(Event<LogEventData> evt)
        {
            // Build action
            var url = BaseUrl;
            if (string.IsNullOrWhiteSpace(url))
                url = Host.ListenUris.FirstOrDefault();

            TeamsPotentialAction action = new TeamsPotentialAction()
            {
                Name = "Click here to open in Seq",
                Targets = new[]
                {
                    new TeamsActionTarget { Uri = $"{url}#/events?filter=@Id%20%3D%3D%20%22{evt.Id}%22&show=expanded" }
                }
            };

            // Build message
            var msg = evt.Data.RenderedMessage;
            if (msg.Length > 1000)
                msg = msg.EscapeMarkdown().Substring(0, 1000);

            var color = Color;
            if (string.IsNullOrWhiteSpace(color))
            {
                color = _levelColorMap[evt.Data.Level];
            }

            TeamsCard body = new TeamsCard
            {
                Title = evt.Data.Level.ToString().EscapeMarkdown(),
                ThemeColor = color,
                Text = msg,
                PotentialAction = new []
                {
                    action
                }
            };

            // Build sections
            var sections = new List<TeamsSection>();
            if (!ExcludeProperties && evt.Data.Properties != null)
            {
                var facts = evt.Data.Properties
                    .Where(i => i.Value != null)
                    .Select(i => new TeamsFact { Name = i.Key, Value = i.Value.ToString().EscapeMarkdown() })
                    .ToArray();

                if (facts.Any())
                    sections.Add(new TeamsSection { Facts = facts});
            }

            if (!string.IsNullOrWhiteSpace(evt.Data.Exception))
                sections.Add(new TeamsSection { Title = "Exception", Text = evt.Data.Exception.EscapeMarkdown() });

            body.Sections = sections.ToArray();

            return body;
        }
    }
}
