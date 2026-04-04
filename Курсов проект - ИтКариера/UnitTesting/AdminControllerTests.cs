using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Курсов_проект___ИтКариера.Controllers;
using Курсов_проект___ИтКариера.Data;

namespace UnitTesting
{
    public class AdminControllerTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

            _controller = new AdminController(_mockUserManager.Object);
        }

        [Fact]
        public async Task ChangeRole_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            _mockUserManager.Setup(x => x.FindByIdAsync("invalid-id")).ReturnsAsync((User)null);

            // Act
            var result = await _controller.ChangeRole("invalid-id", "Moderator");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}