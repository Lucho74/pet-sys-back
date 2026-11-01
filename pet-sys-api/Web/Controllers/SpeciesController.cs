using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpeciesController : ControllerBase
    {
        private readonly ISpeciesServices _speciesServices;
        public SpeciesController(ISpeciesServices speciesServices)
        {
            _speciesServices = speciesServices;
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpGet("")]
        public async Task<IActionResult> GetAllSpecies()
        {
            var species = await _speciesServices.GetAllSpeciesAsync();
            return Ok(species);
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var species = await _speciesServices.GetSpeciesByIdAsync(id);
            return Ok(species);
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpPost("")]
        public async Task<IActionResult> AddSpecies([FromBody] CreateEditSpeciesDTO dto)
        {
            var created = await _speciesServices.AddSpeciesAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSpecies(int id, [FromBody] CreateEditSpeciesDTO dto)
        {
            var updated = await _speciesServices.UpdateSpeciesAsync(id, dto);
            return Ok(updated);
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSpecies(int id)
        {
            await _speciesServices.DeleteSpeciesAsync(id);
            return NoContent();
        }
    }
}
