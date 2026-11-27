using Application.Interfaces;
using Application.Models;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Authorization;

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

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var user = await _userServices.GetCurrentUserAsync();
            return Ok(user);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userServices.GetUserByIdAsync(id);
            return Ok(user);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpPost("")]
        public async Task<IActionResult> AddUser([FromBody] CreateUserDTO dto)
        {
            var created = await _userServices.AddUserAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO dto)
        {
            var updated = await _userServices.UpdateUserAsync(id, dto);
            return Ok(updated);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _userServices.DeleteUserAsync(id);
            return NoContent();
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpPost("{id:int}/reactivate")]
        public async Task<IActionResult> ReactivateUser(int id)
        {
            await _userServices.ReactivateUserAsync(id);
            return NoContent();
        }

        [Authorize(Policy = Policies.SysAdminOnly)]
        [HttpPut("{id:int}/role")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateUserRoleDTO dto)
        {
            await _userServices.UpdateUserRoleAsync(id, dto);
            return NoContent();
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpGet]
        public async Task<IActionResult> GetUsersByPagination([FromQuery] UserQuery query)
        {
            var users = await _userServices.GetUsersPagedAsync(query);

            return Ok(users);
        }
    }
}
