using System.Text.RegularExpressions;

namespace UrlParser;

public static class StringExtensions
{
    public static string StripHtmlTags(this string input)
    {
        return Regex.Replace(input, "<.*?>", string.Empty);
    }
}