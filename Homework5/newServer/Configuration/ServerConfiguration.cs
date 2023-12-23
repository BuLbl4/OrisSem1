using System.Net;
using System.Text.Json;

namespace newServer.Configuration
{
    public class ServerConfiguration
    {
        private const string _configFilePath = "appsettings.json";
        private AppSettings? _config;

        public ServerConfiguration()
        {
            _config = new AppSettings();
        }

        public void Set(HttpListener httplistener)
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    Console.WriteLine("json file not found!");
                }

                using (FileStream file = File.OpenRead(_configFilePath))
                {
                    _config = JsonSerializer.Deserialize<AppSettings>(file);
                }

                httplistener.Prefixes.Add($"{_config!.Address}:{_config.Port}/");
                httplistener.Prefixes.Add($"http://localhost:{_config.Port}/");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Configuration: {ex.Message}");
                Console.ResetColor();
            }
            finally
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Configuration: All configurations set!");
                Console.ResetColor();
            }
        }

        public string StaticFilesPath
        {
            get
            {
                return _config!.StaticFilesPath!;
            }
            set
            {
                if (!Directory.Exists(_config!.StaticFilesPath))
                {
                    Directory.CreateDirectory(_config.StaticFilesPath!);
                }
            }
        }
    }
}