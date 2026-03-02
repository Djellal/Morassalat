using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Morassalat.Data;
using Morassalat.Models;

namespace Morassalat.Pages.Users;

[Authorize(Roles = $"{Roles.Admin},{Roles.TenantAdmin}")]
public class IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager) : PageModel
{
    public IList<UserViewModel> Users { get; set; } = [];

    public async Task OnGetAsync()
    {
        if (User.IsInRole(Roles.Admin))
        {
            var users = await userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                var tenantNames = await context.TenantUsers
                    .Where(tu => tu.UserId == user.Id)
                    .Select(tu => tu.Tenant.Name)
                    .ToListAsync();
                Users.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    Roles = string.Join(", ", roles),
                    Tenants = string.Join(", ", tenantNames)
                });
            }
        }
        else
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var tenantIds = await context.TenantUsers
                .Where(tu => tu.UserId == currentUserId)
                .Select(tu => tu.TenantId)
                .ToListAsync();

            var userIds = await context.TenantUsers
                .Where(tu => tenantIds.Contains(tu.TenantId))
                .Select(tu => tu.UserId)
                .Distinct()
                .ToListAsync();

            foreach (var userId in userIds)
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user == null) continue;
                var roles = await userManager.GetRolesAsync(user);
                var tenantNames = await context.TenantUsers
                    .Where(tu => tu.UserId == user.Id && tenantIds.Contains(tu.TenantId))
                    .Select(tu => tu.Tenant.Name)
                    .ToListAsync();
                Users.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    Roles = string.Join(", ", roles),
                    Tenants = string.Join(", ", tenantNames)
                });
            }
        }
    }

    public class UserViewModel
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string Roles { get; set; } = "";
        public string Tenants { get; set; } = "";
    }
}
