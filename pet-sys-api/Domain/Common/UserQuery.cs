using Domain.Entities;

namespace Domain.Common
{
    public class UserQuery : PaginationQuery
    {
        public UserType? Role { get; set; }
    }
}
