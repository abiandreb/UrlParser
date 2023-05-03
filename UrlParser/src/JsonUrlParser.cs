using System.Net;
using System.Text.Json;

namespace UrlParser
{
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
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;

            _filePath = filePath;
            _resultPath = resultPath;

            var socketsHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(1),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
                MaxConnectionsPerServer = 10,
            };

            _httpClient = new HttpClient(socketsHandler);
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

                    {
                        if (_siteDataList != null) Console.Write(_siteDataList.Count);
                    }

                    _siteDataList?.Add(siteData);
                });

                await Task.WhenAll(tasks);
            }
        }

        private async Task<SiteData?> RetrieveSiteDataAsync(string url, CancellationToken token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.ConnectionClose = false;

                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(token);
                    return new SiteData(url, content, content);
                }
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
}
