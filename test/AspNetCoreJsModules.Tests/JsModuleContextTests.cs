using System;
using System.Linq;
using Xunit;

namespace AspNetCoreJsModules.Tests
{
    public class JsModuleContextTests
    {
        [Fact]
        public void AddModuleImport_UninitializedModule_ThrowsArgumentException()
        {
            // Arrange
            var ctx = new JsModuleContext();

            // Act
            var ex = Record.Exception(() => ctx.AddModuleImport(new JsModuleImport()));

            // Assert
            var argumentException = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("moduleImport", argumentException.ParamName);
            Assert.Equal("JsModuleImport has not been initialized. (Parameter 'moduleImport')", argumentException.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void AddModuleImport_EmptyModuleName_ThrowsArgumentException(string? moduleName)
        {
            // Arrange
            var ctx = new JsModuleContext();

            // Act
#pragma warning disable CS8604 // Possible null reference argument.
            var ex = Record.Exception(() => ctx.AddModuleImport(moduleName, "/path/to/module.js"));
#pragma warning restore CS8604 // Possible null reference argument.

            // Assert
            var argumentException = Assert.IsAssignableFrom<ArgumentException>(ex);
            Assert.Equal("name", argumentException.ParamName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void AddModuleImport_EmptyModulePath_ThrowsArgumentException(string? modulePath)
        {
            // Arrange
            var ctx = new JsModuleContext();

            // Act
#pragma warning disable CS8604 // Possible null reference argument.
            var ex = Record.Exception(() => ctx.AddModuleImport("MyModule", modulePath));
#pragma warning restore CS8604 // Possible null reference argument.

            // Assert
            var argumentException = Assert.IsAssignableFrom<ArgumentException>(ex);
            Assert.Equal("path", argumentException.ParamName);
        }

        [Fact]
        public void AddModuleImport_ValidModule_AddsToModulesCollection()
        {
            // Arrange
            var ctx = new JsModuleContext();
            var module = new JsModuleImport("MyModule", "/path/to/module.js");

            // Act
            ctx.AddModuleImport(module);

            // Assert
            Assert.Contains(module, ctx.ModuleImports);
        }

        [Fact]
        public void AddModuleImport_ValidModuleNameAndPath_AddsToModulesCollection()
        {
            // Arrange
            var ctx = new JsModuleContext();
            var name = "MyModule";
            var path = "/path/to/module.js";

            // Act
            ctx.AddModuleImport(name, path);

            // Assert
            Assert.Contains(ctx.ModuleImports, m => m.Name == name && m.Path == path);
        }

        [Fact]
        public void AddModuleImport_IdenticalModuleExists_DoesNotAddDuplicateToModulesCollection()
        {
            // Arrange
            var ctx = new JsModuleContext();
            var module = new JsModuleImport("MyModule", "/path/to/module.js");
            ctx.AddModuleImport(module);

            // Act
            ctx.AddModuleImport(module);

            // Assert
            Assert.True(ctx.ModuleImports.Count(m => m == module) == 1);
        }

        [Fact]
        public void AddModuleImport_ModuleWithMatchingNameButDifferentPath_ThrowsInvalidOperationException()
        {
            // Arrange
            var ctx = new JsModuleContext();
            ctx.AddModuleImport("MyModule", "another/path");
            var module = new JsModuleImport("MyModule", "/path/to/module.js");

            // Act
            var ex = Record.Exception(() => ctx.AddModuleImport(module));

            // Assert
            Assert.IsType<InvalidOperationException>(ex);
            Assert.Equal("A module named 'MyModule' has already been added with path 'another/path'.", ex.Message);
        }

        [Fact]
        public void AddModuleImport_ModulesAreFrozen_ThrowsInvalidOperationException()
        {
            // Arrange
            var ctx = new JsModuleContext();
            ctx.FreezeModuleImports();
            var module = new JsModuleImport("MyModule", "/path/to/module.js");

            // Act
            var ex = Record.Exception(() => ctx.AddModuleImport(module));

            // Assert
            Assert.IsType<InvalidOperationException>(ex);
            Assert.Equal("Module imports cannot be added to a frozen context.", ex.Message);
        }

        [Fact]
        public void FreezeModuleImports_AlreadyFrozen_ThrowsInvalidOperationException()
        {
            // Arrange
            var ctx = new JsModuleContext();
            ctx.FreezeModuleImports();

            // Act
            var ex = Record.Exception(() => ctx.FreezeModuleImports());

            // Assert
            Assert.IsType<InvalidOperationException>(ex);
            Assert.Equal("JsModuleContext is already frozen.", ex.Message);
        }

        [Fact]
        public void FreezeModuleImports_UnfrozenContext_Succeeds()
        {
            // Arrange
            var ctx = new JsModuleContext();

            // Act
            ctx.FreezeModuleImports();

            // Assert
            Assert.False(ctx.CanAddModuleImports);
        }
    }
}
