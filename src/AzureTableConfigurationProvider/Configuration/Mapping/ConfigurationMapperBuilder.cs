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
    /// <summary>
/// Adds a new configuration section identified by the specified key and allows configuring its mappings.
/// </summary>
/// <param name="key">The section key; leading and trailing whitespace are ignored and the key must not be null, empty, or consist only of whitespace.</param>
/// <param name="configure">An action that populates the new section's mappings.</param>
/// <returns>The current mapper instance for chaining.</returns>
/// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> or <paramref name="configure"/> is null.</exception>
/// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is empty or consists only of whitespace.</exception>
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
    /// <summary>
/// Adds a configuration section using a key derived from the specified entity property.
/// </summary>
/// <param name="keyExpression">An expression selecting the entity property whose member name will be used as the section key.</param>
/// <param name="configure">An action that configures the newly created section via the mapper API.</param>
/// <returns>The current mapper instance with the new section applied, allowing fluent chaining.</returns>
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
    /// <summary>
/// Adds a value mapping for an entity property.
/// </summary>
/// <param name="valueExpression">An expression selecting the entity property to map.</param>
/// <param name="nameOverride">Optional explicit name to use for the mapped value instead of the derived property name.</param>
/// <returns>The mapper with the configured value mapping.</returns>
/// <exception cref="ArgumentNullException">Thrown if <paramref name="valueExpression"/> is null.</exception>
    IConfigurationMapper<TEntity> WithValue<TValue>(Expression<Func<TEntity, TValue>> valueExpression, string? nameOverride = null);
}

internal interface IConfigurationMapperBuilder<TEntity> where TEntity : class, ITableEntity
{
    /// <summary>
/// Build the configured root mapping section for the entity type.
/// </summary>
/// <returns>The constructed <see cref="MappingRootSection{TEntity}"/> containing configured sections and values.</returns>
/// <exception cref="InvalidOperationException">Thrown if this builder is not the root section or if the root has no configured sections or values.</exception>
MappingRootSection<TEntity> Build();
}
public sealed class ConfigurationMapperBuilder<TEntity> : IConfigurationMapper<TEntity>, IConfigurationMapperBuilder<TEntity> where TEntity : class, ITableEntity
{
    private readonly MappingSection<TEntity> _section;

    /// <summary>
/// Initializes a new instance of <see cref="ConfigurationMapperBuilder{TEntity}"/> with an empty root mapping section.
/// </summary>
internal ConfigurationMapperBuilder() => _section = new MappingRootSection<TEntity>();

    /// <summary>
/// Initializes a new ConfigurationMapperBuilder using the provided mapping key section as the current section.
/// </summary>
/// <param name="section">The mapping key section to assign as the builder's active section.</param>
private ConfigurationMapperBuilder(MappingKeySection<TEntity> section) => _section = section;

    /// <summary>
    /// Adds a configuration section identified by the given static key and invokes the provided configuration action for that section.
    /// </summary>
    /// <param name="key">The section key (trimmed); must not be null, empty, or whitespace.</param>
    /// <param name="configure">An action that configures the new section via an <see cref="IConfigurationMapper{TEntity}"/>.</param>
    /// <returns>The current <see cref="IConfigurationMapper{TEntity}"/> instance for fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null, empty, or consists only of white-space characters.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is null.</exception>
    public IConfigurationMapper<TEntity> AddSection(string key, Action<IConfigurationMapper<TEntity>> configure)
    {
        key = key?.Trim() ?? string.Empty;
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(configure);

        return AddSection(new MappingStaticKeySection<TEntity>(key), configure, key);
    }

    /// <summary>
    /// Adds a configuration section whose key is derived from a string property of the entity.
    /// </summary>
    /// <param name="keyExpression">An expression selecting the entity string property to use as the section key.</param>
    /// <param name="configure">An action to configure the mapper for the newly created section.</param>
    /// <returns>The current mapper instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="keyExpression"/> or <paramref name="configure"/> is null.</exception>
    public IConfigurationMapper<TEntity> AddSection(Expression<Func<TEntity, string>> keyExpression, Action<IConfigurationMapper<TEntity>> configure)
    {
        ArgumentNullException.ThrowIfNull(keyExpression);
        ArgumentNullException.ThrowIfNull(configure);
        
        return AddSection(MappingDynamicKeySection<TEntity>.Create(keyExpression), configure, keyExpression.GetMemberName());
    }

    /// <summary>
    /// Configures a mapping for the specified entity property.
    /// </summary>
    /// <param name="valueExpression">An expression that selects the entity property to map.</param>
    /// <param name="nameOverride">An optional name to use for the mapped value; if null, the property name is used.</param>
    /// <returns>The current configuration mapper instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="valueExpression"/> is null.</exception>
    public IConfigurationMapper<TEntity> WithValue<TValue>(Expression<Func<TEntity, TValue>> valueExpression, string? nameOverride = null)
    {
        ArgumentNullException.ThrowIfNull(valueExpression);

        var valueElement = MappingValueElement<TEntity>.Create(valueExpression, nameOverride);

        _section.Add(valueElement);

        return this;
    }

    /// <summary>
        /// Builds and returns the configured root mapping section for the mapper.
        /// </summary>
        /// <returns>The populated <see cref="MappingRootSection{TEntity}"/> representing the root mapping.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the root section has no child elements: "At least one section or value must be configured for the mapper."</exception>
        /// <exception cref="InvalidOperationException">Thrown if the current builder is not a root section: "Build can only be called on the root section."</exception>
        MappingRootSection<TEntity> IConfigurationMapperBuilder<TEntity>.Build()
        => _section switch
        {
            MappingRootSection<TEntity> rootSection => rootSection.HasElements ? rootSection : throw new InvalidOperationException("At least one section or value must be configured for the mapper."),
            _ => throw new InvalidOperationException($"{nameof(Build)} can only be called on the root section.")
        };

    /// <summary>
/// Builds the configured root mapping section for the entity type.
/// </summary>
/// <returns>The constructed MappingRootSection&lt;TEntity&gt; representing the root mapping.</returns>
internal MappingRootSection<TEntity> Build() => ((IConfigurationMapperBuilder<TEntity>)this).Build();

    /// <summary>
    /// Adds a configured key section to the root mapping and returns the current builder for chaining.
    /// </summary>
    /// <param name="section">The key-based mapping section to configure and add.</param>
    /// <param name="configure">An action that populates the provided section using an IConfigurationMapper for that section.</param>
    /// <param name="sectionName">The section identifier used in the error message if configuration produces no child elements.</param>
    /// <returns>The current <see cref="ConfigurationMapperBuilder{TEntity}"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the configured section has no child elements after configuration.</exception>
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