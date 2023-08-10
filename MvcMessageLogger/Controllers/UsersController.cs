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

        [Route("users/{id:int}/edit")]
        public IActionResult Edit(int id)
        {
            var user = _context.Users.Find(id);
            return View(user);
        }

        [HttpPost]
        [Route("users/{id:int}")]
        public IActionResult Update(int id, User user)
        {
            user.Id = id;
            _context.Users.Update(user);
            _context.SaveChanges();

            return RedirectToAction("show", new { id = user.Id });
        }

        [HttpPost]
        public IActionResult Delete(int userId)
        {
            var user = _context.Users
                .Where(u => u.Id == userId)
                .Include(u => u.Messages)
                .First();

            _context.Users.Remove(user);
            _context.SaveChanges();

            return RedirectToAction("index");

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
                .Select(x => new { Word = x.Key, Count = x.Count() })
                .OrderByDescending(x => x.Count);
            var mostCommonWordOverall = groups.FirstOrDefault();

            ViewData["MostCommonWordOverall"] = mostCommonWordOverall?.Word;
            ViewData["MostCommonWordCountOverall"] = mostCommonWordOverall?.Count;

            // Most common word per user
            var userMessages1 = _context.Users.Include(u => u.Messages).ToList();
            var userMostCommonWords = new Dictionary<User, string>();

            foreach (var user in userMessages1)
            {
                var allMessages1 = user.Messages.Select(m => m.Content).ToList();
                string allMessagesString1 = string.Join(" ", allMessages1);
                string[] words = allMessagesString1.Split(" ");
                var groups1 = words.GroupBy(x => x.ToLower())
                    .Select(x => new { Word = x.Key, Count = x.Count() })
                    .OrderByDescending(x => x.Count);
                var mostCommonWord = groups1.FirstOrDefault()?.Word; // If FirstOrDefualt returns null, 'Word' property won't output a null reference exception

                userMostCommonWords[user] = mostCommonWord ?? ""; // If mostCommonWord is not null, it uses the value set in mostCommonWord. If is null, then it assigns an empty string to dictionary value
            }

            ViewData["MostCommonWordPerUser"] = userMostCommonWords;
            ViewData["UserMessages"] = userMessages1;

            return View();
        }
    }
}
