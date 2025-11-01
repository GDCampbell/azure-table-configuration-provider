using AzureTable.Provider.Configuration.Mapping.Elements;
using AzureTable.Provider.Extensions;
using System.Linq.Expressions;

namespace AzureTable.Provider.Configuration.Mapping;

public sealed class ObjectMapper<TEntity> : BaseConfigurationMapper<TEntity> where TEntity : class
{
    internal IElement<TEntity> Element;
    internal ObjectMapper(NamedObjectElement<TEntity> namedObjectElement) : base(namedObjectElement.AsObjectElement())
    {
        Element = namedObjectElement;
    }

    internal ObjectMapper(ObjectElement<TEntity> objectElement) : base(objectElement)
    {
        Element = objectElement;
    }
}

public sealed class TypedObjectMapper<TEntity, TObject> : BaseConfigurationMapper<TEntity>
    where TEntity : class
    where TObject : class
{
    internal IElement<TEntity> Element { get; }

    internal TypedObjectMapper(NamedObjectElement<TEntity> namedObjectElement) : base(namedObjectElement)
    {
        Element = namedObjectElement;
    }

    internal TypedObjectMapper(ObjectElement<TEntity> objectElement) : base(objectElement)
    {
        Element = objectElement;
    }

    public TypedObjectMapper<TEntity, TObject> AddTypedObject<TValue>(Expression<Func<TObject, TValue>> keyExpression, Action<TypedObjectMapper<TEntity, TValue>> configure)
        where TValue : class
    {
        ArgumentNullException.ThrowIfNull(keyExpression);
        ArgumentNullException.ThrowIfNull(configure);

        var namedObjectElement = new NamedObjectElement<TEntity>(keyExpression.GetMemberName());
        var objectMapper = new TypedObjectMapper<TEntity, TValue>(namedObjectElement);

        configure(objectMapper);

        Container.Add(namedObjectElement);

        return this;
    }

    /// <summary>
    /// Adds a value mapping for the specified property using the provided value factory.
    /// </summary>
    /// <remarks>Use this method to associate a computed value with a property of the mapped object. This
    /// enables custom value generation during the mapping process.</remarks>
    /// <typeparam name="TValue">The type of the value to map. Must be a value type that implements IFormattable.</typeparam>
    /// <param name="keyExpression">An expression that identifies the property of the object to map the value to. Cannot be null.</param>
    /// <param name="valueFactory">A function that produces the value to be mapped from the entity. Cannot be null.</param>
    /// <returns>The current instance of the mapper with the new value mapping applied.</returns>
    public TypedObjectMapper<TEntity, TObject> WithMappedValue<TValue>(Expression<Func<TObject, TValue>> keyExpression, Func<TEntity, TValue> valueFactory)
        where TValue : struct, IFormattable
    {
        ArgumentNullException.ThrowIfNull(keyExpression);
        ArgumentNullException.ThrowIfNull(valueFactory);

        var valueElement = ValueElement<TEntity>.CreateFrom(valueFactory, keyExpression.GetMemberName());
        Container.Add(valueElement);

        return this;
    }

    public TypedObjectMapper<TEntity, TObject> WithMappedValue<TValue>(Expression<Func<TObject, TValue>> keyExpression, Func<TEntity, TValue?> valueFactory)
        where TValue : struct, IFormattable
    {
        ArgumentNullException.ThrowIfNull(keyExpression);
        ArgumentNullException.ThrowIfNull(valueFactory);

        var valueElement = ValueElement<TEntity>.CreateFrom(valueFactory, keyExpression.GetMemberName());
        Container.Add(valueElement);

        return this;
    }

    public TypedObjectMapper<TEntity, TObject> WithMappedValue(Expression<Func<TObject, string>> keyExpression, Func<TEntity, string?> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(keyExpression);
        ArgumentNullException.ThrowIfNull(valueFactory);

        var valueElement = ValueElement<TEntity>.CreateFrom(valueFactory, keyExpression.GetMemberName());
        Container.Add(valueElement);

        return this;
    }

    public TypedObjectMapper<TEntity, TObject> AddMappedComplexArray<TValue>(Expression<Func<TObject, IEnumerable<TValue>>> keyExpression, Action<TypedComplexArrayMapper<TEntity, TValue>> configure)
        where TValue : class
    {
        ArgumentNullException.ThrowIfNull(keyExpression);
        ArgumentNullException.ThrowIfNull(configure);

        var complexTypedArray = new TypedComplexArrayMapper<TEntity, TValue>(keyExpression.GetMemberName());

        configure(complexTypedArray);

        Container.Add(complexTypedArray.Element);

        return this;
    }
}

