using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using MvcMessageLogger.DataAccess;
using MvcMessageLogger.Models;

namespace MvcMessageLogger.FeatureTests
{
    public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public UsersControllerTests(WebApplicationFactory<Program> factory)
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
        public async Task Index_ReturnsView()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();

            context.Users.Add(new User { Name = "Jacob", Username = "jCob" });
            context.SaveChanges();

            var response = await client.GetAsync("/users");
            var html = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            Assert.Contains("Jacob", html);
            Assert.Contains("jCob", html);
        }

        [Fact]
        public async Task New_ReturnsForm()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/users/new");
            var html = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            Assert.Contains("Create a New User Profile", html);
            Assert.Contains("Name:", html);
            Assert.Contains("Username:", html);
        }

        [Fact]
        public async Task PostIndex_RedirectsToShow()
        {
            var client = _factory.CreateClient();

            var addUserForm = new Dictionary<string, string>
            {
                {"Name", "James" },
                {"Username", "jcepriano" }
            };

            var response = await client.PostAsync("/users", new FormUrlEncodedContent(addUserForm));
            var html = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            Assert.Contains("User Details", html);
            Assert.Contains("Name: James", html);
            Assert.Contains("Username: jcepriano", html);
        }

        [Fact]
        public async Task Stats_ReturnsStatistics()
        {
            var client = _factory.CreateClient();
            var context = GetDbContext();

            // User 1
            User user = new User { Name = "James", Username = "jcepriano" };
            Message message1 = new Message("Test Message");
            Message message2 = new Message("Test");
            Message message3 = new Message("Hello");
            Message message4 = new Message("World");
            user.Messages.Add(message1);
            user.Messages.Add(message2);
            user.Messages.Add(message3);
            user.Messages.Add(message4);
            context.Users.Add(user);

            // User 2
            User user2 = new User { Name = "Bella", Username = "bcepriano" };
            Message message5 = new Message("Test Message");
            Message message6 = new Message("Hello");
            user2.Messages.Add(message5);
            user2.Messages.Add(message6);
            context.Users.Add(user2);

            // User 3
            User user3 = new User { Name = "Ariel", Username = "acepriano" };
            context.Users.Add(user3);

            context.SaveChanges();


            var response = await client.GetAsync("/users/stats?");
            var html = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            Assert.Contains("Statistics", html);
            Assert.Contains("User with Most Messages", html);
            Assert.Contains("Users Ordered By Amount of Messages", html);
            Assert.Contains("Most common word used overall", html);
            Assert.Contains("Most common word per user", html);
            Assert.Contains("Total Number of Users: 3", html);
            Assert.Contains("Total Number of Messages: 6", html);
            Assert.Contains("James | Messages: 4", html);
            Assert.Contains("Word: test", html);
            Assert.Contains("Count: 3", html);
            Assert.Contains("James | Most Common Word: test", html);
            Assert.Contains("Ariel | No messages found.", html);
        }
    }
}
