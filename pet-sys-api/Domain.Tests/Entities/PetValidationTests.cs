using Domain.Entities;
using Domain.Tests.Helpers;

namespace Domain.Tests.Entities
{
    public class PetValidationTests
    {
        private static Pet CreateValidPet() => new()
        {
            Name = "Firulais",
            Specie = "Dog",
            Breed = "Labrador",
            BirthDate = new DateOnly(2020, 1, 1),
            ClientId = 1,
        };

        [Fact]
        public void ValidPet_HasNoValidationErrors()
        {
            var results = ValidationHelper.Validate(CreateValidPet());

            Assert.Empty(results);
        }

        [Theory]
        [InlineData(nameof(Pet.Name))]
        [InlineData(nameof(Pet.Specie))]
        [InlineData(nameof(Pet.Breed))]
        public void MissingRequiredStringField_FailsValidation(string propertyName)
        {
            var pet = CreateValidPet();
            typeof(Pet).GetProperty(propertyName)!.SetValue(pet, null);

            var results = ValidationHelper.Validate(pet);

            Assert.True(ValidationHelper.HasErrorFor(results, propertyName));
        }

        [Fact]
        public void NameLongerThanMaxLength_FailsValidation()
        {
            var pet = CreateValidPet();
            pet.Name = new string('a', 51);

            var results = ValidationHelper.Validate(pet);

            Assert.True(ValidationHelper.HasErrorFor(results, nameof(Pet.Name)));
        }

        [Fact]
        public void NewPet_StartsWithEmptyConsultationsCollection()
        {
            var pet = CreateValidPet();

            Assert.Empty(pet.Consultations);
        }
    }
}
