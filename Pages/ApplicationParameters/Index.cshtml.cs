using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Morassalat.Data;
using Morassalat.Models;

namespace Morassalat.Pages.ApplicationParameters;

[Authorize(Roles = $"{Roles.Admin},{Roles.TenantAdmin}")]
public class IndexModel(ApplicationDbContext context) : PageModel
{
    public IList<ApplicationParameter> ApplicationParameters { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.IsInRole(Roles.TenantAdmin))
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var tenantId = await context.TenantUsers
                .Where(tu => tu.UserId == userId)
                .Select(tu => tu.TenantId)
                .FirstOrDefaultAsync();

            if (tenantId == 0) return NotFound();

            var param = await context.ApplicationParameters
                .FirstOrDefaultAsync(p => p.TenantId == tenantId);

            if (param == null)
            {
                param = new ApplicationParameter { TenantId = tenantId };
                context.ApplicationParameters.Add(param);
                await context.SaveChangesAsync();
            }

            return RedirectToPage("./Edit", new { id = param.Id });
        }

        ApplicationParameters = await context.ApplicationParameters
            .Include(p => p.Tenant)
            .ToListAsync();

        return Page();
    }
}
