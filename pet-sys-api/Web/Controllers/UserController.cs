using Application.Interfaces;
using Application.Models;
using Domain.Entities;
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
        private readonly IAuthorizationService _authorizationService;
        public UserController(IUserServices userServices, IAuthorizationService authorizationService)
        {
            _userServices = userServices;
            _authorizationService = authorizationService;
        }

        private int CallerId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var user = await _userServices.GetUserByIdAsync(CallerId);
            return Ok(user);
        }

        [Authorize(Policy = Policies.UsersRead)]
        [HttpGet("")]
        public async Task<IActionResult> GetAllUser([FromQuery] string? role)
        {
            var readableRoles = UserAccountAccess.GetManageableRoles(User);
            if (role != null)
            {
                readableRoles = readableRoles
                    .Where(allowedRole => allowedRole == role)
                    .ToArray();
            }
            var users = await _userServices.GetAllUserAsync(readableRoles);
            return Ok(users);
        }

        [Authorize(Policy = Policies.UsersRead)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userServices.GetUserByIdAsync(id);
            var authorizationResult =
                await _authorizationService.AuthorizeAsync(
                    User,
                    resource: user,
                    policyName: Policies.UsersReadAccount);
            if (!authorizationResult.Succeeded)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [Authorize(Policy = Policies.UsersCreate)]
        [HttpPost("")]
        public async Task<IActionResult> AddUser([FromBody] CreateUserDTO dto)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                resource: dto.Role,
                policyName: Policies.UsersCreateWithRole);

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var created = await _userServices.AddUserAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Policy = Policies.UsersUpdate)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO dto)
        {
            var user = await _userServices.GetUserByIdAsync(id);
            var authorizationResult =
                await _authorizationService.AuthorizeAsync(
                    User,
                    resource: user,
                    policyName: Policies.UsersUpdateAccount);
            if (!authorizationResult.Succeeded)
            {
                return NotFound();
            }
            var updated = await _userServices.UpdateUserAsync(id, dto);
            return Ok(updated);
        }

        [Authorize(Policy = Policies.UsersSuspend)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userServices.GetUserByIdAsync(id);
            var authorizationResult =
                await _authorizationService.AuthorizeAsync(
                    User,
                    resource: user,
                    policyName: Policies.UsersSuspendAccount);
            if (!authorizationResult.Succeeded)
            {
                return NotFound();
            }
            await _userServices.DeleteUserAsync(id);
            return NoContent();
        }

        [Authorize(Policy = Policies.UsersReactivate)]
        [HttpPost("{id:int}/reactivate")]
        public async Task<IActionResult> ReactivateUser(int id)
        {
            var user = await _userServices.GetUserByIdAsync(id);
            var authorizationResult =
                await _authorizationService.AuthorizeAsync(
                    User,
                    resource: user,
                    policyName: Policies.UsersReactivateAccount);
            if (!authorizationResult.Succeeded)
            {
                return NotFound();
            }
            await _userServices.ReactivateUserAsync(id);
            return NoContent();
        }

        [Authorize(Policy = Policies.UsersAssignRole)]
        [HttpPut("{id:int}/role")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateUserRoleDTO dto)
        {
            await _userServices.UpdateUserRoleAsync(id, dto);
            return NoContent();
        }
    }
}
