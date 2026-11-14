using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;
using System.Collections.Generic;

namespace MiniHttpServer.Controllers
{
    [Controller]
    public class UserController : BaseController
    {
        [Get]
        public IHttpResult Profile()
        {
            var user = new
            {
                Name = "Alex",
                Age = 20,
                Email = "alex@example.com"
            };

            return Page("User/Profile.html", user);
        }

        [Get]
        public IHttpResult List()
        {
            var users = new List<object>
            {
                new { Name = "Alex", IsActive = true },
                new { Name = "Maria", IsActive = false },
                new { Name = "Ivan", IsActive = true }
            };

            return Page("User/List.html", new { Users = users });
        }
    }
}