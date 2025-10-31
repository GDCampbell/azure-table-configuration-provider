using AzureTable.Provider.Configuration.Mapping.Elements;
using System.Linq.Expressions;

namespace AzureTable.Provider.Configuration.Mapping;

public sealed class ComplexArrayMapper<TEntity>
    where TEntity : class
{
    internal ComplexArrayElement<TEntity> Element { get; }

    internal ComplexArrayMapper(string name)
        => Element = new(name);

    internal ComplexArrayMapper(Expression<Func<TEntity, string>> nameExpression)
        => Element = new(nameExpression);

    public ComplexArrayMapper<TEntity> AddItem(Action<ObjectMapper<TEntity>> configure)
    {
        var objectElement = new ObjectElement<TEntity>();
        var objectMapper = new ObjectMapper<TEntity>(objectElement);

        configure(objectMapper);

        Element.Add(objectElement);

        return this;
    }
}

public sealed class TypedComplexArrayMapper<TEntity, TObject>
    where TEntity : class
    where TObject : class
{
    internal ComplexArrayElement<TEntity> Element { get; }

    internal TypedComplexArrayMapper(string name)
        => Element = new(name);

    internal TypedComplexArrayMapper(Expression<Func<TEntity, string>> nameExpression)
        => Element = new(nameExpression);

    public TypedComplexArrayMapper<TEntity, TObject> AddItem(Action<TypedObjectMapper<TEntity, TObject>> configure)
    {
        var objectElement = new ObjectElement<TEntity>();
        var typedObjectMapper = new TypedObjectMapper<TEntity, TObject>(objectElement);

        configure(typedObjectMapper);

        Element.Add(objectElement);

        return this;
    }
}