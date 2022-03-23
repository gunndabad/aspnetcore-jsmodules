using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AspNetCoreJsModules.TagHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace AspNetCoreJsModules.Tests.TagHelpers
{
    public class JavascriptModulesTagHelperTests
    {
        [Fact]
        public async Task ProcessAsync_NoJsModuleContext_ThrowsInvalidOperationException()
        {
            // Arrange
            var context = new TagHelperContext(
                tagName: "js-modules",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            var output = new TagHelperOutput(
                "js-modules",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            var viewContext = new ViewContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            var tagHelper = new JavascriptModulesTagHelper()
            {
                ViewContext = viewContext
            };

            tagHelper.Init(context);

            // Act
            var ex = await Record.ExceptionAsync(() => tagHelper.ProcessAsync(context, output));

            // Assert
            Assert.IsType<InvalidOperationException>(ex);
            Assert.Equal("No JsModuleContext was found for the current HttpContext. Please add the required services by calling 'services.AddJsModules()' in the application startup code.", ex.Message);
        }

        [Fact]
        public async Task ProcessAsync_RendersImportMapAndScriptTags()
        {
            // Arrange
            var context = new TagHelperContext(
                tagName: "js-modules",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            var output = new TagHelperOutput(
                "js-modules",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            var jsModuleContext = new JsModuleContext();
            jsModuleContext.AddModuleImport("MyModule", "/path/to/my-module.js");

            var httpContextFeatures = new FeatureCollection();
            httpContextFeatures.Set(new JsModuleContextFeature(jsModuleContext));

            var viewContext = new ViewContext()
            {
                HttpContext = new DefaultHttpContext(httpContextFeatures)
            };

            var tagHelper = new JavascriptModulesTagHelper()
            {
                Preload = false,
                ViewContext = viewContext
            };

            tagHelper.Init(context);

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            var elements = output.Content.RenderToElements();

            Assert.Collection(
                elements,
                e =>
                {
                    Assert.Equal("script", e.Name);
                    Assert.Equal("importmap", e.GetAttributeValue("type", def: ""));

                    var json = JsonDocument.Parse(e.InnerHtml);

                    Assert.Collection(
                        json.RootElement.EnumerateObject(),
                        e =>
                        {
                            Assert.Equal("imports", e.Name);

                            Assert.Collection(
                                e.Value.EnumerateObject(),
                                prop =>
                                {
                                    Assert.Equal("MyModule", prop.Name);
                                    Assert.Equal("/path/to/my-module.js", prop.Value.GetString());
                                });
                        });
                },
                e =>
                {
                    Assert.Equal("script", e.Name, ignoreCase: true);
                    Assert.Equal("module", e.GetAttributeValue("type", def: ""));
                    Assert.Equal("/path/to/my-module.js", e.GetAttributeValue("src", def: ""));
                });
        }

        [Fact]
        public async Task ProcessAsync_WithPreload_RendersPreloadLinks()
        {
            // Arrange
            var context = new TagHelperContext(
                tagName: "js-modules",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            var output = new TagHelperOutput(
                "js-modules",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            var jsModuleContext = new JsModuleContext();
            jsModuleContext.AddModuleImport("MyModule", "/path/to/my-module.js");

            var httpContextFeatures = new FeatureCollection();
            httpContextFeatures.Set(new JsModuleContextFeature(jsModuleContext));

            var viewContext = new ViewContext()
            {
                HttpContext = new DefaultHttpContext(httpContextFeatures)
            };

            var tagHelper = new JavascriptModulesTagHelper()
            {
                Preload = true,
                ViewContext = viewContext
            };

            tagHelper.Init(context);

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            var elements = output.Content.RenderToElements();

            Assert.Contains(
                elements,
                e => e.Name.Equals("link") &&
                    e.GetAttributeValue("rel", def: "") == "modulepreload" &&
                    e.GetAttributeValue("href", def: "") == "/path/to/my-module.js");
        }

        [Fact]
        public async Task ProcessAsync_WithShim_RendersShimScriptReference()
        {
            // Arrange
            var context = new TagHelperContext(
                tagName: "js-modules",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            var output = new TagHelperOutput(
                "js-modules",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var ctx = (JavascriptModulesTagHelperContext)context.Items[typeof(JavascriptModulesTagHelperContext)];
                    ctx.TrySetShimPath("/shim-path.js");

                    var tagHelperContent = new DefaultTagHelperContent();
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            var jsModuleContext = new JsModuleContext();
            jsModuleContext.AddModuleImport("MyModule", "/path/to/my-module.js");

            var httpContextFeatures = new FeatureCollection();
            httpContextFeatures.Set(new JsModuleContextFeature(jsModuleContext));

            var viewContext = new ViewContext()
            {
                HttpContext = new DefaultHttpContext(httpContextFeatures)
            };

            var tagHelper = new JavascriptModulesTagHelper()
            {
                ViewContext = viewContext
            };

            tagHelper.Init(context);

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            var elements = output.Content.RenderToElements();

            Assert.Contains(
                elements,
                e => e.Name.Equals("script") &&
                    e.Attributes.Contains("async") &&
                    e.GetAttributeValue("src", def: "") == "/shim-path.js");
        }
    }
}
