
using Tenants.Services;

namespace Tenants.Middleware;

public class TenantMiddleware : IMiddleware
{
    private readonly TenantService _tenantService;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(TenantService tenantService, ILogger<TenantMiddleware> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _logger.LogInformation("Tenant middleware invoked.");
        _tenantService.SetTenant(context.Request.Host.Host);
        return next(context);
    }
}
