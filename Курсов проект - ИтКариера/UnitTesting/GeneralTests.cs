using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using Курсов_проект___ИтКариера.Controllers;
using Курсов_проект___ИтКариера.Data;
using Курсов_проект___ИтКариера.Models;

namespace UnitTesting
{
    public class GeneralTests
    {
        private DbContextOptions<ApplicationDbContext> _options;

        public GeneralTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private ApplicationDbContext GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new ApplicationDbContext(options);
            databaseContext.Database.EnsureCreated();
            return databaseContext;
        }

        private void MockUser(Controller controller, string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "test@user.com")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task Catalogue_FiltersByMultipleCriteria_ReturnsCorrectBooks()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "C# Guide",
                AuthorName = "John Doe",
                Description = "Essential reading for C# developers",
                PublishedDate = new DateTime(2022, 1, 1)
            };

            context.Books.Add(book);
            await context.SaveChangesAsync();

            var controller = new HomeController(null, context);

            // Act
            var result = await controller.Catalogue("John", 2022, null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BookFilterViewModel>(viewResult.Model);
            Assert.Single(model.Books);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsMissing()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var controller = new HomeController(null, context);

            // Act
            var result = await controller.Details(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AddReview_RedirectsToDetails_OnSuccess()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var bookId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var controller = new HomeController(null, context);
            MockUser(controller, userId);

            // Act
            var result = await controller.AddReview(bookId, 5.0, "Great book!");

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
            Assert.Equal(1, context.Reviews.Count());
        }

        [Fact]
        public async Task Moderator_AddNewBook_SavesToDatabase_WhenValid()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var controller = new ModeratorController(context);

            controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()
            );

            var model = new BookViewModel
            {
                Book = new Book
                {
                    Id = Guid.NewGuid(),
                    Title = "New Test Book",
                    Description = "Test Desc"
                },
                SelectedCategoryIds = new List<Guid>()
            };

            // Act
            var result = await controller.AddNewBook(model);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(1, context.Books.Count());
        }


        [Fact]
        public async Task Catalogue_FiltersByYear_ReturnsOnlyMatchingBooks()
        {
            // Arrange
            var context = GetDatabaseContext();
            var book1 = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Old Book",
                Description = "A classic tale.",
                AuthorName = "Ancient Author",
                PublishedDate = new DateTime(2010, 1, 1)
            };
            var book2 = new Book
            {
                Id = Guid.NewGuid(),
                Title = "New Book",
                Description = "A modern story.",
                AuthorName = "Modern Author",
                PublishedDate = new DateTime(2024, 1, 1)
            };

            context.Books.AddRange(book1, book2);
            await context.SaveChangesAsync();

            var controller = new HomeController(null, context);

            // Act
            var result = await controller.Catalogue(null, 2024, null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BookFilterViewModel>(viewResult.Model);
            Assert.Single(model.Books);
            Assert.Equal("New Book", model.Books.First().Title);
        }

        [Fact]
        public async Task Delete_RemovesBookFromDatabase_WhenBookExists()
        {
            // Arrange
            var context = GetDatabaseContext();
            var bookId = Guid.NewGuid();
            var book = new Book
            {
                Id = bookId,
                Title = "Disposable Book",
                Description = "Will be deleted.",
                AuthorName = "Temporary Author"
            };

            context.Books.Add(book);
            await context.SaveChangesAsync();

            var controller = new HomeController(null, context);

            controller.TempData = new Mock<ITempDataDictionary>().Object;

            // Act
            var result = await controller.Delete(bookId);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Catalogue", redirect.ActionName);

            var exists = await context.Books.AnyAsync(b => b.Id == bookId);
            Assert.False(exists);
        }

        [Fact]
        public async Task ToggleFavorite_AddsBookToFavorites_WhenNotPresent()
        {
            // Arrange
            var context = GetDatabaseContext();
            var bookId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
            }, "mock"));

            var controller = new HomeController(null, context);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            controller.TempData = new Mock<ITempDataDictionary>().Object;

            // Act
            await controller.ToggleFavorite(bookId);

            // Assert
            var favorite = await context.Favorites
                .FirstOrDefaultAsync(f => f.BookId == bookId && f.UserId == Guid.Parse(userId));

            Assert.NotNull(favorite);
            Assert.Equal(bookId, favorite.BookId);
        }
    }
}
