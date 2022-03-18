using System;
using System.Diagnostics.CodeAnalysis;

namespace AspNetCoreJsModules
{
    /// <summary>
    /// Represents a JavaScript module import.
    /// </summary>
    public struct JsModuleImport : IEquatable<JsModuleImport>
    {
        internal static StringComparison NameComparison { get; } = StringComparison.Ordinal;
        internal static StringComparison PathComparison { get; } = StringComparison.Ordinal;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsModuleImport"/> struct.
        /// </summary>
        /// <param name="name">The name of the module.</param>
        /// <param name="path">The path to the module.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> or <paramref name="path"/> argument is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="name"/> or <paramref name="path"/> argument is not valid.</exception>
        public JsModuleImport(string name, string path)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name == string.Empty)
            {
                throw new ArgumentException($"{nameof(name)} cannot be empty.", nameof(name));
            }

            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path == string.Empty)
            {
                throw new ArgumentException($"{nameof(path)} cannot be empty.", nameof(path));
            }

            Name = name;
            Path = path;
        }

        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the path to the module.
        /// </summary>
        public string Path { get; }

        /// <inheritdoc/>
        public static bool operator ==(JsModuleImport left, JsModuleImport right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(JsModuleImport left, JsModuleImport right) => !(left == right);

        /// <inheritdoc/>
        public bool Equals([AllowNull] JsModuleImport other) =>
            string.Equals(Path, other.Path, PathComparison) && string.Equals(Name, other.Name, NameComparison);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is JsModuleImport jsModuleImport && Equals(jsModuleImport);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Name, Path);
    }
}
