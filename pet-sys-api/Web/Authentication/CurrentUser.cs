using Application.Interfaces;
using Domain.Enums;
using System.Security.Claims;

namespace Web.Authentication
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? Id
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;

                if (user?.Identity?.IsAuthenticated != true)
                {
                    return null;
                }

                var value = user.FindFirstValue(ClaimTypes.NameIdentifier);

                return int.TryParse(value, out var id) && id > 0 ? id : null;
            }
        }

        public UserRole? Role
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;

                if (user?.Identity?.IsAuthenticated != true)
                {
                    return null;
                }

                var value = user.FindFirstValue(ClaimTypes.Role);

                if (Enum.TryParse<UserRole>(value, out var role) && Enum.IsDefined(typeof(UserRole), role))
                {
                    return role;
                }

                return null;
            }
        }
    }
}
