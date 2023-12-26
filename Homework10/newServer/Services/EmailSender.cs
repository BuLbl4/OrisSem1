using MailKit.Net.Smtp;
using MimeKit;
using newServer.Configuration;

namespace newServer.Services
{
    public class EmailSender: IEmailSenderService
    {
        private readonly ServerConfiguration _configuration;

        public EmailSender(ServerConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmail(string email, string password)
        {
            try
            {
                using var smtpClient = new SmtpClient();
                
                MimeMessage message = GenerateMessage(email, password);
                smtpClient.Connect(_configuration.SmtpServerHost, _configuration.SmtpServerPort, true);
                smtpClient.Authenticate(_configuration.MailSender, _configuration.PasswordSender);
                smtpClient.Send(message);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Message\n");
                Console.ResetColor();
                smtpClient.Disconnect(true);
                smtpClient.Dispose();
                
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to send mail message: {ex.Message}");
                Console.ResetColor();
            }
        }

        private MimeMessage GenerateMessage(string email, string password)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Server", _configuration!.MailSender));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Hello from battle.net!";
            var builder = new BodyBuilder();

            builder.HtmlBody = string.Format
            (
                $"<body><h4>Email: {email}, Password: {password}</h4></body>"
                
            );

            builder.Attachments.Add(@"C:\Users\Булат\Desktop\1676904410_new_preview_5b7IoDZ3xbQ.jpg");
            message.Body = builder.ToMessageBody();

            return message;
        }
    }
}