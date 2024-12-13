using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentInjections.Tests.Middleware
{
    public class NamedMiddleware : IMiddleware
    {
        private readonly string _name;
        private readonly List<string> _iterationList;
        private readonly ILogger<NamedMiddleware> _logger;

        public NamedMiddleware(string name, List<string> iterationList, ILogger<NamedMiddleware> logger)
        {
            _name = name;
            _iterationList = iterationList;
            _logger = logger;
        }

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            _iterationList.Add(_name);
            _logger.LogInformation(_name);
            var result = next(context);
            return result;
        }
    }
}
