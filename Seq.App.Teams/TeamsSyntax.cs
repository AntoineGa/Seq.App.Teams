using System;
using System.Collections.Generic;
using System.Linq;

namespace Seq.App.Teams
{
    public static class TeamsSyntax
    {
        public static string Escape(string text)
        {
            return text.Replace(@"\", @"\\")
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

                       // Line breaks require a two leading spaces.
                       .Replace("\r\n", "  \r\n");
        }

        public static string Code(string code)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));

            return "`" + Escape(code) + "`";
        }

        public static string Link(string text, string url)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (url == null) throw new ArgumentNullException(nameof(url));

            return "[" + Escape(text) + "](" + Escape(url) + ")";
        }

        public static string List(IEnumerable<string> items)
        {
            return string.Join("\r\n", items.Select(x => "- " + x));
        }
    }
}
