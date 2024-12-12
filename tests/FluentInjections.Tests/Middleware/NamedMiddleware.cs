using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FluentInjections.Tests.Middleware
{
    public class NamedMiddleware : IMiddleware
    {
        public string Name { get; init; }
        public List<string> IterationList { get; init; }

        public NamedMiddleware(string name, List<string> iterationList)
        {
            Name = name;
            IterationList = iterationList;
        }

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            IterationList.Add(Name);
            var result = next(context);
            IterationList.Add(Name);
            return result;
        }
    }
}
