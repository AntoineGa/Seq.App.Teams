using Newtonsoft.Json;

namespace Seq.App.Teams
{
    public class TeamsFact
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
}
