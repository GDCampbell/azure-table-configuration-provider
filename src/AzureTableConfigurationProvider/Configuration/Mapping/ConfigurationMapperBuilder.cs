using Azure.Data.Tables;
using AzureTable.Provider.Configuration.Mapping.Elements;
using AzureTable.Provider.Extensions;
using System.Linq.Expressions;

namespace AzureTable.Provider.Configuration.Mapping;

public partial interface IConfigurationMapper<TEntity> where TEntity : class, ITableEntity
{
    /// <summary>
    /// Adds a new configuration section with the specified name and applies the provided configuration action to it.
    /// </summary>
    /// <param name="name">The name of the configuration section to add. Cannot be null or empty.</param>
    /// <param name="configure">An action that configures the newly added section. Cannot be null.</param>
    /// <returns>The current <see cref="IConfigurationMapper{TEntity}"/> instance to allow method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null, empty, or white space.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="configure"/> is null.</exception>
    IConfigurationMapper<TEntity> AddSection(string name, Action<IConfigurationMapper<TEntity>> configure);
    /// <summary>
    /// Adds a configuration section for the specified key and applies additional configuration to it.
    /// </summary>
    /// <remarks>Use this method to organize configuration by logical sections based on a key property. This
    /// is useful for grouping related configuration settings for different parts of an entity.</remarks>
    /// <typeparam name="TKey">The type of the key used to identify the configuration section. Must be a non-nullable type.</typeparam>
    /// <param name="keyExpression">An expression that specifies the key property of the entity to which the configuration section applies. Value of 
    /// property for the entity will be used as the key. Cannot be null.</param>
    /// <param name="configure">An action that configures the newly added section. The provided mapper is used to define additional
    /// configuration for the section. Cannot be null.</param>
    /// <returns>The current configuration mapper instance, allowing for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="keyExpression"/> or <paramref name="configure"/> is null.</exception>
    IConfigurationMapper<TEntity> AddSection<TKey>(Expression<Func<TEntity, TKey>> keyExpression, Action<IConfigurationMapper<TEntity>> configure);
    /// <summary>
    /// Configures the mapper to use the specified value expression for mapping an entity property, optionally
    /// overriding the default property name.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value to be mapped.</typeparam>
    /// <param name="valueExpression">An expression that specifies the property of the entity to map.</param>
    /// <param name="nameOverride">An optional name to use instead of the default property name. If null, the property name from the expression is
    /// used.</param>
    /// <returns>An updated configuration mapper with the specified value mapping applied.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="valueExpression"/> is null.</exception>
    IConfigurationMapper<TEntity> WithValue<TValue>(Expression<Func<TEntity, TValue>> valueExpression, string? nameOverride = null);
}

internal interface IConfigurationMapperBuilder<TEntity> where TEntity : class, ITableEntity
{
    MappingRootSection<TEntity> Build();
}
public sealed class ConfigurationMapperBuilder<TEntity> : IConfigurationMapper<TEntity>, IConfigurationMapperBuilder<TEntity> where TEntity : class, ITableEntity
{
    private readonly MappingSection<TEntity> _section;

    internal ConfigurationMapperBuilder() => _section = new MappingRootSection<TEntity>();

    private ConfigurationMapperBuilder(MappingKeySection<TEntity> section) => _section = section;

    public IConfigurationMapper<TEntity> AddSection(string name, Action<IConfigurationMapper<TEntity>> configure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(configure);

        return AddSection(new MappingStaticKeySection<TEntity>(name), configure, name);
    }

    public IConfigurationMapper<TEntity> AddSection<TKey>(Expression<Func<TEntity, TKey>> keyExpression, Action<IConfigurationMapper<TEntity>> configure)
    {
        ArgumentNullException.ThrowIfNull(keyExpression);
        ArgumentNullException.ThrowIfNull(configure);
        
        return AddSection(MappingDynamicKeySection<TEntity>.Create(keyExpression), configure, keyExpression.GetMemberName());
    }

    public IConfigurationMapper<TEntity> WithValue<TValue>(Expression<Func<TEntity, TValue>> valueExpression, string? nameOverride = null)
    {
        ArgumentNullException.ThrowIfNull(valueExpression);

        var valueElement = MappingValueElement<TEntity>.Create(valueExpression, nameOverride);

        _section.Add(valueElement);

        return this;
    }

    MappingRootSection<TEntity> IConfigurationMapperBuilder<TEntity>.Build()
        => _section is MappingRootSection<TEntity> rootSection
        ? rootSection
        : throw new InvalidOperationException($"{nameof(Build)} can only be called on the root section.");

    internal MappingRootSection<TEntity> Build() => ((IConfigurationMapperBuilder<TEntity>)this).Build();

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
}