using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Web;
using newServer.Configuration;
using newServer.Attributs;
using newServer.Handler;
using newServer.Model;

namespace newServer.Handler;

public class ControllHander : IHandler
{
    private ServerConfiguration configuration;
    
    public ControllHander(ServerConfiguration _configuration)
    {
        configuration = _configuration;
    }

    public async void Handler(HttpListenerContext context)
    {
        try
        {
            var strParams = context.Request.Url!
                .Segments
                .Skip(1)
                .Select(s => s.Replace("/", ""))
                .ToArray();
            
            if (strParams.Length < 2)
                throw new ArgumentException("the number of lines in the query string is less than two!");
            if (strParams.Length >= 2)
            {
                string input = await new StreamReader(context.Request.InputStream).ReadToEndAsync();
                
                var queryParams = HttpUtility.ParseQueryString(input);
                List<object> parameterValues = new List<object>();
                foreach (var key in queryParams.AllKeys)
                    parameterValues.Add(queryParams[key]!);
                    
                
                string controllerName = strParams[0];
                string methodName = strParams[1];
                
                if (strParams.Length > 2 && String.IsNullOrEmpty(input))
                {
                    for (int i = 2; i < strParams.Length; i++)
                    {
                        parameterValues.Add(strParams[i]);
                    }
                }
                var assembly = Assembly.GetExecutingAssembly();

                var controller = assembly.GetTypes()
                    .Where(t => Attribute.IsDefined(t, typeof(ControllerAttribute)))
                    .FirstOrDefault(c =>
                        ((ControllerAttribute)Attribute.GetCustomAttribute(c, typeof(ControllerAttribute))!)
                        .ControllerName.Equals(controllerName, StringComparison.OrdinalIgnoreCase));

                var method = controller?.GetMethods()
                    .Where(x => x.GetCustomAttributes(true)
                        .Any(attr => attr.GetType().Name.Equals($"{context.Request.HttpMethod}Attribute",
                            StringComparison.OrdinalIgnoreCase)))
                    .FirstOrDefault(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase));


                if (strParams[1] == "SendToEmail")
                    parameterValues.Add(configuration);
                
                parameterValues.Add(context);



                foreach (var parameterValue in parameterValues)
                {
                    Console.WriteLine(parameterValue);
                }
                var result = method?.Invoke(Activator.CreateInstance(controller!), parameterValues.ToArray());
                if (result != null)
                {
                    if (result is Account || result is Account[])
                    {
                        string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
                        SendHtml(context, json);
                    }
                }
                context.Response.Close();
            }
            else
            {
                Console.WriteLine("Another handler");
                throw new ArgumentException("Failed to process request!");
            }


        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Controller handler: " + ex.Message);
            Console.ResetColor();
        }
        
    }
    private async void SendHtml(HttpListenerContext context, string html)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(html);
        context.Response.ContentType = "application/json";
        context.Response.ContentLength64 = buffer.Length;

        using Stream output = context.Response.OutputStream;
        await output.WriteAsync(buffer);
        await output.FlushAsync();
        context.Response.Close();
    }
}