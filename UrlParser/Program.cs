using Newtonsoft.Json;
using System.Diagnostics;
using HtmlAgilityPack;
using UrlParser;

DirectoryInfo? solutionDir = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent;

string filePath = Path.Combine(Directory.GetCurrentDirectory(), solutionDir?.ToString() ?? string.Empty, "raw-urls.json");

var urlArr = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(filePath));

Stopwatch sw = new Stopwatch();

sw.Start();
JsonUrlParser parser = new JsonUrlParser();



/*ConcurrentBag<string> results = new ConcurrentBag<string>();

if (urlArr != null)
    Parallel.ForEach(urlArr, url =>
    {
        SiteData parsed = parser.Parse(url);
        results.Add(JsonConvert.SerializeObject(parsed));
    });

JArray merged = new JArray(results);*/

sw.Stop();

Console.WriteLine("Elapsed={0}",sw.Elapsed);
