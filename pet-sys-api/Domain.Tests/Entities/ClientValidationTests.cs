using Domain.Entities;
using Domain.Tests.Helpers;

namespace Domain.Tests.Entities
{
    public class ClientValidationTests
    {
        private static Client CreateValidClient() => new()
        {
            FullName = "Juana Perez",
            Password = "secret123",
            Email = "juana@example.com",
            Phone = "+54 351 555 1234",
            Dni = "30123456",
        };

        [Fact]
        public void ValidClient_HasNoValidationErrors()
        {
            var results = ValidationHelper.Validate(CreateValidClient());

            Assert.Empty(results);
        }

        [Fact]
        public void IsDeleted_DefaultsToFalse()
        {
            var client = CreateValidClient();

            Assert.False(client.IsDeleted);
        }

        [Fact]
        public void MissingFullName_FailsValidation()
        {
            var client = CreateValidClient();
            client.FullName = null!;

            var results = ValidationHelper.Validate(client);

            Assert.True(ValidationHelper.HasErrorFor(results, nameof(Client.FullName)));
        }

        [Theory]
        [InlineData("12345")]
        [InlineData("")]
        public void PasswordShorterThanMinimumLength_FailsValidation(string shortPassword)
        {
            var client = CreateValidClient();
            client.Password = shortPassword;

            var results = ValidationHelper.Validate(client);

            Assert.True(ValidationHelper.HasErrorFor(results, nameof(Client.Password)));
        }

        [Theory]
        [InlineData("not-an-email")]
        [InlineData("missing-at-sign.com")]
        public void InvalidEmail_FailsValidation(string invalidEmail)
        {
            var client = CreateValidClient();
            client.Email = invalidEmail;

            var results = ValidationHelper.Validate(client);

            Assert.True(ValidationHelper.HasErrorFor(results, nameof(Client.Email)));
        }

        [Fact]
        public void InvalidPhone_FailsValidation()
        {
            var client = CreateValidClient();
            client.Phone = "not-a-phone-number";

            var results = ValidationHelper.Validate(client);

            Assert.True(ValidationHelper.HasErrorFor(results, nameof(Client.Phone)));
        }

        [Fact]
        public void MissingDni_FailsValidation()
        {
            var client = CreateValidClient();
            client.Dni = null!;

            var results = ValidationHelper.Validate(client);

            Assert.True(ValidationHelper.HasErrorFor(results, nameof(Client.Dni)));
        }

        [Fact]
        public void DniLongerThanMaxLength_FailsValidation()
        {
            var client = CreateValidClient();
            client.Dni = new string('1', 21);

            var results = ValidationHelper.Validate(client);

            Assert.True(ValidationHelper.HasErrorFor(results, nameof(Client.Dni)));
        }

        [Fact]
        public void NewClient_StartsWithEmptyPetsCollection()
        {
            var client = CreateValidClient();

            Assert.Empty(client.Pets);
        }
    }
}
