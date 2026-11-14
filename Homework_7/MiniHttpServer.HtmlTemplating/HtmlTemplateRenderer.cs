using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniHttpServer.HtmlTemplating
{
    public class HtmlTemplateRenderer : IHtmlTemplateRenderer
    {
        public string RenderFromString(string htmlTemplate, object dataModel)
        {
            htmlTemplate = ProcessForeach(htmlTemplate, dataModel);
            htmlTemplate = ProcessIf(htmlTemplate, dataModel);
            htmlTemplate = ProcessVariables(htmlTemplate, dataModel);
            return htmlTemplate;
        }

        public string RenderFromFile(string filePath, object dataModel)
        {
            string template = File.ReadAllText(filePath);
            return RenderFromString(template, dataModel);
        }

        public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
        {
            string result = RenderFromFile(inputFilePath, dataModel);
            File.WriteAllText(outputFilePath, result);
            return result;
        }

        private string ProcessVariables(string html, object model)
        {
            return Regex.Replace(html, @"\$\{([^}]+)\}", match =>
            {
                string path = match.Groups[1].Value;
                object? current = model;
                
                foreach (var part in path.Split('.'))
                {
                    if (current == null) return "";
                    var prop = current.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (prop == null) return "";
                    current = prop.GetValue(current);
                }
                return System.Net.WebUtility.HtmlEncode(current?.ToString() ?? "");
            });
        }

        private string ProcessIf(string html, object model)
        {
            string pattern = @"\$if\s*\(\s*([^)]+)\s*\)(.*?)(?:\$else(.*?))?\$endif";
            return Regex.Replace(html, pattern, match =>
            {
                string conditionProp = match.Groups[1].Value.Trim();
                string trueBlock = match.Groups[2].Value;
                string falseBlock = match.Groups[3].Value;

                bool condition = GetBoolValue(model, conditionProp);
                return condition ? trueBlock : falseBlock;
            }, RegexOptions.Singleline);
        }

        private string ProcessForeach(string html, object model)
        {
            string pattern = @"\$foreach\s*\(\s*var\s+([a-zA-Z0-9_]+)\s+in\s+([a-zA-Z0-9_.]+)\s*\)(.*?)\$endfor";
            return Regex.Replace(html, pattern, match =>
            {
                string itemName = match.Groups[1].Value.Trim();
                string collectionName = match.Groups[2].Value.Trim();
                string content = match.Groups[3].Value;

                object? collection = GetPropertyValue(model, collectionName);
                if (collection is System.Collections.IEnumerable enumerable)
                {
                    StringBuilder sb = new();
                    foreach (var item in enumerable)
                    {
                        string itemHtml = content;
                        itemHtml = Regex.Replace(itemHtml, @"\$\{([^}]+)\}", m =>
                        {
                            string path = m.Groups[1].Value;
                            if (path.StartsWith(itemName + "."))
                                path = path.Substring(itemName.Length + 1);
                            
                            object? val = GetPropertyValue(item, path);
                            return System.Net.WebUtility.HtmlEncode(val?.ToString() ?? "");
                        });
                        sb.Append(itemHtml);
                    }
                    return sb.ToString();
                }
                return "";
            }, RegexOptions.Singleline);
        }

        private object? GetPropertyValue(object obj, string propertyPath)
        {
            object? current = obj;
            foreach (var part in propertyPath.Split('.'))
            {
                if (current == null) return null;
                var prop = current.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop == null) return null;
                current = prop.GetValue(current);
            }
            return current;
        }

        private bool GetBoolValue(object obj, string property)
        {
            var val = GetPropertyValue(obj, property);
            if (val is bool b) return b;
            if (val is string s) return s.Equals("true", StringComparison.OrdinalIgnoreCase);
            return false;
        }
    }
}