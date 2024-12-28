
namespace Tenants.Services;

public class TenantService : ITenantService
{
    private string? _host;

    public void SetTenant(string host)
    {
        _host = host;
    }

    public string? GetTenant()
    {
        return _host;
    }
}