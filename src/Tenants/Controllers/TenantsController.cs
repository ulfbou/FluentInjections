using FluentInjections.Internal.Configurators;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using Tenants.Services;

namespace Tenants.Controllers;

[ApiController]
[Route("[controller]")]
public class TenantsController : ControllerBase
{
    public TenantsController(ITenantService tenantService) : base()
    {
        TenantService = tenantService;
    }

    internal ITenantService TenantService { get; }
    public IServiceProvider Provider { get; }

    [HttpGet]
    public IActionResult Get()
    {
        var tenant = TenantService.GetTenant();
        if (string.IsNullOrEmpty(tenant))
        {
            return NotFound();
        }
        return Ok(tenant);
    }

    [HttpPost]
    public IActionResult Post([FromBody] string tenantHost)
    {
        TenantService.SetTenant(tenantHost);
        return Ok();
    }
}
