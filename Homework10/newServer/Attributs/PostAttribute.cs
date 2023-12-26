namespace newServer.Attributs;

public class PostAttribute : Attribute, IHttpMethodAttribute
{
    public PostAttribute(string actionName)
    {
        ActionName = actionName;
    }

    public string ActionName { get; }
}