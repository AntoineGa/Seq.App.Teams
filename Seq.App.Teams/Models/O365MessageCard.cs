using Seq.App.Teams.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seq.App.Teams.Models
{
    public class O365MessageCard : O365ConnectorCard
    {
        [JsonProperty(PropertyName = "@context")]
        public readonly string Context = "http://schema.org/extensions";
        [JsonProperty(PropertyName = "@type")]
        public readonly string Type = "MessageCard";
    }
}
