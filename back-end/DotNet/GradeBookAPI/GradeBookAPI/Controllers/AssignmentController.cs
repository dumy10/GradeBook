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
    public class AssignmentController(IAssignmentService assignmentService) : ControllerBase
    {
        private readonly IAssignmentService _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));

        // POST: api/assignment/create
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentRequest request)
        {
            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "CreateAssignment",
                    EntityType = "Assignment",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in CreateAssignment" }),
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
                    Action = "CreateAssignment",
                    EntityType = "Assignment",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Invalid input in CreateAssignment" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };

                GradeLogger.Instance.LogWarning(auditLog);

                return BadRequest(new { Success = false, Message = ModelState });
            }

            try
            {
                var assignment = await _assignmentService.CreateAssignmentAsync(request);

                var auditLogSuccess = new AuditLog
                {
                    UserId = teacherId,
                    Action = "CreateAssignment",
                    EntityType = "Assignment",
                    EntityId = assignment.AssignmentId,
                    Details = JsonSerializer.Serialize(new { message = "Assignment created successfully" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };

                GradeLogger.Instance.LogMessage(auditLogSuccess);

                return Ok(assignment);
            }
            catch (Exception ex)
            {
                var auditLogError = new AuditLog
                {
                    UserId = teacherId,
                    Action = "CreateAssignment",
                    EntityType = "Assignment",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = ex.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };

                GradeLogger.Instance.LogError(auditLogError);

                return BadRequest(new { Success = false, Message = "An error occurred while creating the assignment." });
            }
        }

        // GET: api/assignments
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAssignments()
        {
            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "GetAssignments",
                    EntityType = "Assignment",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in GetAssignments" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);

                return Unauthorized(new { Success = false, Message = "Unauthorized" });
            }
            var assignments = await _assignmentService.GetAllAssignmentsAsync();

            if (assignments == null)
            {
                var auditLogError = new AuditLog
                {
                    UserId = teacherId,
                    Action = "GetAssignments",
                    EntityType = "Assignment",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "No assignments found." }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLogError);

                return NotFound(new { Success = false, Message = "No assignments found." });
            }

            var auditLogSuccess = new AuditLog
            {
                UserId = teacherId,
                Action = "GetAssignments",
                EntityType = "Assignment",
                EntityId = 0,
                Details = JsonSerializer.Serialize(new { message = "Assignments retrieved successfully" }),
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                CreatedAt = DateTime.UtcNow
            };
            GradeLogger.Instance.LogMessage(auditLogSuccess);

            return Ok(assignments);
        }

        // GET: api/assignments/class/{classId}
        [HttpGet("assignments/class/{classId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAssignmentsForClass(int classId)
        {
            if (!AuthHelper.IsAuthenticated(HttpContext, out int userId))
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = "GetAssignmentsForClass",
                    EntityType = "Assignment",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in GetAssignmentsForClass" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);

                return Unauthorized(new { Success = false, Message = "Unauthorized" });
            }

            try
            {
                var assignments = await _assignmentService.GetAssignmentsForClassAsync(classId);

                if (assignments == null)
                {
                    var auditLogError = new AuditLog
                    {
                        UserId = userId,
                        Action = "GetAssignmentsForClass",
                        EntityType = "Assignment",
                        EntityId = 0,
                        Details = JsonSerializer.Serialize(new { message = "No assignments found." }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogError(auditLogError);

                    return NotFound(new { Success = false, Message = "No assignments found." });
                }

                var auditLogSuccess = new AuditLog
                {
                    UserId = userId,
                    Action = "GetAssignmentsForClass",
                    EntityType = "Assignment",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = "Assignments retrieved successfully" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogMessage(auditLogSuccess);

                return Ok(assignments);
            }
            catch (Exception ex)
            {
                var auditLogError = new AuditLog
                {
                    UserId = userId,
                    Action = "GetAssignmentsForClass",
                    EntityType = "Assignment",
                    EntityId = 0,
                    Details = JsonSerializer.Serialize(new { message = ex.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLogError);

                return BadRequest(new { Success = false, Message = "An error occurred while retrieving assignments." });
            }
        }

        // PUT: api/assignment/update/{assignmentId}
        [HttpPut("assignment/update/{assignmentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAssignment(int assignmentId, [FromBody] CreateAssignmentRequest request)
        {
            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "UpdateAssignment",
                    EntityType = "Assignment",
                    EntityId = assignmentId,
                    Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in UpdateAssignment" }),
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
                    Action = "UpdateAssignment",
                    EntityType = "Assignment",
                    EntityId = assignmentId,
                    Details = JsonSerializer.Serialize(new { message = "Invalid input in UpdateAssignment" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogWarning(auditLog);
                return BadRequest(new { Success = false, Message = ModelState });
            }
            try
            {
                var assignment = await _assignmentService.UpdateAssignmentAsync(assignmentId, request);
                if (assignment == null)
                {
                    var auditLogError = new AuditLog
                    {
                        UserId = teacherId,
                        Action = "UpdateAssignment",
                        EntityType = "Assignment",
                        EntityId = assignmentId,
                        Details = JsonSerializer.Serialize(new { message = "Assignment not found." }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogError(auditLogError);
                    return NotFound(new { Success = false, Message = "Assignment not found." });
                }
                var auditLogSuccess = new AuditLog
                {
                    UserId = teacherId,
                    Action = "UpdateAssignment",
                    EntityType = "Assignment",
                    EntityId = assignmentId,
                    Details = JsonSerializer.Serialize(new { message = "Assignment updated successfully" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogMessage(auditLogSuccess);
                return Ok(assignment);
            }
            catch (Exception ex)
            {
                var auditLogError = new AuditLog
                {
                    UserId = teacherId,
                    Action = "Update Assignment",
                    EntityType = "Assignment",
                    EntityId = assignmentId,
                    Details = JsonSerializer.Serialize(new { message = ex.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLogError);
                return BadRequest(new { Success = false, Message = "An error occurred while updating the assignment." });
            }
        }

        // DELETE: api/assignment/delete/{assignmentId}
        [HttpDelete("assignment/delete/{assignmentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAssignment(int assignmentId)
        {
            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = new AuditLog
                {
                    UserId = teacherId,
                    Action = "DeleteAssignment",
                    EntityType = "Assignment",
                    EntityId = assignmentId,
                    Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in DeleteAssignment" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLog);
                return Unauthorized(new { Success = false, Message = "Unauthorized" });
            }
            try
            {
                var isDeleted = await _assignmentService.DeleteAssignmentAsync(assignmentId);
                if (!isDeleted)
                {
                    var auditLogError = new AuditLog
                    {
                        UserId = teacherId,
                        Action = "DeleteAssignment",
                        EntityType = "Assignment",
                        EntityId = assignmentId,
                        Details = JsonSerializer.Serialize(new { message = "Assignment not found." }),
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        CreatedAt = DateTime.UtcNow
                    };
                    GradeLogger.Instance.LogError(auditLogError);
                    return NotFound(new { Success = false, Message = "Assignment not found." });
                }
                var auditLogSuccess = new AuditLog
                {
                    UserId = teacherId,
                    Action = "DeleteAssignment",
                    EntityType = "Assignment",
                    EntityId = assignmentId,
                    Details = JsonSerializer.Serialize(new { message = "Assignment deleted successfully" }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogMessage(auditLogSuccess);
                return Ok(new { Success = true, Message = "Assignment deleted successfully" });
            }
            catch (Exception ex)
            {
                var auditLogError = new AuditLog
                {
                    UserId = teacherId,
                    Action = "DeleteAssignment",
                    EntityType = "Assignment",
                    EntityId = assignmentId,
                    Details = JsonSerializer.Serialize(new { message = ex.Message }),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                GradeLogger.Instance.LogError(auditLogError);
                return BadRequest(new { Success = false, Message = "An error occurred while deleting the assignment." });
            }
        }
    }
}