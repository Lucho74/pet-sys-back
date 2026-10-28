using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("UserRoles")]
    [PrimaryKey(nameof(UserId), nameof(RoleId))]
    public class UserRole
    {
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public User User { get; set; } = null!;

        public int RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Role Role { get; set; } = null!;
    }
}
