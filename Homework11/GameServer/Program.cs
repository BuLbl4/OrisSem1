


using Game.Utils.Paths;
using System.Drawing;
using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using System.Text;

var server = new ServerObject();

server.Listen();

internal class ClientObject
{
    protected internal string Id { get; } = Guid.NewGuid().ToString();
    protected internal StreamWriter Writer { get; }
    protected internal StreamReader Reader { get; }
    public string? UserName { get; set; }
    public string? Color { get; set; }

    private readonly TcpClient _client;
    private readonly ServerObject _server;

    public ClientObject(TcpClient tcpClient, ServerObject serverObject)
    {
        _client = tcpClient;
        _server = serverObject;

        var stream = _client.GetStream();

        Reader = new StreamReader(stream);

        Writer = new StreamWriter(stream);
    }


    public async Task ProcessAsync()
    {
        try
        {
            UserName = await Reader.ReadLineAsync();
            var addUserMessage = new AddUser { UserName = UserName, Color = "" };
            await _server.BroadcastColoredMessageAsync(addUserMessage);

            var message = $"{UserName} вошел ";
            Console.WriteLine(message);

            await _server.SendListAsync();
            await _server.BroadcastPointsFieldMessageAsync();

            while (true)
            {
                await Task.Delay(10);

                try
                {
                    message = await Reader.ReadLineAsync();
                    var point = JsonSerializer.Deserialize<SendPoint>(message!);
                    await _server.AddPoint(point!);
                }
                catch
                {
                    message = $"{UserName} GG";
                    Console.WriteLine(message);
                    _server.RemoveConnection(Id);
                    await _server.SendListAsync();
                    await _server.BroadcastMessageAsync(message, Id);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            _server.RemoveConnection(Id);
        }
    }

    protected internal void Close()
    {
        Writer.Close();
        Reader.Close();
        _client.Close();
    }
}
internal class ServerObject
{
    private readonly TcpListener _tcpListener = new(IPAddress.Any, 8888);
    private readonly List<ClientObject> _clients = new();
    private readonly List<SendPoint> _pointsField = new();

    protected internal void RemoveConnection(string id)
    {
        var client = _clients.FirstOrDefault(c => c.Id.Equals(id));

        if (client != null)
            _clients.Remove(client);
        client?.Close();
    }

    protected internal void Listen()
    {
        try
        {
            _tcpListener.Start();
            Console.WriteLine("Сервер пашет");

            while (true)
            {
                var tcpClient = _tcpListener.AcceptTcpClient();

                var clientObject = new ClientObject(tcpClient, this);
                _clients.Add(clientObject);
                Task.Run(clientObject.ProcessAsync);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Disconnect();
        }
    }

    private string GenerateRandomColor()
    {
        var random = new Random();
        var color = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
        var hexColor = ColorTranslator.ToHtml(color);
        while (_clients.Select(i => i.Color).Contains(hexColor))
            color = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
        return ColorTranslator.ToHtml(color);
    }

    protected internal async Task SendListAsync()
    {
        var sb = new StringBuilder();
        sb.Append("SendList ");
        sb.Append(JsonSerializer.Serialize(_clients.Select(x => new AddUser(x.UserName!, x.Color!)).ToList()));

        foreach (var client in _clients)
        {
            await client.Writer.WriteLineAsync(sb);
            await client.Writer.FlushAsync();
        }
    }

    protected internal async Task BroadcastColoredMessageAsync(AddUser addUser)
    {
        var color = GenerateRandomColor();
        addUser.Color = color;
        _clients.Last().Color = color;

        var sb = new StringBuilder();
        sb.Append("AddUser ");
        var message = JsonSerializer.Serialize(addUser);
        sb.Append(message);
        {
            try
            {
                await _clients.Last().Writer.WriteLineAsync(sb);
                await _clients.Last().Writer.FlushAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Всё плохо: " + ex.Message);
            }
        }
    }

    protected internal async Task BroadcastPointsFieldMessageAsync()
    {
        foreach (var point in _pointsField)
        {
            var sb = new StringBuilder();
            sb.Append("SendPoint ");
            {
                try
                {
                    var message = JsonSerializer.Serialize(point);
                    sb.Append(message);
                    await _clients.Last().Writer.WriteLineAsync(sb);
                    await _clients.Last().Writer.FlushAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Всё плохо: " + ex.Message);
                }
            }
        }
    }

    protected internal async Task BroadcastPointMessageAsync(SendPoint point)
    {
        var sb = new StringBuilder();
        sb.Append("SendPoint ");
        {
            try
            {
                sb.Append(JsonSerializer.Serialize(point));
                foreach (var client in _clients)
                {
                    await client.Writer.WriteLineAsync(sb);
                    await client.Writer.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("всё Плохо: " + ex.Message);
            }
        }
    }

    protected internal void Disconnect()
    {
        foreach (var client in _clients)
            client.Close();
        _tcpListener.Stop();
    }

    protected internal async Task BroadcastMessageAsync(string message, string id)
    {
        var usersToJson = JsonSerializer.Serialize(_clients.Select(x => x.UserName).ToList());
        foreach (var client in _clients)
        {
            await client.Writer.WriteLineAsync(usersToJson);
            await client.Writer.FlushAsync();
        }
    }

    protected internal async Task AddPoint(SendPoint point)
    {
        _pointsField.Add(point);
        await BroadcastPointMessageAsync(point);
    }
}