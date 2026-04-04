using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Курсов_проект___ИтКариера.Controllers;
using Курсов_проект___ИтКариера.Data;

namespace UnitTesting
{
    public class HomeControllerTests
    {
        private ApplicationDbContext GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new ApplicationDbContext(options);
            databaseContext.Database.EnsureCreated();
            return databaseContext;
        }

        [Fact]
        public async Task AddReview_RedirectsToDetails_WhenCommentIsInvalid()
        {
            // Arrange
            var context = GetDatabaseContext();
            var controller = new HomeController(null, context); // Passing null for logger for brevity
            var bookId = Guid.NewGuid();

            // Act: Sending an empty string
            var result = await controller.AddReview(bookId, 5.0, "");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal(bookId, redirectResult.RouteValues["id"]);

            // Ensure nothing was actually saved to the DB
            Assert.Equal(0, await context.Reviews.CountAsync());
        }
    }
}