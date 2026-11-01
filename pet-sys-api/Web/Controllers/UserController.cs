using Application.Interfaces;
using Application.Models;
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
        public async Task<IActionResult> GetAllUser()
        {
            var readableRoles = UserAccountAccess.GetReadableRoles(User);
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

        [Authorize(Policy = Policies.ClientProfilesRead)]
        [HttpGet("client-profiles")]
        public async Task<IActionResult> GetClientProfiles()
        {
            var profiles = await _userServices.GetClientProfilesAsync();

            return Ok(profiles);
        }

        [Authorize(Policy = Policies.ClientProfilesRead)]
        [HttpGet("{id:int}/client-profile")]
        public async Task<IActionResult> GetClientProfile(int id)
        {
            var profile = await _userServices.GetClientProfileByUserIdAsync(id);

            return Ok(profile);
        }

        [Authorize(Policy = Policies.UsersCreate)]
        [HttpPost("")]
        public async Task<IActionResult> AddUser([FromBody] CreateUserDTO dto)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                resource: dto.Roles,
                policyName: Policies.UsersCreateWithRoles);

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
            var updated = await _userServices.UpdateUserAsync(id, dto);
            return Ok(updated);
        }

        [Authorize(Policy = Policies.UsersSuspend)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _userServices.DeleteUserAsync(id);
            return NoContent();
        }

        [Authorize(Policy = Policies.UsersReactivate)]
        [HttpPost("{id:int}/reactivate")]
        public async Task<IActionResult> ReactivateUser(int id)
        {
            await _userServices.ReactivateUserAsync(id);
            return NoContent();
        }

        [Authorize(Policy = Policies.UsersAssignRoles)]
        [HttpPut("{id:int}/roles")]
        public async Task<IActionResult> UpdateRoles(int id, [FromBody] UpdateUserRolesDTO dto)
        {
            await _userServices.UpdateUserRolesAsync(id, dto);
            return NoContent();
        }
    }
}
