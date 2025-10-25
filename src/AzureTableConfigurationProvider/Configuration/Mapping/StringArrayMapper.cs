using System.Linq.Expressions;
using AzureTable.Provider.Configuration.Mapping.Elements;

namespace AzureTable.Provider.Configuration.Mapping;

public interface IStringArrayMapper<TEntity> where TEntity : class
{
    IStringArrayMapper<TEntity> AddItem(Func<TEntity, string?> itemFactory);
    IStringArrayMapper<TEntity> AddItem(string? item);
}

public sealed class StringArrayMapper<TEntity> /*: IStringArrayMapper<TEntity>*/ where TEntity : class
{
    internal PrimitiveArrayElement<TEntity> Section { get; }
    internal StringArrayMapper(string sectionName) => Section = new(sectionName);
    internal StringArrayMapper(Expression<Func<TEntity, string>> sectionNameExpression)
        => Section = new(sectionNameExpression);

    public StringArrayMapper<TEntity> AddItem(Func<TEntity, string?> itemFactory)
    {
        Section.AddItem(itemFactory);
        return this;
    }

    public StringArrayMapper<TEntity> AddItem(string? item)
    {
        Section.AddItem(item);
        return this;
    }

    //IStringArrayMapper<TEntity> IStringArrayMapper<TEntity>.AddItem(Func<TEntity, string?> itemFactory) => AddItem(itemFactory);
    //IStringArrayMapper<TEntity> IStringArrayMapper<TEntity>.AddItem(string? item) => AddItem(item);
}