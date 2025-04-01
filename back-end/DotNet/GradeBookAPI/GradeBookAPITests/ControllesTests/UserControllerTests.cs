using GradeBookAPI.Controllers;
using GradeBookAPI.DTOs.AuthDTOs;
using GradeBookAPI.Services.Interfaces;
using GradeBookAPITests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace GradeBookAPITests.ControllesTests
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _controller;
        private readonly DefaultHttpContext _httpContext;

        public UserControllerTests()
        {
            // Setup dependencies
            _mockUserService = new Mock<IUserService>();
            _controller = new UserController(_mockUserService.Object);

            // Create HTTP context
            _httpContext = new DefaultHttpContext();

            // Setup IP address for AuditLog
            _httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");

            // Configure the controller with the HTTP context
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };

            // Setup environment variables for token validation
            AuthHelperMock.SetupEnvironmentVariables();

            // Setup authentication in the HTTP context
            AuthHelperMock.SetupHttpContext(_httpContext, 1, "STUDENT");
        }

        [Fact]
        public async Task GetProfile_ReturnsOkResult_WhenUserExists()
        {
            // Arrange
            var userProfile = new UserProfileDto
            {
                UserId = 1,
                Username = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                Role = "STUDENT"
            };

            _mockUserService.Setup(service => service.GetUserProfileAsync(1))
                .ReturnsAsync(userProfile);

            // Act
            var result = await _controller.GetProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Convert to anonymous object to avoid dynamic
            var responseObj = okResult.Value as object;
            Assert.NotNull(responseObj);

            // Get properties using reflection to avoid dynamic
            var responseType = responseObj.GetType();
            var successProp = responseType.GetProperty("Success");
            var profileProp = responseType.GetProperty("Profile");

            Assert.NotNull(successProp);
            Assert.NotNull(profileProp);

            var success = (bool)successProp.GetValue(responseObj);
            var profile = profileProp.GetValue(responseObj);

            Assert.True(success);
            Assert.Same(userProfile, profile);
        }

        [Fact]
        public async Task GetProfile_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            _mockUserService.Setup(service => service.GetUserProfileAsync(1))
                .ReturnsAsync((UserProfileDto)null);

            // Act
            var result = await _controller.GetProfile();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

            // Convert to anonymous object to avoid dynamic
            var responseObj = notFoundResult.Value as object;
            Assert.NotNull(responseObj);

            // Get properties using reflection to avoid dynamic
            var responseType = responseObj.GetType();
            var successProp = responseType.GetProperty("Success");
            var messageProp = responseType.GetProperty("Message");

            Assert.NotNull(successProp);
            Assert.NotNull(messageProp);

            var success = (bool)successProp.GetValue(responseObj);
            var message = (string)messageProp.GetValue(responseObj);

            Assert.False(success);
            Assert.Equal("User not found", message);
        }

        [Fact]
        public async Task GetUserDetails_ReturnsOkResult_WhenUserExists()
        {
            // Arrange
            int userId = 2;
            var userDetails = new UserDetailsDto
            {
                UserId = userId,
                Email = "user2@example.com",
                FirstName = "Another",
                LastName = "User",
                Role = "TEACHER"
            };

            _mockUserService.Setup(service => service.GetUserDetailsAsync(userId))
                .ReturnsAsync(userDetails);

            // Act
            var result = await _controller.GetUserDetails(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(userDetails, okResult.Value);
        }

        [Fact]
        public async Task GetUserDetails_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            int userId = 2;
            _mockUserService.Setup(service => service.GetUserDetailsAsync(userId))
                .ReturnsAsync((UserDetailsDto)null);

            // Act
            var result = await _controller.GetUserDetails(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

            // Convert to anonymous object to avoid dynamic
            var responseObj = notFoundResult.Value as object;
            Assert.NotNull(responseObj);

            // Get properties using reflection to avoid dynamic
            var responseType = responseObj.GetType();
            var successProp = responseType.GetProperty("Success");
            var messageProp = responseType.GetProperty("Message");

            Assert.NotNull(successProp);
            Assert.NotNull(messageProp);

            var success = (bool)successProp.GetValue(responseObj);
            var message = (string)messageProp.GetValue(responseObj);

            Assert.False(success);
            Assert.Equal("User not found", message);
        }

        [Fact]
        public async Task GetAllUsersDetails_ReturnsOkResult_WhenUsersExist()
        {
            // Arrange
            var users = new List<UserDetailsDto>
            {
                new UserDetailsDto
                {
                    UserId = 1,
                    Email = "user1@example.com",
                    FirstName = "User",
                    LastName = "One",
                    Role = "STUDENT"
                },
                new UserDetailsDto
                {
                    UserId = 2,
                    Email = "user2@example.com",
                    FirstName = "User",
                    LastName = "Two",
                    Role = "TEACHER"
                }
            };

            _mockUserService.Setup(service => service.GetAllUsersDetailsAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _controller.GetAllUsersDetails();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(users, okResult.Value);
        }

        [Fact]
        public async Task GetAllUsersDetails_ReturnsNotFound_WhenNoUsersExist()
        {
            // Arrange
            _mockUserService.Setup(service => service.GetAllUsersDetailsAsync())
                .ReturnsAsync((IEnumerable<UserDetailsDto>)null);

            // Act
            var result = await _controller.GetAllUsersDetails();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

            // Convert to anonymous object to avoid dynamic
            var responseObj = notFoundResult.Value as object;
            Assert.NotNull(responseObj);

            // Get properties using reflection to avoid dynamic
            var responseType = responseObj.GetType();
            var successProp = responseType.GetProperty("Success");
            var messageProp = responseType.GetProperty("Message");

            Assert.NotNull(successProp);
            Assert.NotNull(messageProp);

            var success = (bool)successProp.GetValue(responseObj);
            var message = (string)messageProp.GetValue(responseObj);

            Assert.False(success);
            Assert.Equal("No users found", message);
        }

        [Fact]
        public async Task UpdateProfile_ReturnsOkResult_WhenValidInput()
        {
            // Arrange
            var updateRequest = new UpdateProfileRequest
            {
                FirstName = "Updated",
                LastName = "User",
                Phone = "1234567890",
                Address = "123 Main St"
            };

            _mockUserService.Setup(service => service.UpdateUserProfileAsync(1, updateRequest))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateProfile(updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Convert to anonymous object to avoid dynamic
            var responseObj = okResult.Value as object;
            Assert.NotNull(responseObj);

            // Get properties using reflection to avoid dynamic
            var responseType = responseObj.GetType();
            var successProp = responseType.GetProperty("Success");
            var messageProp = responseType.GetProperty("Message");

            Assert.NotNull(successProp);
            Assert.NotNull(messageProp);

            var success = (bool)successProp.GetValue(responseObj);
            var message = (string)messageProp.GetValue(responseObj);

            Assert.True(success);
            Assert.Equal("Profile updated successfully", message);
        }

        [Fact]
        public async Task UpdateProfile_ReturnsBadRequest_WhenInvalidInput()
        {
            // Arrange
            var updateRequest = new UpdateProfileRequest
            {
                FirstName = "Updated",
                LastName = "User",
                Phone = "invalid_phone", // Invalid phone number
                Address = "123 Main St"
            };

            _mockUserService.Setup(service => service.UpdateUserProfileAsync(1, updateRequest))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateProfile(updateRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // Convert to anonymous object to avoid dynamic
            var responseObj = badRequestResult.Value as object;
            Assert.NotNull(responseObj);

            // Get properties using reflection to avoid dynamic
            var responseType = responseObj.GetType();
            var successProp = responseType.GetProperty("Success");
            var messageProp = responseType.GetProperty("Message");

            Assert.NotNull(successProp);
            Assert.NotNull(messageProp);

            var success = (bool)successProp.GetValue(responseObj);
            var message = (string)messageProp.GetValue(responseObj);

            Assert.False(success);
            Assert.Equal("Failed to update profile", message);
        }

        [Fact]
        public async Task UpdateProfile_ReturnsBadRequest_WhenModelStateInvalid()
        {
            // Arrange
            var updateRequest = new UpdateProfileRequest
            {
                FirstName = "Updated",
                LastName = "User"
            };

            _controller.ModelState.AddModelError("Phone", "Phone number is required");

            // Act
            var result = await _controller.UpdateProfile(updateRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // Convert to anonymous object to avoid dynamic
            var responseObj = badRequestResult.Value as object;
            Assert.NotNull(responseObj);

            // Get properties using reflection to avoid dynamic
            var responseType = responseObj.GetType();
            var successProp = responseType.GetProperty("Success");

            Assert.NotNull(successProp);

            var success = (bool)successProp.GetValue(responseObj);
            Assert.False(success);
        }

        [Fact]
        public async Task ChangePassword_ReturnsOkResult_WhenValidInput()
        {
            // Arrange
            var changePasswordRequest = new ChangePasswordRequest
            {
                CurrentPassword = "oldPassword",
                NewPassword = "newPassword",
                ConfirmPassword = "newPassword"
            };

            _mockUserService.Setup(service => service.ChangePasswordAsync(1, changePasswordRequest))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.ChangePassword(changePasswordRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Convert to anonymous object to avoid dynamic
            var responseObj = okResult.Value as object;
            Assert.NotNull(responseObj);

            // Get properties using reflection to avoid dynamic
            var responseType = responseObj.GetType();
            var successProp = responseType.GetProperty("Success");
            var messageProp = responseType.GetProperty("Message");

            Assert.NotNull(successProp);
            Assert.NotNull(messageProp);

            var success = (bool)successProp.GetValue(responseObj);
            var message = (string)messageProp.GetValue(responseObj);

            Assert.True(success);
            Assert.Equal("Password changed successfully", message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsBadRequest_WhenInvalidCurrentPassword()
        {
            // Arrange
            var changePasswordRequest = new ChangePasswordRequest
            {
                CurrentPassword = "wrongPassword",
                NewPassword = "newPassword",
                ConfirmPassword = "newPassword"
            };

            _mockUserService.Setup(service => service.ChangePasswordAsync(1, changePasswordRequest))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.ChangePassword(changePasswordRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // Convert to anonymous object to avoid dynamic
            var responseObj = badRequestResult.Value as object;
            Assert.NotNull(responseObj);

            // Get properties using reflection to avoid dynamic
            var responseType = responseObj.GetType();
            var successProp = responseType.GetProperty("Success");
            var messageProp = responseType.GetProperty("Message");

            Assert.NotNull(successProp);
            Assert.NotNull(messageProp);

            var success = (bool)successProp.GetValue(responseObj);
            var message = (string)messageProp.GetValue(responseObj);

            Assert.False(success);
            Assert.Equal("Current password is incorrect", message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsBadRequest_WhenModelStateInvalid()
        {
            // Arrange
            var changePasswordRequest = new ChangePasswordRequest
            {
                CurrentPassword = "oldPassword",
                NewPassword = "newPassword",
                ConfirmPassword = "differentPassword" // Passwords don't match
            };

            _controller.ModelState.AddModelError("ConfirmPassword", "Passwords do not match");

            // Act
            var result = await _controller.ChangePassword(changePasswordRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // Convert to anonymous object to avoid dynamic
            var responseObj = badRequestResult.Value as object;
            Assert.NotNull(responseObj);

            // Get properties using reflection to avoid dynamic
            var responseType = responseObj.GetType();
            var successProp = responseType.GetProperty("Success");

            Assert.NotNull(successProp);

            var success = (bool)successProp.GetValue(responseObj);
            Assert.False(success);
        }
    }
}