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

[Authorize(Roles = $"{Roles.Admin},{Roles.StructAdmin}")]
public class EditModel(ApplicationDbContext context, UserManager<IdentityUser> userManager) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList? RoleList { get; set; }
    public MultiSelectList TenantList { get; set; } = default!;
    public bool IsAdmin { get; set; }

    public async Task<IActionResult> OnGetAsync(string? id)
    {
        if (id == null) return NotFound();

        var user = await userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (!await IsAuthorizedForUserAsync(id)) return Forbid();

        IsAdmin = User.IsInRole(Roles.Admin);
        var roles = await userManager.GetRolesAsync(user);
        var userTenantIds = await context.TenantUsers
            .Where(tu => tu.UserId == id)
            .Select(tu => tu.TenantId)
            .ToListAsync();

        Input = new InputModel
        {
            Id = user.Id,
            Email = user.Email ?? "",
            Role = roles.FirstOrDefault(),
            TenantIds = userTenantIds
        };

        await PopulateListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!await IsAuthorizedForUserAsync(Input.Id)) return Forbid();

        IsAdmin = User.IsInRole(Roles.Admin);

        if (!ModelState.IsValid)
        {
            await PopulateListsAsync();
            return Page();
        }

        var user = await userManager.FindByIdAsync(Input.Id);
        if (user == null) return NotFound();

        if (IsAdmin)
        {
            // Update email
            if (user.Email != Input.Email)
            {
                user.Email = Input.Email;
                user.UserName = Input.Email;
                await userManager.UpdateAsync(user);
            }

            // Update role
            var currentRoles = await userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!string.IsNullOrEmpty(Input.Role))
                await userManager.AddToRoleAsync(user, Input.Role);

            // Update password if provided
            if (!string.IsNullOrEmpty(Input.NewPassword))
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var result = await userManager.ResetPasswordAsync(user, token, Input.NewPassword);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    await PopulateListsAsync();
                    return Page();
                }
            }
        }

        // Update tenant assignments
        var existingTenantUsers = await context.TenantUsers
            .Where(tu => tu.UserId == Input.Id)
            .ToListAsync();

        if (IsAdmin)
        {
            context.TenantUsers.RemoveRange(existingTenantUsers);
        }
        else
        {
            // StructAdmin can only manage assignments for their own tenants
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var myTenantIds = await context.TenantUsers
                .Where(tu => tu.UserId == currentUserId)
                .Select(tu => tu.TenantId)
                .ToListAsync();
            var toRemove = existingTenantUsers.Where(tu => myTenantIds.Contains(tu.TenantId)).ToList();
            context.TenantUsers.RemoveRange(toRemove);
        }

        if (Input.TenantIds != null)
        {
            foreach (var tenantId in Input.TenantIds)
            {
                context.TenantUsers.Add(new TenantUser { UserId = Input.Id, TenantId = tenantId });
            }
        }

        await context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }

    private async Task PopulateListsAsync()
    {
        if (User.IsInRole(Roles.Admin))
        {
            RoleList = new SelectList(new[] { Roles.Admin, Roles.StructAdmin, Roles.User });
            TenantList = new MultiSelectList(
                await context.Tenants.ToListAsync(), "Id", "Name", Input.TenantIds);
        }
        else
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var myTenantIds = await context.TenantUsers
                .Where(tu => tu.UserId == currentUserId)
                .Select(tu => tu.TenantId)
                .ToListAsync();
            TenantList = new MultiSelectList(
                await context.Tenants.Where(t => myTenantIds.Contains(t.Id)).ToListAsync(),
                "Id", "Name", Input.TenantIds);
        }
    }

    private async Task<bool> IsAuthorizedForUserAsync(string userId)
    {
        if (User.IsInRole(Roles.Admin)) return true;

        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var myTenantIds = await context.TenantUsers
            .Where(tu => tu.UserId == currentUserId)
            .Select(tu => tu.TenantId)
            .ToListAsync();

        return await context.TenantUsers
            .AnyAsync(tu => tu.UserId == userId && myTenantIds.Contains(tu.TenantId));
    }

    public class InputModel
    {
        public string Id { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "New Password (leave empty to keep current)")]
        public string? NewPassword { get; set; }

        public string? Role { get; set; }

        [Display(Name = "Tenants")]
        public List<int>? TenantIds { get; set; }
    }
}
