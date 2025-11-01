using AzureTable.Provider.Extensions;
using System.Linq.Expressions;

namespace AzureTable.Provider.Configuration.Mapping.Elements;

// Generic constraint on interface ensures type consistency in
// implementations so warning has been suppressed.
#pragma warning disable S2326 // Unused type parameters should be removed
internal interface IElement<T> where T : class;
#pragma warning restore S2326 // Unused type parameters should be removed

internal interface IKeyedElement<T> : IElement<T> where T : class
{
    string GetKey(T instance);
}

internal abstract class ContainerElement<T> : IElement<T> where T : class
{
    private readonly List<IElement<T>> _elements = [];
    public bool HasElements => _elements.Count > 0;
    public IReadOnlyCollection<IElement<T>> Elements => _elements;
    protected virtual void ValidateElement(IElement<T> element)
    {
        if (element is not IKeyedElement<T>)
        {
            throw new ArgumentException("Container element can only contain keyed elements");
        }
    }

    public void Add(IElement<T> element)
    {
        ValidateElement(element);
        _elements.Add(element);
    }
}


internal sealed class RootElement<T> : ContainerElement<T>, IElement<T> where T : class;

internal sealed class ObjectElement<T> : ContainerElement<T> where T : class;

internal sealed class NamedObjectElement<T> : KeyedElement<T> where T : class
{
    private readonly ObjectElement<T> _objectElement = new();
    public NamedObjectElement(string staticName) : base(staticName)
    {
    }

    public NamedObjectElement(Expression<Func<T, string>> expression) : base(expression)
    {
    }

    public ObjectElement<T> AsObjectElement() => _objectElement;

    public static implicit operator ObjectElement<T>(NamedObjectElement<T> element)
        => element.AsObjectElement();
}

internal sealed class ComplexArrayContainer<T> : ContainerElement<T> where T : class
{
    protected override void ValidateElement(IElement<T> element)
    {
        if (element is not ObjectElement<T>)
        {
            throw new ArgumentException("Complex arrays can only contain object elements");
        }
    }
}

internal abstract class KeyedElement<T> : IKeyedElement<T> where T : class
{
    private readonly ElementKey<T> _elementKey;
    protected KeyedElement(string staticName)
        => _elementKey = ElementKey<T>.FromStaticKey(staticName);

    protected KeyedElement(Expression<Func<T, string>> expression)
        => _elementKey = ElementKey<T>.FromMemberExpression(expression);

    public string GetKey(T instance)
        => _elementKey.ExtractKey(instance);
}

internal sealed class ElementKey<T> where T : class
{
    private readonly Func<T, string> _keyFactory;
    public string Name { get; }
    public string ExtractKey(T instance)
        => _keyFactory(instance);

    private ElementKey(Func<T, string> keyFactory, string name)
        => (_keyFactory, Name) = (keyFactory, name);

    public static ElementKey<T> FromStaticKey(string key)
    {
        key = key?.Trim() ?? string.Empty;
        ArgumentException.ThrowIfNullOrEmpty(key);

        return new(_ => key, $"From static: {key}");
    }

    public static ElementKey<T> FromMemberExpression(Expression<Func<T, string>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var name = $"From member expression: {expression.GetMemberName()}";

        var compiled = expression.Compile();
        string KeyFactory(T instance)
        {
            var key = compiled(instance)?.Trim();
            return !string.IsNullOrWhiteSpace(key)
                ? key
                : throw new InvalidOperationException($"Value extracted for {expression.GetMemberName()} cannot be null, empty or whitespace when used as a section key.");
        }

        return new(KeyFactory, name);
    }
}

internal sealed class ComplexArrayElement<T> : KeyedElement<T> where T : class
{
    private readonly List<ObjectElement<T>> _elements = [];
    public IReadOnlyCollection<ObjectElement<T>> Elements => _elements;

    public ComplexArrayElement(Expression<Func<T, string>> expression) : base(expression)
    {
    }
    public ComplexArrayElement(string staticKey) : base(staticKey)
    {
    }

    public void Add(ObjectElement<T> item) => _elements.Add(item);
}

internal sealed class LeafValue<T>
{
    private readonly Func<T, string?> _mapValue;

    private LeafValue(Func<T, string?> mapValue)
        => _mapValue = mapValue;

    public string? MapFrom(T instance)
        => _mapValue(instance);

    public static LeafValue<T> Create<TValue>(Func<T, TValue?> valueFactory)
    {
        string? Mapper(T instance)
            => valueFactory(instance).ToInvariantString();

        return new(Mapper);
    }

}

internal sealed class ValueElement<T> : KeyedElement<T> where T : class
{
    public LeafValue<T> Value { get; }
    private ValueElement(string staticName, LeafValue<T> leafValue) : base(staticName)
        => Value = leafValue;

    public static ValueElement<T> CreateFrom<TValue>(Func<T, TValue> valueFactory, string name)
    {
        ArgumentNullException.ThrowIfNull(valueFactory);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new(name, LeafValue<T>.Create(valueFactory));
    }

    public static ValueElement<T> CreateFrom<TValue>(Expression<Func<T, TValue>> expression, string? nameOverride = null)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var name = !string.IsNullOrWhiteSpace(nameOverride) ? nameOverride : expression.GetMemberName();
        return new(name, LeafValue<T>.Create(expression.Compile()));
    }
}

internal sealed class SimpleArrayElement<T> : KeyedElement<T> where T : class
{
    private readonly List<LeafValue<T>> _values = [];
    public IReadOnlyCollection<LeafValue<T>> Values => _values;
    public SimpleArrayElement(string staticName)
        : base(staticName)
    {

    }

    public SimpleArrayElement(Expression<Func<T, string>> expression)
        : base(expression)
    {

    }

    public void Add(LeafValue<T> value) => _values.Add(value);

    public void Add<TValue>(Func<T, TValue> factory) => _values.Add(LeafValue<T>.Create(factory));
}

