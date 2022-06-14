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

        public static (string description, string href) GetOpenLink(string seqBaseUrl, Event<LogEventData> evt)
        {
            var config = new SeqConfig { SeqBaseUrl = seqBaseUrl };

            if (evt.EventType == AlertV1EventType)
            {
                return ("Open Seq Alert", GetProperty(config, evt, SeqProperties.ResultsUrlPropertyPathV1, raw: true));
            }

            if (evt.EventType == AlertV2EventType)
            {
                return ("Open Seq Alert", GetProperty(config, evt, SeqProperties.ResultsUrlPropertyPathV2, raw: true));
            }

            return ("Open Seq Event", GetLinkToEvent(config.SeqBaseUrl, evt.Id));
        }

        public static string GetLinkToEvent(string seqBaseUrl, string eventId)
        {
            return $"{seqBaseUrl.TrimEnd('/')}/#/events?filter=@Id%20%3D%3D%20%22{eventId}%22&show=expanded";
        }

        public static string GetProperty(SeqConfig config, Event<LogEventData> evt, string propertyPath, bool raw = false)
        {
            var properties = GetProperties(config, evt, raw);

            return properties.TryGetValue(propertyPath, out var value) ? value : "";
        }

        public static IDictionary<string, string> GetProperties(SeqConfig config, Event<LogEventData> evt, bool raw = false)
        {
            return GetProperties(config, evt.Data.Properties, "", raw).ToDictionary(x => x.Key, x => x.Value);
        }

        private static IEnumerable<KeyValuePair<string, string>> GetProperties(SeqConfig config, IReadOnlyDictionary<string, object> properties, string parentPropertyPath = "", bool raw = false)
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

        private static string GetPropertyValue(string propertyPath, KeyValuePair<string, object> property, SeqConfig config, bool raw = false)
        {
            if (property.Value is null)
            {
                return TeamsSyntax.Code("null");
            }
            else if (config.JsonSerializedProperties.Contains(propertyPath) || propertyPath == SeqProperties.ResultsPropertyPath)
            {
                var settings = config.JsonSerializedPropertiesAsIndented ? JsonSettingsIndented : JsonSettingsDefault;

                var value = JsonConvert.SerializeObject(property.Value, settings);

                return raw ? value : TeamsSyntax.Escape(value);
            }
            else if (string.Equals(propertyPath, SeqProperties.ContributingEventsPropertyPath) &&
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
            else
            {
                var value = property.Value.ToString();

                return raw ? value : TeamsSyntax.Escape(value);
            }
        }
    }
}
