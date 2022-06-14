using System.Collections.Generic;

namespace Seq.App.Teams
{
    public static class SeqProperties
    {
        public const string ResultsPropertyPath = "Source.Results";
        public const string ResultsUrlPropertyPathV1 = "ResultsUrl";
        public const string ResultsUrlPropertyPathV2 = "Source.ResultsUrl";
        public const string ContributingEventsPropertyPath = "Source.ContributingEvents";

        public static readonly List<string> SpecialTreatedPropertyPaths = new List<string>
        {
            ResultsUrlPropertyPathV1,
            ResultsUrlPropertyPathV2
        };
    }
}
