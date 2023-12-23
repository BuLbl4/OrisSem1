namespace newServer.Services;

public interface IEmailSenderService
{
    public void SendEmail(string email, string password);
}