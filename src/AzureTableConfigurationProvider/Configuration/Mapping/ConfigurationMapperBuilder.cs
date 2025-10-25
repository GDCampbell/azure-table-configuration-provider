using AzureTable.Provider.Configuration.Mapping.Elements;
using AzureTable.Provider.Extensions;
using System.Linq.Expressions;

namespace AzureTable.Provider.Configuration.Mapping;

public interface IConfigurationMapper<T> where T : class
{
    IConfigurationMapper<T> AddSection(string key, Action<IConfigurationMapper<T>> configure);
    IConfigurationMapper<T> AddSection(Expression<Func<T, string>> keyExpression, Action<IConfigurationMapper<T>> configure);
    IConfigurationMapper<T> WithValue<TValue>(Expression<Func<T, TValue>> valueExpression, string name) where TValue : struct, IFormattable;
    IConfigurationMapper<T> WithValue<TValue>(Expression<Func<T, TValue?>> valueExpression, string name) where TValue : struct, IFormattable;
    IConfigurationMapper<T> WithValue(Expression<Func<T, string?>> valueExpression, string name);

    IConfigurationMapper<T> WithValue<TValue>(Expression<Func<T, TValue>> valueExpression) where TValue : struct, IFormattable;
    IConfigurationMapper<T> WithValue<TValue>(Expression<Func<T, TValue?>> valueExpression) where TValue : struct, IFormattable;
    IConfigurationMapper<T> WithValue(Expression<Func<T, string?>> valueExpression);
}

internal interface IConfigurationMapperBuilder<TEntity> where TEntity : class
{
    MappingRootSection<TEntity> Build();
}

internal sealed class ConfigurationMapperBuilder<TEntity> : IConfigurationMapper<TEntity>, IConfigurationMapperBuilder<TEntity> where TEntity : class
{
    private readonly MappingSection<TEntity> _section;

    internal ConfigurationMapperBuilder() => _section = new MappingRootSection<TEntity>();

    private ConfigurationMapperBuilder(MappingKeySection<TEntity> section) => _section = section;

    public IConfigurationMapper<TEntity> AddSection(string key, Action<IConfigurationMapper<TEntity>> configure)
    {
        key = key?.Trim() ?? string.Empty;
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(configure);

        return AddSection(new MappingKeySection<TEntity>(key), configure, key);
    }

    public IConfigurationMapper<TEntity> AddSection(Expression<Func<TEntity, string>> keyExpression, Action<IConfigurationMapper<TEntity>> configure)
    {
        ArgumentNullException.ThrowIfNull(keyExpression);
        ArgumentNullException.ThrowIfNull(configure);

        return AddSection(new MappingKeySection<TEntity>(keyExpression), configure, keyExpression.GetMemberName());
    }

    // public IConfigurationMapper<TEntity> AddPrimitiveArraySection<TValue>(string key, Action<IPrimitiveArrayMapperBuilder<TEntity, TValue>> configure) where TValue : struct, IFormattable
    // {
    //     key = key?.Trim() ?? string.Empty;
    //     ArgumentException.ThrowIfNullOrWhiteSpace(key);
    //     ArgumentNullException.ThrowIfNull(configure);

    //     var arrayBuilder = new PrimitiveArrayMapperBuilder<TEntity, TValue>(key);

    //     return AddPrimitiveArray(arrayBuilder, configure);
    // }

    // public IConfigurationMapper<TEntity> AddPrimitiveArraySection<TValue>(Expression<Func<TEntity, string>> keyExpression, Action<IPrimitiveArrayMapperBuilder<TEntity, TValue>> configure) where TValue : struct, IFormattable
    // {
    //     ArgumentNullException.ThrowIfNull(keyExpression);
    //     ArgumentNullException.ThrowIfNull(configure);

    //     var arrayBuilder = new PrimitiveArrayMapperBuilder<TEntity, TValue>(keyExpression);

    //     return AddPrimitiveArray(arrayBuilder, configure);
    // }

    // private IConfigurationMapper<TEntity> AddPrimitiveArray<TValue>(PrimitiveArrayMapperBuilder<TEntity, TValue> arrayBuilder, Action<IPrimitiveArrayMapperBuilder<TEntity, TValue>> configure) where TValue : struct, IFormattable
    // {
    //     configure(arrayBuilder);

    //     var arraySection = arrayBuilder.Build();

    //     _section.Add(arraySection);
    //     return this;
    // }

    MappingRootSection<TEntity> IConfigurationMapperBuilder<TEntity>.Build()
        => _section switch
        {
            MappingRootSection<TEntity> rootSection => rootSection.HasElements ? rootSection : throw new InvalidOperationException("At least one section or value must be configured for the mapper."),
            _ => throw new InvalidOperationException($"{nameof(Build)} can only be called on the root section.")
        };

    internal MappingRootSection<TEntity> Build() => ((IConfigurationMapperBuilder<TEntity>)this).Build();

    public IConfigurationMapper<TEntity> WithValue<TValue>(Expression<Func<TEntity, TValue>> valueExpression, string name) where TValue : struct, IFormattable
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return WithValueInternal(valueExpression, name);
    }

    public IConfigurationMapper<TEntity> WithValue<TValue>(Expression<Func<TEntity, TValue?>> valueExpression, string name) where TValue : struct, IFormattable
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return WithValueInternal(valueExpression, name);
    }

    public IConfigurationMapper<TEntity> WithValue(Expression<Func<TEntity, string?>> valueExpression, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return WithValueInternal(valueExpression, name);
    }

    public IConfigurationMapper<TEntity> WithValue<TValue>(Expression<Func<TEntity, TValue>> valueExpression) where TValue : struct, IFormattable
    {
        return WithValueInternal(valueExpression, null);
    }

    public IConfigurationMapper<TEntity> WithValue<TValue>(Expression<Func<TEntity, TValue?>> valueExpression) where TValue : struct, IFormattable
    {
        return WithValueInternal(valueExpression, null);
    }

    public IConfigurationMapper<TEntity> WithValue(Expression<Func<TEntity, string?>> valueExpression)
    {
        return WithValueInternal(valueExpression, null);
    }

    private ConfigurationMapperBuilder<TEntity> AddSection(MappingKeySection<TEntity> section, Action<IConfigurationMapper<TEntity>> configure, string sectionName)
    {
        var builder = new ConfigurationMapperBuilder<TEntity>(section);
        configure(builder);

        if (!section.HasElements)
        {
            throw new InvalidOperationException($"Configured section \"{sectionName}\" must have at least one child element.");
        }

        _section.Add(section);
        return this;
    }

    private ConfigurationMapperBuilder<TEntity> WithValueInternal<TValue>(Expression<Func<TEntity, TValue>> valueExpression, string? name)
    {
        ArgumentNullException.ThrowIfNull(valueExpression);
        var valueElement = MappingValueElement<TEntity>.Create(name ?? valueExpression.GetMemberName(), valueExpression.Compile());

        _section.Add(valueElement);

        return this;
    }
}
