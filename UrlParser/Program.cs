using System.Diagnostics;
using UrlParser;

using HttpClient client = new HttpClient();

var response = await client.GetAsync("https://djinni.co/jobs/?primary_keyword=.NET&exp_level=2y");

var dou = await response.Content.ReadAsStringAsync();

DirectoryInfo? solutionDir = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent;

string filePath = Path.Combine(Directory.GetCurrentDirectory(), solutionDir?.ToString() ?? string.Empty, "raw-urls.json");
string resultPath = Path.Combine(Directory.GetCurrentDirectory(), solutionDir?.ToString() ?? string.Empty, "parsed-urls.json");

Stopwatch sw = new Stopwatch();

sw.Start();

JsonUrlParser parser = new JsonUrlParser(filePath, resultPath);
await parser.Run();

sw.Stop();

Console.WriteLine("\n\n\nElapsed={0}",sw.Elapsed);
