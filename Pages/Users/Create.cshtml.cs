using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Morassalat.Data;
using Morassalat.Models;

namespace Morassalat.Pages.Users;

[Authorize(Roles = Roles.Admin)]
public class CreateModel(ApplicationDbContext context, UserManager<IdentityUser> userManager) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList RoleList { get; set; } = default!;
    public MultiSelectList TenantList { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync()
    {
        await PopulateListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await PopulateListsAsync();
            return Page();
        }

        var user = new IdentityUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            await PopulateListsAsync();
            return Page();
        }

        if (!string.IsNullOrEmpty(Input.Role))
        {
            await userManager.AddToRoleAsync(user, Input.Role);
        }

        if (Input.TenantIds != null)
        {
            foreach (var tenantId in Input.TenantIds)
            {
                context.TenantUsers.Add(new TenantUser { UserId = user.Id, TenantId = tenantId });
            }
            await context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }

    private async Task PopulateListsAsync()
    {
        RoleList = new SelectList(new[] { Roles.Admin, Roles.TenantAdmin, Roles.User });
        TenantList = new MultiSelectList(await context.Tenants.ToListAsync(), "Id", "Name");
    }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        public string? Role { get; set; }

        [Display(Name = "Tenants")]
        public List<int>? TenantIds { get; set; }
    }
}
