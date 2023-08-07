using Microsoft.AspNetCore.Mvc;
using MvcMessageLogger.DataAccess;
using MvcMessageLogger.Models;

namespace MvcMessageLogger.Controllers
{
    public class UsersController : Controller
    {
        private readonly MvcMessageLoggerContext _context;

        public UsersController(MvcMessageLoggerContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var users = _context.Users.AsEnumerable();

            return View(users);
        }

        [Route("users/{id:int}")]
        public IActionResult Show(int id)
        {
            var user = _context.Users.Find(id);
            return View(user);
        }

        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();

            var userId = user.Id;

            return RedirectToAction("show", new {id = userId});
        }
    }
}
