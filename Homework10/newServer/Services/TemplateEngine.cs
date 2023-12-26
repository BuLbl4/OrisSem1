using System.Collections;
using System.Text.RegularExpressions;

namespace newServer.Services;
public class TemplateEngine
{
    public string Render(string template, object data)
    {
        var renderedTemplate = template;

        var properties = data.GetType().GetProperties();
        foreach (var property in properties)
        {
            
            var regex = new Regex($"@{property.Name}");
            renderedTemplate = regex.Replace(renderedTemplate, property.GetValue(data)!.ToString()!);
        }

        renderedTemplate = ProcessIfConditions(renderedTemplate, data);

        renderedTemplate = ProcessForLoops(renderedTemplate, data);

        return renderedTemplate;
    }

    private string ProcessIfConditions(string template, object data)
    {
        var regex = new Regex(@"@if\((.*?)\)(.*?)@endif", RegexOptions.Singleline);
        var matches = regex.Matches(template);

        foreach (Match match in matches)
        {
            var condition = match.Groups[1].Value;
            var content = match.Groups[2].Value;

            if (EvaluateCondition(condition, data))
            {
                template = template.Replace(match.Value, content);
            }
            else
            {
                template = template.Replace(match.Value, "");
            }
        }

        return template;
    }

    private bool EvaluateCondition(string condition, object data)
    {
        var parts = condition.Split(new[] { "==", "!=" }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
            var propertyName = parts[0].Trim();
            var propertyValue = parts[1].Trim();

            var property = data.GetType().GetProperty(propertyName);
            if (property != null)
            {
                var value = property.GetValue(data)?.ToString();
                return value == propertyValue;
            }
        }

        return false;
    }

    private string ProcessForLoops(string template, object data)
    {
        var regex = new Regex(@"@for\((.*?)\)(.*?)@endfor", RegexOptions.Singleline);
        var matches = regex.Matches(template);

        foreach (Match match in matches)
        {
            var loopData = match.Groups[1].Value;
            var content = match.Groups[2].Value;

            var property = data.GetType().GetProperty(loopData);
            if (property != null)
            {
                var value = property.GetValue(data);

                if (value is IEnumerable enumerable)
                {
                    var loopContent = "";
                    foreach (var item in enumerable)
                    {
                        loopContent += content.Replace("@item", item.ToString());
                    }
                    template = template.Replace(match.Value, loopContent);
                }
            }
        }
        return template;
    }
}