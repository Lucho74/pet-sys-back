using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.TestHelpers
{
    internal static class InMemoryDbContextFactory
    {
        public static ApplicationDbContext Create()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}
