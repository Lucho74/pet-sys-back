using Application.Interfaces;
using Application.Models;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Authorization;

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

        [Authorize(Policy = Policies.Staff)]
        [HttpGet]
        public async Task<IActionResult> GetBreedsByPagination([FromQuery] BreedQuery query)
        {
            var breeds = await _breedServices.GetBreedsPagedAsync(query);
            return Ok(breeds);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var breed = await _breedServices.GetBreedByIdAsync(id);
            return Ok(breed);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpGet("species/{speciesId:int}")]
        public async Task<IActionResult> GetBySpeciesId(int speciesId)
        {
            var breeds = await _breedServices.GetBreedsBySpeciesIdAsync(speciesId);
            return Ok(breeds);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpPost("")]
        public async Task<IActionResult> AddBreed([FromBody] CreateBreedDTO dto)
        {
            var created = await _breedServices.AddBreedAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateBreed(int id, [FromBody] UpdateBreedDTO dto)
        {
            var updated = await _breedServices.UpdateBreedAsync(id, dto);
            return Ok(updated);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBreed(int id)
        {
            await _breedServices.DeleteBreedAsync(id);
            return NoContent();
        }
    }
}
