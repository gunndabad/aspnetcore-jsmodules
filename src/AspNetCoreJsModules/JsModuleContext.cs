using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCoreJsModules
{
    /// <summary>
    /// Contains the collection of JavaScript modules for the currently executing request.
    /// </summary>
    public sealed class JsModuleContext
    {
        private bool _importsFrozen = false;
        private readonly List<JsModuleImport> _moduleImports;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsModuleContext"/> class.
        /// </summary>
        public JsModuleContext()
        {
            _moduleImports = new();
        }

        /// <summary>
        /// Gets the list of JavaScript modules.
        /// </summary>
        public IReadOnlyCollection<JsModuleImport> ModuleImports => _moduleImports.AsReadOnly();

        /// <summary>
        /// Gets whether module imports can be added to this <see cref="JsModuleContext"/>.
        /// </summary>
        /// <remarks>
        /// When module imports are consumed e.g. by a tag helper rendering an import map, this context is frozen to
        /// prevent any additional imports being added.
        /// </remarks>
        public bool CanAddModuleImports => !_importsFrozen;

        /// <summary>
        /// Adds a new module import to <see cref="ModuleImports"/>.
        /// </summary>
        /// <param name="moduleImport">The module import to add.</param>
        /// <exception cref="ArgumentException">The <paramref name="moduleImport"/> argument is not valid.</exception>
        /// <exception cref="InvalidOperationException">A module with the same <see cref="JsModuleImport.Name"/> has already been added.</exception>
        public void AddModuleImport(JsModuleImport moduleImport)
        {
            if (moduleImport.Equals(default))
            {
                throw new ArgumentException($"{nameof(JsModuleImport)} has not been initialized.", nameof(moduleImport));
            }

            if (_importsFrozen)
            {
                throw new InvalidOperationException("Module imports cannot be added to a frozen context.");
            }

            var existingImportWithName = _moduleImports.SingleOrDefault(import => import.Name.Equals(moduleImport.Name, JsModuleImport.NameComparison));

            if (existingImportWithName != default)
            {
                if (!existingImportWithName.Equals(moduleImport))
                {
                    throw new InvalidOperationException(
                        $"A module named '{moduleImport.Name}' has already been added with path '{existingImportWithName.Path}'.");
                }
                else
                {
                    // Matching import already exists
                    return;
                }
            }

            _moduleImports.Add(moduleImport);
        }

        /// <summary>
        /// Adds a new module import to <see cref="ModuleImports"/>.
        /// </summary>
        /// <param name="moduleName">The name of the module.</param>
        /// <param name="modulePath">The path to the module.</param>
        /// <exception cref="ArgumentException">The <paramref name="moduleName"/> or <paramref name="modulePath"/> argument is not valid.</exception>
        /// <exception cref="InvalidOperationException">A module with the same <see cref="JsModuleImport.Name"/> has already been added.</exception>
        public void AddModuleImport(string moduleName, string modulePath)
        {
            var jsModuleImport = new JsModuleImport(moduleName, modulePath);
            AddModuleImport(jsModuleImport);
        }

        /// <summary>
        /// Freezes this <see cref="JsModuleContext"/> to prevent any additional module imports from being added.
        /// </summary>
        /// <exception cref="InvalidOperationException">The <see cref="JsModuleContext"/> has already been frozen.</exception>
        internal void FreezeModuleImports()
        {
            if (_importsFrozen)
            {
                throw new InvalidOperationException($"{nameof(JsModuleContext)} is already frozen.");
            }

            _importsFrozen = true;
        }
    }
}
