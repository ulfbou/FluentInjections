namespace Tenants.Services;

public interface ITenantService
{
    string? GetTenant();
    void SetTenant(string host);
}
