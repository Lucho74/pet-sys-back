using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IUserServices _userServices;

        public AuthenticationController(IConfiguration config, IUserServices userServices)
        {
            _config = config;
            _userServices = userServices;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] LoginDTO credentials)
        {
            var user = await _userServices.AuthenticateAsync(credentials);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Authentication:SecretForKey"]!));
            var signature = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claimsForToken = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
            };

            claimsForToken.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var expirationMinutes =
                _config.GetValue<int?>("Authentication:AccessTokenExpirationMinutes")
                ?? throw new InvalidOperationException("Access token expiration has not been configured.");

            if (expirationMinutes <= 0)
            {
                throw new InvalidOperationException("Access token expiration must be greater than zero.");
            }

            var now = DateTime.UtcNow;

            var jwtSecurityToken = new JwtSecurityToken(
                _config["Authentication:Issuer"],
                _config["Authentication:Audience"],
                 claimsForToken,
                 now,
                now.AddMinutes(expirationMinutes),
                 signature
            );

            var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            return Ok(tokenToReturn);
        }
    }
}
