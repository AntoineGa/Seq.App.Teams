using System.Collections.Generic;

namespace Seq.App.Teams
{
    public class PropertyConfig
    {
        public List<string> JsonSerializedProperties { get; set; } = new List<string>();
        public List<string> ExcludedProperties { get; set; } = new List<string>();
        public bool JsonSerializedPropertiesAsIndented { get; set; }
        public bool SkipEscapeMarkDown { get; set; }
    }
}
