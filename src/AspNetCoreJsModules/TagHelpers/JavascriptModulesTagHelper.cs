using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AspNetCoreJsModules.TagHelpers
{
    /// <summary>
    /// Renders script imports and an import map for registered javascript modules.
    /// </summary>
    [HtmlTargetElement(TagName)]
    [RestrictChildren(JavascriptModuleImportTagHelper.TagName)]
    public class JavascriptModulesTagHelper : TagHelper
    {
        internal const string TagName = "js-modules";

        private const string PreloadAttributeName = "preload";

        /// <summary>
        /// Initializes a new instance of the <see cref="JavascriptModulesTagHelper"/> class.
        /// </summary>
        public JavascriptModulesTagHelper()
        {
        }

        /// <summary>
        /// Whether to render modulepreload links for each imported module.
        /// </summary>
        /// <remarks>
        /// The default is <c>true</c>.
        /// </remarks>
        [HtmlAttributeName(PreloadAttributeName)]
        public bool Preload { get; set; } = true;

        /// <summary>
        /// Gets the <see cref="ViewContext"/> of the executing view.
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; } = default!;

        /// <inheritdoc/>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var jsModuleContext = ViewContext.HttpContext.GetJsModuleContext();

            if (jsModuleContext is null)
            {
                throw new InvalidOperationException(
                    $"No {nameof(JsModuleContext)} was found for the current {nameof(HttpContext)}. " +
                    $"Please add the required services by calling 'services.{nameof(JsModulesExtensions.AddJsModules)}()' in the application startup code.");
            }

            await output.GetChildContentAsync();

            jsModuleContext.FreezeModuleImports();

            output.TagName = null;
            AppendImportMap(output, jsModuleContext);
            AppendScripts(output, jsModuleContext);

            if (Preload)
            {
                AppendPreloads(output, jsModuleContext);
            }
        }

        private void AppendImportMap(TagHelperOutput output, JsModuleContext jsModuleContext)
        {
            var script = new TagBuilder("script");
            script.Attributes.Add("type", "importmap");

            script.InnerHtml.AppendHtml("\n");
            script.InnerHtml.AppendHtml(CreateImportMapJson());
            script.InnerHtml.AppendHtml("\n");

            output.Content.AppendHtml(script);
            output.Content.AppendHtml("\n");

            string CreateImportMapJson()
            {
                using (var ms = new MemoryStream())
                {
                    using (var writer = new Utf8JsonWriter(ms, new JsonWriterOptions() { Indented = true }))
                    {
                        writer.WriteStartObject();
                        writer.WritePropertyName("imports");
                        writer.WriteStartObject();

                        foreach (var import in jsModuleContext.ModuleImports)
                        {
                            writer.WritePropertyName(import.Name);
                            writer.WriteStringValue(import.Path);
                        }

                        writer.WriteEndObject();  // imports
                        writer.WriteEndObject();
                    }

                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }

        private void AppendPreloads(TagHelperOutput output, JsModuleContext jsModuleContext)
        {
            foreach (var module in jsModuleContext.ModuleImports)
            {
                var link = new TagBuilder("link")
                {
                    TagRenderMode = TagRenderMode.SelfClosing
                };

                link.Attributes.Add("rel", "modulepreload");
                link.Attributes.Add("href", module.Path);

                output.Content.AppendHtml(link);
                output.Content.AppendHtml("\n");
            }
        }

        private void AppendScripts(TagHelperOutput output, JsModuleContext jsModuleContext)
        {
            foreach (var module in jsModuleContext.ModuleImports)
            {
                var script = new TagBuilder("script");

                script.Attributes.Add("type", "module");
                script.Attributes.Add("src", module.Path);

                output.Content.AppendHtml(script);
                output.Content.AppendHtml("\n");
            }
        }
    }
}
