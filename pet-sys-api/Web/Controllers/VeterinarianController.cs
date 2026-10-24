using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Controllers
{
    [Authorize(Roles = "Veterinarian")]
    [Route("api/[controller]")]
    [ApiController]
    public class VeterinarianController : ControllerBase
    {
        private readonly IVeterinarianServices _veterinarianServices;

        public VeterinarianController(IVeterinarianServices veterinarianServices)
        {
            _veterinarianServices = veterinarianServices;
        }

        [HttpGet("me/work-shifts")]
        public async Task<IActionResult> GetMyWorkShifts()
        {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var shifts = await _veterinarianServices.GetWorkShiftsAsync(id);
            return Ok(shifts);
        }

        [HttpPut("me/work-shifts")]
        public async Task<IActionResult> UpdateMyWorkShifts([FromBody] List<WorkShiftDTO> shifts)
        {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var updated = await _veterinarianServices.UpdateWorkShiftsAsync(id, shifts);
            return Ok(updated);
        }
    }
}
