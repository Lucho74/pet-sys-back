using Domain.Entities;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Common
{
    public class UserQuery : PaginationQuery
    {
        [EnumDataType(typeof(UserRole))]
        public UserRole? Role { get; set; }
        public bool? IsSuspend { get; set; }
    }
}
