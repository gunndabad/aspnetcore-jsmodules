using System;

namespace AspNetCoreJsModules.TagHelpers
{
    internal class JavascriptModulesTagHelperContext
    {
        public string? ShimPath { get; private set; }

        public bool TrySetShimPath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (ShimPath != null)
            {
                return false;
            }

            ShimPath = path;
            return true;
        }
    }
}
