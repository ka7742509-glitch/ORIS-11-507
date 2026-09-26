using System.Text.Json;

namespace SearchEngineServer;

class Program
{
    static async Task Main(string[] args)
    {
        string settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        if (!File.Exists(settingsPath))
        {
            settingsPath = "settings.json";
        }

        string json = await File.ReadAllTextAsync(settingsPath);
        var settings = JsonSerializer.Deserialize<ServerSettings>(json);

        if (settings == null)
        {
            Console.WriteLine("Ошибка: не удалось прочитать settings.json");
            return;
        }

        string address = $"http://{settings.Host}:{settings.Port}{settings.Path}";

        string htmlContent = GetHtmlContent();

        var server = new HttpServer(address, htmlContent);
        server.Start();

        while (true)
        {
            string? input = Console.ReadLine();
            if (input?.Trim().ToLower() == "stop")
            {
                server.Stop();
                break;
            }
        }
    }

    private static string GetHtmlContent()
    {
        return @"<!DOCTYPE html>
<html lang=""ru"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Поисковик</title>
  <style>
    * { box-sizing: border-box; }

    body {
      margin: 0;
      min-height: 100vh;
      min-height: 100svh;
      font-family: system-ui, -apple-system, ""Segoe UI"", Roboto, sans-serif;
      color: #16222c;
      background: #edf0f3;
    }

    .corner { position: fixed; top: 18px; right: 22px; text-align: right; }
    .temp   { font-size: 2rem; font-weight: 700; }
    .city   { color: #2b7de0; font-size: .95rem; }

    .scene {
      min-height: 100vh;
      min-height: 100svh;
      display: grid;
      grid-template-rows: 1fr auto 1fr;
      justify-items: center;
      padding: 24px;
      text-align: center;
    }

    .hello  { grid-row: 2; display: grid; gap: 12px; }
    .clock  {
      font-weight: 700;
      font-size: clamp(2.6rem, 13vw, 5rem);
    }
    .greeting { margin: 0; color: #2b7de0; font-size: clamp(1rem, 4vw, 1.3rem); }

    .bottom {
      grid-row: 3;
      align-self: end;
      width: 100%;
      display: grid;
      justify-items: center;
      gap: clamp(20px, 4vh, 32px);
    }

    .search {
      display: flex; align-items: center; gap: 12px;
      width: min(560px, 100%);
      height: 56px; padding: 0 20px;
      background: rgba(24, 42, 48, .09);
      border: 1px solid transparent;
      border-radius: 18px;
      transition: background .25s, border-color .25s, box-shadow .25s;
    }
    .search__icon { flex: none; color: #7d8b93; }
    .search input {
      flex: 1; min-width: 0;
      border: 0; background: transparent; outline: none;
      font: inherit; font-size: 16px;
      color: #16222c;
    }
    .search input::placeholder { color: #9aa7ad; }
    .search:hover { background: rgba(24, 42, 48, .12); }
    .search:focus-within {
      background: #fff;
      border-color: rgba(43, 125, 224, .4);
      box-shadow: 0 0 0 4px rgba(43, 125, 224, .12);
    }

    .shortcuts {
      display: grid;
      grid-template-columns: repeat(5, min(72px, 17vw));
      gap: 14px;
      justify-content: center;
    }
    .tile {
      aspect-ratio: 1;
      display: grid; place-items: center;
      background: rgba(24, 42, 48, .07);
      border-radius: 16px;
      transition: transform .2s, background .2s, box-shadow .2s;
    }
    .tile img { width: 26px; height: 26px; }
    .tile:hover {
      transform: translateY(-3px);
      background: #fff;
      box-shadow: 0 12px 22px -12px rgba(24, 42, 48, .35);
    }
    .tile--add { font-size: 1.5rem; color: #55656d; text-decoration: none; }

    @media (min-width: 720px) {
      .scene { grid-template-rows: auto; align-content: center; gap: clamp(30px, 6vh, 56px); }
      .hello, .bottom { grid-row: auto; align-self: auto; }
    }

    @media (prefers-reduced-motion: reduce) {
      * { transition: none !important; }
    }
  </style>
</head>
<body>

  <header class=""corner"">
    <div class=""temp"">23°</div>
    <div class=""city"">Amsterdam</div>
  </header>

  <main class=""scene"">
    <div class=""hello"">
      <div class=""clock"">09:11:01</div>
      <p class=""greeting"">Hello, my friend!</p>
    </div>

    <div class=""bottom"">
      <div class=""search"">
        <svg class=""search__icon"" width=""20"" height=""20"" viewBox=""0 0 24 24""
             fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"">
          <circle cx=""11"" cy=""11"" r=""7""></circle>
          <path d=""m20 20-3.5-3.5""></path>
        </svg>
        <input type=""text"" placeholder=""Search..."" aria-label=""Поиск"">
      </div>

      <nav class=""shortcuts"">
        <a class=""tile"" href=""#""><img src=""https://cdn.simpleicons.org/github/1b1f23"" alt=""GitHub""></a>
        <a class=""tile"" href=""#""><img src=""https://cdn.simpleicons.org/openai/000000"" alt=""ChatGPT""></a>
        <a class=""tile"" href=""#""><img src=""https://cdn.simpleicons.org/youtube/FF0000"" alt=""YouTube""></a>
        <a class=""tile"" href=""#""><img src=""https://cdn.simpleicons.org/telegram/229ED9"" alt=""Telegram""></a>
        <a class=""tile"" href=""#""><img src=""https://cdn.simpleicons.org/instagram/D62976"" alt=""Instagram""></a>
        <a class=""tile tile--add"" href=""#"">+</a>
      </nav>
    </div>
  </main>

</body>
</html>";
    }
}

public class ServerSettings
{
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 8888;
    public string Path { get; set; } = "/connection/";
}