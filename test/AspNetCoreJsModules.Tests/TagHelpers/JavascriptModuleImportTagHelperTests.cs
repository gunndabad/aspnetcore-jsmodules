using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AspNetCoreJsModules.TagHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using Xunit;

namespace AspNetCoreJsModules.Tests.TagHelpers
{
    public class JavascriptModuleImportTagHelperTests
    {
        [Fact]
        public async Task ProcessAsync_NoJsModuleContext_ThrowsInvalidOperationException()
        {
            // Arrange
            var context = new TagHelperContext(
                tagName: "js-import",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            var output = new TagHelperOutput(
                "js-import",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            var urlHelperFactory = new Mock<IUrlHelperFactory>();

            var viewContext = new ViewContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            var tagHelper = new JavascriptModuleImportTagHelper(
                urlHelperFactory.Object,
                HtmlEncoder.Default)
            {
                Name = "MyModule",
                Path = "/path/to/my-module.js",
                ViewContext = viewContext
            };

            // Act
            var ex = await Record.ExceptionAsync(() => tagHelper.ProcessAsync(context, output));

            // Assert
            Assert.IsType<InvalidOperationException>(ex);
            Assert.Equal("No JsModuleContext was found for the current HttpContext. Please add the required services by calling 'services.AddJsModules()' in the application startup code.", ex.Message);
        }

        [Fact]
        public async Task ProcessAsync_NoName_ThrowsInvalidOperationException()
        {
            // Arrange
            var context = new TagHelperContext(
                tagName: "js-import",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            var output = new TagHelperOutput(
                "js-import",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            var urlHelperFactory = new Mock<IUrlHelperFactory>();

            var httpContextFeatures = new FeatureCollection();
            var jsModuleContext = new JsModuleContext();
            httpContextFeatures.Set(new JsModuleContextFeature(jsModuleContext));

            var viewContext = new ViewContext()
            {
                HttpContext = new DefaultHttpContext(httpContextFeatures)
            };

            var tagHelper = new JavascriptModuleImportTagHelper(
                urlHelperFactory.Object,
                HtmlEncoder.Default)
            {
                Path = "/path/to/my-module.js",
                ViewContext = viewContext
            };

            // Act
            var ex = await Record.ExceptionAsync(() => tagHelper.ProcessAsync(context, output));

            // Assert
            Assert.IsType<InvalidOperationException>(ex);
            Assert.Equal("The 'name' attribute must be specified.", ex.Message);
        }

        [Fact]
        public async Task ProcessAsync_NoPath_ThrowsInvalidOperationException()
        {
            // Arrange
            var context = new TagHelperContext(
                tagName: "js-import",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            var output = new TagHelperOutput(
                "js-import",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            var urlHelperFactory = new Mock<IUrlHelperFactory>();

            var httpContextFeatures = new FeatureCollection();
            var jsModuleContext = new JsModuleContext();
            httpContextFeatures.Set(new JsModuleContextFeature(jsModuleContext));

            var viewContext = new ViewContext()
            {
                HttpContext = new DefaultHttpContext(httpContextFeatures)
            };

            var tagHelper = new JavascriptModuleImportTagHelper(
                urlHelperFactory.Object,
                HtmlEncoder.Default)
            {
                Name = "MyModule",
                Path = null,
                ViewContext = viewContext
            };

            // Act
            var ex = await Record.ExceptionAsync(() => tagHelper.ProcessAsync(context, output));

            // Assert
            Assert.IsType<InvalidOperationException>(ex);
            Assert.Equal("The 'path' attribute must be specified.", ex.Message);
        }

        [Fact]
        public async Task ProcessAsync_AddsImportToJsModuleContext()
        {
            // Arrange
            var context = new TagHelperContext(
                tagName: "js-import",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            var output = new TagHelperOutput(
                "js-import",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            var urlHelperFactory = new Mock<IUrlHelperFactory>();

            var httpContextFeatures = new FeatureCollection();
            var jsModuleContext = new JsModuleContext();
            httpContextFeatures.Set(new JsModuleContextFeature(jsModuleContext));

            var viewContext = new ViewContext()
            {
                HttpContext = new DefaultHttpContext(httpContextFeatures)
            };

            var tagHelper = new JavascriptModuleImportTagHelper(
                urlHelperFactory.Object,
                HtmlEncoder.Default)
            {
                Name = "MyModule",
                Path = "/path/to/my-module.js",
                ViewContext = viewContext
            };

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            Assert.Contains(
                jsModuleContext.ModuleImports,
                import => import.Name == "MyModule" && import.Path == "/path/to/my-module.js");
        }

        [Fact]
        public async Task ProcessAsync_LocalPath_ResolvesRelativeToWebRoot()
        {
            // Arrange
            var context = new TagHelperContext(
                tagName: "js-import",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            var output = new TagHelperOutput(
                "js-import",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            var path = "~/path/to/my-module.js";
            var resolvedPath = "/custom-root/path/to/my-module.js";

            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(mock => mock.Content(path)).Returns(resolvedPath);
            urlHelperFactory.Setup(mock => mock.GetUrlHelper(It.IsAny<ActionContext>())).Returns(urlHelper.Object);

            var jsModuleContext = new JsModuleContext();

            var httpContext = new DefaultHttpContext();
            httpContext.Features.Set(new JsModuleContextFeature(jsModuleContext));

            var viewContext = new ViewContext()
            {
                HttpContext = httpContext
            };

            var tagHelper = new JavascriptModuleImportTagHelper(
                urlHelperFactory.Object,
                HtmlEncoder.Default)
            {
                Name = "MyModule",
                Path = path,
                ViewContext = viewContext
            };

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            Assert.Contains(
                jsModuleContext.ModuleImports,
                import => import.Name == "MyModule" && import.Path == resolvedPath);
        }

        [Fact]
        public async Task ProcessAsync_LocalPathWithAppendVersion_AppendsVersionToPath()
        {
            // Arrange
            var context = new TagHelperContext(
                tagName: "js-import",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            var output = new TagHelperOutput(
                "js-import",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            var path = "~/path/to/my-module.js";
            var resolvedPath = "/path/to/my-module.js";
            var pathWithVersion = resolvedPath + "?v=1234";

            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(mock => mock.Content(path)).Returns(resolvedPath);
            urlHelperFactory.Setup(mock => mock.GetUrlHelper(It.IsAny<ActionContext>())).Returns(urlHelper.Object);

            var jsModuleContext = new JsModuleContext();

            var fileVersionProvider = new Mock<IFileVersionProvider>();
            fileVersionProvider.Setup(mock => mock.AddFileVersionToPath(It.IsAny<PathString>(), resolvedPath)).Returns(pathWithVersion);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(mock => mock.GetService(typeof(IFileVersionProvider))).Returns(fileVersionProvider.Object);

            var httpContext = new DefaultHttpContext()
            {
                RequestServices = serviceProvider.Object
            };
            httpContext.Features.Get<IHttpRequestFeature>()!.PathBase = "/";
            httpContext.Features.Set(new JsModuleContextFeature(jsModuleContext));

            var viewContext = new ViewContext()
            {
                HttpContext = httpContext
            };

            var tagHelper = new JavascriptModuleImportTagHelper(
                urlHelperFactory.Object,
                HtmlEncoder.Default)
            {
                Name = "MyModule",
                Path = path,
                ViewContext = viewContext,
                AppendVersion = true
            };

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            Assert.Contains(
                jsModuleContext.ModuleImports,
                import => import.Name == "MyModule" && import.Path == pathWithVersion);
        }
    }
}
