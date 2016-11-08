using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Seq.App.Teams
{
    public class TeamsPotentialAction
    {
        [JsonProperty(PropertyName = "@context")]
        public string Context { get; set; } = "https://schema.org";
        [JsonProperty(PropertyName = "@type")]
        public string Type { get; set; } = "ViewAction";
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "target")]
        public string[] Target { get; set; }
    }
}
