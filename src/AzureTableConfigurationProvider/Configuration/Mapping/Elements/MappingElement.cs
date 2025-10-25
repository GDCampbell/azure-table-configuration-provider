using System.Globalization;
using System.Linq.Expressions;
using AzureTable.Provider.Extensions;

namespace AzureTable.Provider.Configuration.Mapping.Elements;

internal interface ITraversableMappingElement<T>
{
    IEnumerable<KeyValuePair<string, string?>> ExtractFrom(T instance, SectionPath currentPath);
}

internal interface IMappingSection<T>
{
    string Name { get; }
    void Add(ITraversableMappingElement<T> element);
    bool HasElements { get; }
}

internal abstract class MappingSection<T> : IMappingSection<T> where T : class
{
    public string Name { get; }
    protected readonly List<ITraversableMappingElement<T>> _elements = [];

    protected readonly Func<T, string> _keyFactory;

    protected MappingSection(string staticKey = "")
    {
        staticKey = staticKey?.Trim() ?? string.Empty;

        _keyFactory = _ => staticKey;
        Name = $"Fixed: {staticKey}";
    }

    protected MappingSection(Expression<Func<T, string>> keyExpression)
    {
        Name = $"MemberName: {keyExpression.GetMemberName()}";

        var compiled = keyExpression.Compile();
        string KeyFactory(T instance)
        {
            var key = compiled(instance)?.Trim();
            return !string.IsNullOrWhiteSpace(key)
                ? key
                : throw new InvalidOperationException($"Value for {Name} cannot be null, empty, or whitespace when used as a section key.");
        }
        _keyFactory = KeyFactory;
    }

    public bool HasElements => _elements.Count > 0;

    public void Add(ITraversableMappingElement<T> element)
    {
        _elements.Add(element);
    }
}

internal sealed class MappingRootSection<T> : MappingSection<T> where T : class
{
    public IEnumerable<KeyValuePair<string, string?>> ExtractFrom(T instance)
        => HasElements
        ? _elements.SelectMany(e => e.ExtractFrom(instance, SectionPath.Root))
        : throw new InvalidOperationException("Root level must have at least one child element.");
}

internal sealed class MappingKeySection<T> : MappingSection<T>, ITraversableMappingElement<T> where T : class
{
    public MappingKeySection(string staticKey)
        : base(staticKey)
    {
    }

    public MappingKeySection(Expression<Func<T, string>> keyExpression)
        : base(keyExpression)
    {
    }

    public IEnumerable<KeyValuePair<string, string?>> ExtractFrom(T instance, SectionPath currentPath)
    {
        if (!HasElements)
        {
            throw new InvalidOperationException("Section must have at least one child element.");
        }

        var key = _keyFactory(instance).Trim();

        var newPath = currentPath.AddSegment(key);

        return _elements.SelectMany(e => e.ExtractFrom(instance, newPath));
    }
}

internal sealed class MappingValueElement<T> : ITraversableMappingElement<T> where T : class
{
    private readonly string _name;
    private readonly Func<T, string?> _valueFactory;

    private MappingValueElement(string name, Func<T, string?> valueFactory)
        => (_name, _valueFactory) = (name, valueFactory);

    public static MappingValueElement<T> Create<TValue>(string name, Func<T, TValue> valueFactory)
    {
        name = name?.Trim() ?? string.Empty;
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        string? ValueFactory(T instance)
            => valueFactory(instance).ToInvariantString();

        return new(name, ValueFactory);
    }

    public IEnumerable<KeyValuePair<string, string?>> ExtractFrom(T instance, SectionPath currentPath)
    {
        var path = currentPath.AddSegment(_name);

        yield return new(path.Path, _valueFactory(instance));
    }
}

internal sealed class PrimitiveArrayElement<T> : MappingSection<T>, ITraversableMappingElement<T> where T : class
{
    private readonly List<Func<T, string?>> _itemFactories = [];

    public PrimitiveArrayElement(string staticKey)
        : base(staticKey)
    {
    }

    public PrimitiveArrayElement(Expression<Func<T, string>> keyExpression)
        : base(keyExpression)
    {
    }

    public IEnumerable<KeyValuePair<string, string?>> ExtractFrom(T instance, SectionPath currentPath)
    {
        var key = _keyFactory(instance).Trim();

        if (!string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("Array key cannot be null, empty, or whitespace.");
        }

        var basePath = currentPath.AddSegment(key);

        for (var i = 0; i < _itemFactories.Count; i++)
        {
            var itemFactory = _itemFactories[i];
            var itemPath = basePath.AddSegment(i.ToString(CultureInfo.InvariantCulture));

            var itemValue = itemFactory(instance);

            yield return new(itemPath.Path, itemValue);
        }
    }

    public void AddItem<TValue>(Func<T, TValue> valueFactory)
    {
        _itemFactories.Add(e => valueFactory(e).ToInvariantString());
    }

    public void AddItem<TValue>(TValue value)
    {
        _itemFactories.Add(_ => value.ToInvariantString());
    }
}