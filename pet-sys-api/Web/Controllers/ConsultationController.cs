using Application.Interfaces;
using Application.Models;
using Domain.Common;
using Domain.Constants;
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
    public class ConsultationController : ControllerBase
    {
        private readonly IConsultationServices _consultationServices;
        private readonly IPetServices _petServices;
        private readonly IAzureBlobStorage _azureBlobStorage;
        public ConsultationController(IConsultationServices consultationServices, IPetServices petServices, IAzureBlobStorage azureBlobStorage)
        {
            _consultationServices = consultationServices;
            _petServices = petServices;
            _azureBlobStorage = azureBlobStorage;
        }



        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var consultation = await _consultationServices.GetConsultationByIdAsync(id);
            var pet = await _petServices.GetPetByIdAsync(consultation.PetId);
            if (!ResourceAccess.CanReadPet(User, pet))
            {
                return NotFound();
            }
            return Ok(consultation);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpPost("")]
        public async Task<IActionResult> AddConsultation([FromBody] CreateConsultationDTO dto)
        {
            int callerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            int veterinarianId;

            if (User.IsInRole(RoleNames.SysAdmin))
            {
                if (dto.VeterinarianId == null)
                {
                    return BadRequest("SysAdmin must specify the responsible professional.");
                }

                veterinarianId = dto.VeterinarianId.Value;
            }
            else
            {
                veterinarianId = dto.VeterinarianId ?? callerId;
            }

            if (!ResourceAccess.CanCreateConsultationFor(User, veterinarianId))
            {
                return Forbid();
            }

            bool veterinarianOnly = User.IsInRole(RoleNames.Admin) && veterinarianId != callerId;
            var created = await _consultationServices.AddConsultationAsync(dto, veterinarianId, veterinarianOnly);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Policy = Policies.Staff)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateConsultation(int id, [FromBody] UpdateConsultationDTO dto)
        {
            bool veterinarianOnly = false;
            if (dto.VeterinarianId is int veterinarianId)
            {
                bool isAdmin = User.IsInRole(RoleNames.Admin);
                if (!isAdmin && !User.IsInRole(RoleNames.SysAdmin))
                    return Forbid();

                veterinarianOnly = isAdmin &&
                    User.FindFirstValue(ClaimTypes.NameIdentifier) != veterinarianId.ToString();
            }

            var updated = await _consultationServices.UpdateConsultationAsync(id, dto, veterinarianOnly);
            return Ok(updated);
        }

        [HttpGet("pet/{petId:int}")]
        public async Task<IActionResult> GetByPetId(int petId)
        {
            var pet = await _petServices.GetPetByIdAsync(petId);
            if (!ResourceAccess.CanReadPet(User, pet))
            {
                return NotFound();
            }
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
            int? responsibleUserId = null;
            if (mine)
            {
                if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId) || callerId <= 0)
                    return Forbid();

                responsibleUserId = callerId;
            }

            var consultations = await _consultationServices.GetConsultationsPagedAsync(query, responsibleUserId);
            return Ok(consultations);
        }

        [HttpGet("{id:int}/attachments")]
        public async Task<IActionResult> GetImage(int id, [FromQuery] string blobName, CancellationToken cancellationToken)
        {
            var consultation = await _consultationServices.GetConsultationByIdAsync(id);
            var pet = await _petServices.GetPetByIdAsync(consultation.PetId);
            if (!ResourceAccess.CanReadPet(User, pet) || !consultation.Attachments.Any(attachment => attachment.BlobName == blobName))
            {
                return NotFound();
            }

            var url = await _azureBlobStorage.GetReadUrlAsync(blobName, cancellationToken);
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
