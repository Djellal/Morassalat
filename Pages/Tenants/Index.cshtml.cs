using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Morassalat.Data;
using Morassalat.Models;

namespace Morassalat.Pages.Tenants;

[Authorize(Roles = Roles.Admin)]
public class IndexModel(ApplicationDbContext context) : PageModel
{
    public IList<Tenant> Tenants { get; set; } = [];

    public async Task OnGetAsync()
    {
        Tenants = await context.Tenants.ToListAsync();
    }
}
