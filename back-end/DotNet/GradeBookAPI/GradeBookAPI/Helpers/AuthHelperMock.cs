using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GradeBookAPITests.Helpers
{
    /// <summary>
    /// Helper class for tests that need to bypass AuthHelper validation
    /// </summary>
    public static class AuthHelperMock
    {
        /// <summary>
        /// Sets up environment variables needed for AuthHelper token validation
        /// </summary>
        public static void SetupEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("JWT_SECRET", "testsecretkeythatisatleast32byteslong");
            Environment.SetEnvironmentVariable("JWT_ISSUER", "test-issuer");
            Environment.SetEnvironmentVariable("JWT_AUDIENCE", "test-audience");
            Environment.SetEnvironmentVariable("JWT_EXPIRY_HOURS", "1");
        }

        /// <summary>
        /// Sets up HttpContext to have valid authentication for tests
        /// </summary>
        public static void SetupHttpContext(HttpContext httpContext, int userId, string role)
        {
            // Create identity with authentication type to make IsAuthenticated return true
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, $"user{userId}@example.com")
            };

            var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuthentication");
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(identity);

            // Add a mock Bearer token to the Authorization header
            string mockToken = GenerateMockJwtToken(userId, role);
            httpContext.Request.Headers["Authorization"] = $"Bearer {mockToken}";
        }

        /// <summary>
        /// Generates a mock JWT token for testing
        /// </summary>
        private static string GenerateMockJwtToken(int userId, string role)
        {
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? "testsecretkeythatisatleast32byteslong");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, $"user{userId}@example.com")
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "test-issuer",
                Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "test-audience"
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}