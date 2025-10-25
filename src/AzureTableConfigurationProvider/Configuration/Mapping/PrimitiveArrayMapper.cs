using System.Linq.Expressions;
using AzureTable.Provider.Configuration.Mapping.Elements;

namespace AzureTable.Provider.Configuration.Mapping;

public interface IPrimitiveArrayMapper<TEntity, TValue>
where TEntity : class
where TValue : struct, IFormattable
{
    IPrimitiveArrayMapper<TEntity, TValue> AddItem(Func<TEntity, TValue?> itemFactory);
    IPrimitiveArrayMapper<TEntity, TValue> AddItem(Func<TEntity, TValue> itemFactory);
    IPrimitiveArrayMapper<TEntity, TValue> AddItem(TValue value);
    IPrimitiveArrayMapper<TEntity, TValue> AddItem(TValue? value);
}

// TODO: get rid of interface and just make the class public with certains properties/fields/methods internal/private
public sealed class PrimitiveArrayMapper<TEntity, TValue> /*: IPrimitiveArrayMapper<TEntity, TValue>*/ where TEntity : class
    where TValue : struct, IFormattable
{
    internal PrimitiveArrayElement<TEntity> Section { get; } // TODO: validate if it's empty or not? 

    internal PrimitiveArrayMapper(string sectionName) => Section = new(sectionName);

    internal PrimitiveArrayMapper(Expression<Func<TEntity, string>> sectionNameExpression)
        => Section = new(sectionNameExpression);

    public PrimitiveArrayMapper<TEntity, TValue> AddItem(Func<TEntity, TValue> itemFactory)
    {
        ArgumentNullException.ThrowIfNull(itemFactory);
        Section.AddItem(itemFactory);
        return this;
    }

    public PrimitiveArrayMapper<TEntity, TValue> AddItem(Func<TEntity, TValue?> itemFactory)
    {
        ArgumentNullException.ThrowIfNull(itemFactory);
        Section.AddItem(itemFactory);
        return this;
    }

    public PrimitiveArrayMapper<TEntity, TValue> AddItem(TValue value)
    {
        Section.AddItem(value);
        return this;
    }

    public PrimitiveArrayMapper<TEntity, TValue> AddItem(TValue? value)
    {
        Section.AddItem(value);
        return this;
    }

    //IPrimitiveArrayMapper<TEntity, TValue> IPrimitiveArrayMapper<TEntity, TValue>.AddItem(Func<TEntity, TValue> itemFactory) => AddItem(itemFactory);
    //IPrimitiveArrayMapper<TEntity, TValue> IPrimitiveArrayMapper<TEntity, TValue>.AddItem(Func<TEntity, TValue?> itemFactory) => AddItem(itemFactory);
    //IPrimitiveArrayMapper<TEntity, TValue> IPrimitiveArrayMapper<TEntity, TValue>.AddItem(TValue value) => AddItem(value);
    //IPrimitiveArrayMapper<TEntity, TValue> IPrimitiveArrayMapper<TEntity, TValue>.AddItem(TValue? value) => AddItem(value);
}
