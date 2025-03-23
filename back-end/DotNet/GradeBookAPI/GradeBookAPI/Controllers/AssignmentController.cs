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

        private AuditLog AuditLog => new()
        {
            UserId = 1,
            EntityType = "Assignment",
            EntityId = 0,
            Action = "AssignmentController",
            Details = JsonSerializer.Serialize(new { message = "AssignmentController initialized" }),
            IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            CreatedAt = DateTime.UtcNow
        };

        // POST: api/assignment/create
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentRequest request)
        {
            if (!AuthHelper.IsTeacher(HttpContext, out int teacherId))
            {
                var auditLog = AuditLog;
                auditLog.UserId = teacherId;
                auditLog.Action = "CreateAssignment";
                auditLog.Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in CreateAssignment" });
                auditLog.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(auditLog);

                return Unauthorized(new { Success = false, Message = "Unauthorized" });
            }

            if (!ModelState.IsValid)
            {
                var auditLog = AuditLog;
                auditLog.UserId = teacherId;
                auditLog.Action = "CreateAssignment";
                auditLog.Details = JsonSerializer.Serialize(new { message = "Invalid input in CreateAssignment" });
                auditLog.CreatedAt = DateTime.UtcNow;

                GradeLogger.Instance.LogWarning(auditLog);

                return BadRequest(new { Success = false, Message = ModelState });
            }

            try
            {
                var assignment = await _assignmentService.CreateAssignmentAsync(request);

                var auditLogSuccess = AuditLog;
                auditLogSuccess.UserId = teacherId;
                auditLogSuccess.Action = "CreateAssignment";
                auditLogSuccess.EntityId = assignment.AssignmentId;
                auditLogSuccess.Details = JsonSerializer.Serialize(new { message = "Assignment created successfully" });
                auditLogSuccess.CreatedAt = DateTime.UtcNow;

                GradeLogger.Instance.LogMessage(auditLogSuccess);

                return Ok(assignment);
            }
            catch (Exception ex)
            {
                var auditLogError = AuditLog;
                auditLogError.UserId = teacherId;
                auditLogError.Action = "CreateAssignment";
                auditLogError.Details = JsonSerializer.Serialize(new { message = ex.Message });
                auditLogError.CreatedAt = DateTime.UtcNow;

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
                var auditLog = AuditLog;
                auditLog.UserId = teacherId;
                auditLog.Action = "GetAssignments";
                auditLog.Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in GetAssignments" });
                auditLog.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(auditLog);

                return Unauthorized(new { Success = false, Message = "Unauthorized" });
            }
            var assignments = await _assignmentService.GetAllAssignmentsAsync();

            if (assignments == null)
            {
                var auditLogError = AuditLog;
                auditLogError.UserId = teacherId;
                auditLogError.Action = "GetAssignments";
                auditLogError.Details = JsonSerializer.Serialize(new { message = "No assignments found." });
                auditLogError.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(auditLogError);

                return NotFound(new { Success = false, Message = "No assignments found." });
            }

            var auditLogSuccess = AuditLog;
            auditLogSuccess.UserId = teacherId;
            auditLogSuccess.Action = "GetAssignments";
            auditLogSuccess.Details = JsonSerializer.Serialize(new { message = "Assignments retrieved successfully" });
            auditLogSuccess.CreatedAt = DateTime.UtcNow;
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
                var auditLog = AuditLog;
                auditLog.UserId = userId;
                auditLog.Action = "GetAssignmentsForClass";
                auditLog.Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in GetAssignmentsForClass" });
                auditLog.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(auditLog);

                return Unauthorized(new { Success = false, Message = "Unauthorized" });
            }

            try
            {
                var assignments = await _assignmentService.GetAssignmentsForClassAsync(classId);

                if (assignments == null)
                {
                    var auditLogError = AuditLog;
                    auditLogError.UserId = userId;
                    auditLogError.Action = "GetAssignmentsForClass";
                    auditLogError.Details = JsonSerializer.Serialize(new { message = "No assignments found." });
                    auditLogError.CreatedAt = DateTime.UtcNow;
                    GradeLogger.Instance.LogError(auditLogError);

                    return NotFound(new { Success = false, Message = "No assignments found." });
                }

                var auditLogSuccess = AuditLog;
                auditLogSuccess.UserId = userId;
                auditLogSuccess.Action = "GetAssignmentsForClass";
                auditLogSuccess.Details = JsonSerializer.Serialize(new { message = "Assignments retrieved successfully" });
                auditLogSuccess.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogMessage(auditLogSuccess);

                return Ok(assignments);
            }
            catch (Exception ex)
            {
                var auditLogError = AuditLog;
                auditLogError.UserId = userId;
                auditLogError.Action = "GetAssignmentsForClass";
                auditLogError.Details = JsonSerializer.Serialize(new { message = ex.Message });
                auditLogError.CreatedAt = DateTime.UtcNow;
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
                var auditLog = AuditLog;
                auditLog.UserId = teacherId;
                auditLog.Action = "UpdateAssignment";
                auditLog.EntityId = assignmentId;
                auditLog.Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in UpdateAssignment" });
                auditLog.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(auditLog);
                return Unauthorized(new { Success = false, Message = "Unauthorized" });
            }
            if (!ModelState.IsValid)
            {
                var auditLog = AuditLog;
                auditLog.UserId = teacherId;
                auditLog.Action = "UpdateAssignment";
                auditLog.EntityId = assignmentId;
                auditLog.Details = JsonSerializer.Serialize(new { message = "Invalid input in UpdateAssignment" });
                auditLog.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogWarning(auditLog);
                return BadRequest(new { Success = false, Message = ModelState });
            }
            try
            {
                var assignment = await _assignmentService.UpdateAssignmentAsync(assignmentId, request);
                if (assignment == null)
                {
                    var auditLogError = AuditLog;
                    auditLogError.UserId = teacherId;
                    auditLogError.Action = "UpdateAssignment";
                    auditLogError.EntityId = assignmentId;
                    auditLogError.Details = JsonSerializer.Serialize(new { message = "Assignment not found." });
                    auditLogError.CreatedAt = DateTime.UtcNow;
                    GradeLogger.Instance.LogError(auditLogError);
                    return NotFound(new { Success = false, Message = "Assignment not found." });
                }
                var auditLogSuccess = AuditLog;
                auditLogSuccess.UserId = teacherId;
                auditLogSuccess.Action = "UpdateAssignment";
                auditLogSuccess.EntityId = assignmentId;
                auditLogSuccess.Details = JsonSerializer.Serialize(new { message = "Assignment updated successfully" });
                auditLogSuccess.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogMessage(auditLogSuccess);
                return Ok(assignment);
            }
            catch (Exception ex)
            {
                var auditLogError = AuditLog;
                auditLogError.UserId = teacherId;
                auditLogError.Action = "Update Assignment";
                auditLogError.EntityId = assignmentId;
                auditLogError.Details = JsonSerializer.Serialize(new { message = ex.Message });
                auditLogError.CreatedAt = DateTime.UtcNow;
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
                var auditLog = AuditLog;
                auditLog.UserId = teacherId;
                auditLog.Action = "DeleteAssignment";
                auditLog.EntityId = assignmentId;
                auditLog.Details = JsonSerializer.Serialize(new { message = "Unauthorized access attempt in DeleteAssignment" });
                auditLog.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(auditLog);
                return Unauthorized(new { Success = false, Message = "Unauthorized" });
            }
            try
            {
                var isDeleted = await _assignmentService.DeleteAssignmentAsync(assignmentId);
                if (!isDeleted)
                {
                    var auditLogError = AuditLog;
                    auditLogError.UserId = teacherId;
                    auditLogError.Action = "DeleteAssignment";
                    auditLogError.EntityId = assignmentId;
                    auditLogError.Details = JsonSerializer.Serialize(new { message = "Assignment not found." });
                    auditLogError.CreatedAt = DateTime.UtcNow;
                    GradeLogger.Instance.LogError(auditLogError);
                    return NotFound(new { Success = false, Message = "Assignment not found." });
                }
                var auditLogSuccess = AuditLog;
                auditLogSuccess.UserId = teacherId;
                auditLogSuccess.Action = "DeleteAssignment";
                auditLogSuccess.EntityId = assignmentId;
                auditLogSuccess.Details = JsonSerializer.Serialize(new { message = "Assignment deleted successfully" });
                auditLogSuccess.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogMessage(auditLogSuccess);
                return Ok(new { Success = true, Message = "Assignment deleted successfully" });
            }
            catch (Exception ex)
            {
                var auditLogError = AuditLog;
                auditLogError.UserId = teacherId;
                auditLogError.Action = "DeleteAssignment";
                auditLogError.EntityId = assignmentId;
                auditLogError.Details = JsonSerializer.Serialize(new { message = ex.Message });
                auditLogError.CreatedAt = DateTime.UtcNow;
                GradeLogger.Instance.LogError(auditLogError);
                return BadRequest(new { Success = false, Message = "An error occurred while deleting the assignment." });
            }
        }
    }
}