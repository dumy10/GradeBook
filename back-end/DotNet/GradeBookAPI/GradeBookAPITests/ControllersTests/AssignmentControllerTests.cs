using GradeBookAPI.Controllers;
using GradeBookAPI.DTOs.DataDTOs;
using GradeBookAPI.Entities;
using GradeBookAPI.Services.Interfaces;
using GradeBookAPITests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace GradeBookAPITests.ControllersTests
{
    public class AssignmentControllerTests
    {
        private readonly Mock<IAssignmentService> _mockAssignmentService;
        private readonly AssignmentController _controller;
        private readonly DefaultHttpContext _httpContext;

        public AssignmentControllerTests()
        {
            // Setup dependencies
            _mockAssignmentService = new Mock<IAssignmentService>();
            _controller = new AssignmentController(_mockAssignmentService.Object);

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

            // Setup authentication in the HTTP context with TEACHER role
            // Assignments can only be created by teachers
            AuthHelperMock.SetupHttpContext(_httpContext, 1, "TEACHER");
        }

        [Fact]
        public async Task CreateAssignment_ReturnsOkResult_WhenAssignmentIsCreated()
        {
            // Arrange
            var request = new CreateAssignmentRequest
            {
                ClassId = 1,
                Title = "Test Assignment",
                Description = "This is a test assignment",
                MaxPoints = 100,
                MinPoints = 0,
                DueDate = DateTime.UtcNow,
                TypeName = "Homework",
                Weight = 10
            };

            var createdAssignment = new Assignment
            {
                AssignmentId = 1,
                ClassId = request.ClassId,
                Title = request.Title,
                Description = request.Description,
                MaxPoints = request.MaxPoints,
                MinPoints = request.MinPoints,
                DueDate = request.DueDate,
                TypeId = 1,
                IsPublished = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockAssignmentService.Setup(service => service.CreateAssignmentAsync(request))
                .ReturnsAsync(createdAssignment);

            // Act
            var result = await _controller.CreateAssignment(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(createdAssignment, okResult.Value);
        }

        [Fact]
        public async Task CreateAssignment_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var request = new CreateAssignmentRequest
            {
                ClassId = 1,
                Title = "", // Title is required
                Description = "This is a test assignment",
                MaxPoints = 100,
                MinPoints = 0,
                DueDate = DateTime.Now.AddDays(7),
                TypeName = "Homework",
                Weight = 10
            };

            _controller.ModelState.AddModelError("Title", "Title is required");

            // Act
            var result = await _controller.CreateAssignment(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task CreateAssignment_ReturnsUnauthorized_WhenNotTeacher()
        {
            // Arrange
            var request = new CreateAssignmentRequest
            {
                ClassId = 1,
                Title = "Test Assignment",
                Description = "This is a test assignment",
                MaxPoints = 100,
                MinPoints = 0,
                DueDate = DateTime.Now.AddDays(7),
                TypeName = "Homework",
                Weight = 10
            };

            // Setup student role for this test
            AuthHelperMock.SetupHttpContext(_httpContext, 2, "STUDENT");

            // Act
            var result = await _controller.CreateAssignment(request);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetAssignments_ReturnsOkResult_WhenAssignmentsExist()
        {
            // Arrange
            var assignments = new List<Assignment>
            {
                new Assignment
                {
                    AssignmentId = 1,
                    ClassId = 1,
                    Title = "Assignment 1",
                    Description = "This is assignment 1",
                    MaxPoints = 100,
                    MinPoints = 0,
                    DueDate = DateTime.Now.AddDays(7),
                    TypeId = 1,
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    AssignmentType = new AssignmentType
                    {
                        TypeId = 1,
                        TypeName = "Homework",
                        Weight = 10,
                        Description = "Regular homework",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                },
                new Assignment
                {
                    AssignmentId = 2,
                    ClassId = 1,
                    Title = "Assignment 2",
                    Description = "This is assignment 2",
                    MaxPoints = 100,
                    MinPoints = 0,
                    DueDate = DateTime.Now.AddDays(14),
                    TypeId = 2,
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    AssignmentType = new AssignmentType
                    {
                        TypeId = 2,
                        TypeName = "Exam",
                        Weight = 30,
                        Description = "Final exam",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                }
            };

            _mockAssignmentService.Setup(service => service.GetAllAssignmentsAsync())
                .ReturnsAsync(assignments);

            // Act
            var result = await _controller.GetAssignments();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(assignments, okResult.Value);
        }

        [Fact]
        public async Task GetAssignments_ReturnsNotFound_WhenNoAssignmentsExist()
        {
            // Arrange
            _mockAssignmentService.Setup(service => service.GetAllAssignmentsAsync())
                .ReturnsAsync(new List<Assignment>());

            // Act
            var result = await _controller.GetAssignments();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
        }

        [Fact]
        public async Task GetAssignmentsForClass_ReturnsOkResult_WhenAssignmentsExist()
        {
            // Arrange
            int classId = 1;
            var assignments = new List<Assignment>
            {
                new Assignment
                {
                    AssignmentId = 1,
                    ClassId = classId,
                    Title = "Assignment 1",
                    Description = "This is assignment 1",
                    MaxPoints = 100,
                    MinPoints = 0,
                    DueDate = DateTime.Now.AddDays(7),
                    TypeId = 1,
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    AssignmentType = new AssignmentType
                    {
                        TypeId = 1,
                        TypeName = "Homework",
                        Weight = 10,
                        Description = "Regular homework",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                },
                new Assignment
                {
                    AssignmentId = 2,
                    ClassId = classId,
                    Title = "Assignment 2",
                    Description = "This is assignment 2",
                    MaxPoints = 100,
                    MinPoints = 0,
                    DueDate = DateTime.Now.AddDays(14),
                    TypeId = 2,
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    AssignmentType = new AssignmentType
                    {
                        TypeId = 2,
                        TypeName = "Exam",
                        Weight = 30,
                        Description = "Final exam",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                }
            };

            _mockAssignmentService.Setup(service => service.GetAssignmentsForClassAsync(classId))
                .ReturnsAsync(assignments);

            // Act
            var result = await _controller.GetAssignmentsForClass(classId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(assignments, okResult.Value);
        }

        [Fact]
        public async Task GetAssignmentsForClass_ReturnsNotFound_WhenNoAssignmentsExist()
        {
            // Arrange
            int classId = 1;
            _mockAssignmentService.Setup(service => service.GetAssignmentsForClassAsync(classId))
                .ReturnsAsync(new List<Assignment>());

            // Act
            var result = await _controller.GetAssignmentsForClass(classId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateAssignment_ReturnsOkResult_WhenAssignmentIsUpdated()
        {
            // Arrange
            int assignmentId = 1;
            var request = new CreateAssignmentRequest
            {
                ClassId = 1,
                Title = "Updated Assignment",
                Description = "This is an updated assignment",
                MaxPoints = 100,
                MinPoints = 0,
                DueDate = DateTime.Now.AddDays(7),
                TypeName = "Homework",
                Weight = 10
            };

            var updatedAssignment = new Assignment
            {
                AssignmentId = assignmentId,
                ClassId = request.ClassId,
                Title = request.Title,
                Description = request.Description,
                MaxPoints = request.MaxPoints,
                MinPoints = request.MinPoints,
                DueDate = request.DueDate,
                TypeId = 1,
                IsPublished = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockAssignmentService.Setup(service => service.UpdateAssignmentAsync(assignmentId, request))
                .ReturnsAsync(updatedAssignment);

            // Act
            var result = await _controller.UpdateAssignment(assignmentId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(updatedAssignment, okResult.Value);
        }

        [Fact]
        public async Task UpdateAssignment_ReturnsNotFound_WhenAssignmentDoesNotExist()
        {
            // Arrange
            int assignmentId = 1;
            var request = new CreateAssignmentRequest
            {
                ClassId = 1,
                Title = "Updated Assignment",
                Description = "This is an updated assignment",
                MaxPoints = 100,
                MinPoints = 0,
                DueDate = DateTime.Now.AddDays(7),
                TypeName = "Homework",
                Weight = 10
            };

            _mockAssignmentService.Setup(service => service.UpdateAssignmentAsync(assignmentId, request))
                .ReturnsAsync((Assignment)null!);

            // Act
            var result = await _controller.UpdateAssignment(assignmentId, request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
        }

        [Fact]
        public async Task DeleteAssignment_ReturnsOkResult_WhenAssignmentIsDeleted()
        {
            // Arrange
            int assignmentId = 1;
            _mockAssignmentService.Setup(service => service.DeleteAssignmentAsync(assignmentId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteAssignment(assignmentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task DeleteAssignment_ReturnsNotFound_WhenAssignmentDoesNotExist()
        {
            // Arrange
            int assignmentId = 1;
            _mockAssignmentService.Setup(service => service.DeleteAssignmentAsync(assignmentId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteAssignment(assignmentId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
        }
    }
}