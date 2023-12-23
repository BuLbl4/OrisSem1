namespace newServer.Attributs;
[AttributeUsage(AttributeTargets.Class)]
public class ControllerAttribute: Attribute
{
    public string ControllerName { get; set; }
    public ControllerAttribute(string controllName) => ControllerName = controllName;
}