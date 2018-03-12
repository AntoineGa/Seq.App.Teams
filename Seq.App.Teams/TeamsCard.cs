using Newtonsoft.Json;

namespace Seq.App.Teams
{
    public class TeamsCard
    {
        [JsonProperty(PropertyName = "@context")]
        public readonly string Context= "http://schema.org/extensions";
        [JsonProperty(PropertyName = "@type")]
        public readonly string Type = "MessageCard";
        [JsonProperty(PropertyName ="title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
        [JsonProperty(PropertyName = "themeColor")]
        public string ThemeColor { get; set; }
        [JsonProperty(PropertyName = "sections")]
        public TeamsSection[] Sections { get; set; }
        [JsonProperty(PropertyName = "potentialAction")]
        public TeamsPotentialAction[] PotentialAction { get; set; }
    }
}
