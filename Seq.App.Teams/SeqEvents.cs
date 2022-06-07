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
            var config = new PropertyConfig { SkipEscapeMarkDown = true };

            if (evt.EventType == AlertV1EventType)
            {
                return ("Open Seq Alert", GetProperty(evt, "ResultsUrl", config));
            }

            if (evt.EventType == AlertV2EventType)
            {
                return ("Open Seq Alert", GetProperty(evt, "Source.ResultsUrl", config));
            }

            return ("Open Seq Event", UILinkTo(seqBaseUrl, evt));
        }

        public static string GetProperty(Event<LogEventData> evt, string propertyPath, PropertyConfig config = null)
        {
            var properties = GetProperties(evt, config ?? new PropertyConfig());

            return properties.TryGetValue(propertyPath, out var value) ? value : "";
        }

        public static IDictionary<string, string> GetProperties(Event<LogEventData> evt, PropertyConfig config)
        {
            return GetProperties(evt.Data.Properties, config, "").ToDictionary(x => x.Key, x => x.Value);
        }

        public static string UILinkTo(string seqBaseUrl, Event<LogEventData> evt)
        {
            return $"{seqBaseUrl.TrimEnd('/')}/#/events?filter=@Id%20%3D%3D%20%22{evt.Id}%22&show=expanded";
        }

        private static IEnumerable<KeyValuePair<string, string>> GetProperties(IReadOnlyDictionary<string, object> properties, PropertyConfig config, string parentPropertyPath)
        {
            if (properties == null) yield break;

            foreach (var property in properties)
            {
                var propertyPath = $"{parentPropertyPath}.{property.Key}".TrimStart(new[] { '.' });

                if (config.ExcludedProperties.Contains(propertyPath)) continue;

                if (property.Value is IReadOnlyDictionary<string, object> childProperties && !config.JsonSerializedProperties.Contains(propertyPath))
                {
                    foreach (var nestedProperty in GetProperties(childProperties, config, propertyPath))
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

        private static string GetPropertyValue(string propertyPath, KeyValuePair<string, object> property, PropertyConfig config)
        {
            string value;

            if (property.Value is null)
            {
                return "`null`";
            }
            else if (config.JsonSerializedProperties.Contains(propertyPath))
            {
                var settings = config.JsonSerializedPropertiesAsIndented ? JsonSettingsIndented : JsonSettingsDefault;

                value = JsonConvert.SerializeObject(property.Value, settings);
            }
            else
            {
                value = property.Value.ToString();
            }

            return config.SkipEscapeMarkDown ? value : value.EscapeMarkdown();
        }
    }
}
