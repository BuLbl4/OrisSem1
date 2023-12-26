using Newtonsoft.Json;

namespace newServer.Configuration
{
    public class AppSettings
    {
        [JsonProperty("Address")]
        public string ? Address { get; set; }

        [JsonProperty("Port")]
        public uint Port { get; set; }

        [JsonProperty("StaticFilesPath")]
        public string StaticFilesPath { get; set; }

        [JsonProperty("MailSender")]
        public string MailSender { get; set; }

        [JsonProperty("PasswordSender")]
        public string PasswordSender { get; set; }

        [JsonProperty("FromEmail")]
        public string FromEmail { get; set; }

        [JsonProperty("SmtpServerHost")]
        public string SmtpServerHost { get; set; }

        [JsonProperty("SmtpServerPort")]
        public int SmtpServerPort { get; set; }

    }
}