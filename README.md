# AspNetCoreJsModules
ASP.NET Core MVC Tag helpers for working with JavaScript modules and import maps.

![ci](https://github.com/gunndabad/aspnetcore-jsmodules/workflows/ci/badge.svg)
![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/AspNetCoreJsModules)

## Installation

### 1. Install NuGet package

Install the [AspNetCoreJsModules NuGet package](https://www.nuget.org/packages/AspNetCoreJsModules/):

    Install-Package AspNetCoreJsModules

Or via the .NET Core command line interface:

    dotnet add package AspNetCoreJsModules

### 2. Configure your ASP.NET Core application

Add services to your application's `Startup` class:

```cs
using AspNetCoreJsModules;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddJsModules();
    }
}
```

### 3. Register tag helpers

In your `_ViewImports.cshtml` file:

```razor:
@addTagHelper *, AspNetCoreJsModules
```

## Working with modules and import maps

The `<js-modules>` tag helper generates an import map was as `script type="module"` and `link rel="modulepreload"` elements for every `<js-import>` on the page. Each `<js-import>` requires a `name` and `path` attribute. The `path` attribute can reference a local or remote file. For local files, you can set `append-version="true"` to have ASP.NET Core append a version hash.

```razor
<js-modules>
  <js-import name="@@hotwired/stimulus" path="https://unpkg.com/@@hotwired/stimulus@3.0.1/dist/stimulus.js" />
  <js-import name="MyModule" path="~/modules/my-module.js" append-version="true" />
</js-modules>
```

Renders:

```html
<script type="importmap">
{
  "imports": {
    "@hotwired/stimulus": "https://unpkg.com/@hotwired/stimulus@3.0.1/dist/stimulus.js",
    "MyModule": "/modules/my-module.js?v=NDJARtKDTcKIE8ytjKwNo_-SSH8LKz_chEuHcASozBk"
  }
}
</script>
<script src="https://unpkg.com/@hotwired/stimulus@3.0.1/dist/stimulus.js" type="module"></script>
<script src="/modules/my-module.js?v=NDJARtKDTcKIE8ytjKwNo_-SSH8LKz_chEuHcASozBk" type="module"></script>
<link href="https://unpkg.com/@hotwired/stimulus@3.0.1/dist/stimulus.js" rel="modulepreload" />
<link href="/modules/my-module.js?v=NDJARtKDTcKIE8ytjKwNo_-SSH8LKz_chEuHcASozBk" rel="modulepreload" />
```

### Adding modules from outside tag helpers

You can also add modules from code:

```cs
using AspNetCoreJsModules;

JsModuleContext jsModuleContext = HttpContext.GetJsModuleContext();
jsModuleContext.AddModuleImport("my-module", "path-to-my-module.js");
```
