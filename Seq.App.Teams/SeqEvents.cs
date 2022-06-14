using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Seq.Apps;
using Seq.Apps.LogEvents;

namespace Seq.App.Teams
{
    public static class SeqEvents
    {
        private const uint AlertV1EventType = 0xA1E77000, AlertV2EventType = 0xA1E77001;
        private const string ResultsPropertyPath = "Source.Results";
        private const string ResultsUrlPropertyPathV1 = "ResultsUrl";
        private const string ResultsUrlPropertyPathV2 = "Source.ResultsUrl";
        private const string ContributingEventsPropertyPath = "Source.ContributingEvents";

        private static readonly JsonSerializerSettings JsonSettingsDefault = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None
        };
        private static readonly JsonSerializerSettings JsonSettingsIndented = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };
        private static readonly List<string> RawProperties = new List<string>
        {
            ResultsUrlPropertyPathV1,
            ResultsUrlPropertyPathV2
        };

        public static (string description, string href) GetOpenLink(SeqConfig config, Event<LogEventData> evt)
        {
            if (evt.EventType == AlertV1EventType)
            {
                return ("Open Seq Alert", GetProperty(config, evt, ResultsUrlPropertyPathV1));
            }

            if (evt.EventType == AlertV2EventType)
            {
                return ("Open Seq Alert", GetProperty(config, evt, ResultsUrlPropertyPathV2));
            }

            return ("Open Seq Event", GetLinkToEvent(config.SeqBaseUrl, evt.Id));
        }

        public static string GetLinkToEvent(string seqBaseUrl, string eventId)
        {
            return $"{seqBaseUrl.TrimEnd('/')}/#/events?filter=@Id%20%3D%3D%20%22{eventId}%22&show=expanded";
        }

        public static string GetProperty(SeqConfig config, Event<LogEventData> evt, string propertyPath)
        {
            var properties = GetProperties(config, evt);

            return properties.TryGetValue(propertyPath, out var value) ? value : "";
        }

        public static IDictionary<string, string> GetProperties(SeqConfig config, Event<LogEventData> evt)
        {
            return GetProperties(config, evt.Data.Properties).ToDictionary(x => x.Key, x => x.Value);
        }

        private static IEnumerable<KeyValuePair<string, string>> GetProperties(SeqConfig config, IReadOnlyDictionary<string, object> properties, string parentPropertyPath = "")
        {
            if (properties == null) yield break;

            foreach (var property in properties)
            {
                var propertyPath = $"{parentPropertyPath}.{property.Key}".TrimStart(new[] { '.' });

                if (config.ExcludedProperties.Contains(propertyPath)) continue;

                if (property.Value is IReadOnlyDictionary<string, object> childProperties && !config.JsonSerializedProperties.Contains(propertyPath))
                {
                    foreach (var nestedProperty in GetProperties(config, childProperties, propertyPath))
                    {
                        yield return nestedProperty;
                    }
                }
                else
                {
                    yield return new KeyValuePair<string, string>(propertyPath, GetPropertyValue(propertyPath, property, config));
                }
            }
        }

        private static string GetPropertyValue(string propertyPath, KeyValuePair<string, object> property, SeqConfig config)
        {
            if (property.Value is null)
            {
                return TeamsSyntax.Code("null");
            }
            else if (string.Equals(propertyPath, ContributingEventsPropertyPath) &&
                     property.Value is IEnumerable<object> contributingEvents)
            {
                var contributingEventsListItems = new List<string>();

                // First entry contains column names, so we skip those.
                foreach (var contributingEvent in contributingEvents.Skip(1).Cast<IEnumerable<object>>())
                {
                    var columns = contributingEvent.Cast<string>().ToArray();

                    var eventId = columns[0];
                    var date = columns[1];
                    var message = columns[2];

                    var link = TeamsSyntax.Link(message, GetLinkToEvent(config.SeqBaseUrl, eventId));
                    var listItem = date + " " + link;

                    contributingEventsListItems.Add(listItem);
                }

                return TeamsSyntax.List(contributingEventsListItems);
            }
            else if (config.JsonSerializedProperties.Contains(propertyPath) || propertyPath == ResultsPropertyPath)
            {
                var settings = config.JsonSerializedPropertiesAsIndented ? JsonSettingsIndented : JsonSettingsDefault;

                return TeamsSyntax.Escape(JsonConvert.SerializeObject(property.Value, settings));
            }
            else
            {
                var value = property.Value.ToString();

                return RawProperties.Contains(propertyPath) ? value : TeamsSyntax.Escape(value);
            }
        }
    }
}
