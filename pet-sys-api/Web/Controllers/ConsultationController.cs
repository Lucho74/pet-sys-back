using Application.Interfaces;
using Application.Models;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Authorization;

namespace Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultationController : ControllerBase
    {
        private readonly IConsultationServices _consultationServices;
        public ConsultationController(IConsultationServices consultationServices)
        {
            _consultationServices = consultationServices;
        }



        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var consultation = await _consultationServices.GetConsultationByIdAsync(id);
            return Ok(consultation);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpPost("")]
        public async Task<IActionResult> AddConsultation([FromBody] CreateConsultationDTO dto)
        {
            var created = await _consultationServices.AddConsultationAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateConsultation(int id, [FromBody] UpdateConsultationDTO dto)
        {
            var updated = await _consultationServices.UpdateConsultationAsync(id, dto);
            return Ok(updated);
        }

        [HttpGet("pet/{petId:int}")]
        public async Task<IActionResult> GetByPetId(int petId)
        {
            var consultations = await _consultationServices.GetConsultationsByPetIdAsync(petId);
            return Ok(consultations);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpGet("veterinarian/{veterinarianId:int}")]
        public async Task<IActionResult> GetByVeterinarianId(int veterinarianId)
        {
            var consultations = await _consultationServices.GetConsultationsByVeterinarianIdAsync(veterinarianId);
            return Ok(consultations);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SuspendConsultation(int id)
        {
            await _consultationServices.SuspendConsultationAsync(id);
            return NoContent();
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpPost("{id:int}/reactivate")]
        public async Task<IActionResult> ReactivateConsultation(int id)
        {
            await _consultationServices.ReactivateConsultationAsync(id);
            return NoContent();
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpGet]
        public async Task<IActionResult> GetConsultationsByPagination([FromQuery] PaginationQuery query, [FromQuery] bool mine = false)
        {
            var consultations = await _consultationServices.GetConsultationsPagedAsync(query, mine);
            return Ok(consultations);
        }

        [HttpGet("{id:int}/attachments")]
        public async Task<IActionResult> GetImage(int id, [FromQuery] string blobName, CancellationToken cancellationToken)
        {
            var url = await _consultationServices.GetAttachmentUrlAsync(id, blobName, cancellationToken);
            return Ok(new { url });
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpPost("{id:int}/attachments")]
        public async Task<IActionResult> UploadAttachment(int id, IFormFile file, CancellationToken cancellationToken)
        {
            using var stream = file.OpenReadStream();

            await _consultationServices.UploadAttachmentAsync(id, stream, file.ContentType, file.FileName, cancellationToken);

            return NoContent();
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpDelete("{id:int}/attachments")]
        public async Task<IActionResult> DeleteAttachment(int id, [FromQuery] string blobName, CancellationToken cancellationToken)
        {
            await _consultationServices.DeleteAttachmentAsync(id, blobName, cancellationToken);

            return NoContent();
        }
    }
}
