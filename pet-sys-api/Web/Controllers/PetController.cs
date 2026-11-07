using Application.Interfaces;
using Application.Models;
using Domain.Common;
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
    public class PetController : ControllerBase
    {
        private readonly IPetServices _petServices;
        public PetController(IPetServices petServices)
        {
            _petServices = petServices;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pet = await _petServices.GetPetByIdAsync(id);
            if (!ResourceAccess.CanReadPet(User, pet))
            {
                return NotFound();
            }
            return Ok(pet);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpPost("")]
        public async Task<IActionResult> AddPet([FromBody] CreateEditPetDTO dto)
        {
            var created = await _petServices.AddPetAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePet(int id, [FromBody] CreateEditPetDTO dto)
        {
            var updated = await _petServices.UpdatePetAsync(id, dto);
            return Ok(updated);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpGet("client/{clientId:int}")]
        public async Task<IActionResult> GetByClientId(int clientId)
        {
            var pets = await _petServices.GetPetsByClientAsync(clientId);
            return Ok(pets);
        }

        [Authorize(Policy = Policies.ClientOnly)]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyPets()
        {
            var clientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var pets = await _petServices.GetPetsByClientAsync(clientId);
            return Ok(pets);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SuspendPet(int id)
        {
            await _petServices.SuspendPetAsync(id);
            return NoContent();
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpPost("{id:int}/reactivate")]
        public async Task<IActionResult> ReactivatePet(int id)
        {
            await _petServices.ReactivatePetAsync(id);
            return NoContent();
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpGet]
        public async Task<IActionResult> GetPetsByPagination([FromQuery] PetQuery query)
        {
            var pets = await _petServices.GetPetsPagedAsync(query);
            return Ok(pets);
        }
    }
}
