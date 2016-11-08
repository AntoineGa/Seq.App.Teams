using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seq.App.Teams
{
    public class TeamsPotentialAction
    {

        public string Context { get; set; } = "https://schema.org";
        public string Type { get; set; } = "ViewAction";
        public string Name { get; set; }
        public string Target { get; set; }
    }
}
