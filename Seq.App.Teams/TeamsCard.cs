using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seq.App.Teams
{
    public class TeamsCard
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public string ThemeColor { get; set; } = "black";
        public TeamsPotentialAction[] PotentialAction { get; set; }
    }
}
