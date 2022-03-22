using System;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreJsModules.TagHelpers
{
    /// <summary>
    /// Adds a module import to the request's <see cref="JsModuleContext"/>.
    /// </summary>
    [HtmlTargetElement(TagName, TagStructure = TagStructure.WithoutEndTag)]
    public class JavascriptModuleImportTagHelper : UrlResolutionTagHelper
    {
        internal const string TagName = "js-import";

        private const string AppendVersionAttributeName = "append-version";
        private const string NameAttributeName = "name";
        private const string PathAttributeName = "path";

        /// <summary>
        /// Initializes a new instance of the <see cref="JavascriptModuleImportTagHelper"/> class.
        /// </summary>
        public JavascriptModuleImportTagHelper(IUrlHelperFactory urlHelperFactory, HtmlEncoder htmlEncoder)
            : base(urlHelperFactory, htmlEncoder)
        {
        }

        /// <summary>
        /// Value indicating if file version should be appended to src urls.
        /// </summary>
        /// <remarks>
        /// A query string "v" with the encoded content of the file is added.
        /// </remarks>
        [HtmlAttributeName(AppendVersionAttributeName)]
        public bool? AppendVersion { get; set; }

        /// <summary>
        /// The name of the module import.
        /// </summary>
        [HtmlAttributeName(NameAttributeName)]
        public string? Name { get; set; }

        /// <summary>
        /// The path to the module import.
        /// </summary>
        /// <remarks>
        /// A path starting with '~' will be resolved relative to the application's 'webroot' setting.
        /// </remarks>
        [HtmlAttributeName(PathAttributeName)]
        public string? Path { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var jsModuleContext = ViewContext.HttpContext.GetJsModuleContext();

            if (jsModuleContext is null)
            {
                throw new InvalidOperationException(
                    $"No {nameof(JsModuleContext)} was found for the current {nameof(HttpContext)}. " +
                    $"Please add the required services by calling 'services.{nameof(JsModulesExtensions.AddJsModules)}()' in the application startup code.");
            }

            if (!jsModuleContext.CanAddModuleImports)
            {
                throw new InvalidOperationException("Module imports cannot be added after the import map has been generated.");
            }

            if (string.IsNullOrEmpty(Name))
            {
                throw new InvalidOperationException($"The '{NameAttributeName}' attribute must be specified.");
            }

            if (string.IsNullOrEmpty(Path))
            {
                throw new InvalidOperationException($"The '{PathAttributeName}' attribute must be specified.");
            }

            string path = Path;

            if (TryResolveUrl(path, out string? resolvedUrl))
            {
                path = resolvedUrl!;
            }

            if (AppendVersion == true)
            {
                var fileVersionProvider = ViewContext.HttpContext.RequestServices.GetRequiredService<IFileVersionProvider>();
                path = fileVersionProvider.AddFileVersionToPath(ViewContext.HttpContext.Request.PathBase, path);
            }

            jsModuleContext.AddModuleImport(Name, path);

            output.SuppressOutput();
        }
    }
}
