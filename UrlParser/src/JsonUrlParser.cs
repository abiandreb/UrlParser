using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Text.Json;

namespace UrlParser;

public class JsonUrlParser
{
    private readonly HttpClient _httpClient;

    private readonly List<SiteData?>? _siteDataList = new();
    private List<string>? _urlList;
    private readonly CancellationToken _token = new CancellationTokenSource().Token;
    private readonly string _filePath;
    private readonly string _resultPath;
    
    public JsonUrlParser(string filePath, string resultPath)
    {
        _filePath = filePath;
        _resultPath = resultPath;

        _httpClient = new HttpClient(
            new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                MaxConnectionsPerServer = 50,
                SslProtocols = SslProtocols.Tls12,
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            }
        );
        
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
    }

    public async Task Run()
    {
        await ReadFromJsonAsync(_filePath);
        await ReadToMemoryAsync();
        await WriteToJsonFileAsync(_resultPath);
    }
    
    private async Task ReadToMemoryAsync()
    {
        if (_urlList != null)
        {
            var tasks = _urlList.Select(async url =>
            {
                var siteData = await RetrieveSiteDataAsync(url, _token);
                _siteDataList?.Add(siteData);
            });

            await Task.WhenAll(tasks);
        }
    }

    private async Task<SiteData?> RetrieveSiteDataAsync(string url, CancellationToken token)
    {
        try
        {
            var response = await _httpClient.GetAsync(url, token);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(token);
            
            return new SiteData(url, content, content.StripHtmlTags());
        }
        
        catch (Exception e)
        {
            Console.WriteLine($"Can not parse: {url}. Error: {e.Message}.");
        }

        return null;
    }
    
    private async Task ReadFromJsonAsync(string path)
    {
        await using FileStream fileStream = File.OpenRead(path);

        _urlList = await JsonSerializer.DeserializeAsync<List<string>>(fileStream, cancellationToken: _token) ?? new List<string>();
    }
    
    private async Task WriteToJsonFileAsync(string path, int chunkSize = 250)
    {
        if (_siteDataList != null)
        {
            var groups = _siteDataList
                .Select((value, index) => new { value, index })
                .GroupBy(x => x.index / chunkSize)
                .Select(x => x.Select(y => y.value))
                .ToArray();

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        
            for (int i = 0; i < groups.Length; i++)
            {
                await using FileStream fileStream = File.OpenWrite(path + $"/parsed-urls-{i}.json");
                {
                    await JsonSerializer.SerializeAsync(fileStream, groups[i], options, _token);
                }
            }
        }
    }
}