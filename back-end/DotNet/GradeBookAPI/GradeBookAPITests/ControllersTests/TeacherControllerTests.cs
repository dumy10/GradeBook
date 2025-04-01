using GradeBookAPI.Controllers;
using GradeBookAPI.DTOs.DataDTOs;
using GradeBookAPI.Services.Interfaces;
using GradeBookAPITests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace GradeBookAPITests.ControllersTests
{
    public class TeacherControllerTests
    {
        private readonly Mock<ITeacherService> _mockTeacherService;
        private readonly TeacherController _controller;
        private readonly DefaultHttpContext _httpContext;

        public TeacherControllerTests()
        {
            // Setup mock service
            _mockTeacherService = new Mock<ITeacherService>();
            _controller = new TeacherController(_mockTeacherService.Object);

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
            AuthHelperMock.SetupHttpContext(_httpContext, 1, "TEACHER");
        }

        [Fact]
        public async Task AddStudent_ReturnsOkResult_WhenStudentIsAdded()
        {
            // Arrange
            int classId = 1;
            var request = new AddStudentRequest { StudentId = 2 };

            _mockTeacherService.Setup(service => service.AddStudentToClassAsync(1, classId, request))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AddStudent(classId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Student added successfully.", okResult.Value);
        }

        [Fact]
        public async Task AddStudent_ReturnsBadRequest_WhenStudentCannotBeAdded()
        {
            // Arrange
            int classId = 1;
            var request = new AddStudentRequest { StudentId = 2 };

            _mockTeacherService.Setup(service => service.AddStudentToClassAsync(1, classId, request))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.AddStudent(classId, request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Unable to add student.", badRequestResult.Value);
        }

        [Fact]
        public async Task AddStudent_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            int classId = 1;
            var request = new AddStudentRequest { StudentId = 0 }; // Invalid student ID

            _controller.ModelState.AddModelError("StudentId", "Student ID is required");

            // Act
            var result = await _controller.AddStudent(classId, request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task RemoveStudent_ReturnsOkResult_WhenStudentIsRemoved()
        {
            // Arrange
            int classId = 1;
            int studentId = 2;

            _mockTeacherService.Setup(service => service.RemoveStudentFromClassAsync(1, classId, studentId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.RemoveStudent(classId, studentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Student removed successfully.", okResult.Value);
        }

        [Fact]
        public async Task RemoveStudent_ReturnsBadRequest_WhenStudentCannotBeRemoved()
        {
            // Arrange
            int classId = 1;
            int studentId = 2;

            _mockTeacherService.Setup(service => service.RemoveStudentFromClassAsync(1, classId, studentId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.RemoveStudent(classId, studentId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Unable to remove student.", badRequestResult.Value);
        }

        [Fact]
        public async Task AddStudents_ReturnsOkResult_WhenStudentsAreAdded()
        {
            // Arrange
            int classId = 1;
            var request = new AddStudentsRequest { StudentIds = new List<int> { 2, 3, 4 } };

            _mockTeacherService.Setup(service => service.AddStudentsToClassAsync(1, classId, request))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AddStudents(classId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Students added successfully.", okResult.Value);
        }

        [Fact]
        public async Task AddStudents_ReturnsBadRequest_WhenStudentsCannotBeAdded()
        {
            // Arrange
            int classId = 1;
            var request = new AddStudentsRequest { StudentIds = new List<int> { 2, 3, 4 } };

            _mockTeacherService.Setup(service => service.AddStudentsToClassAsync(1, classId, request))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.AddStudents(classId, request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Unable to add some or all students.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateCourse_ReturnsOkResult_WhenCourseIsCreated()
        {
            // Arrange
            var request = new CreateCourseRequest
            {
                CourseName = "Test Course",
                CourseCode = "TEST101",
                Description = "Test course description"
            };

            _mockTeacherService.Setup(service => service.CreateCourseAsync(1, request))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.CreateCourse(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Course created successfully.", okResult.Value);
        }

        [Fact]
        public async Task CreateCourse_ReturnsBadRequest_WhenCourseCreationFails()
        {
            // Arrange
            var request = new CreateCourseRequest
            {
                CourseName = "Test Course",
                CourseCode = "TEST101",
                Description = "Test course description"
            };

            _mockTeacherService.Setup(service => service.CreateCourseAsync(1, request))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.CreateCourse(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Course creation failed.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateClass_ReturnsOkResult_WhenClassIsCreated()
        {
            // Arrange
            var request = new CreateClassRequest
            {
                CourseId = 1,
                ClassName = "Test Class",
                Description = "Test class description",
                Semester = "Fall",
                AcademicYear = "2023-2024",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(3)
            };

            _mockTeacherService.Setup(service => service.CreateClassAsync(1, request))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.CreateClass(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Class created successfully.", okResult.Value);
        }

        [Fact]
        public async Task CreateClass_ReturnsBadRequest_WhenClassCreationFails()
        {
            // Arrange
            var request = new CreateClassRequest
            {
                CourseId = 1,
                ClassName = "Test Class",
                Description = "Test class description",
                Semester = "Fall",
                AcademicYear = "2023-2024",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(3)
            };

            _mockTeacherService.Setup(service => service.CreateClassAsync(1, request))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.CreateClass(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Class creation failed. Verify that the course exists.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetClasses_ReturnsOkResult_WhenClassesExist()
        {
            // Arrange
            var classes = new List<ClassDto>
            {
                new ClassDto
                {
                    ClassId = 1,
                    CourseId = 1,
                    Semester = "Fall",
                    AcademicYear = "2023-2024",
                    StartDate = DateOnly.FromDateTime(DateTime.Now),
                    EndDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(3))
                },
                new ClassDto
                {
                    ClassId = 2,
                    CourseId = 2,
                    Semester = "Spring",
                    AcademicYear = "2023-2024",
                    StartDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(4)),
                    EndDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(7))
                }
            };

            _mockTeacherService.Setup(service => service.GetClassesAsync(1))
                .ReturnsAsync(classes);

            // Act
            var result = await _controller.GetClasses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(classes, okResult.Value);
        }

        [Fact]
        public async Task GetClasses_ReturnsNotFound_WhenNoClassesExist()
        {
            // Arrange
            _mockTeacherService.Setup(service => service.GetClassesAsync(1))
                .ReturnsAsync(new List<ClassDto>());

            // Act
            var result = await _controller.GetClasses();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No classes found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetAssignedCourses_ReturnsOkResult_WhenCoursesExist()
        {
            // Arrange
            var courses = new List<CourseDto>
            {
                new CourseDto { CourseId = 1, CourseName = "Math", CourseCode = "MTH101", Description = "Mathematics" },
                new CourseDto { CourseId = 2, CourseName = "Science", CourseCode = "SCI101", Description = "Science" }
            };

            _mockTeacherService.Setup(service => service.GetCoursesAsync(1))
                .ReturnsAsync(courses);

            // Act
            var result = await _controller.GetAssignedCourses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(courses, okResult.Value);
        }

        [Fact]
        public async Task GetAssignedCourses_ReturnsNotFound_WhenNoCoursesExist()
        {
            // Arrange
            _mockTeacherService.Setup(service => service.GetCoursesAsync(1))
                .ReturnsAsync(new List<CourseDto>());

            // Act
            var result = await _controller.GetAssignedCourses();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No courses found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetCourses_ReturnsOkResult_WhenCoursesExist()
        {
            // Arrange
            var courses = new List<CourseDto>
            {
                new CourseDto { CourseId = 1, CourseName = "Math", CourseCode = "MTH101", Description = "Mathematics" },
                new CourseDto { CourseId = 2, CourseName = "Science", CourseCode = "SCI101", Description = "Science" }
            };

            _mockTeacherService.Setup(service => service.GetAllCoursesAsync())
                .ReturnsAsync(courses);

            // Act
            var result = await _controller.GetCourses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(courses, okResult.Value);
        }

        [Fact]
        public async Task GetCourses_ReturnsNotFound_WhenNoCoursesExist()
        {
            // Arrange
            _mockTeacherService.Setup(service => service.GetAllCoursesAsync())
                .ReturnsAsync(new List<CourseDto>());

            // Act
            var result = await _controller.GetCourses();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No courses found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetStudents_ReturnsOkResult_WhenStudentsExist()
        {
            // Arrange
            int classId = 1;
            var students = new List<StudentDto>
            {
                new StudentDto { StudentId = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" },
                new StudentDto { StudentId = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
            };

            _mockTeacherService.Setup(service => service.GetStudentsInClassAsync(1, classId))
                .ReturnsAsync(students);

            // Act
            var result = await _controller.GetStudents(classId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(students, okResult.Value);
        }

        [Fact]
        public async Task GetStudents_ReturnsNotFound_WhenNoStudentsExist()
        {
            // Arrange
            int classId = 1;
            _mockTeacherService.Setup(service => service.GetStudentsInClassAsync(1, classId))
                .ReturnsAsync(new List<StudentDto>());

            // Act
            var result = await _controller.GetStudents(classId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No students found.", notFoundResult.Value);
        }
    }
}