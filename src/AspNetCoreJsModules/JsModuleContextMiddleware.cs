using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AspNetCoreJsModules
{
    internal class JsModuleContextMiddleware
    {
        private readonly RequestDelegate _next;

        public JsModuleContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task InvokeAsync(HttpContext context)
        {
            context.Features.Set(new JsModuleContextFeature(new JsModuleContext()));

            return _next(context);
        }
    }
}
