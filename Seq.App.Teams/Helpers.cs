namespace Seq.App.Teams
{
    public static class Helpers
    {
        public static string EscapeMarkdown(this string str)
        {
            return str.Replace(@"\", @"\\")
                .Replace("`", @"\`")
                .Replace("*", @"\*")
                .Replace("{", @"\{")
                .Replace("}", @"\}")
                .Replace("[", @"\[")
                .Replace("]", @"\]")
                .Replace("(", @"\(")
                .Replace(")", @"\)")
                .Replace("#", @"\#")
                .Replace(">", @"\>")
                .Replace("+", @"\+")
                .Replace("-", @"\-")
                .Replace("_", @"\_")
                .Replace(".", @"\.")
                .Replace("!", @"\!")
                .Replace("~", @"\~")

                // Line breaks require a two following spaces
                .Replace("\r\n", "  \r\n");
        }
    }
}
