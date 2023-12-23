using System.Net;
using System.Text;
using newServer.Configuration;

namespace newServer;

public class Server
{
    private readonly HttpListener _server;
    private readonly ServerConfiguration _configuration; 
    private bool _isAlive; 
    private readonly object _lock = new object();
    public Server() 
    { 
        _configuration = new ServerConfiguration(); 
        _server = new HttpListener();
    }
    public async Task Start()
    { 
        _configuration.Set(_server); 
        _server.Start(); 
        _isAlive = true; 
        Console.WriteLine("The server started working!");
        var serverTask = ServerProcessAsync();

        Console.WriteLine("Type 'stop' and press Enter to stop the server.");
        while (_isAlive)
        {
            if (Console.ReadLine()?.ToLower() == "stop")
            {
                Stop();
                _isAlive = false;
            }
        }

        await serverTask;
    }

    private void Stop()
    {
        lock (_lock)
        {
            if (_isAlive)
            {
                _server.Stop();
                _server.Close();
                Console.WriteLine("The server has been stopped.");
            }
        }
    }

    private async Task ServerProcessAsync()
    {
        while (_isAlive)
        {
            Console.WriteLine();
            var context = await _server.GetContextAsync();
            HandleRequest(context);
        }
    }

    private void HandleRequest(HttpListenerContext context)
    {
        try
        {
            HttpListenerRequest request = context.Request;
            string filePath = request.Url!.AbsolutePath;
            var responseText = String.Empty;

            if (filePath.EndsWith("/"))
                filePath = _configuration.StaticFilesPath + "/index.html";

            if (filePath.StartsWith("/"))
                filePath = filePath.Trim('/');

            if (!File.Exists(filePath))
            {
                filePath = $"{_configuration.StaticFilesPath}/404.html";
            }

            ParseExtention(filePath.Substring(filePath.LastIndexOf('.')), context.Response);

            responseText = File.ReadAllText(filePath);

            HttpListenerResponse response = context.Response;
            byte[] buffer = Encoding.UTF8.GetBytes(responseText);

            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error handling request: {ex.Message}");
            Console.ResetColor();
        }
    }

    private void ParseExtention(string extention, HttpListenerResponse response)
    {
        switch (extention)
        {
            case ".html":
                response.ContentType = "text/html";
                break;
            case ".css":
                response.ContentType = "text/css";
                break;
            case ".jpeg":
                response.ContentType = "image/jpeg";
                break;
            case ".png":
                response.ContentType = "image/png";
                break;
            case ".svg":
                response.ContentType = "image/svg+xml";
                break;
            case ".ico":
                response.ContentType = "image/x-icon";
                break;
            case ".webp":
                response.ContentType = "image/webp";
                break;
        }
    }
}