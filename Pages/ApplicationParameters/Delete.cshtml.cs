using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Morassalat.Data;
using Morassalat.Models;

namespace Morassalat.Pages.ApplicationParameters;

[Authorize(Roles = Roles.Admin)]
public class DeleteModel(ApplicationDbContext context) : PageModel
{
    [BindProperty]
    public ApplicationParameter ApplicationParameter { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var param = await context.ApplicationParameters
            .Include(p => p.Tenant)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (param == null) return NotFound();

        if (!await IsAuthorizedForTenantAsync(param.TenantId)) return Forbid();

        ApplicationParameter = param;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null) return NotFound();

        var param = await context.ApplicationParameters.FindAsync(id);
        if (param != null)
        {
            if (!await IsAuthorizedForTenantAsync(param.TenantId)) return Forbid();

            context.ApplicationParameters.Remove(param);
            await context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }

    private async Task<bool> IsAuthorizedForTenantAsync(int tenantId)
    {
        if (User.IsInRole(Roles.Admin)) return true;

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return await context.TenantUsers.AnyAsync(tu => tu.UserId == userId && tu.TenantId == tenantId);
    }
}
