using Azure.Data.Tables;
using AzureTable.Provider.Configuration.Mapping.Elements;
using AzureTable.Provider.Extensions;
using System.Linq.Expressions;

namespace AzureTable.Provider.Configuration.Mapping;

public partial interface IConfigurationMapper<TEntity> where TEntity : class, ITableEntity
{
    /// <summary>
    /// Adds a new configuration section with the specified key and applies the provided configuration action to it.
    /// </summary>
    /// <remarks>
    /// <para>Note: Mapped configuration keys may include ':' to represent hierarchy; this is intentional and supported by the configuration system.</para>
    /// </remarks>
    /// <param name="key">The key of the configuration section to add. Cannot be null or empty.</param>
    /// <param name="configure">An action that configures the newly added section. Cannot be null.</param>
    /// <returns>The current <see cref="IConfigurationMapper{TEntity}"/> instance to allow method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is null, empty, or white space.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="configure"/> is null.</exception>
    IConfigurationMapper<TEntity> AddSection(string key, Action<IConfigurationMapper<TEntity>> configure);
    /// <summary>
    /// Adds a configuration section for the specified key and applies additional configuration to it.
    /// </summary>
    /// <remarks>Use this method to organize configuration by logical sections based on a key property. This
    /// is useful for grouping related configuration settings for different parts of an entity.
    /// <para>Note: Mapped configuration keys may include ':' to represent hierarchy; this is intentional and supported by the configuration system.</para>
    /// </remarks>
    /// <param name="keyExpression">An expression that specifies the key property of the entity. 
    /// The property value will be extracted from each entity instance  
    /// and used as the configuration section key. Cannot be null.</param>
    /// <param name="configure">An action that configures the newly added section. The provided mapper is used to define additional
    /// configuration for the section. Cannot be null.</param>
    /// <returns>The current configuration mapper instance, allowing for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="keyExpression"/> or <paramref name="configure"/> is null.</exception>
    IConfigurationMapper<TEntity> AddSection(Expression<Func<TEntity, string>> keyExpression, Action<IConfigurationMapper<TEntity>> configure);
    /// <summary>
    /// Configures the mapper to use the specified value expression for mapping an entity property, optionally
    /// overriding the default property name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Property values are converted to strings using culture-invariant formatting to ensure consistent 
    /// configuration values that can be parsed reliably across different locales. For types implementing 
    /// <see cref="IFormattable"/> (such as <see cref="decimal"/>, <see cref="double"/>, 
    /// <see cref="DateTime"/>), <see cref="CultureInfo.InvariantCulture"/> is used. Null values are 
    /// preserved as null in the configuration.
    /// </para>
    /// </remarks>
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

    public IConfigurationMapper<TEntity> AddSection(string key, Action<IConfigurationMapper<TEntity>> configure)
    {
        key = key?.Trim() ?? string.Empty;
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(configure);

        return AddSection(new MappingStaticKeySection<TEntity>(key), configure, key);
    }

    public IConfigurationMapper<TEntity> AddSection(Expression<Func<TEntity, string>> keyExpression, Action<IConfigurationMapper<TEntity>> configure)
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
        => _section switch
        {
            MappingRootSection<TEntity> rootSection => rootSection.HasElements ? rootSection : throw new InvalidOperationException("At least one section or value must be configured for the mapper."),
            _ => throw new InvalidOperationException($"{nameof(Build)} can only be called on the root section.")
        };

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