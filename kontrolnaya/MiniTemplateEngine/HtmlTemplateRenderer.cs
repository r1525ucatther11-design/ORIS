using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MiniTemplateEngine
{
    /// <summary>
    /// Класс для рендеринга HTML-шаблонов.
    /// </summary>
    public class HtmlTemplateRenderer : IHtmlTemplateRenderer
    {
        /// <summary>
        /// Преобразует строку шаблона в готовый HTML.
        /// </summary>
        public string RenderFromString(string htmlTemplate, object dataModel)
        {
            // Сохраняем исходный шаблон в переменную.
            string result = htmlTemplate;

            // 1. Обрабатываем циклы $foreach
            result = ProcessForeach(result, dataModel);

            // 2. Обрабатываем условия $if/$else
            result = ProcessIfElse(result, dataModel);

            // 3. Заменяем переменные ${...}
            result = ProcessVariables(result, dataModel);

            return result;
        }

        private string ProcessForeach(string template, object dataModel)
        {
            string pattern = @"\$foreach\s*\(\s*(\w+)\s+in\s+([\w\.]+)\s*\)([\s\S]*?)\$endfor";

            return Regex.Replace(template, pattern, (match) =>
            {
                string itemName = match.Groups[1].Value; // user
                string listName = match.Groups[2].Value; // Users
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
            // Ищем $if (условие) ... $else ... $endif
            string pattern = @"\$if\s*\(([^)]+)\)([\s\S]*?)(?:\$else([\s\S]*?))?\$endif";

            return Regex.Replace(template, pattern, (match) =>
            {
                string condition = match.Groups[1].Value.Trim(); // например, "gender"
                string trueBlock = match.Groups[2].Value;
                string falseBlock = match.Groups[3].Success ? match.Groups[3].Value : "";

                bool isTrue = CheckCondition(condition, dataModel);
                return isTrue ? trueBlock : falseBlock;
            });
        }

        private bool CheckCondition(string condition, object dataModel)
        {
            // Убираем скобки и пробелы
            condition = condition.Replace("(", "").Replace(")", "").Trim();

            // Если есть знак равно (например, gender == true)
            if (condition.Contains("=="))
            {
                string[] parts = condition.Split("==");
                string propName = parts[0].Trim();
                string targetValue = parts[1].Trim();

                object value = GetValue(dataModel, propName);
                if (value == null) return false;

                if (value is bool b)
                    return (targetValue.ToLower() == "true" && b) || (targetValue.ToLower() == "false" && !b);
                
                return value.ToString() == targetValue;
            }

            // Если просто имя свойства (например, просто "gender")
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

        // Вспомогательный метод для поиска свойств
        private object GetValue(object obj, string path)
        {
            if (obj == null) return null;
            string[] parts = path.Split('.');
            object current = obj;

            foreach (string part in parts)
            {
                if (current == null) return null;
                
                // ДОБАВЛЕНО BindingFlags.NonPublic, чтобы видеть свойства анонимных типов из тестов!
                PropertyInfo prop = current.GetType().GetProperty(part, 
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                
                if (prop == null) return null;
                current = prop.GetValue(current, null);
            }
            return current;
        }

        /// <summary>
        /// Читает шаблон из файла и рендерит его.
        /// </summary>
        public string RenderFromFile(string filePath, object dataModel)
        {
            if (!File.Exists(filePath)) return "Файл не найден";
            return RenderFromString(File.ReadAllText(filePath), dataModel);
        }

        /// <summary>
        /// Сохраняет результат рендеринга в файл.
        /// </summary>
        public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
        {
            string result = RenderFromFile(inputFilePath, dataModel);
            File.WriteAllText(outputFilePath, result);
            return result;
        }
    }
}