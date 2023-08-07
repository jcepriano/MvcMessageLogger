﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMessageLogger.DataAccess;
using MvcMessageLogger.Models;

namespace MvcMessageLogger.Controllers
{
    public class MessagesController : Controller
    {
        private readonly MvcMessageLoggerContext _context;

        public MessagesController(MvcMessageLoggerContext context)
        {
            _context = context;
        }

        [Route("users/{id:int}/messages")]
        public IActionResult Index(int id)
        {
            var userMessages = _context.Users
                .Where(u => u.Id == id)
                .Include(u => u.Messages)
                .FirstOrDefault();

            return View(userMessages);
        }

        [Route("users/{id:int}/messages/new")]
        public IActionResult New(int id)
        {
            var userMessages = _context.Users
                .Where(u => u.Id == id)
                .Include(u => u.Messages)
                .FirstOrDefault();

            return View(userMessages);
        }

        [HttpPost]
        [Route("users/{id:int}/messages")]
        public IActionResult Create(Message message, int id)
        {
            var userMessages = _context.Users
                .Where(u => u.Id == id)
                .Include(u => u.Messages)
                .FirstOrDefault();

            userMessages.Messages.Add(message);
            _context.SaveChanges();
            return RedirectToAction("Index", new { id = userMessages.Id });

        }
    }
}