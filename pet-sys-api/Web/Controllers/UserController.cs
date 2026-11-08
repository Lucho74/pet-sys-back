using Application.Interfaces;
using Application.Models;
using Domain.Common;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Web.Authorization;
using Web.Authorization.Rules;

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

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var user = await _userServices.GetUserByIdAsync(CallerId);
            return Ok(user);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userServices.GetUserByIdAsync(id);
            if (!ResourceAccess.CanManageAccount(User, user.RoleName))
            {
                return NotFound();
            }
            return Ok(user);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpPost("")]
        public async Task<IActionResult> AddUser([FromBody] CreateUserDTO dto)
        {
            if (!ResourceAccess.CanManageAccount(User, dto.Role))
            {
                return Forbid();
            }

            var created = await _userServices.AddUserAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO dto)
        {
            var user = await _userServices.GetUserByIdAsync(id);
            if (!ResourceAccess.CanUpdateAccount(User, user))
            {
                return NotFound();
            }
            if (dto.RegistrationNumber != null &&
                !ResourceAccess.CanUpdateRegistrationNumber(User))
            {
                return Forbid();
            }
            var updated = await _userServices.UpdateUserAsync(id, dto);
            return Ok(updated);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userServices.GetUserByIdAsync(id);
            if (!ResourceAccess.CanManageAccount(User, user.RoleName))
            {
                return NotFound();
            }
            await _userServices.DeleteUserAsync(id);
            return NoContent();
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpPost("{id:int}/reactivate")]
        public async Task<IActionResult> ReactivateUser(int id)
        {
            var user = await _userServices.GetUserByIdAsync(id);
            if (!ResourceAccess.CanManageAccount(User, user.RoleName))
            {
                return NotFound();
            }
            await _userServices.ReactivateUserAsync(id);
            return NoContent();
        }

        [Authorize(Policy = Policies.SysAdminOnly)]
        [HttpPut("{id:int}/role")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateUserRoleDTO dto)
        {
            var user = await _userServices.GetUserByIdAsync(id);
            if (user.RoleName == RoleNames.SysAdmin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { title = "The SysAdmin account roles cannot be modified." });
            }
            await _userServices.UpdateUserRoleAsync(id, dto);
            return NoContent();
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpGet]
        public async Task<IActionResult> GetUsersByPagination([FromQuery] UserQuery query)
        {
            var readableRoles = ResourceAccess.GetManageableRoles(User);

            var users = await _userServices.GetUsersPagedAsync(query, readableRoles);

            return Ok(users);
        }
    }
}
