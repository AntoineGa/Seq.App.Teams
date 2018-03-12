using Newtonsoft.Json;

namespace Seq.App.Teams
{
    public class TeamsSection
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
        [JsonProperty(PropertyName = "facts")]
        public TeamsFact[] Facts { get; set; }
    }
}
