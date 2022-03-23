using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AspNetCoreJsModules.TagHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using Xunit;

namespace AspNetCoreJsModules.Tests.TagHelpers
{
    public class JavascriptModulesShimTagHelperTests
    {
        [Fact]
        public async Task ProcessAsync_EmptyPath_AddsDefaultShimPathToContext()
        {
            // Arrange
            var modulesTagHelperContext = new JavascriptModulesTagHelperContext();

            var context = new TagHelperContext(
                tagName: "js-modules-shim",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>()
                {
                    { typeof(JavascriptModulesTagHelperContext), modulesTagHelperContext }
                },
                uniqueId: "test");

            var output = new TagHelperOutput(
                "js-modules-shim",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(mock => mock.Content(Constants.ModulesShimPath)).Returns(Constants.ModulesShimPath);
            urlHelperFactory.Setup(mock => mock.GetUrlHelper(It.IsAny<ActionContext>())).Returns(urlHelper.Object);

            var viewContext = new ViewContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            var tagHelper = new JavascriptModulesShimTagHelper(
                urlHelperFactory.Object,
                HtmlEncoder.Default)
            {
                ShimPath = null,
                ViewContext = viewContext
            };

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            Assert.Equal(Constants.ModulesShimPath, modulesTagHelperContext.ShimPath);
        }

        [Fact]
        public async Task ProcessAsync_NonEmptyPath_AddsSpecifiedShimPathToContext()
        {
            // Arrange
            var modulesTagHelperContext = new JavascriptModulesTagHelperContext();

            var context = new TagHelperContext(
                tagName: "js-modules-shim",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>()
                {
                    { typeof(JavascriptModulesTagHelperContext), modulesTagHelperContext }
                },
                uniqueId: "test");

            var output = new TagHelperOutput(
                "js-modules-shim",
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

            var shimPath = "/my-custom-shim-path.js";

            var tagHelper = new JavascriptModulesShimTagHelper(
                urlHelperFactory.Object,
                HtmlEncoder.Default)
            {
                ShimPath = shimPath,
                ViewContext = viewContext
            };

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            Assert.Equal(shimPath, modulesTagHelperContext.ShimPath);
        }

        [Fact]
        public async Task ProcessAsync_AlreadyGotShimPath_ThrowsInvalidOperationException()
        {
            // Arrange
            var modulesTagHelperContext = new JavascriptModulesTagHelperContext();
            modulesTagHelperContext.TrySetShimPath("shim-path.js");

            var context = new TagHelperContext(
                tagName: "js-modules-shim",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>()
                {
                    { typeof(JavascriptModulesTagHelperContext), modulesTagHelperContext }
                },
                uniqueId: "test");

            var output = new TagHelperOutput(
                "js-modules-shim",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(mock => mock.Content(Constants.ModulesShimPath)).Returns(Constants.ModulesShimPath);
            urlHelperFactory.Setup(mock => mock.GetUrlHelper(It.IsAny<ActionContext>())).Returns(urlHelper.Object);

            var viewContext = new ViewContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            var tagHelper = new JavascriptModulesShimTagHelper(
                urlHelperFactory.Object,
                HtmlEncoder.Default)
            {
                ShimPath = null,
                ViewContext = viewContext
            };

            // Act
            var ex = await Record.ExceptionAsync(() => tagHelper.ProcessAsync(context, output));

            // Assert
            Assert.IsType<InvalidOperationException>(ex);
            Assert.Equal("Only a single JavascriptModulesShimTagHelper can be declared.", ex.Message);
        }
    }
}
