using Newtonsoft.Json;

namespace Seq.App.Teams
{
    public class TeamsActionTarget
    {
        [JsonProperty(PropertyName = "os")]
        public string Os { get; set; } = "default";

        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; }
    }
}
