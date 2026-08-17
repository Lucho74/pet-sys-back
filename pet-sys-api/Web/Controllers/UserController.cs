using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Application.Models;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userServices;
        public UserController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllUser()
        {
            var users = await _userServices.GetAllUserAsync();
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userServices.GetUserByIdAsync(id);
            return Ok(user);
        }

        [HttpPost("")]
        public async Task<IActionResult> AddUser([FromBody] CreateUserDTO dto)
        {
            var created = await _userServices.AddUserAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDTO dto)
        {
            var updated = await _userServices.UpdateUserAsync(id, dto);
            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _userServices.DeleteUserAsync(id);
            return NoContent();
        }
    }
}
