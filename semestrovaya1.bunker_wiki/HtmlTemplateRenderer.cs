using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

public class HtmlTemplateRenderer
{
    public string RenderFromString(string htmlTemplate, object dataModel)
    {
        string result = htmlTemplate;
        result = ProcessForeach(result, dataModel);
        result = ProcessIfElse(result, dataModel);
        result = ProcessVariables(result, dataModel);
        return result;
    }

    private string ProcessForeach(string template, object dataModel)
    {
        string pattern = @"\$foreach\s*\(\s*(\w+)\s+in\s+([\w\.]+)\s*\)([\s\S]*?)\$endfor";
        return Regex.Replace(template, pattern, (match) =>
        {
            string itemName = match.Groups[1].Value;
            string listName = match.Groups[2].Value;
            string innerHtml = match.Groups[3].Value;

            object listObj = GetValue(dataModel, listName);
            if (listObj is IEnumerable list)
            {
                string resultHtml = "";
                foreach (var item in list)
                {
                    string tempHtml = ProcessIfElse(innerHtml, item);
                    string prefix = itemName + ".";
                    
                    tempHtml = Regex.Replace(tempHtml, @"\$\{" + prefix + @"([^\}]+)\}", (m) =>
                    {
                        string prop = m.Groups[1].Value;
                        object val = GetValue(item, prop);
                        return val != null ? val.ToString() : m.Value;
                    });
                    resultHtml += tempHtml;
                }
                return resultHtml;
            }
            return match.Value;
        });
    }

    private string ProcessIfElse(string template, object dataModel)
    {
        string pattern = @"\$if\s*\(([^)]+)\)([\s\S]*?)(?:\$else([\s\S]*?))?\$endif";
        return Regex.Replace(template, pattern, (match) =>
        {
            string condition = match.Groups[1].Value.Trim();
            string trueBlock = match.Groups[2].Value;
            string falseBlock = match.Groups[3].Success ? match.Groups[3].Value : "";
            bool isTrue = CheckCondition(condition, dataModel);
            return isTrue ? trueBlock : falseBlock;
        });
    }

    private bool CheckCondition(string condition, object dataModel)
    {
        condition = condition.Replace("(", "").Replace(")", "").Trim();
        if (condition.Contains("=="))
        {
            string[] parts = condition.Split("==");
            string propName = parts[0].Trim();
            string targetValue = parts[1].Trim();
            object value = GetValue(dataModel, propName);
            if (value == null) return false;
            if (value is bool b) return (targetValue.ToLower() == "true" && b) || (targetValue.ToLower() == "false" && !b);
            return value.ToString() == targetValue;
        }
        object directValue = GetValue(dataModel, condition);
        if (directValue is bool boolVal) return boolVal;
        return false;
    }

    private string ProcessVariables(string template, object dataModel)
    {
        string pattern = @"\$\{([^\}]+)\}";
        return Regex.Replace(template, pattern, (match) =>
        {
            string path = match.Groups[1].Value.Trim();
            object value = GetValue(dataModel, path);
            return value != null ? value.ToString() : match.Value;
        });
    }

    private object GetValue(object obj, string path)
    {
        if (obj == null) return null;
        string[] parts = path.Split('.');
        object current = obj;
        foreach (string part in parts)
        {
            if (current == null) return null;
            PropertyInfo prop = current.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null) return null;
            current = prop.GetValue(current, null);
        }
        return current;
    }

    public string RenderFromFile(string filePath, object dataModel)
    {
        if (!File.Exists(filePath)) return "Файл не найден: " + filePath;
        return RenderFromString(File.ReadAllText(filePath), dataModel);
    }
}