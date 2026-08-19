using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Application.Models;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PetController : ControllerBase
    {
        private readonly IPetServices _petServices;
        public PetController(IPetServices petServices)
        {
            _petServices = petServices;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllPet()
        {
            var pets = await _petServices.GetAllPetAsync();
            return Ok(pets);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pet = await _petServices.GetPetByIdAsync(id);
            return Ok(pet);
        }

        [HttpPost("")]
        public async Task<IActionResult> AddPet([FromBody] CreatePetDTO dto)
        {
            var created = await _petServices.AddPetAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePet(int id, [FromBody] PetDTO dto)
        {
            var updated = await _petServices.UpdatePetAsync(id, dto);
            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePet(int id)
        {
            await _petServices.DeletePetAsync(id);
            return NoContent();
        }
    }
}
