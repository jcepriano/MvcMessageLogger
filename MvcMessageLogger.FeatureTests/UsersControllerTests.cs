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
    }
}
