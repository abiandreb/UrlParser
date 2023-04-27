using System.Collections.Concurrent;
using HtmlAgilityPack;

namespace UrlParser;

public class JsonUrlParser
{
    private readonly ConcurrentBag<string> _results = new ConcurrentBag<string>();

    public async Task<SiteData> ParseSiteData(string url)
    {
        HttpClient client = new HttpClient();


        var response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            HtmlDocument doc = new HtmlDocument();

            doc.LoadHtml(content);

            return new SiteData
            {
                Url = url,
                RawHtml = doc.DocumentNode.InnerHtml ?? String.Empty,
                PlainText = doc.DocumentNode.InnerText ?? String.Empty
            };
        }

        return new SiteData();
    }
}