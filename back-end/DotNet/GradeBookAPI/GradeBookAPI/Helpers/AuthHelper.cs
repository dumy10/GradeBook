using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
                IsTokenValid(httpContext);
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

        private static bool IsTokenValid(HttpContext httpContext)
        {
            var token = GetTokenFromHeader(httpContext.Request);
            if (string.IsNullOrEmpty(token))
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, // Verify the signature key
                IssuerSigningKey = new SymmetricSecurityKey(key), // Set the key
                ValidateIssuer = true, // Validate the issuer
                ValidateAudience = true, // Validate the audience
                ValidateLifetime = true, // Validate the token expiration
                ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"), // Set the issuer
                ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") // Set the audience
            };

            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out _);
                return true; // Token is valid
            }
            catch
            {
                return false; // Token is not valid
            }
        }

        private static string? GetTokenFromHeader(HttpRequest request)
        {
            var authHeader = request.Headers.Authorization.ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return null;

            return authHeader["Bearer ".Length..].Trim();
        }
    }
}
