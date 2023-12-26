using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace newServer.Services;

public class PageGenerator
{
    private HttpListenerContext _context;

    public PageGenerator(HttpListenerContext context)
    {
        _context = context;
    }

    private TemplateEngine _templateEngine = new TemplateEngine();
    
    public async void Generate(string fileName)
    {
        string filePath = "text.txt"; 
            
        string[] fileLines = File.ReadAllLines(filePath);
        var obj = new
        {
            Lines = new List<string>(fileLines)
        };
            
       

      

        string html = File.ReadAllText(fileName);

        string newHtml = "";
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);
        HtmlNode elementToRemove = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'shablon')]");
        Console.WriteLine(elementToRemove.InnerText);

        if (elementToRemove != null)
        {
            newHtml += $"<div>{_templateEngine.Render(elementToRemove.InnerText, obj )}</div>";
            HtmlNode newNode = HtmlNode.CreateNode(newHtml);
            elementToRemove.ParentNode.ReplaceChild(newNode, elementToRemove);
        }

        await SendHtml(_context, doc.DocumentNode.OuterHtml);
    }
    private async Task SendHtml(HttpListenerContext context, string html)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(html);
        context.Response.ContentType = "text/html";
        context.Response.ContentLength64 = buffer.Length;

        using Stream output = context.Response.OutputStream;
        await output.WriteAsync(buffer);
        await output.FlushAsync();
        context.Response.Close();
    }
}