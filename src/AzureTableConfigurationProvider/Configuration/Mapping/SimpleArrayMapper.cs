using AzureTable.Provider.Configuration.Mapping.Elements;
using System.Linq.Expressions;

namespace AzureTable.Provider.Configuration.Mapping;

public sealed class SimpleArrayMapper<TEntity, TValue>
    where TEntity : class
{
    internal SimpleArrayElement<TEntity> Element { get; }

    internal SimpleArrayMapper(string name)
        => Element = new(name);

    internal SimpleArrayMapper(Expression<Func<TEntity, string>> nameExpression)
        => Element = new(nameExpression);

    public SimpleArrayMapper<TEntity, TValue> AddItem(Func<TEntity, TValue> itemFactory)
    {
        Element.Add(itemFactory);
        return this;
    }
}
