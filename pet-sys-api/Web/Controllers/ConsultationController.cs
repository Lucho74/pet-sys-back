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
    public class ConsultationController : ControllerBase
    {
        private readonly IConsultationServices _consultationServices;
        private readonly IAuthorizationService _authorizationService;
        private readonly IPetServices _petServices;
        public ConsultationController(IConsultationServices consultationServices, IAuthorizationService authorizationService, IPetServices petServices)
        {
            _consultationServices = consultationServices;
            _authorizationService = authorizationService;
            _petServices = petServices;
        }

        [Authorize(Policy = Policies.ConsultationsRead)]
        [HttpGet("")]
        public async Task<IActionResult> GetAllConsultation()
        {
            var consultations = await _consultationServices.GetAllConsultationAsync();
            return Ok(consultations);
        }

        [Authorize(Policy = Policies.ConsultationsReadSingle)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var consultation = await _consultationServices.GetConsultationByIdAsync(id);
            var pet = await _petServices.GetPetByIdAsync(consultation.PetId);
            var authorizationResult =
                await _authorizationService.AuthorizeAsync(
                    User,
                    resource: pet,
                    policyName: Policies.PetsReadRecord);
            if (!authorizationResult.Succeeded)
            {
                return NotFound();
            }
            return Ok(consultation);
        }

        [Authorize(Policy = Policies.ConsultationsCreate)]
        [HttpPost("")]
        public async Task<IActionResult> AddConsultation([FromBody] CreateEditConsultationDTO dto)
        {
        int callerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var created = await _consultationServices.AddConsultationAsync(dto, callerId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Policy = Policies.ConsultationsUpdate)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateConsultation(int id, [FromBody] CreateEditConsultationDTO dto)
        {
            var consultation = await _consultationServices.GetConsultationByIdAsync(id);
            var authorizationResult =
                await _authorizationService.AuthorizeAsync(
                    User,
                    resource: consultation,
                    policyName: Policies.ConsultationsUpdateOwn);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var updated = await _consultationServices.UpdateConsultationAsync(id, dto);
            return Ok(updated);
        }

        [Authorize(Policy = Policies.ConsultationsReadByPet)]
        [HttpGet("pet/{petId:int}")]
        public async Task<IActionResult> GetByPetId(int petId)
        {
            var pet = await _petServices.GetPetByIdAsync(petId);
            var authorizationResult =
                await _authorizationService.AuthorizeAsync(
                    User,
                    resource: pet,
                    policyName: Policies.PetsReadRecord);
            if (!authorizationResult.Succeeded)
            {
                return NotFound();
            }
            var consultations = await _consultationServices.GetConsultationsByPetIdAsync(petId);
            return Ok(consultations);
        }

        [Authorize(Policy = Policies.ConsultationsRead)]
        [HttpGet("veterinarian/{veterinarianId:int}")]
        public async Task<IActionResult> GetByVeterinarianId(int veterinarianId)
        {
            var consultations = await _consultationServices.GetConsultationsByVeterinarianIdAsync(veterinarianId);
            return Ok(consultations);
        }

        [Authorize(Policy = Policies.ConsultationsSuspend)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SuspendConsultation(int id)
        {
            var consultation = await _consultationServices.GetConsultationByIdAsync(id);
            var authorizationResult =
                await _authorizationService.AuthorizeAsync(
                    User,
                    resource: consultation,
                    policyName: Policies.ConsultationsSuspendRecord);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            await _consultationServices.SuspendConsultationAsync(id);
            return NoContent();
        }

        [Authorize(Policy = Policies.ConsultationsReactivate)]
        [HttpPost("{id:int}/reactivate")]
        public async Task<IActionResult> ReactivateConsultation(int id)
        {
            var consultation =  await _consultationServices.GetConsultationByIdAsync(id);
            var authorizationResult =
                await _authorizationService.AuthorizeAsync(
                    User,
                    resource: consultation,
                    policyName: Policies.ConsultationsReactivateRecord);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            await _consultationServices.ReactivateConsultationAsync(id);
            return NoContent();
        }
    }
}
