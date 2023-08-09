using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMessageLogger.DataAccess;
using MvcMessageLogger.Models;
using System.Runtime.Intrinsics.X86;

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
            var user = _context.Users.Where(u => u.Id == id).Include(user => user.Messages).FirstOrDefault();

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

        public IActionResult Stats()
        {
            // Total Number of Users and Messages
            var usersWithMessages = _context.Users
                .Include(u => u.Messages)
                .ToList();
            ViewData["TotalUserCount"] = usersWithMessages.Count;
            ViewData["TotalMessageCount"] = usersWithMessages.Sum(u => u.Messages.Count);

            // The user with most messages
            var userWithMostMessages = usersWithMessages
                .OrderByDescending(u => u.Messages.Count)
                .FirstOrDefault();
            ViewData["UserWithMostMessages"] = userWithMostMessages;

            // Users ordered by amount of messages
            var usersOrderedByMessageCount = _context.Users
                .Include(u => u.Messages)
                .OrderByDescending(u => u.Messages.Count)
                .ToList();
            ViewData["UsersOrderedByMessageCount"] = usersOrderedByMessageCount;

            // Most common word used overall
            var allMessages = _context.Messages.Select(m => m.Content).ToList();
            string allMessagesString = string.Join(" ", allMessages);
            string[] stringArray = allMessagesString.Split(' ');
            var groups = stringArray.GroupBy(x => x.ToLower())
                .Select(x => new { x.Key, Count = x.Count() })
                .OrderByDescending(x => x.Count);
            int max = groups.First().Count;
            var mostCommons = groups.Where(x => x.Count == max);
            foreach (var group in mostCommons)
            {
                ViewData["MostCommonWord"] = group.Key;
                ViewData["MostCommonWordCount"] = group.Count;
            }

            //Most common word per user
            var userMessages = _context.Users.Include(u => u.Messages).ToList();

            foreach (var user in userMessages)
            {
                var allMessages1 = user.Messages.Select(m => m.Content).ToList();

                string allMessagesString1 = string.Join(" ", allMessages1);
                string[] words = allMessagesString1.Split(" ");
                var groups1 = words.GroupBy(x => x.ToLower())
                    .Select(x => new { Word = x.Key, Count = x.Count() })
                    .OrderByDescending(x => x.Count);
                var mostCommonWord = groups1.FirstOrDefault();

                ViewData["AllMessages"] = allMessages1;
                ViewData["mostCommonWord"] = mostCommonWord;
                ViewData["commonKey"] = mostCommonWord.Word;
            }

            ViewData["userMessages"] = userMessages;


            return View();
        }
    }
}
