using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreJsModules
{
    /// <summary>
    /// Extension methods for setting up JS Modules in an ASP.NET Core application.
    /// </summary>
    public static class JsModulesExtensions
    {
        /// <summary>
        /// Adds JS Module services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="services"/> argument is null.</exception>
        public static IServiceCollection AddJsModules(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddSingleton<IStartupFilter, JsModulesStartupFilter>();
        }

        /// <summary>
        /// Gets the <see cref="JsModuleContext"/> for the specified <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/>.</param>
        /// <returns>The <see cref="JsModuleContext"/> for the specified <see cref="HttpContext"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="httpContext"/> is <c>null</c>.</exception>
        public static JsModuleContext? GetJsModuleContext(this HttpContext httpContext)
        {
            if (httpContext is null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var feature = httpContext.Features.Get<JsModuleContextFeature>();
            return feature?.JsModuleContext;
        }
    }
}
