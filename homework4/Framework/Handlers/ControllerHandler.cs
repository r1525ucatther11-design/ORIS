using System.Net;
using System.Reflection;
using System.Text;
using CustomHttpServer.Framework.Attributes;
using CustomHttpServer.Http;
using CustomHttpServer.Http.HttpResponses;

namespace CustomHttpServer.Framework.Handlers
{
    internal class ControllerHandler : Handler
    {
        public override async Task HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            string path = request.Url.LocalPath;

            // Если в URL есть точка — это файл, не контроллер
            bool isFile = path.Contains(".");
            if (isFile)
            {
                if (Successor != null)
                {
                    await Successor.HandleRequest(context);
                }
                return;
            }

            // Правильный разбор URL — через Split
            // /Dashboard/profile/1 → ["Dashboard", "profile", "1"]
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 2)
            {
                response.StatusCode = 404;
                byte[] buf = Encoding.UTF8.GetBytes("404 — Неверный путь");
                response.ContentLength64 = buf.Length;
                await response.OutputStream.WriteAsync(buf);
                response.OutputStream.Close();
                return;
            }

            string controllerName = segments[0];
            string methodRoute = segments[1];
            string[] methodParams = segments.Skip(2).ToArray();

            var assembly = Assembly.GetExecutingAssembly();
            var controller = assembly.GetTypes()
                .Where(t => Attribute.IsDefined(t, typeof(HttpControllerAttribute)))
                .FirstOrDefault(c =>
                {
                    var attr = (HttpControllerAttribute)Attribute
                        .GetCustomAttribute(c, typeof(HttpControllerAttribute))!;
                    return attr.Name.Equals(controllerName, StringComparison.OrdinalIgnoreCase);
                });

            if (controller == null)
            {
                response.StatusCode = 404;
                byte[] buf = Encoding.UTF8.GetBytes($"404 — Контроллер '{controllerName}' не найден");
                response.ContentLength64 = buf.Length;
                await response.OutputStream.WriteAsync(buf);
                response.OutputStream.Close();
                return;
            }

            // Поддержка /user/ в URL
            if (methodParams.Length > 0 && methodParams[0].Equals("user", StringComparison.OrdinalIgnoreCase))
                methodParams = methodParams.Skip(1).ToArray();

            string httpMethod = request.HttpMethod;

            var method = controller.GetMethods()
                .FirstOrDefault(m => m.GetCustomAttributes(true).Any(attr =>
                {
                    var attrType = attr.GetType();
                    if (!attrType.Name.Equals($"Http{httpMethod}Attribute", StringComparison.OrdinalIgnoreCase) &&
                        !attrType.Name.Equals($"{httpMethod}Attribute", StringComparison.OrdinalIgnoreCase))
                        return false;

                    var routeProp = attrType.GetProperty("Route");
                    return routeProp?.GetValue(attr)?.ToString()
                        ?.Equals(methodRoute, StringComparison.OrdinalIgnoreCase) ?? false;
                }));

            if (method == null)
            {
                response.StatusCode = 404;
                byte[] buf = Encoding.UTF8.GetBytes($"404 — Метод '{methodRoute}' не найден");
                response.ContentLength64 = buf.Length;
                await response.OutputStream.WriteAsync(buf);
                response.OutputStream.Close();
                return;
            }

            var instance = Activator.CreateInstance(controller);
            if (instance is ControllerBase controllerBase)
            {
                controllerBase.SetContext(context);
            }

            var parameters = method.GetParameters();
            object?[] args = new object?[parameters.Length];

            // Тело POST
            string? formBody = null;
            if (httpMethod == "POST")
            {
                using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                formBody = await reader.ReadToEndAsync();
            }

            var formData = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(formBody))
            {
                foreach (var pair in formBody.Split('&'))
                {
                    var parts = pair.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        formData[parts[0]] = Uri.UnescapeDataString(parts[1].Replace('+', ' '));
                    }
                }
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];

                if (p.GetCustomAttribute<FromFormAttribute>() != null)
                {
                    args[i] = formData.TryGetValue(p.Name ?? "", out var v)
                        ? Convert.ChangeType(v, p.ParameterType)
                        : (p.HasDefaultValue ? p.DefaultValue : null);
                }
                else if (p.GetCustomAttribute<FromQueryAttribute>() != null)
                {
                    string qv = request.QueryString[p.Name] ?? "";
                    args[i] = string.IsNullOrEmpty(qv)
                        ? (p.HasDefaultValue ? p.DefaultValue : null)
                        : Convert.ChangeType(qv, p.ParameterType);
                }
                else if (httpMethod == "POST")
                {
                    args[i] = formData.TryGetValue(p.Name ?? "", out var v)
                        ? Convert.ChangeType(v, p.ParameterType)
                        : (p.HasDefaultValue ? p.DefaultValue : null);
                }
                else
                {
                    if (i < methodParams.Length)
                        args[i] = Convert.ChangeType(methodParams[i], p.ParameterType);
                    else
                        args[i] = p.ParameterType.IsValueType
                            ? Activator.CreateInstance(p.ParameterType)
                            : null;
                }
            }

            var result = method.Invoke(instance, args);

            if (result is Task task)
                await task;

            if (result is IHttpResponseTypeResult httpResult)
            {
                byte[] buffer = httpResult.Execute();
                response.ContentType = GetContentType(result);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer);
                response.OutputStream.Close();
            }
            else if (result is string str)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(str);
                response.ContentType = "text/html; charset=utf-8";
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer);
                response.OutputStream.Close();
            }
        }

        private string GetContentType(object result) => result switch
        {
            JsonResult => "application/json; charset=utf-8",
            HtmlResult => "text/html; charset=utf-8",
            _ => "text/plain; charset=utf-8"
        };
    }
}