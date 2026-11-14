using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;
using MiniHttpServer.Services;

namespace MiniHttpServer.Controllers
{
    [Controller]
    public class AuthController : BaseController
    {
        private readonly IEmailService _emailService = new EmailService();

        [Post]
        public async Task<IHttpResult> SendEmail(string name, string message)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(message))
            {
                return Json(new
                {
                    success = false,
                    message = "Не удалось отправить сообщение: пустые поля"
                }, 400);
            }

            try
            {
                await _emailService.SendEmailAsync("student@example.com", name, message);
                
                return Json(new
                {
                    success = true,
                    message = "Сообщение отправлено"
                });
            }
            catch (System.Exception)
            {
                return Json(new
                {
                    success = false,
                    message = "Не удалось отправить сообщение"
                }, 500);
            }
        }
    }
}