using System;

namespace AspNetCoreJsModules
{
    internal sealed class JsModuleContextFeature
    {
        public JsModuleContextFeature(JsModuleContext jsModuleContext)
        {
            JsModuleContext = jsModuleContext ?? throw new ArgumentNullException(nameof(jsModuleContext));
        }

        public JsModuleContext JsModuleContext { get; }
    }
}
