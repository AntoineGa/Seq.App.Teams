using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Seq.App.Teams
{
    public class TeamsCard
    {
        [JsonProperty(PropertyName ="title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
        [JsonProperty(PropertyName = "themeColor")]
        public string ThemeColor { get; set; } = "black";
        [JsonProperty(PropertyName = "potentialAction")]
        public TeamsPotentialAction[] PotentialAction { get; set; }
    }
}
