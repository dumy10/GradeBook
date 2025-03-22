using GradeBookAPI.DTOs.DataDTOs;
using GradeBookAPI.Entities;
using GradeBookAPI.Helpers;
using GradeBookAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

using GradeLogger = GradeBookAPI.Logger.Logger;

namespace GradeBookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GradeController(IGradeService gradeService) : ControllerBase
    {
        private readonly IGradeService _gradeService = gradeService ?? throw new ArgumentNullException(nameof(gradeService));

        // POST: api/Grade
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateGrade([FromBody] CreateGradeRequest request)
        {
            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "CreateGrade",
                    EntityType = "Grade",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in CreateGrade" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);

                return Unauthorized(new { Success = false, Message = "Unauthorized" });
            }

            if (!ModelState.IsValid)
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "CreateGrade",
                    EntityType = "Grade",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Invalid input data for CreateGrade", errors = ModelState }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);

                return BadRequest(new { Success = false, Message = ModelState });
            }

            var grade = new Grade
            {
                AssignmentId = request.AssignmentId,
                StudentId = request.StudentId,
                Points = request.Points,
                Comment = request.Comment,
                GradedBy = teacherId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            try
            {
                var created = await _gradeService.CreateGradeAsync(grade);

                var successLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "CreateGrade",
                    EntityType = "Grade",
                    EntityId = created.GradeId,
                    Details = JsonSerializer.Serialize(new { message = "Grade created successfully" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogMessage(successLog);

                return Ok(created);
            }
            catch (Exception ex)
            {
                var errorLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "CreateGrade",
                    EntityType = "Grade",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Error creating grade", exception = ex.Message, innerException = ex.InnerException?.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };

                GradeLogger.Instance.LogError(errorLog);

                return BadRequest(new { Succes = false, Message = "An error occurred while creating the grade." });
            }
        }

        // POST: api/Grade/batch
        [HttpPost("batch")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateGradesBatch([FromBody] List<CreateGradeRequest> requests)
        {
            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "CreateGradesBatch",
                    EntityType = "Grade",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in CreateGradesBatch" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);

                return Unauthorized(new { Success = false, Message = "Unauthorized" });
            }

            if (requests == null || requests.Count == 0)
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "CreateGradesBatch",
                    EntityType = "Grade",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "No grade requests provided." }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);

                return BadRequest(new { Success = false, Message = "No grade requests provided." });
            }

            var createdGrades = new List<Grade>();
            var errors = new List<object>();

            foreach (var req in requests)
            {
                var grade = new Grade
                {
                    AssignmentId = req.AssignmentId,
                    StudentId = req.StudentId,
                    Points = req.Points,
                    Comment = req.Comment,
                    GradedBy = teacherId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                try
                {
                    var created = await _gradeService.CreateGradeAsync(grade);
                    createdGrades.Add(created);
                }
                catch (Exception ex)
                {
                    errors.Add(new { Request = req, Error = ex.Message });

                    var errorLog = new AuditLog
                    {
                        UserId = teacherId,
                        Action = "CreateGradesBatch",
                        EntityType = "Grade",
                        EntityId = 0,
                        Details = JsonSerializer.Serialize(new { message = "Error creating grade", exception = ex.Message, innerException = ex.InnerException?.Message }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };

                    GradeLogger.Instance.LogError(errorLog);
                }
            }

            var response = new
            {
                Success = true,
                CreatedCount = createdGrades.Count,
                Errors = errors,
                Grades = createdGrades
            };

            return Ok(response);
        }

        // GET: api/Grade/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGrade(int id)
        {
            if (!AuthHelper.IsAuthenticated(HttpContext, out int userId))
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = "GetGrade",
                    EntityType = "Grade",
                    EntityId = id,
                    Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in GetGrade" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);

                return Unauthorized(new { Success = false, Message = "Unauthorized" });
            }

            var grade = await _gradeService.GetGradeByIdAsync(id);

            if (grade == null)
            {
                return NotFound(new { Success = false, Message = "Grade not found." });
            }

            return Ok(grade);
        }

        // GET: api/Grade
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGrades()
        {
            if (!AuthHelper.IsAuthenticated(HttpContext, out int userId))
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = "GetGrade",
                    EntityType = "Grade",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in GetGrade" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);

                return Unauthorized(new { Success = false, Message = "Unauthorized" });
            }

            var grades = await _gradeService.GetGradesAsync();

            if (grades == null)
            {
                return NotFound(new { Success = false, Message = "No grades found." });
            }

            return Ok(grades);
        }

        // GET: api/Grade/class/{classId}
        [HttpGet("class/{classId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGradesByClass(int classId)
        {
            if (!AuthHelper.IsAuthenticated(HttpContext, out int userId))
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = "GetGradesByClass",
                    EntityType = "Grade",
                    EntityId = classId,
                    Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in GetGradesByClass" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);

                return Unauthorized(new { Success = false, Message = "Unauthorized" });
            }

            try
            {
                var grades = await _gradeService.GetGradesForClassAsync(classId);

                if (grades == null)
                {
                    return NotFound(new { Success = false, Message = "No grades found." });
                }

                return Ok(grades);
            }
            catch (Exception ex)
            {
                var auditLogError = new AuditLog
                {
                    UserId = userId,
                    Action = "GetGradesByClass",
                    EntityType = "Grade",
                    EntityId = classId,
                    Details = JsonSerializer.Serialize(new { message = ex.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };

                GradeLogger.Instance.LogError(auditLogError);

                return BadRequest(new { Success = false, Message = "An error occurred while fetching grades." });
            }
        }

        // GET: api/Grade/student/{studentId}
        [HttpGet("student/{studentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGradesByStudent(int studentId)
        {
            if (!AuthHelper.IsAuthenticated(HttpContext, out int userId))
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = "GetGradesByStudent",
                    EntityType = "Grade",
                    EntityId = studentId,
                    Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in GetGradesByStudent" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);

                return Unauthorized(new { Success = false, Message = "Unauthorized" });
            }

            try
            {
                var grades = await _gradeService.GetGradesForStudentAsync(studentId);

                if (grades == null)
                {
                    return NotFound(new { Success = false, Message = "No grades found." });
                }

                return Ok(grades);
            }
            catch (Exception ex)
            {
                var auditLogError = new AuditLog
                {
                    UserId = userId,
                    Action = "GetGradesByStudent",
                    EntityType = "Grade",
                    EntityId = studentId,
                    Details = JsonSerializer.Serialize(new { message = ex.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };

                GradeLogger.Instance.LogError(auditLogError);

                return BadRequest(new { Success = false, Message = "An error occurred while fetching grades." });
            }
        }

        // GET: api/Grade/class/{classId}/student/{studentId}
        [HttpGet("class/{classId}/student/{studentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGradesByClassAndStudent(int classId, int studentId)
        {
            if (!AuthHelper.IsAuthenticated(HttpContext, out int userId))
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = "GetGradesByClassAndStudent",
                    EntityType = "Grade",
                    EntityId = classId,
                    Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in GetGradesByClassAndStudent" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);

                return Unauthorized(new { Success = false, Message = "Unauthorized" });
            }

            try
            {
                var grades = await _gradeService.GetGradesForAClassAndStudentAsync(classId, studentId);

                if (grades == null)
                {
                    return NotFound(new { Success = false, Message = "No grades found." });
                }

                return Ok(grades);
            }
            catch (Exception ex)
            {
                var auditLogError = new AuditLog
                {
                    UserId = userId,
                    Action = "GetGradesByClassAndStudent",
                    EntityType = "Grade",
                    EntityId = classId,
                    Details = JsonSerializer.Serialize(new { message = ex.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };

                GradeLogger.Instance.LogError(auditLogError);

                return Unauthorized(new { Success = false, Message = "An error occurred while fetching grades." });
            }
        }

        // GET: api/Grade/assignment/{assignmentId}
        [HttpGet("assignment/{assignmentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGradesByAssignment(int assignmentId)
        {
            if (!AuthHelper.IsAuthenticated(HttpContext, out int userId))
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = "GetGradesByAssignment",
                    EntityType = "Grade",
                    EntityId = assignmentId,
                    Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in GetGradesByAssignment" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);

                return Unauthorized(new { Success = false, Message = "Unauthorized" });
            }

            try
            {
                var grades = await _gradeService.GetGradesForAssignmentAsync(assignmentId);

                if (grades == null)
                {
                    return NotFound(new { Success = false, Message = "No grades found." });
                }

                return Ok(grades);
            }
            catch (Exception ex)
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = "GetGradesByAssignment",
                    EntityType = "Grade",
                    EntityId = assignmentId,
                    Details = JsonSerializer.Serialize(new { message = ex.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };

                GradeLogger.Instance.LogError(auditLog);

                return BadRequest(new { Success = false, Message = "An error occurred while fetching grades." });
            }
        }

        // GET: api/Grade/assignment/{assignmentId}/student/{studentId}
        [HttpGet("assignment/{assignmentId}/student/{studentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGradesByAssignmentAndStudent(int assignmentId, int studentId)
        {
            if (!AuthHelper.IsAuthenticated(HttpContext, out int userId))
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = "GetGradesByAssignmentAndStudent",
                    EntityType = "Grade",
                    EntityId = assignmentId,
                    Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in GetGradesByAssignmentAndStudent" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);

                return Unauthorized(new { Success = false, Message = "Unauthorized" });
            }

            try
            {
                var grades = await _gradeService.GetGradesForAssignemntAndStudentAsync(assignmentId, studentId);
                return Ok(grades);
            }
            catch (Exception ex)
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = "GetGradesByAssignmentAndStudent",
                    EntityType = "Grade",
                    EntityId = assignmentId,
                    Details = JsonSerializer.Serialize(new { message = ex.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };

                GradeLogger.Instance.LogError(auditLog);

                return BadRequest(new { Success = false, Message = "An error occurred while fetching grades." });
            }

        }

        // PUT: api/Grade/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateGrade(int id, [FromBody] UpdateGradeRequest request)
        {
            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "UpdateGrade",
                    EntityType = "Grade",
                    EntityId = id,
                    Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in UpdateGrade" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);

                return Unauthorized(new { Success = false, Message = "Unauthorized" });
            }

            if (!ModelState.IsValid)
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "UpdateGrade",
                    EntityType = "Grade",
                    EntityId = id,
                    Details = JsonSerializer.Serialize(new { message = "Invalid input data for UpdateGrade", errors = ModelState }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);

                return BadRequest(new { Success = false, Message = ModelState });
            }

            var grade = new Grade
            {
                GradeId = id,
                AssignmentId = request.AssignmentId,
                StudentId = request.StudentId,
                Points = request.Points,
                Comment = request.Comment,
                GradedBy = teacherId,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                var result = await _gradeService.UpdateGradeAsync(grade);

                if (!result)
                {
                    var auditLog = new AuditLog
                    {
                        UserId = teacherId,
                        Action = "UpdateGrade",
                        EntityType = "Grade",
                        EntityId = id,
                        Details = JsonSerializer.Serialize(new { message = "Grade not found for update" }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogError(auditLog);

                    return NotFound(new { Success = false, Message = "Grade not found." });
                }

                var successLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "UpdateGrade",
                    EntityType = "Grade",
                    EntityId = id,
                    Details = JsonSerializer.Serialize(new { message = "Grade updated successfully" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogMessage(successLog);

                return Ok(result);
            }
            catch (Exception ex)
            {
                var errorLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "UpdateGrade",
                    EntityType = "Grade",
                    EntityId = id,
                    Details = JsonSerializer.Serialize(new { message = "Error updating grade", exception = ex.Message, innerException = ex.InnerException?.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(errorLog);

                return BadRequest(new { Success = false, Message = "An error occurred while updating the grade." });
            }
        }

        // DELETE: api/Grade/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "DeleteGrade",
                    EntityType = "Grade",
                    EntityId = id,
                    Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in DeleteGrade" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);

                return Unauthorized(new { Success = false, Message = "Unauthorized" });
            }

            try
            {
                var result = await _gradeService.DeleteGradeAsync(id);
                if (!result)
                {
                    var auditLog = new AuditLog
                    {
                        UserId = teacherId,
                        Action = "DeleteGrade",
                        EntityType = "Grade",
                        EntityId = id,
                        Details = JsonSerializer.Serialize(new { message = "Grade not found for deletion" }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogError(auditLog);

                    return NotFound(new { Success = false, Message = "Grade not found." });
                }

                var successLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "DeleteGrade",
                    EntityType = "Grade",
                    EntityId = id,
                    Details = JsonSerializer.Serialize(new { message = "Grade deleted successfully" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogMessage(successLog);

                return Ok("Grade deleted successfully.");
            }
            catch (Exception ex)
            {
                var errorLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "DeleteGrade",
                    EntityType = "Grade",
                    EntityId = id,
                    Details = JsonSerializer.Serialize(new { message = "Error deleting grade", exception = ex.Message, innerException = ex.InnerException?.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(errorLog);

                return BadRequest(new { Success = false, Message = "An error occurred while deleting the grade." });
            }
        }
    }
}
