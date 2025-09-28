using AzureTable.Provider.Extensions;
using System.Globalization;
using System.Linq.Expressions;

namespace AzureTable.Provider.Configuration.Mapping.Elements;

internal interface ITraversableMappingElement<T>
{
    /// <summary>
/// Extracts zero or more key/value pairs from the given instance using the provided path as the base.
/// </summary>
/// <param name="instance">The source object to read values from.</param>
/// <param name="currentPath">The current section path to which extracted segment names will be appended.</param>
/// <returns>An enumerable of key/value pairs where each key is the full path string and each value is the corresponding string value or null.</returns>
IEnumerable<KeyValuePair<string, string?>> ExtractFrom(T instance, SectionPath currentPath);
}

internal interface IMappingSection<T>
{
    /// <summary>
/// Adds a traversable mapping element as a child of this mapping section.
/// </summary>
/// <param name="element">The element to add to the section's child elements.</param>
void Add(ITraversableMappingElement<T> element);
    bool HasElements { get; }
}

internal abstract class MappingSection<T> : IMappingSection<T> where T : class
{
    protected readonly List<ITraversableMappingElement<T>> _elements = [];

    public bool HasElements => _elements.Count > 0;

    /// <summary>
    /// Adds a traversable mapping element as a child of this mapping section.
    /// </summary>
    /// <param name="element">The traversable mapping element to add.</param>
    public void Add(ITraversableMappingElement<T> element)
    {
        _elements.Add(element);
    }
}

internal sealed class MappingRootSection<T> : MappingSection<T> where T : class
{
    /// <summary>
        /// Extracts all configured key/value mappings from the provided instance using the root path.
        /// </summary>
        /// <param name="instance">The source object from which to derive mapping keys and values.</param>
        /// <returns>An enumerable of key/value pairs where each key is a full section path and each value is the corresponding string value (or null).</returns>
        /// <exception cref="InvalidOperationException">Thrown when the root section has no child elements.</exception>
        public IEnumerable<KeyValuePair<string, string?>> ExtractFrom(T instance)
        => HasElements
        ? _elements.SelectMany(e => e.ExtractFrom(instance, SectionPath.Root))
        : throw new InvalidOperationException("Root level must have at least one child element.");
}

internal abstract class MappingKeySection<T>(Func<T, string> keyFactory) : MappingSection<T>, ITraversableMappingElement<T> where T : class
{
    /// <summary>
    /// Extracts the child elements' key/value pairs for the given instance under a section whose key is computed from the instance and appended to the provided path.
    /// </summary>
    /// <param name="instance">The instance to extract values from.</param>
    /// <param name="currentPath">The current section path to which the computed key segment will be appended.</param>
    /// <returns>An enumerable of key/value pairs representing flattened extraction results from this section's child elements; keys are full path strings and values may be null.</returns>
    /// <exception cref="InvalidOperationException">Thrown when this key section has no child elements.</exception>
    public IEnumerable<KeyValuePair<string, string?>> ExtractFrom(T instance, SectionPath currentPath)
    {
        if (!HasElements)
        {
            throw new InvalidOperationException("Key section must have at least one child element.");
        }

        var key = keyFactory(instance).Trim();

        var newPath = currentPath.AddSegment(key);

        return _elements.SelectMany(e => e.ExtractFrom(instance, newPath));
    }
}

internal sealed class MappingStaticKeySection<T>(string key) : MappingKeySection<T>(_ => key) where T : class;

internal sealed class MappingDynamicKeySection<T> : MappingKeySection<T> where T : class
{
    /// <summary>
        /// Initializes a new instance that produces a section key for each mapped instance using the provided key factory.
        /// </summary>
        /// <param name="keyFactory">A function that derives the section key from an instance of <typeparamref name="T"/>.</param>
        private MappingDynamicKeySection(Func<T, string> keyFactory)
        : base(keyFactory) { }

    /// <summary>
    /// Creates a dynamic key section that derives a section key from the provided string member expression.
    /// </summary>
    /// <param name="expression">An expression that selects the string member used as the section key; the member name is used in error messages.</param>
    /// <returns>A MappingDynamicKeySection&lt;T&gt; that computes and validates keys by evaluating the compiled expression against instances of T.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the evaluated key is null, empty, or whitespace for a given instance.</exception>
    public static MappingDynamicKeySection<T> Create(Expression<Func<T, string>> expression)
    {
        var compiled = expression.Compile();

        string KeyFactory(T instance)
        {
            var key = compiled(instance)?.Trim();

            return !string.IsNullOrWhiteSpace(key)
                ? key
                : throw new InvalidOperationException($"Value for {expression.GetMemberName()} cannot be null, empty, or whitespace when used as a section key.");
        }

        return new(KeyFactory);
    }
}

internal sealed class MappingValueElement<T> : ITraversableMappingElement<T> where T : class
{
    private readonly string _name;
    private readonly Func<T, string?> _valueFactory;

    /// <summary>
        /// Initializes a new MappingValueElement with the given segment name and value factory.
        /// </summary>
        /// <param name="name">The path segment name to use for this value element.</param>
        /// <param name="valueFactory">A function that produces the element's string value for a given instance; may return null.</param>
        private MappingValueElement(string name, Func<T, string?> valueFactory)
        => (_name, _valueFactory) = (name, valueFactory);

    /// <summary>
    /// Creates a leaf mapping element that produces a string value for a specific member of T and associates it with a path segment name.
    /// </summary>
    /// <param name="expression">An expression selecting the member whose value will be extracted for this mapping element; its member name is used as the element name when <paramref name="nameOverride"/> is null or whitespace.</param>
    /// <param name="nameOverride">Optional explicit name for the path segment; when provided and not whitespace, this value (trimmed) is used instead of the expression's member name.</param>
    /// <returns>A <see cref="MappingValueElement{T}"/> that yields a single key/value pair where the key is the current path extended by the element name and the value is the selected member converted to a string.</returns>
    /// <exception cref="ArgumentException">Thrown when the resulting element name (derived from <paramref name="nameOverride"/> or the expression) is null, empty, or whitespace.</exception>
    public static MappingValueElement<T> Create<TValue>(Expression<Func<T, TValue>> expression, string? nameOverride = null)
    {
        var name = (string.IsNullOrWhiteSpace(nameOverride)
            ? expression.GetMemberName()
            : nameOverride)
            .Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null, empty, or whitespace.", nameof(nameOverride));
        }

        var compiled = expression.Compile();

        string? ValueFactory(T instance)
        {
            var value = compiled(instance);

            return value switch
            {
                IFormattable fmt => fmt.ToString(null, CultureInfo.InvariantCulture),
                not null => value.ToString(),
                _ => null
            };
        }

        return new(name, ValueFactory);
    }

    /// <summary>
    /// Produces a single key/value mapping entry by appending this element's name to the provided path and computing its value from the instance.
    /// </summary>
    /// <param name="instance">The source object from which the element's value is obtained.</param>
    /// <param name="currentPath">The current section path to which this element's name will be appended.</param>
    /// <returns>An <see cref="IEnumerable{KeyValuePair}"/> containing one entry: key is the full path after adding this element's name, value is the computed string or null.</returns>
    public IEnumerable<KeyValuePair<string, string?>> ExtractFrom(T instance, SectionPath currentPath)
    {
        var path = currentPath.AddSegment(_name);

        yield return new(path.Path, _valueFactory(instance));
    }
}