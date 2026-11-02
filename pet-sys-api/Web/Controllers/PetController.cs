using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Web.Authorization;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PetController : ControllerBase
    {
        private readonly IPetServices _petServices;
        private readonly IAuthorizationService _authorizationService;
        public PetController(IPetServices petServices, IAuthorizationService authorizationService)
        {
            _petServices = petServices;
            _authorizationService = authorizationService;
        }

        [Authorize(Policy = Policies.PetsRead)]
        [HttpGet("")]
        public async Task<IActionResult> GetAllPet()
        {
            var pets = await _petServices.GetAllPetAsync();
            return Ok(pets);
        }

        [Authorize(Policy = Policies.PetsReadSingle)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pet = await _petServices.GetPetByIdAsync(id);
            var authorizationResult =
                await _authorizationService.AuthorizeAsync(
                    User,
                    resource: pet,
                    policyName: Policies.PetsReadRecord);
            if (!authorizationResult.Succeeded)
            {
                return NotFound();
            }
            return Ok(pet);
        }

        [Authorize(Policy = Policies.PetsCreate)]
        [HttpPost("")]
        public async Task<IActionResult> AddPet([FromBody] CreateEditPetDTO dto)
        {
            var created = await _petServices.AddPetAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Policy = Policies.PetsUpdate)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePet(int id, [FromBody] CreateEditPetDTO dto)
        {
            var updated = await _petServices.UpdatePetAsync(id, dto);
            return Ok(updated);
        }

        [Authorize(Policy = Policies.PetsRead)]
        [HttpGet("client/{clientId:int}")]
        public async Task<IActionResult> GetByClientId(int clientId)
        {
            var pets = await _petServices.GetPetsByClientAsync(clientId);
            return Ok(pets);
        }

        [Authorize(Policy = Policies.PetsReadOwn)]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyPets()
        {
            var clientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var pets = await _petServices.GetPetsByClientAsync(clientId);
            return Ok(pets);
        }

        [Authorize(Policy = Policies.PetsSuspend)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SuspendPet(int id)
        {
            await _petServices.SuspendPetAsync(id);
            return NoContent();
        }

        [Authorize(Policy = Policies.PetsReactivate)]
        [HttpPost("{id:int}/reactivate")]
        public async Task<IActionResult> ReactivatePet(int id)
        {
            await _petServices.ReactivatePetAsync(id);
            return NoContent();
        }
    }
}
