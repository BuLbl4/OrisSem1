using System.Net;
using newServer.Attributs;
using newServer.Configuration;
using newServer.Model;
using newServer.ORM;
using newServer.Services;

namespace newServer.Controllers;
[Controller("Authorize")]
public class AuthorizeController
{
    private MyDataContext db = new MyDataContext();
    
    [Post("Add")]
   public void Add(string email, string password,ServerConfiguration configuration, HttpListenerContext context)
    {
        var accounts = db.Select<Account>();
        var acc = new Account()
        {
            id = accounts[accounts.Count - 1].id + 1,
            email = email,
            password = password
        };
        db.Add(acc);
        new EmailSender(configuration).SendEmail(email,password);
    }
 
    [Post("Delete")]
    public void Delete(string id,HttpListenerContext context)
    {
        db.Delete<Account>(int.Parse(id));
        context.Response.Redirect("/");
    }
 
    [Post("Update")]
    public void Update(string id, string email, string password, HttpListenerContext context)
    {
        var acc = new Account()
        {
            id = int.Parse(id),
            email = email,
            password = password
        };
        db.Update(acc);
    }
 
    [Get("GetById")]
    public Account GetById(string id, HttpListenerContext context)
    {
        return db.SelectById<Account>(id);
    }
 
    [Get("GetAll")]
    public Account[] GetAll(HttpListenerContext context)
    {
        return db.Select<Account>().ToArray();
    }
}
