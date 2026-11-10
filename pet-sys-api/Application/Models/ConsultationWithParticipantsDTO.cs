namespace Application.Models
{
    public class ConsultationWithParticipantsDTO : ConsultationDTO
    {
        public string PetName { get; set; } = string.Empty;
        public string VeterinarianName { get; set; } = string.Empty;
    }
}
