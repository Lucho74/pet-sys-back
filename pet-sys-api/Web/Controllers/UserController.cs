using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userServices;
        public UserController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        private int CallerId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string CallerRole => User.FindFirstValue(ClaimTypes.Role)!;

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var user = await _userServices.GetUserByIdAsync(CallerId, CallerId, CallerRole);
            return Ok(user);
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpGet("")]
        public async Task<IActionResult> GetAllUser()
        {
            var users = await _userServices.GetAllUserAsync(CallerId, CallerRole);
            return Ok(users);
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userServices.GetUserByIdAsync(id, CallerId, CallerRole);
            return Ok(user);
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpPost("")]
        public async Task<IActionResult> AddUser([FromBody] CreateUserDTO dto)
        {
            var created = await _userServices.AddUserAsync(dto, CallerRole);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDTO dto)
        {
            var updated = await _userServices.UpdateUserAsync(id, dto, CallerId, CallerRole);
            return Ok(updated);
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _userServices.DeleteUserAsync(id, CallerId, CallerRole);
            return NoContent();
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpPost("{id:int}/reactivate")]
        public async Task<IActionResult> ReactivateUser(int id)
        {
            await _userServices.ReactivateUserAsync(id, CallerId, CallerRole);
            return NoContent();
        }
    }
}
