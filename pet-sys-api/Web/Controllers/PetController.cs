using Application.Interfaces;
using Application.Models;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pet = await _petServices.GetPetByIdAsync(id);
            return Ok(pet);
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpPost("")]
        public async Task<IActionResult> AddPet([FromBody] CreateEditPetDTO dto)
        {
            var created = await _petServices.AddPetAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePet(int id, [FromBody] CreateEditPetDTO dto)
        {
            var updated = await _petServices.UpdatePetAsync(id, dto);
            return Ok(updated);
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpGet("client/{clientId:int}")]
        public async Task<IActionResult> GetByClientId(int clientId)
        {
            var pets = await _petServices.GetPetsByClientAsync(clientId);
            return Ok(pets);
        }

        [Authorize(Roles = "Client")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyPets()
        {
            var clientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var pets = await _petServices.GetPetsByClientAsync(clientId);
            return Ok(pets);
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpGet]
        public async Task<IActionResult> GetPetsByPagination([FromQuery] PetQuery query)
        {
            var pets = await _petServices.GetPetsPagedAsync(query);
            return Ok(pets);
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpPut("{id:int}/image")]
        public async Task<IActionResult> UploadImage(int id, IFormFile file, CancellationToken cancellationToken)
        {
            using var stream = file.OpenReadStream();

            await _petServices.UploadImageAsync(id, stream, file.ContentType, cancellationToken);

            return NoContent();
        }

        [Authorize(Roles = "SysAdmin,Admin,Veterinarian")]
        [HttpDelete("{id:int}/image")]
        public async Task<IActionResult> DeleteImage(int id, CancellationToken cancellationToken)
        {
            await _petServices.DeleteImageAsync(id, cancellationToken);

            return NoContent();
        }
    }
}
