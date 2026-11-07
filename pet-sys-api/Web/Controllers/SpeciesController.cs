using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Authorization;

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

        [Authorize(Policy = Policies.Staff)]
        [HttpGet("")]
        public async Task<IActionResult> GetAllSpecies()
        {
            var species = await _speciesServices.GetAllSpeciesAsync();
            return Ok(species);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var species = await _speciesServices.GetSpeciesByIdAsync(id);
            return Ok(species);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpPost("")]
        public async Task<IActionResult> AddSpecies([FromBody] CreateEditSpeciesDTO dto)
        {
            var created = await _speciesServices.AddSpeciesAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSpecies(int id, [FromBody] CreateEditSpeciesDTO dto)
        {
            var updated = await _speciesServices.UpdateSpeciesAsync(id, dto);
            return Ok(updated);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSpecies(int id)
        {
            await _speciesServices.DeleteSpeciesAsync(id);
            return NoContent();
        }
    }
}
