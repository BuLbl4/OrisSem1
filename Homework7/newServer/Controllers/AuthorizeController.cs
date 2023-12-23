using System.Net;
using newServer.Attributs;
using newServer.Configuration;
using newServer.Services;

namespace newServer.Controllers;
[Controller("Authorize")]
public class AuthorizeController
{
    [Post("SendEmail")]
    public void SendEmail(string email, string password, ServerConfiguration configuration, HttpListenerContext context)
    {
        var sender = new EmailSender(configuration);
        sender.SendEmail(email,password);
        context.Response.Redirect("/");
    }
}