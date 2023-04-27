namespace UrlParser;

public class SiteData
{
    public SiteData(string url, string rawHtml, string plainText)
    {
        Url = url;
        RawHtml = rawHtml;
        PlainText = plainText;
    }

    public string Url { get; set; }
    public string RawHtml { get; set; }
    public string PlainText { get; set; }
}