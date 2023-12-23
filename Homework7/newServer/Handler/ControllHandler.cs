using System.Net;
using System.Reflection;
using System.Web;
using newServer.Configuration;
using newServer.Attributs;
using newServer.Handler;

namespace newServer.Handler;

public class ControllHander : IHandler
{
    private ServerConfiguration configuration;

    public ControllHander(ServerConfiguration _configuration)
    {
        configuration = _configuration;
    }

    public async void Handle(HttpListenerContext context)
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

              

                parameterValues.Add(configuration);
                parameterValues.Add(context);

                foreach (var parameterValue in parameterValues)
                {
                    Console.WriteLine(parameterValue);
                }
                method?.Invoke(Activator.CreateInstance(controller!), parameterValues.ToArray());
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
}