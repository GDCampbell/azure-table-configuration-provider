using System.Linq.Expressions;

namespace AzureTable.Provider.Configuration.Mapping;

public partial interface IConfigurationMapper<T> where T : class
{
    IConfigurationMapper<T> AddPrimitiveArraySection<TValue>(string sectionName, Action<IPrimitiveArrayMapper<T, TValue>> configure)
        where TValue : struct, IFormattable;

    IConfigurationMapper<T> AddPrimitiveArraySection<TValue>(Expression<Func<T, string>> sectionNameExpression, Action<IPrimitiveArrayMapper<T, TValue>> configure)
        where TValue : struct, IFormattable;

    IConfigurationMapper<T> AddStringArraySection(string sectionName, Action<IStringArrayMapper<T>> configure);
    IConfigurationMapper<T> AddStringArraySection(Expression<Func<T, string>> sectionNameExpression, Action<IStringArrayMapper<T>> configure);
}

public sealed partial class ConfigurationMapper<TEntity> /*: IConfigurationMapper<TEntity>*/ where TEntity : class
{
    public ConfigurationMapper<TEntity> AddPrimitiveArraySection<TValue>(string sectionName, Action<PrimitiveArrayMapper<TEntity, TValue>> configure) where TValue : struct, IFormattable
    {
        sectionName = sectionName?.Trim() ?? string.Empty;
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);
        ArgumentNullException.ThrowIfNull(configure);

        var arrayBuilder = new PrimitiveArrayMapper<TEntity, TValue>(sectionName);

        configure(arrayBuilder);

        _section.Add(arrayBuilder.Section);

        return this;
    }

    public ConfigurationMapper<TEntity> AddPrimitiveArraySection<TValue>(Expression<Func<TEntity, string>> sectionNameExpression, Action<PrimitiveArrayMapper<TEntity, TValue>> configure) where TValue : struct, IFormattable
    {
        ArgumentNullException.ThrowIfNull(sectionNameExpression);
        ArgumentNullException.ThrowIfNull(configure);

        var arrayBuilder = new PrimitiveArrayMapper<TEntity, TValue>(sectionNameExpression);

        configure(arrayBuilder);

        _section.Add(arrayBuilder.Section);

        return this;
    }

    public ConfigurationMapper<TEntity> AddStringArraySection(string sectionName, Action<StringArrayMapper<TEntity>> configure)
    {
        sectionName = sectionName?.Trim() ?? string.Empty;
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);
        ArgumentNullException.ThrowIfNull(configure);

        var arrayBuilder = new StringArrayMapper<TEntity>(sectionName);

        configure(arrayBuilder);

        _section.Add(arrayBuilder.Section);

        return this;
    }

    public ConfigurationMapper<TEntity> AddStringArraySection(Expression<Func<TEntity, string>> sectionNameExpression, Action<StringArrayMapper<TEntity>> configure)
    {
        ArgumentNullException.ThrowIfNull(sectionNameExpression);
        ArgumentNullException.ThrowIfNull(configure);

        var arrayBuilder = new StringArrayMapper<TEntity>(sectionNameExpression);

        configure(arrayBuilder);

        _section.Add(arrayBuilder.Section);

        return this;
    }

    //IConfigurationMapper<TEntity> IConfigurationMapper<TEntity>.AddPrimitiveArraySection<TValue>(string sectionName, Action<IPrimitiveArrayMapper<TEntity, TValue>> configure)
    //    => AddPrimitiveArraySection(sectionName, configure);

    //IConfigurationMapper<TEntity> IConfigurationMapper<TEntity>.AddPrimitiveArraySection<TValue>(Expression<Func<TEntity, string>> sectionNameExpression, Action<IPrimitiveArrayMapper<TEntity, TValue>> configure)
    //    => AddPrimitiveArraySection(sectionNameExpression, configure);

    //IConfigurationMapper<TEntity> IConfigurationMapper<TEntity>.AddStringArraySection(string sectionName, Action<IStringArrayMapper<TEntity>> configure)
    //    => AddStringArraySection(sectionName, configure);

    //IConfigurationMapper<TEntity> IConfigurationMapper<TEntity>.AddStringArraySection(Expression<Func<TEntity, string>> sectionNameExpression, Action<IStringArrayMapper<TEntity>> configure)
    //    => AddStringArraySection(sectionNameExpression, configure);
}
