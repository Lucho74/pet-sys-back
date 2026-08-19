using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Application.Models;
using Domain.Entities;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultationController : ControllerBase
    {
        private readonly IConsultationServices _consultationServices;
        public ConsultationController(IConsultationServices consultationServices)
        {
            _consultationServices = consultationServices;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllConsultation()
        {
            var consultations = await _consultationServices.GetAllConsultationAsync();
            return Ok(consultations);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var consultation = await _consultationServices.GetConsultationByIdAsync(id);
            return Ok(consultation);
        }

        [HttpPost("")]
        public async Task<IActionResult> AddConsultation([FromBody] CreateConsultationDTO dto)
        {
            var created = await _consultationServices.AddConsultationAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateConsultation(int id, [FromBody] ConsultationDTO dto)
        {
            var updated = await _consultationServices.UpdateConsultationAsync(id, dto);
            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteConsultation(int id)
        {
            await _consultationServices.DeleteConsultationAsync(id);
            return NoContent();
        }

        [HttpGet("pet/{petId:int}")]
        public async Task<IActionResult> GetByPetId(int petId)
        {
            var consultations = await _consultationServices.GetConsultationsByPetIdAsync(petId);
            return Ok(consultations);
        }

        [HttpGet("veterinarian/{veterinarianId:int}")]
        public async Task<IActionResult> GetByVeterinarianId(int veterinarianId)
        {
            var consultations = await _consultationServices.GetConsultationsByVeterinarianIdAsync(veterinarianId);
            return Ok(consultations);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(StatusConsultation status)
        {
            var consultations = await _consultationServices.GetConsultationsByStatusAsync(status);
            return Ok(consultations);
        }
    }
}
