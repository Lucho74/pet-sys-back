using System.ComponentModel.DataAnnotations;

namespace Domain.Tests.Helpers
{
    internal static class ValidationHelper
    {
        public static IList<ValidationResult> Validate(object instance)
        {
            var context = new ValidationContext(instance);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(instance, context, results, validateAllProperties: true);
            return results;
        }

        public static bool HasErrorFor(IList<ValidationResult> results, string memberName)
        {
            return results.Any(r => r.MemberNames.Contains(memberName));
        }
    }
}
