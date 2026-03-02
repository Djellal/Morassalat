using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Morassalat.Models;

namespace Morassalat.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<ApplicationParameter> ApplicationParameters { get; set; }
}
