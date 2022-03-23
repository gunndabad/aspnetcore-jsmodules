using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace AspNetCoreJsModules
{
    internal class JsModulesStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
            app =>
            {
                app.UseMiddleware<JsModuleContextMiddleware>();

                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = new ManifestEmbeddedFileProvider(
                      typeof(JsModulesStartupFilter).Assembly,
                      root: "Content"),
                    RequestPath = ""
                });

                next(app);
            };
    }
}
