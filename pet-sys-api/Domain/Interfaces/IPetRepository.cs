using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IPetRepository : IBaseRepository<Pet>
    {
        Task<IEnumerable<Pet>> GetByClientIdAsync(int clientId);
    }
}
