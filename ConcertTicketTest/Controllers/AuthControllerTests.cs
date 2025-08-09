using ConcertTicketSystem.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ConcertTicketTest.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<ILogger<AuthController>> _loggerMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);

            _configMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<AuthController>>();

            // Setup Configuration
            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(x => x["Secret"]).Returns("YourSuperSecretKeyHereYourSuperSecretKeyHere1234512345");
            configSection.Setup(x => x["Issuer"]).Returns("ConcertAPI");
            configSection.Setup(x => x["Audience"]).Returns("ConcertUsers");
            configSection.Setup(x => x["ExpiryMinutes"]).Returns("360");

            _configMock.Setup(x => x.GetSection("JwtSettings")).Returns(configSection.Object);

            _controller = new AuthController(_userManagerMock.Object, _configMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Register_ValidUser_ReturnsOkResult()
        {
            // Arrange
            var registerDto = new RegisterDto { Email = "test@example.com", Password = "Test123!" };
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var properties = okResult.Value.GetType().GetProperties();
            var messageProp = properties.FirstOrDefault(p => p.Name.ToLower() == "message");
            Assert.NotNull(messageProp);
            Assert.Equal("User registered successfully.", messageProp.GetValue(okResult.Value)?.ToString());
        }

        [Fact]
        public async Task Register_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto { Email = "", Password = "" };

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Register_UserCreationFails_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto { Email = "test@example.com", Password = "Test123!" };
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "User creation failed" }));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }        

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "test@example.com", Password = "WrongPassword" };
            
            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync((IdentityUser)null);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_EmptyCredentials_ReturnsBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "", Password = "" };

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidUser_WrongPassword_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "test@example.com", Password = "WrongPassword" };
            var user = new IdentityUser { Email = loginDto.Email, UserName = loginDto.Email };

            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}