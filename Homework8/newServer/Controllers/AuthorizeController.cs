using System.Net;
using System.Text.Json;
using newServer.Attributs;
using newServer.Configuration;
using newServer.Model;
using newServer.Services;

namespace newServer.Controllers;
[Controller("Authorize")]
public class AuthorizeController
{
    private const string _accountsFilePath = "account.json"; 
    private List<Account> ? _accounts;
    public AuthorizeController() 
    { 
        GetAccountsFromJson();
    }
 
    private void GetAccountsFromJson()
    {
        if (!File.Exists(_accountsFilePath))
        {
            Console.WriteLine("json file not found!");
            throw new FileNotFoundException("json file not found!");
        }
 
        using (FileStream file = File.OpenRead(_accountsFilePath))
        {
            _accounts = JsonSerializer.Deserialize<List<Account>>(file);
        }
    }
 
    [Post("Add")]
    public void Add(string email, string password, HttpListenerContext context)
    {
 
        _accounts!.Add(new Account
        {
            id = _accounts[_accounts.Count - 1].id++,
            email = email,
            password = password
        });
        string json = JsonSerializer.Serialize(_accounts, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_accountsFilePath, json);
        context.Response.Redirect("/");
    }
 
    [Post("Delete")]
    public void Delete(string id,HttpListenerContext context)
    {
        foreach(var account in _accounts!)
        {
            if (account.id == int.Parse(id))
            {
                _accounts.Remove(account);
                break;
            } 
        }
        string json = JsonSerializer.Serialize(_accounts, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_accountsFilePath, json);
        context.Response.Redirect("/");

    }
 
    [Post("Update")]
    public void Update(string id, string email, string password, HttpListenerContext context)
    {
        Account acc = new Account();
 
        foreach (var account in _accounts!)
        {
            if (account.id == int.Parse(id))
            {
                acc = account;
                acc.email = email;
                acc.password = password;
                break;
            }   
        }
        string json = JsonSerializer.Serialize(_accounts, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_accountsFilePath, json);
    }
 
    [Get("GetById")]
    public Account GetById(string id, HttpListenerContext context)
    {
        Account acc = new Account();
 
        foreach (var account in _accounts!)
        {
            if (account.id == int.Parse(id))
            {
                acc = account;
                break;
            }   
        }
 
        return acc;
    }
 
    [Get("GetAll")]
    public Account[] GetAll(HttpListenerContext context)
    {
        return _accounts!.ToArray();
    }
}