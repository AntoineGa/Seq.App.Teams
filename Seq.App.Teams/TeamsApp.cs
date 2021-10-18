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
using System.Threading.Tasks;
using Serilog;

// ReSharper disable UnusedAutoPropertyAccessor.Global, MemberCanBePrivate.Global, UnusedType.Global

namespace Seq.App.Teams
{
    [SeqApp("Teams",
    Description = "Sends events and notifications to Microsoft Teams.")]
    public class TeamsApp : SeqApp, ISubscribeToAsync<LogEventData>
    {
        private static readonly IDictionary<LogEventLevel, string> LevelColorMap = new Dictionary<LogEventLevel, string>
        {
            {LogEventLevel.Verbose, "808080"},
            {LogEventLevel.Debug, "808080"},
            {LogEventLevel.Information, "008000"},
            {LogEventLevel.Warning, "ffff00"},
            {LogEventLevel.Error, "ff0000"},
            {LogEventLevel.Fatal, "ff0000"}
        };
        
        private HttpClientHandler _httpClientHandler;
        private ILogger _log;

        #region "Settings"
        
        [SeqAppSetting(
            DisplayName = "Seq Base URL",
            HelpText = "Used for generating links to events in Teams messages; if not specified, Seq's configured base URL will be used.",
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
            HelpText = "Used to show all messages to trace; note that this will cause the Teams Webhook URL to appear in diagnostic messages.",
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

        [SeqAppSetting(
            DisplayName = "Comma seperated list of event levels",
            IsOptional = true,
            HelpText = "If specified Teams card will be created only for the specified event levels, other levels will be discarded (useful for streaming events). Valid Values: Verbose,Debug,Information,Warning,Error,Fatal")]
        public string LogEventLevels { get; set; }

        #endregion

        protected override void OnAttached()
        {
            _log = TraceMessage ? Log.ForContext("Uri", TeamsBaseUrl) : Log;
            
            _httpClientHandler = new HttpClientHandler();
            if (!string.IsNullOrEmpty(WebProxy))
            {
                ICredentials credentials = null;
                if (!string.IsNullOrEmpty(WebProxyUserName))
                {
                    credentials = new NetworkCredential(WebProxyUserName, WebProxyPassword);
                }
                _httpClientHandler.Proxy = new WebProxy(WebProxy, false, null, credentials);
                _httpClientHandler.UseProxy = true;
            }
            else
            {
                _httpClientHandler.UseProxy = false;
            }
        }

        public async Task OnAsync(Event<LogEventData> evt)
        {
            try
            {
                //If the event level is defined and it is not in the list do not log it
                if ((LogEventLevelList?.Count ?? 0) > 0 && !LogEventLevelList.Contains(evt.Data.Level))
                    return;

                if (TraceMessage)
                {
                    _log.Information("Start Processing {Message}", evt.Data.RenderedMessage);
                }
                
                using (var client = new HttpClient(_httpClientHandler, disposeHandler: false))
                {
                    client.BaseAddress = new Uri(TeamsBaseUrl);

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var js = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    };

                    O365ConnectorCard body = BuildBody(evt);
                    var bodyJson = JsonConvert.SerializeObject(body, js);
                    var response = await client.PostAsync("", new StringContent(bodyJson, Encoding.UTF8, "application/json"));

                    if (!response.IsSuccessStatusCode)
                    {
                        _log.Error("Could not send Teams message, server replied {StatusCode} {StatusMessage}: {Message}. Request Body: {RequestBody}", Convert.ToInt32(response.StatusCode), response.StatusCode, await response.Content.ReadAsStringAsync(), bodyJson);
                    }
                    else
                    {
                        if (TraceMessage)
                        {
                            var responseResult = await response.Content.ReadAsStringAsync();
                            _log.Information("Server replied {StatusCode} {StatusMessage}: {Message}", Convert.ToInt32(response.StatusCode), response.StatusCode, responseResult);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
               _log.Error(ex, "An error occured while constructing the request");
            }
        }

        private List<LogEventLevel> LogEventLevelList
        {
            get
            {
                var result = new List<LogEventLevel>();
                if (string.IsNullOrEmpty(LogEventLevels))
                    return result;

                var strValues = LogEventLevels.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if ((strValues?.Length ?? 0) == 0)
                    return result;

                strValues.Aggregate(result, (acc, strValue) =>
                {
                    if (Enum.TryParse(strValue, out LogEventLevel enumValue))
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

            var (openTitle, openUrl) = SeqEvents.GetOpenLink(url, evt);
            var action = new O365ConnectorCardOpenUri
            {
                Name = openTitle,
                Type = "OpenUri", //Failure to provide this will cause a 400 badrequest
                Targets = new[]
                {
                    new O365ConnectorCardOpenUriTarget
                    {
                        Uri = openUrl,
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
                color = LevelColorMap[evt.Data.Level];
            }

            var body = new O365MessageCard
            {
                Title = evt.Data.Level.ToString().EscapeMarkdown(),
                ThemeColor = color,
                Text = msg,
                PotentialAction = new O365ConnectorCardActionBase[]
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
    }
}
