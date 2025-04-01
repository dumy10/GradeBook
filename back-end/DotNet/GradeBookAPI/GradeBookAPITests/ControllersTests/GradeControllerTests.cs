using GradeBookAPI.Controllers;
using GradeBookAPI.DTOs.DataDTOs;
using GradeBookAPI.Entities;
using GradeBookAPI.Services.Interfaces;
using GradeBookAPITests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GradeBookAPITests.ControllersTests
{
    public class GradeControllerTests
    {
        private readonly Mock<IGradeService> _mockGradeService;
        private readonly GradeController _controller;

        public GradeControllerTests()
        {
            // Setup environment variables for JWT validation
            AuthHelperMock.SetupEnvironmentVariables();

            _mockGradeService = new Mock<IGradeService>();
            _controller = new GradeController(_mockGradeService.Object);

            // Setup controller context with proper auth
            var httpContext = new DefaultHttpContext();
            // Use "TEACHER" (not "Teacher") to match the enum in AuthHelper
            AuthHelperMock.SetupHttpContext(httpContext, 1, "TEACHER");

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task GetAllGrades_ReturnsOkResult_WithListOfGrades()
        {
            // Arrange
            var grades = new List<GradeDto>
            {
                CreateTestGradeDto(1),
                CreateTestGradeDto(2)
            };

            _mockGradeService.Setup(service => service.GetGradesAsync())
                .ReturnsAsync(grades);

            // Act
            var result = await _controller.GetGrades();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedGrades = Assert.IsAssignableFrom<IEnumerable<GradeDto>>(okResult.Value);
            Assert.Equal(2, returnedGrades.Count());
        }

        [Fact]
        public async Task GetGradeById_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var gradeId = 1;
            var grade = CreateTestGradeDto(gradeId);

            _mockGradeService.Setup(service => service.GetGradeByIdAsync(gradeId))
                .ReturnsAsync(grade);

            // Act
            var result = await _controller.GetGrade(gradeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedGrade = Assert.IsType<GradeDto>(okResult.Value);
            Assert.Equal(gradeId, returnedGrade.GradeId);
        }

        [Fact]
        public async Task GetGradeById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var gradeId = 999;
            _mockGradeService.Setup(service => service.GetGradeByIdAsync(gradeId))
                .ReturnsAsync((GradeDto)null!);

            // Act
            var result = await _controller.GetGrade(gradeId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CreateGrade_WithValidModel_ReturnsCreatedAtAction()
        {
            // Arrange
            var newGrade = new CreateGradeRequest
            {
                Comment = "Good job",
                Points = 85,
                StudentId = 1,
                AssignmentId = 3
            };

            var createdGrade = new Grade
            {
                GradeId = 1,
                Comment = "Good job",
                Points = 85,
                StudentId = 1,
                GradedBy = 2,
                AssignmentId = 3,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _mockGradeService.Setup(service => service.CreateGradeAsync(It.IsAny<Grade>()))
                .ReturnsAsync(createdGrade);

            // Act
            var result = await _controller.CreateGrade(newGrade);

            // Assert
            // Change this line to match what the controller actually returns
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Grade>(okResult.Value);
            Assert.Equal(1, returnValue.GradeId);
        }

        [Fact]
        public async Task UpdateGrade_WithValidModel_ReturnsNoContent()
        {
            // Arrange
            var gradeId = 1;
            var gradeToUpdate = new UpdateGradeRequest
            {
                Comment = "Updated comment",
                Points = 90,
                StudentId = 1,
                AssignmentId = 3
            };
            _mockGradeService.Setup(service => service.UpdateGradeAsync(It.IsAny<Grade>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateGrade(gradeId, gradeToUpdate);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateGrade_WithMismatchedIds_ReturnsBadRequest()
        {
            // Arrange
            var gradeId = 1;
            var gradeToUpdate = new UpdateGradeRequest
            {
                Comment = "Updated comment",
                Points = 90
            };
            // Act
            var result = await _controller.UpdateGrade(gradeId, gradeToUpdate);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateGrade_WhenUpdateFails_ReturnsNotFound()
        {
            // Arrange
            var gradeId = 1;
            var gradeToUpdate = new UpdateGradeRequest
            {
                Comment = "Updated comment",
                Points = 90
            };
            _mockGradeService.Setup(service => service.UpdateGradeAsync(It.IsAny<Grade>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateGrade(gradeId, gradeToUpdate);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteGrade_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var gradeId = 1;
            _mockGradeService.Setup(service => service.DeleteGradeAsync(gradeId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteGrade(gradeId);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DeleteGrade_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var gradeId = 999;
            _mockGradeService.Setup(service => service.DeleteGradeAsync(gradeId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteGrade(gradeId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetGradesForClass_ReturnsOkResult_WithListOfGrades()
        {
            // Arrange
            var classId = 1;
            var grades = new List<GradeDto>
            {
                CreateTestGradeDto(1),
                CreateTestGradeDto(2)
            };

            _mockGradeService.Setup(service => service.GetGradesForClassAsync(classId))
                .ReturnsAsync(grades);

            // Act
            var result = await _controller.GetGradesByClass(classId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Convert okResult.Value to the expected type (IEnumerable<GradeDto>, List<GradeDto>, etc.)
            var returnedGrades = Assert.IsAssignableFrom<IEnumerable<GradeDto>>(okResult.Value);

            Assert.Equal(2, returnedGrades.Count());
        }

        [Fact]
        public async Task GetGradesForStudent_ReturnsOkResult_WithListOfGrades()
        {
            // Arrange
            var studentId = 1;
            var grades = new List<GradeDto>
            {
                CreateTestGradeDto(1),
                CreateTestGradeDto(2)
            };

            _mockGradeService.Setup(service => service.GetGradesForStudentAsync(studentId))
                .ReturnsAsync(grades);

            // Act
            var result = await _controller.GetGradesByStudent(studentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedGrades = Assert.IsAssignableFrom<IEnumerable<GradeDto>>(okResult.Value);

            Assert.Equal(2, returnedGrades.Count());
        }


        private GradeDto CreateTestGradeDto(int id)
        {
            return new GradeDto
            {
                GradeId = id,
                Comment = $"Test Comment {id}",
                Points = 80 + id,
                CreatedAt = DateTime.Now.AddDays(-1),
                UpdatedAt = DateTime.Now,
                Grader = new GradeDto.UserDto
                {
                    UserId = 1,
                    FirstName = "Teacher",
                    LastName = "Test",
                    Email = "teacher@test.com",
                    ProfilePicture = ""
                },
                Student = new GradeDto.UserDto
                {
                    UserId = 2,
                    FirstName = "Student",
                    LastName = "Test",
                    Email = "student@test.com",
                    ProfilePicture = ""
                },
                Assignment = new GradeDto.AssignmentDto
                {
                    Title = "Test Assignment",
                    Description = "Test Description",
                    MaxPoints = 100,
                    MinPoints = 0,
                    DueDate = DateTime.Now.AddDays(7),
                    AssignmentType = new GradeDto.AssignmentTypeDto
                    {
                        Weight = 10,
                        Name = "Test Type",
                        Description = "Test Description"
                    },
                    Class = new GradeDto.ClassDto
                    {
                        Semester = "Fall",
                        AcademicYear = "2023-2024",
                        Course = new GradeDto.CourseDto
                        {
                            Name = "Test Course",
                            Description = "Test Course Description"
                        }
                    }
                }
            };
        }
    }
}