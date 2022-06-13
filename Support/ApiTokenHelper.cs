mayusing Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace vMotion.Api.Specs.Unit_Tests
{
    [Trait(Constants.Category, Constants.Integration)]
    public class GenerateJwtToken
    {
        private readonly ITestOutputHelper _output;

        public GenerateJwtToken(ITestOutputHelper output)
        {
            _output = output;
        }

        [RunnableInDebugOnly]
        public void GetTokenTest()
        {
            var result = ApiTokenHelper.CreateToken(new[] { new Claim("sub", "bobby") });

            _output.WriteLine(result);
        }
    }

    public static class ApiTokenHelper
    {
        public static string CreateToken(Claim[] claims)
        {
            var key = Encoding.ASCII.GetBytes(AuthorizationConstants.JWT_SECRET_KEY);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims.ToArray()),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = AuthorizationConstants.ISSUER,
                Audience = AuthorizationConstants.AUDIENCE,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class AuthorizationConstants
    {
        public const string AUTHORITY = "http://localhost/";
        public const string ISSUER = "http://localhost/";

        public const string AUDIENCE = "SomeAudience";

        // TODO: Change this to an environment variable
        public const string JWT_SECRET_KEY = "SecretKeyOfDoomThatMustBeAMinimumNumberOfBytes";
    }
}