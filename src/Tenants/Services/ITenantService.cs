namespace Tenants.Services
{
    internal interface ITenantService
    {
        string? GetTenant();
        void SetTenant(string host);
    }
}