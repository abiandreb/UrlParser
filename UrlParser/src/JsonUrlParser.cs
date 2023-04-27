using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace UrlParser;

public class JsonUrlParser
{
    private IList<string>? _stringUrlList;
    private readonly string _filePath;
    private readonly string _resultPath;
    private readonly ConcurrentBag<string> _results = new();

    public JsonUrlParser(string filePath, string resultPath)
    {
        _filePath = filePath;
        _resultPath = resultPath;
    }

    private async Task ReadJson()
    {
        await using FileStream s = new FileStream(_filePath, FileMode.Open);
        using StreamReader sr = new StreamReader(s);
        await using JsonReader reader = new JsonTextReader(sr);
        JsonSerializer serializer = new JsonSerializer();
        _stringUrlList = serializer.Deserialize<List<string>>(reader);
    }

    private async Task WriteResultToFile()
    {
        await using StreamWriter file = File.CreateText(_resultPath);
        JsonSerializer serializer = new JsonSerializer();
        serializer.Serialize(file, _results.ToArray());
    }
    
    private void WriteSiteDataToMemory()
    {
        if (_stringUrlList != null && _stringUrlList.Count > 0)
            Parallel.ForEach(_stringUrlList, url =>
            {
                if (RetrieveSiteData(url, out var value))
                {
                    _results?.Add(JsonConvert.SerializeObject(value, Formatting.Indented));
                }
            });
    }
    
    private bool RetrieveSiteData(string url, out SiteData? siteData)
    {
        var handler = new SocketsHttpHandler
        {
            SslOptions =
            {
                CipherSuitesPolicy = new CipherSuitesPolicy(
                    new[]
                    {
                        TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384,
                        TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,
                    }),
            },
            MaxConnectionsPerServer = 100,
            Credentials = CredentialCache.DefaultCredentials
        };
        
        HttpClient client = new HttpClient(handler);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            var response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                var byteArr = response.Content.ReadAsByteArrayAsync().Result;

                var content = Encoding.Default.GetString(byteArr);

                HtmlDocument doc = new HtmlDocument();

                doc.LoadHtml(content);

                siteData = new SiteData(url, doc.DocumentNode.InnerHtml,
                    doc.DocumentNode.InnerText);

                return true;
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                Console.WriteLine($"Error retrieving {url}: {response.StatusCode}");
                siteData = null;
                return false;
            }
            
            Console.WriteLine($"Error retrieving {url}: {response.StatusCode}");
            siteData = null;
            return false;
        }

        catch (Exception e)
        {
            Console.WriteLine($"Error retrieving {e.InnerException?.Message}: Exception: {e.Message}");
        }
        
        siteData = null;
        return false;
    }

    public async Task Run()
    {
        await ReadJson();
        WriteSiteDataToMemory();
        await WriteResultToFile();
    }
}