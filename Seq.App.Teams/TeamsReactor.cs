using Seq.App.Teams.Models;
using Newtonsoft.Json;
using Seq.Apps;
using Seq.Apps.LogEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Seq.App.Teams
{
    [SeqApp("Teams",
    Description = "Sends log events to Microsoft Teams.")]
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

        private const uint AlertEventType = 0xA1E77000;

        #region "Settings"
        [SeqAppSetting(
        DisplayName = "Seq Base URL",
        HelpText = "Used for generating perma links to events in Teams messages.",
        IsOptional = true)]
        public string BaseUrl { get; set; }

        [SeqAppSetting(
            DisplayName = "Web proxy",
            HelpText = "When a web proxy is present in the network for connecting to outside URLs.",
            IsOptional = true)]
        public string WebProxy { get; set; }

        [SeqAppSetting(
            DisplayName = "Web proxy user name",
            HelpText = "Proxy user name, if authorization required",
            IsOptional = true)]
        public string WebProxyUserName { get; set; }

        [SeqAppSetting(
            DisplayName = "Web proxy password",
            HelpText = "Proxy password, if authorization required",
            IsOptional = true,
            InputType = SettingInputType.Password)]
        public string WebProxyPassword { get; set; }

        [SeqAppSetting(
        DisplayName = "Teams WebHook URL",
        HelpText = "Used to send message to Teams. This can be retrieved by adding a Incoming Webhook connector to your Teams channel.")]
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
        DisplayName = "Properties to serialize as JSON",
        HelpText = "The properties that should be serialized as JSON instead of the native ToString() on the value. Multiple properties can be specified; enter one per line.",
        InputType = SettingInputType.LongText,
        IsOptional = true)]
        public string JsonSerializedProperties { get; set; }

        [SeqAppSetting(
        DisplayName = "Properties to serialize as JSON - Use Indented JSON?",
        HelpText = "For properties that are serialized as JSON, should they be indented?",
        InputType = SettingInputType.Checkbox,
        IsOptional = true)]
        public bool JsonSerializedPropertiesAsIndented { get; set; }

        [SeqAppSetting(
        DisplayName = "Color",
        HelpText = "Hex theme color for messages (ex. ff0000). (default: auto based on message level)",
        IsOptional = true)]
        public string Color { get; set; }

        [SeqAppSetting(DisplayName = "Comma seperated list of event levels",
        IsOptional = true,
        HelpText = "If specified Teams card will be created only for the specified event levels, other levels will be discarded (useful for streaming events). Valid Values: Verbose,Debug,Information,Warning,Error,Fatal")]
        public string LogEventLevels { get; set; }

        #endregion

        public void On(Event<LogEventData> evt)
        {

            try
            {
                //If the event level is defined and it is not in the list do not log it
                if ((LogEventLevelList?.Count ?? 0) > 0 && !LogEventLevelList.Contains(evt.Data.Level))
                    return;

                if (TraceMessage)
                {
                    Log
                        .ForContext("Uri", new Uri(TeamsBaseUrl))
                        .Information("Start Processing {Message}", evt.Data.RenderedMessage);
                }

                O365ConnectorCard body = BuildBody(evt);

                var httpClientHandler = new HttpClientHandler();

                if (!string.IsNullOrEmpty(WebProxy))
                {
                    ICredentials credentials = null;
                    if (!string.IsNullOrEmpty(WebProxyUserName))
                    {
                        credentials = new NetworkCredential(WebProxyUserName, WebProxyPassword);
                    }
                    httpClientHandler.Proxy = new WebProxy(WebProxy, false, null, credentials);
                    httpClientHandler.UseProxy = true;
                }
                else
                {
                    httpClientHandler.UseProxy = false;
                }

                using (var client = new HttpClient(httpClientHandler))
                {
                    client.BaseAddress = new Uri(TeamsBaseUrl);

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var js = new JsonSerializerSettings();
                    js.NullValueHandling = NullValueHandling.Ignore;

                    var bodyJson = JsonConvert.SerializeObject(body, js);
                    var response = client.PostAsync(
                        "", 
                        new StringContent(bodyJson, Encoding.UTF8, "application/json")
                        ).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        Log
                            .ForContext("Uri", response.RequestMessage.RequestUri)
                            .Error("Could not send Teams message, server replied {StatusCode} {StatusMessage}: {Message}. Request Body: {RequestBody}", Convert.ToInt32(response.StatusCode), response.StatusCode, response.Content.ReadAsStringAsync().Result, bodyJson);
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

        public List<LogEventLevel> LogEventLevelList
        {
            get
            {
                List<LogEventLevel> result = new List<LogEventLevel>();
                if (string.IsNullOrEmpty(LogEventLevels))
                    return result;

                var strValues = LogEventLevels.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if ((strValues?.Length ?? 0) == 0)
                    return result;

                strValues.Aggregate(result, (acc, strValue) =>
                {
                    LogEventLevel enumValue = LogEventLevel.Debug;
                    if (Enum.TryParse(strValue, out enumValue))
                        acc.Add(enumValue);
                    return acc;
                });

                return result;
            }
        }

        private O365MessageCard BuildBody(Event<LogEventData> evt)
        {
            // Build action
            var url = BaseUrl;
            if (string.IsNullOrWhiteSpace(url))
                url = Host.BaseUri;

            var openTitle = "Open Seq Event";
            var openUrl = $"{url}#/events?filter=@Id%20%3D%3D%20%22{evt.Id}%22&show=expanded";
            if (IsAlert(evt))
            {
                openTitle = "Open Seq Alert";
                openUrl = SafeGetProperty(evt, "ResultsUrl");
            }

            O365ConnectorCardOpenUri action = new O365ConnectorCardOpenUri()
            {
                Name = openTitle,
                Type = "OpenUri", //Failure to provide this will cause a 400 badrequest
                Targets = new[]
                {
                    new O365ConnectorCardOpenUriTarget { Uri = openUrl,
                    Os = "default" //Failure to provide this will cause a 400 badrequest
                    }
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

            O365MessageCard body = new O365MessageCard
            {
                Title = evt.Data.Level.ToString().EscapeMarkdown(),
                ThemeColor = color,
                Text = msg,
                PotentialAction = new[]
                {
                    action
                }
            };

            // Build sections
            var sections = new List<O365ConnectorCardSection>();
            if (!ExcludeProperties && evt.Data.Properties != null)
            {
                var jsonSerializedProperties = JsonSerializedProperties?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = JsonSerializedPropertiesAsIndented ? Formatting.Indented : Formatting.None
                };

                var facts = evt.Data.Properties
                    .Where(i => i.Value != null)
                    .Select(i => new O365ConnectorCardFact
                    {
                        Name = i.Key,
                        Value = (jsonSerializedProperties.Contains(i.Key) ? JsonConvert.SerializeObject(i.Value, jsonSettings) : i.Value.ToString()).EscapeMarkdown()
                    })
                    .ToArray();

                if (facts.Any())
                    sections.Add(new O365ConnectorCardSection { Facts = facts });
            }

            if (!string.IsNullOrWhiteSpace(evt.Data.Exception))
                sections.Add(new O365ConnectorCardSection { Title = "Exception", Text = evt.Data.Exception.EscapeMarkdown() });

            body.Sections = sections.ToArray();

            return body;
        }

        /// <summary>
        /// Dashboard alerts create a "virtual" event id, so it doesn't actually point to a specific log, but potentially a set of log events
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        private static bool IsAlert(Event<LogEventData> evt)
        {
            return evt.EventType == AlertEventType;
        }

        private static string SafeGetProperty(Event<LogEventData> evt, string propertyName)
        {
            if (evt.Data.Properties.TryGetValue(propertyName, out var value))
            {
                if (value == null) return "`null`";
                return value.ToString();
            }
            return "";
        }
    }
}
