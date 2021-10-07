using System.Collections.Generic;
using Seq.Apps;
using Seq.Apps.LogEvents;

namespace Seq.App.Teams
{
    public static class SeqEvents
    {
        private const uint AlertV1EventType = 0xA1E77000, AlertV2EventType = 0xA1E77001;
        
        public static (string description, string href) GetOpenLink(string seqBaseUrl, Event<LogEventData> evt)
        {
            if (evt.EventType == AlertV1EventType)
            {
                return ("Open Seq Alert", GetProperty(evt, "ResultsUrl", raw: true));
            }

            if (evt.EventType == AlertV2EventType)
            {
                return ("Open Seq Alert", GetProperty(evt, "Source.ResultsUrl"));
            }

            return ("Open Seq Event", UILinkTo(seqBaseUrl, evt));
        }
        
        public static string GetProperty(Event<LogEventData> evt, string propertyPath, bool raw = false)
        {
            var path = new Queue<string>(propertyPath.Split('.'));
            var root = evt.Data.Properties;

            while(root != null)
            {
                var step = path.Dequeue();
                if (!root.TryGetValue(step, out var next))
                    return "";

                if (path.Count == 0)
                {
                    if (next == null) return "`null`";
                    return raw ? next.ToString() : next.ToString().EscapeMarkdown();
                }

                root = next as IReadOnlyDictionary<string, object>;
            }

            return "";
        }
        
        public static string UILinkTo(string seqBaseUrl, Event<LogEventData> evt)
        {
            return $"{seqBaseUrl.TrimEnd('/')}/#/events?filter=@Id%20%3D%3D%20%22{evt.Id}%22&show=expanded";
        }
    }
}
