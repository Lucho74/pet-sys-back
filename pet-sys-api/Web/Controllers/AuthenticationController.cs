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
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.RoleName)
            };

            var jwtSecurityToken = new JwtSecurityToken(
                _config["Authentication:Issuer"],
                _config["Authentication:Audience"],
                claimsForToken,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1), // TODO: variar según user.RoleName (ej: menor duración para Admin) — no implementado todavía, ver sección Auth del CLAUDE.md
                signature
            );

            var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            return Ok(tokenToReturn);
        }
    }
}
