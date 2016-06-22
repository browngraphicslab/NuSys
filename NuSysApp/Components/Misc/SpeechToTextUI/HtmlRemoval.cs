using System.Text.RegularExpressions;

namespace NuSysApp.Misc.SpeechToTextUI
{
    /// <summary>
    /// Methods to remove HTML from strings.
    /// </summary>
    public static class HtmlRemoval
    {



        public static string StripTagsReplaceDivCloseWithNewLines(string source)
        {
            // create newText option for returning
            string newText;

            // replace div close with new lines
            Regex regex = new Regex(@"(</div>)", RegexOptions.Compiled);
            newText = regex.Replace(source, "\r\n");

            // remove remaining html elements
            Regex _htmlRegex = new Regex("<.*?>", RegexOptions.Compiled);
            return _htmlRegex.Replace(newText, string.Empty);

        }
    }

}
