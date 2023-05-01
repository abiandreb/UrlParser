using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using UrlParser;

DirectoryInfo? solutionDir = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent;

string filePath = Path.Combine(Directory.GetCurrentDirectory(), solutionDir?.ToString() ?? string.Empty, "raw-urls.json");
string resultPath = Path.Combine(Directory.GetCurrentDirectory(), solutionDir?.ToString() ?? string.Empty);

Stopwatch sw = new Stopwatch();

sw.Start();

JsonUrlParser urlParser = new JsonUrlParser(filePath, resultPath);
await urlParser.Run();
sw.Stop();

Console.WriteLine("\n\n\nElapsed={0}",sw.Elapsed);