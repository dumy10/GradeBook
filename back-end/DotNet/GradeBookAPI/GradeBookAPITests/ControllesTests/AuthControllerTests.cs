using FluentAssertions;
using GradeBookAPI.Controllers;
using GradeBookAPI.Data;
using GradeBookAPI.DTOs.AuthDTOs;
using GradeBookAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Net;
using System.Text.Json;

namespace GradeBookAPITests.ControllersTests
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<AppDbContext> _mockContext;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());

            // Setup controller with HttpContext to test IP address logging
            _controller = new AuthController(_mockAuthService.Object, _mockContext.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        // Response classes to deserialize the dynamic objects returned by controller
        private class ApiResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        private class DatabaseConnectionResponse : ApiResponse
        {
            public int UserCount { get; set; }
        }

        [Fact]
        public async Task Register_WithValidModel_ReturnsOkResult()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FirstName = "Test",
                LastName = "User",
                Role = "Student"
            };

            var expectedResponse = new AuthResponse
            {
                Success = true,
                Message = "Registration successful",
                UserId = 1,
                Username = "testuser",
                Email = "test@example.com",
                Token = "jwt-token",
                Role = "STUDENT"
            };

            _mockAuthService
                .Setup(service => service.RegisterAsync(It.IsAny<RegisterRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Register(registerRequest);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var returnValue = okResult.Value.Should().BeAssignableTo<AuthResponse>().Subject;
            returnValue.Success.Should().BeTrue();
            returnValue.Message.Should().Be("Registration successful");
            returnValue.Token.Should().Be("jwt-token");
        }

        [Fact]
        public async Task Register_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "invalid-email",
                Username = "u", // Too short
                Password = "weak",
                ConfirmPassword = "mismatch",
                FirstName = "",
                LastName = "",
                Role = "Invalid"
            };

            _controller.ModelState.AddModelError("Email", "Invalid email format");

            // Act
            var result = await _controller.Register(registerRequest);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_WhenServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FirstName = "Test",
                LastName = "User",
                Role = "Student"
            };

            _mockAuthService
                .Setup(service => service.RegisterAsync(It.IsAny<RegisterRequest>()))
                .ThrowsAsync(new Exception("Database connection error"));

            // Act
            var result = await _controller.Register(registerRequest);

            // Assert
            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

            // Safely deserialize the dynamic response
            var responseJson = JsonSerializer.Serialize(statusCodeResult.Value);
            var response = JsonSerializer.Deserialize<ApiResponse>(responseJson);

            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Be("An error occurred during registration");
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkResultWithToken()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var expectedResponse = new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                UserId = 1,
                Username = "testuser",
                Email = "test@example.com",
                Token = "jwt-token",
                Role = "STUDENT"
            };

            _mockAuthService
                .Setup(service => service.LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var returnValue = okResult.Value.Should().BeAssignableTo<AuthResponse>().Subject;
            returnValue.Success.Should().BeTrue();
            returnValue.Token.Should().Be("jwt-token");
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            var expectedResponse = new AuthResponse
            {
                Success = false,
                Message = "Invalid username or password"
            };

            _mockAuthService
                .Setup(service => service.LoginAsync(It.IsAny<LoginRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var returnValue = badRequestResult.Value.Should().BeAssignableTo<AuthResponse>().Subject;
            returnValue.Success.Should().BeFalse();
            returnValue.Message.Should().Be("Invalid username or password");
        }

        [Fact]
        public async Task ForgotPassword_WithValidEmail_ReturnsOkResult()
        {
            // Arrange
            var forgotPasswordRequest = new ForgotPasswordRequest
            {
                Email = "test@example.com"
            };

            _mockAuthService
                .Setup(service => service.ForgotPasswordAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.ForgotPassword(forgotPasswordRequest);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);

            // Safely deserialize the dynamic response
            var responseJson = JsonSerializer.Serialize(okResult.Value);
            var response = JsonSerializer.Deserialize<ApiResponse>(responseJson);

            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Message.Should().Be("If the email exists, a password reset link has been sent");
        }

        [Fact]
        public async Task ResetPassword_WithValidToken_ReturnsOkResult()
        {
            // Arrange
            var resetPasswordRequest = new ResetPasswordRequest
            {
                Token = "valid-token",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };

            _mockAuthService
                .Setup(service => service.ResetPasswordAsync(It.IsAny<ResetPasswordRequest>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.ResetPassword(resetPasswordRequest);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be((int)HttpStatusCode.OK);

            // Safely deserialize the dynamic response
            var responseJson = JsonSerializer.Serialize(okResult.Value);
            var response = JsonSerializer.Deserialize<ApiResponse>(responseJson);

            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Message.Should().Be("Password reset successful");
        }


        [Fact]
        public async Task ResetPassword_WithInvalidToken_ReturnsBadRequest()
        {
            // Arrange
            var resetPasswordRequest = new ResetPasswordRequest
            {
                Token = "invalid-reset-token",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };

            _mockAuthService
                .Setup(service => service.ResetPasswordAsync(It.IsAny<ResetPasswordRequest>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.ResetPassword(resetPasswordRequest);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            // Safely deserialize the dynamic response
            var responseJson = JsonSerializer.Serialize(badRequestResult.Value);
            var response = JsonSerializer.Deserialize<ApiResponse>(responseJson);

            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Be("Invalid or expired token");
        }
    }
}