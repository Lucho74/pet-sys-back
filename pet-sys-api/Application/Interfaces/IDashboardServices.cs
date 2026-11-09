using Application.Models;

namespace Application.Interfaces
{
    public interface IDashboardServices
    {
        Task<DashboardDTO> GetDashboardAsync(int userId, int take);
    }
}
