using System.Security.Claims;

namespace GradeBookAPI.Helpers
{
    public static class AuthHelper
    {
        public enum Role
        {
            TEACHER,
            STUDENT
        }

        public static bool IsAuthenticated(HttpContext httpContext, out int userId)
        {
            return TryGetUserId(httpContext, out userId) && 
                !IsTokenExpired(httpContext.User) && 
                IsIssuerValid(httpContext.User) && 
                IsAudienceValid(httpContext.User);
        }

        public static bool IsTeacher(HttpContext httpContext, out int teacherId)
        {
            return IsAuthenticated(httpContext, out teacherId) && HasRole(httpContext.User, Role.TEACHER);
        }

        public static bool IsStudent(HttpContext httpContext, out int studentId)
        {
            return IsAuthenticated(httpContext, out studentId) && HasRole(httpContext.User, Role.STUDENT);
        }

        private static bool TryGetUserId(HttpContext httpContext, out int userId)
        {
            userId = 0;
            var user = httpContext.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
                return false;

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return !string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out userId);
        }

        private static bool HasRole(ClaimsPrincipal user, Role role)
        {
            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
            return string.Equals(roleClaim, role.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsIssuerValid(ClaimsPrincipal user)
        {
            var issuerClaim = user.FindFirst("iss")?.Value;
            var expectedIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
            return string.Equals(issuerClaim, expectedIssuer, StringComparison.Ordinal);
        }

        private static bool IsAudienceValid(ClaimsPrincipal user)
        {
            var audienceClaim = user.FindFirst("aud")?.Value;
            var expectedAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
            return string.Equals(audienceClaim, expectedAudience, StringComparison.Ordinal);
        }

        private static bool IsTokenExpired(ClaimsPrincipal user)
        {
            var expClaim = user.FindFirst("exp")?.Value;
            if (expClaim != null && long.TryParse(expClaim, out long expSeconds))
            {
                var expTime = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
                if (expTime < DateTime.UtcNow)
                    return true;
            }

            return false;
        }
    }
}
