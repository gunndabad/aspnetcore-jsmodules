using System;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AspNetCoreJsModules.TagHelpers
{
    /// <summary>
    /// Specifies that the parent <see cref="JavascriptModulesTagHelper"/> should render an ES modules shim script.
    /// </summary>
    [HtmlTargetElement(TagName, ParentTag = JavascriptModulesTagHelper.TagName)]
    public class JavascriptModulesShimTagHelper : UrlResolutionTagHelper
    {
        internal const string TagName = "js-modules-shim";

        private const string ShimPathAttributeName = "path";

        /// <summary>
        /// Initializes a new instance of the <see cref="JavascriptModulesShimTagHelper"/> class.
        /// </summary>
        public JavascriptModulesShimTagHelper(IUrlHelperFactory urlHelperFactory, HtmlEncoder htmlEncoder)
            : base(urlHelperFactory, htmlEncoder)
        {
        }

        /// <summary>
        /// The path to the ES modules shim library.
        /// </summary>
        /// <remarks>
        /// If <c>null</c> then the locally-hosted library will be referenced.
        /// </remarks>
        [HtmlAttributeName(ShimPathAttributeName)]
        public string? ShimPath { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var modulesTagHelperContext = (JavascriptModulesTagHelperContext?)context.Items[typeof(JavascriptModulesTagHelperContext)];

            if (modulesTagHelperContext == null)
            {
                throw new Exception($"No {nameof(JavascriptModulesTagHelperContext)} set in {nameof(TagHelperContext)}.");
            }

            var shimPath = ShimPath ?? Constants.ModulesShimPath;

            if (TryResolveUrl(shimPath, out string? resolvedUrl))
            {
                shimPath = resolvedUrl!;
            }

            if (!modulesTagHelperContext.TrySetShimPath(shimPath))
            {
                throw new InvalidOperationException($"Only a single {nameof(JavascriptModulesShimTagHelper)} can be declared.");
            }

            output.TagName = null;
        }
    }
}
