using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Morassalat.Data;
using Morassalat.Models;

namespace Morassalat.Pages.ApplicationParameters;

[Authorize(Roles = $"{Roles.Admin},{Roles.StructAdmin}")]
public class EditModel(ApplicationDbContext context) : PageModel
{
    [BindProperty]
    public ApplicationParameter ApplicationParameter { get; set; } = default!;

    public SelectList? TenantList { get; set; }
    public bool IsAdmin { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var param = await context.ApplicationParameters.FindAsync(id);
        if (param == null) return NotFound();

        if (!await IsAuthorizedForTenantAsync(param.TenantId)) return Forbid();

        IsAdmin = User.IsInRole(Roles.Admin);
        ApplicationParameter = param;

        if (IsAdmin)
            TenantList = new SelectList(await context.Tenants.ToListAsync(), "Id", "Name");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!await IsAuthorizedForTenantAsync(ApplicationParameter.TenantId))
            return Forbid();

        IsAdmin = User.IsInRole(Roles.Admin);

        if (!ModelState.IsValid)
        {
            if (IsAdmin)
                TenantList = new SelectList(await context.Tenants.ToListAsync(), "Id", "Name");
            return Page();
        }

        context.Attach(ApplicationParameter).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await context.ApplicationParameters.AnyAsync(e => e.Id == ApplicationParameter.Id))
                return NotFound();
            throw;
        }

        if (IsAdmin)
            return RedirectToPage("./Index");

        return RedirectToPage("/Index");
    }

    private async Task<bool> IsAuthorizedForTenantAsync(int tenantId)
    {
        if (User.IsInRole(Roles.Admin)) return true;

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return await context.TenantUsers.AnyAsync(tu => tu.UserId == userId && tu.TenantId == tenantId);
    }
}
