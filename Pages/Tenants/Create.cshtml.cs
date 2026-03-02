using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Morassalat.Data;
using Morassalat.Models;

namespace Morassalat.Pages.Tenants;

[Authorize(Roles = Roles.Admin)]
public class CreateModel(ApplicationDbContext context) : PageModel
{
    [BindProperty]
    public Tenant Tenant { get; set; } = new();

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        context.Tenants.Add(Tenant);
        await context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
