using Microsoft.AspNetCore.Http;

namespace FluentInjections.Tests.Utilities;

public class ShortCircuitTester
{
    public bool IsPipelineShortCircuited { get; private set; }

    public RequestDelegate WrapMiddleware(RequestDelegate middleware)
    {
        return async context =>
        {
            if (IsPipelineShortCircuited)
            {
                return;
            }

            await middleware(context);
            IsPipelineShortCircuited = true;
        };
    }
}
