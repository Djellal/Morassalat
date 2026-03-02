using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Morassalat.Data;
using Morassalat.Models;

namespace Morassalat.Pages.Tenants;

[Authorize(Roles = Roles.Admin)]
public class DeleteModel(ApplicationDbContext context) : PageModel
{
    [BindProperty]
    public Tenant Tenant { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tenant = await context.Tenants.FindAsync(id);
        if (tenant == null)
        {
            return NotFound();
        }

        Tenant = tenant;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tenant = await context.Tenants.FindAsync(id);
        if (tenant != null)
        {
            context.Tenants.Remove(tenant);
            await context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
