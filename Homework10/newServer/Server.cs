using System.Net;
using newServer.Configuration;
using newServer.Handler;

namespace newServer
{
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
                var context = await _server.GetContextAsync();
                new StaticFileHandler(_configuration).Handler(context);
            }
        }
    }
}
