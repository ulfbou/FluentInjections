using Microsoft.AspNetCore.Http;

namespace FluentInjections.Tests.Middleware
{
    internal class NamedMiddleware : IMiddleware
    {
        internal string Name { get; init; }
        public List<string> IterationList { get; init; }

        internal NamedMiddleware(string name, List<string> iterationList)
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
