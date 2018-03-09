using Newtonsoft.Json;

namespace Seq.App.Teams
{
    public class TeamsPotentialAction
    {
        [JsonProperty(PropertyName = "@context")]
        public readonly string Context = "https://schema.org";
        [JsonProperty(PropertyName = "@type")]
        public readonly string Type = "OpenUri";
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "targets")]
        public TeamsActionTarget[] Targets { get; set; }
    }
}
