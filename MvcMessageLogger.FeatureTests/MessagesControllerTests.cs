using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using MvcMessageLogger.DataAccess;
using MvcMessageLogger.Models;

namespace MvcMessageLogger.FeatureTests
{
    public class MessagesControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public MessagesControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        private MvcMessageLoggerContext GetDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<MvcMessageLoggerContext>();
            optionsBuilder.UseInMemoryDatabase("TestDatabase");

            var context = new MvcMessageLoggerContext(optionsBuilder.Options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            return context;
        }

        [Fact]
        public async Task Index_ShowsMessagesForUser()
        {
            var context = GetDbContext();
            var client = _factory.CreateClient();

            User user = new User { Name = "James", Username = "jcepriano" };
            Message message1 = new Message("Test Message");
            user.Messages.Add(message1);
            context.Users.Add(user);
            context.SaveChanges();

            var response = await client.GetAsync($"/users/{user.Id}/messages");
            var html = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            Assert.Contains("James Messages", html);
            Assert.Contains("Test Message", html);
        }

        [Fact]
        public async Task New_ReturnsFormToCreateMessage()
        {
             var context = GetDbContext();
             var client = _factory.CreateClient();

             User user = new User { Name = "James", Username = "jcepriano" };
             context.Users.Add(user);
             context.SaveChanges();

             var response = await client.GetAsync($"/users/{user.Id}/messages/new");
             var html = await response.Content.ReadAsStringAsync();
             response.EnsureSuccessStatusCode();

            Assert.Contains("Add a Message", html);
            Assert.Contains("Submit Message", html);
            Assert.Contains($"<form method=\"post\" action=\"/users/{user.Id}/messages\">", html);
        }

        [Fact]
        public async Task Create_AddsMessage_RedirectsToIndex()
        {
            var context = GetDbContext();
            var client = _factory.CreateClient();

            User user = new User { Name = "James", Username = "jcepriano" };
            context.Users.Add(user);
            context.SaveChanges();

            var addMessageForm = new Dictionary<string, string>
            {
                {"Content", "New Message" },
            };

            var response = await client.PostAsync($"/users/{user.Id}/messages", new FormUrlEncodedContent(addMessageForm));
            var html = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            Assert.Contains($"/users/{user.Id}/messages", response.RequestMessage.RequestUri.ToString());
            Assert.Contains("New Message", html);
        }
    }
}
