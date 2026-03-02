using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Morassalat.Data;
using Morassalat.Models;

namespace Morassalat.Pages.Users;

[Authorize(Roles = Roles.Admin)]
public class DeleteModel(ApplicationDbContext context, UserManager<IdentityUser> userManager) : PageModel
{
    [BindProperty]
    public UserViewModel UserToDelete { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(string? id)
    {
        if (id == null) return NotFound();

        var user = await userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await userManager.GetRolesAsync(user);
        var tenantNames = await context.TenantUsers
            .Where(tu => tu.UserId == id)
            .Select(tu => tu.Tenant.Name)
            .ToListAsync();

        UserToDelete = new UserViewModel
        {
            Id = user.Id,
            Email = user.Email ?? "",
            Roles = string.Join(", ", roles),
            Tenants = string.Join(", ", tenantNames)
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.FindByIdAsync(UserToDelete.Id);
        if (user != null)
        {
            var tenantUsers = await context.TenantUsers
                .Where(tu => tu.UserId == user.Id)
                .ToListAsync();
            context.TenantUsers.RemoveRange(tenantUsers);
            await context.SaveChangesAsync();

            await userManager.DeleteAsync(user);
        }

        return RedirectToPage("./Index");
    }

    public class UserViewModel
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string Roles { get; set; } = "";
        public string Tenants { get; set; } = "";
    }
}
