namespace Morassalat.Models;

public class ApplicationParameter
{
    public int Id { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string CurrentAdministrativeYear { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;

    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
}
