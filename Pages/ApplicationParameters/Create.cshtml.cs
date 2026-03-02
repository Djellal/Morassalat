using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Morassalat.Data;
using Morassalat.Models;

namespace Morassalat.Pages.ApplicationParameters;

[Authorize(Roles = Roles.Admin)]
public class CreateModel(ApplicationDbContext context) : PageModel
{
    [BindProperty]
    public ApplicationParameter ApplicationParameter { get; set; } = new();

    public SelectList TenantList { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync()
    {
        TenantList = new SelectList(await GetTenantsAsync(), "Id", "Name");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!await IsAuthorizedForTenantAsync(ApplicationParameter.TenantId))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            TenantList = new SelectList(await GetTenantsAsync(), "Id", "Name");
            return Page();
        }

        context.ApplicationParameters.Add(ApplicationParameter);
        await context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }

    private async Task<List<Tenant>> GetTenantsAsync()
    {
        if (User.IsInRole(Roles.Admin))
        {
            return await context.Tenants.ToListAsync();
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var tenantIds = await context.TenantUsers
            .Where(tu => tu.UserId == userId)
            .Select(tu => tu.TenantId)
            .ToListAsync();

        return await context.Tenants.Where(t => tenantIds.Contains(t.Id)).ToListAsync();
    }

    private async Task<bool> IsAuthorizedForTenantAsync(int tenantId)
    {
        if (User.IsInRole(Roles.Admin)) return true;

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return await context.TenantUsers.AnyAsync(tu => tu.UserId == userId && tu.TenantId == tenantId);
    }
}
