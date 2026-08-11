using Domain.Entities;
using Domain.Tests.Helpers;

namespace Domain.Tests.Entities
{
    public class ConsultationValidationTests
    {
        private static Consultation CreateValidConsultation() => new()
        {
            Description = "Annual checkup",
            Date = new DateTime(2026, 1, 1),
            Status = StatusConsultation.Pending,
            PetId = 1,
            VeterinarianId = 1,
        };

        [Fact]
        public void ValidConsultation_HasNoValidationErrors()
        {
            var results = ValidationHelper.Validate(CreateValidConsultation());

            Assert.Empty(results);
        }

        [Fact]
        public void MissingDescription_FailsValidation()
        {
            var consultation = CreateValidConsultation();
            consultation.Description = null!;

            var results = ValidationHelper.Validate(consultation);

            Assert.True(ValidationHelper.HasErrorFor(results, nameof(Consultation.Description)));
        }

        [Fact]
        public void DescriptionLongerThanMaxLength_FailsValidation()
        {
            var consultation = CreateValidConsultation();
            consultation.Description = new string('a', 501);

            var results = ValidationHelper.Validate(consultation);

            Assert.True(ValidationHelper.HasErrorFor(results, nameof(Consultation.Description)));
        }

        [Theory]
        [InlineData(StatusConsultation.Pending)]
        [InlineData(StatusConsultation.Completed)]
        [InlineData(StatusConsultation.Cancelled)]
        public void AllDefinedStatuses_AreValid(StatusConsultation status)
        {
            var consultation = CreateValidConsultation();
            consultation.Status = status;

            var results = ValidationHelper.Validate(consultation);

            Assert.Empty(results);
        }
    }
}
