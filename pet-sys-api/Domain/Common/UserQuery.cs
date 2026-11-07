using Domain.Entities;

namespace Domain.Common
{
    public class UserQuery : PaginationQuery
    {
        public string? Role { get; set; }
    }
}
