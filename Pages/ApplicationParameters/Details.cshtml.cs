using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Morassalat.Data;
using Morassalat.Models;

namespace Morassalat.Pages.ApplicationParameters;

[Authorize(Roles = Roles.Admin)]
public class DetailsModel(ApplicationDbContext context) : PageModel
{
    public ApplicationParameter ApplicationParameter { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var param = await context.ApplicationParameters
            .Include(p => p.Tenant)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (param == null) return NotFound();

        if (!User.IsInRole(Roles.Admin))
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var hasAccess = await context.TenantUsers
                .AnyAsync(tu => tu.UserId == userId && tu.TenantId == param.TenantId);
            if (!hasAccess) return Forbid();
        }

        ApplicationParameter = param;
        return Page();
    }
}
