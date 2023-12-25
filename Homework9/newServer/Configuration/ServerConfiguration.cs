using System.Net;
using System.Text.Json;

namespace newServer.Configuration
{
    public class ServerConfiguration
    {
        const string configFilePath = "appsettings.json";
        private AppSettings config;

        public ServerConfiguration()
        {
            config = new AppSettings();
        }

        public void Set(HttpListener httplistener)
        {
            try
            {
                if (!File.Exists(configFilePath))
                {
                    throw new Exception("json file not found!");
                }

                using (FileStream file = File.OpenRead(configFilePath))
                {
                    config = JsonSerializer.Deserialize<AppSettings>(file);
                }
                
                httplistener.Prefixes.Add($"{config.Address}:{config.Port}/");
                httplistener.Prefixes.Add($"http://localhost:{config.Port}/");
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
                
            }
        }
        public string StaticFilesPath
        {
            get
            {
                return config.StaticFilesPath;
            }
            set
            {
                if (!Directory.Exists(config.StaticFilesPath))
                    Directory.CreateDirectory(config.StaticFilesPath);
            }

        }
        public string MailSender
        {
            get { return config.MailSender; }
        }

        public string PasswordSender
        {
            get { return config.PasswordSender; }
        }

        public string FromEmail
        {
            get { return config.FromEmail; }
        }

        public string SmtpServerHost
        {
            get { return config.SmtpServerHost; }
        }

        public int SmtpServerPort
        {
            get { return config.SmtpServerPort; }
        }
    }
}