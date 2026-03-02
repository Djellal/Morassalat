namespace Morassalat.Models;

public class TenantUser
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
}
