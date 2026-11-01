using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BreedController : ControllerBase
    {
        private readonly IBreedServices _breedServices;
        public BreedController(IBreedServices breedServices)
        {
            _breedServices = breedServices;
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpGet("")]
        public async Task<IActionResult> GetAllBreed()
        {
            var breeds = await _breedServices.GetAllBreedAsync();
            return Ok(breeds);
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var breed = await _breedServices.GetBreedByIdAsync(id);
            return Ok(breed);
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpGet("species/{speciesId:int}")]
        public async Task<IActionResult> GetBySpeciesId(int speciesId)
        {
            var breeds = await _breedServices.GetBreedsBySpeciesIdAsync(speciesId);
            return Ok(breeds);
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpPost("")]
        public async Task<IActionResult> AddBreed([FromBody] CreateEditBreedDTO dto)
        {
            var created = await _breedServices.AddBreedAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateBreed(int id, [FromBody] CreateEditBreedDTO dto)
        {
            var updated = await _breedServices.UpdateBreedAsync(id, dto);
            return Ok(updated);
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBreed(int id)
        {
            await _breedServices.DeleteBreedAsync(id);
            return NoContent();
        }
    }
}
