using System.Linq.Expressions;
using AzureTable.Provider.Configuration.Mapping.Elements;
using AzureTable.Provider.Extensions;

namespace AzureTable.Provider.Configuration.Mapping;

public partial interface IConfigurationMapper<T> where T : class
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

public sealed partial class ConfigurationMapper<TEntity> /*: IConfigurationMapper<TEntity>, IConfigurationMapperBuilder<TEntity>*/ where TEntity : class
{
    internal MappingSection<TEntity> Section => _section switch
    {
        MappingRootSection<TEntity> rootSection => rootSection.HasElements
                    ? rootSection
                    : throw new InvalidOperationException("At least one section or value must be configured in the mapper."),
        _ => throw new InvalidOperationException($"Can only be called on the root section.")
    };

    private readonly MappingSection<TEntity> _section;

    internal ConfigurationMapper() => _section = new MappingRootSection<TEntity>();

    private ConfigurationMapper(MappingKeySection<TEntity> section) => _section = section;



    public ConfigurationMapper<TEntity> AddSection(string key, Action<ConfigurationMapper<TEntity>> configure)
    {
        key = key?.Trim() ?? string.Empty;
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(configure);

        return AddSection(new MappingKeySection<TEntity>(key), configure);
    }

    public ConfigurationMapper<TEntity> AddSection(Expression<Func<TEntity, string>> keyExpression, Action<ConfigurationMapper<TEntity>> configure)
    {
        ArgumentNullException.ThrowIfNull(keyExpression);
        ArgumentNullException.ThrowIfNull(configure);

        return AddSection(new MappingKeySection<TEntity>(keyExpression), configure);
    }

    public ConfigurationMapper<TEntity> WithValue<TValue>(Expression<Func<TEntity, TValue>> valueExpression, string name) where TValue : struct, IFormattable
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return WithValueInternal(valueExpression, name);
    }

    public ConfigurationMapper<TEntity> WithValue<TValue>(Expression<Func<TEntity, TValue?>> valueExpression, string name) where TValue : struct, IFormattable
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return WithValueInternal(valueExpression, name);
    }

    public ConfigurationMapper<TEntity> WithValue(Expression<Func<TEntity, string?>> valueExpression, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return WithValueInternal(valueExpression, name);
    }

    public ConfigurationMapper<TEntity> WithValue<TValue>(Expression<Func<TEntity, TValue>> valueExpression) where TValue : struct, IFormattable
    {
        return WithValueInternal(valueExpression, null);
    }

    public ConfigurationMapper<TEntity> WithValue<TValue>(Expression<Func<TEntity, TValue?>> valueExpression) where TValue : struct, IFormattable
    {
        return WithValueInternal(valueExpression, null);
    }

    public ConfigurationMapper<TEntity> WithValue(Expression<Func<TEntity, string?>> valueExpression)
    {
        return WithValueInternal(valueExpression, null);
    }

    private ConfigurationMapper<TEntity> AddSection(MappingKeySection<TEntity> section, Action<ConfigurationMapper<TEntity>> configure)
    {
        var builder = new ConfigurationMapper<TEntity>(section);
        configure(builder);

        if (!section.HasElements)
        {
            throw new InvalidOperationException($"Configured section \"{section.Name}\" must have at least one child element.");
        }

        _section.Add(section);
        return this;
    }

    private ConfigurationMapper<TEntity> WithValueInternal<TValue>(Expression<Func<TEntity, TValue>> valueExpression, string? name)
    {
        ArgumentNullException.ThrowIfNull(valueExpression);
        var valueElement = MappingValueElement<TEntity>.Create(name ?? valueExpression.GetMemberName(), valueExpression.Compile());

        _section.Add(valueElement);

        return this;
    }
}
