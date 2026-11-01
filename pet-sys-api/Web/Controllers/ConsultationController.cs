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
        public ConsultationController(IConsultationServices consultationServices, IAuthorizationService authorizationService)
        {
            _consultationServices = consultationServices;
            _authorizationService = authorizationService;
        }

        [Authorize(Policy = Policies.ConsultationsRead)]
        [HttpGet("")]
        public async Task<IActionResult> GetAllConsultation()
        {
            var consultations = await _consultationServices.GetAllConsultationAsync();
            return Ok(consultations);
        }

        [Authorize(Policy = Policies.ConsultationsRead)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var consultation = await _consultationServices.GetConsultationByIdAsync(id);
            return Ok(consultation);
        }

        [Authorize(Policy = Policies.ConsultationsCreate)]
        [HttpPost("")]
        public async Task<IActionResult> AddConsultation([FromBody] CreateEditConsultationDTO dto)
        {
            var created = await _consultationServices.AddConsultationAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Policy = Policies.ConsultationsUpdate)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateConsultation(int id, [FromBody] CreateEditConsultationDTO dto)
        {
            var updated = await _consultationServices.UpdateConsultationAsync(id, dto);
            return Ok(updated);
        }

        [Authorize(Policy = Policies.ConsultationsReadByPet)]
        [HttpGet("pet/{petId:int}")]
        public async Task<IActionResult> GetByPetId(int petId)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User,
                resource: null,
                policyName: Policies.ConsultationsRead);

            int? clientId = authorizationResult.Succeeded
                ? null
                : int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var consultations = await _consultationServices.GetConsultationsByPetIdAsync(petId, clientId);
            return Ok(consultations);
        }

        [Authorize(Policy = Policies.ConsultationsRead)]
        [HttpGet("veterinarian/{veterinarianId:int}")]
        public async Task<IActionResult> GetByVeterinarianId(int veterinarianId)
        {
            var consultations = await _consultationServices.GetConsultationsByVeterinarianIdAsync(veterinarianId);
            return Ok(consultations);
        }
    }
}
