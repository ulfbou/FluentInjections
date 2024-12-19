using Microsoft.AspNetCore.Http;

namespace FluentInjections;

internal interface IExecutionPolicy
{
    bool CanExecute(HttpContext context, RequestDelegate next);
}