using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;

namespace MiniHttpServer.Framework.Handlers
{
    public class ControllersHandler : RequestHandler
    {
        private readonly Assembly _controllersAssembly;

        public ControllersHandler(Assembly controllersAssembly)
        {
            _controllersAssembly = controllersAssembly;
        }

        public override async Task HandleAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            string path = request.Url?.LocalPath ?? "/";

            if (path.Contains("."))
            {
                await HandleNextAsync(context);
                return;
            }

            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 2)
            {
                await HandleNextAsync(context);
                return;
            }

            string rawName = segments[0];
            string controllerName = char.ToUpper(rawName[0]) + rawName.Substring(1) + "Controller";
            string methodName = segments[1];

            var controllerType = _controllersAssembly.GetTypes()
                .FirstOrDefault(t => t.Name.Equals(controllerName, StringComparison.OrdinalIgnoreCase) 
                                     && t.GetCustomAttribute<ControllerAttribute>() != null);

            if (controllerType == null)
            {
                await HandleNextAsync(context);
                return;
            }

            var method = controllerType.GetMethods().FirstOrDefault(m => 
            {
                if (!m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase)) return false;
                if (request.HttpMethod == "GET" && m.GetCustomAttribute<GetAttribute>() == null) return false;
                if (request.HttpMethod == "POST" && m.GetCustomAttribute<PostAttribute>() == null) return false;
                return true;
            });

            if (method == null)
            {
                await HandleNextAsync(context);
                return;
            }

            var instance = Activator.CreateInstance(controllerType);
            
            if (instance is BaseController baseController)
            {
                baseController.SetContext(context);
            }

            var parameters = method.GetParameters();
            object?[] args = new object?[parameters.Length];

            var query = request.QueryString;
            var formData = new Dictionary<string, string>();

            if (request.HttpMethod == "POST")
            {
                using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                string body = await reader.ReadToEndAsync();
                foreach (var pair in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    var kv = pair.Split('=', 2);
                    if (kv.Length == 2) formData[kv[0]] = Uri.UnescapeDataString(kv[1].Replace('+', ' '));
                }
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];
                string? val = null;

                if (request.HttpMethod == "GET") 
                    val = query[p.Name];
                else if (request.HttpMethod == "POST") 
                    val = formData.GetValueOrDefault(p.Name ?? "");

                if (val != null)
                {
                    try { args[i] = Convert.ChangeType(val, p.ParameterType); }
                    catch { args[i] = p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null; }
                }
                else
                {
                    args[i] = p.HasDefaultValue ? p.DefaultValue : (p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null);
                }
            }

            try
            {
                var result = method.Invoke(instance, args);

                if (result is Task<IHttpResult> taskResult)
                {
                    result = await taskResult;
                }

                if (result is IHttpResult httpResult)
                {
                    await httpResult.ExecuteAsync(context.Response);
                    context.Response.Close();
                }
                else
                {
                    await HandleNextAsync(context);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в контроллере: {ex.InnerException?.Message ?? ex.Message}");
                response.StatusCode = 500;
                response.Close();
            }
        }
    }
}