using AzureTable.Provider.Extensions;
using System.Linq.Expressions;

namespace AzureTable.Provider.Configuration.Mapping.Elements;

internal interface ITraversableMappingElement<T>
{
    IEnumerable<KeyValuePair<string, string?>> ExtractFrom(T instance, SectionPath currentPath);
}

internal interface IMappingSection<T>
{
    void Add(ITraversableMappingElement<T> element);
    bool HasElements { get; }
}

internal abstract class MappingSection<T> : IMappingSection<T> where T : class
{
    protected readonly List<ITraversableMappingElement<T>> _elements = [];

    public bool HasElements => _elements.Count > 0;

    public void Add(ITraversableMappingElement<T> element)
    {
        _elements.Add(element);
    }
}

internal sealed class MappingRootSection<T> : MappingSection<T> where T : class
{
    public IEnumerable<KeyValuePair<string, string?>> ExtractFrom(T instance)
        => _elements.Count > 0
        ? _elements.SelectMany(e => e.ExtractFrom(instance, SectionPath.Root))
        : throw new InvalidOperationException("Root level must have at least one child element.");
}

internal abstract class MappingKeySection<T>(Func<T, string> keyFactory) : MappingSection<T>, ITraversableMappingElement<T> where T : class
{
    public IEnumerable<KeyValuePair<string, string?>> ExtractFrom(T instance, SectionPath currentPath)
    {
        if (_elements.Count == 0)
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
    private MappingDynamicKeySection(Func<T, string> keyFactory)
        : base(keyFactory) { }

    public static MappingDynamicKeySection<T> Create<TKey>(Expression<Func<T, TKey>> expression) where TKey : notnull
    {
        var compiled = expression.Compile();

        string KeyFactory(T instance)
        {
            return compiled(instance) is TKey rawValue &&
                rawValue.ToString()?.Trim() is string strValue &&
                    !string.IsNullOrWhiteSpace(strValue)
                    ? strValue
                    : throw new InvalidOperationException($"Value for {expression.GetMemberName()} cannot be null, empty, or whitespace when used as a section key.");
        }

        return new(KeyFactory);
    }
}

internal sealed class MappingValueElement<T> : ITraversableMappingElement<T> where T : class
{
    private readonly string _name;
    private readonly Func<T, string?> _valueFactory;

    private MappingValueElement(string name, Func<T, string?> valueFactory)
        => (_name, _valueFactory) = (name, valueFactory);

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

        string? ValueFactory(T instance) => compiled(instance)?.ToString();

        return new(name, ValueFactory);
    }

    public IEnumerable<KeyValuePair<string, string?>> ExtractFrom(T instance, SectionPath currentPath)
    {
        var path = currentPath.AddSegment(_name);

        yield return new(path.Path, _valueFactory(instance));
    }
}