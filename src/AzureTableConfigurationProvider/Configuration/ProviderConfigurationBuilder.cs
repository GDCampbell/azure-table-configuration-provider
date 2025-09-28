using Azure.Data.Tables;
using AzureTable.Provider.Configuration.Mapping;
using AzureTable.Provider.Query;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace AzureTable.Provider.Configuration;

public sealed class ProviderConfigurationBuilder<TEntity> where TEntity : class, ITableEntity
{
    private Func<IConfiguration, TableClient>? _tableClientFactory;
    private Action<ConfigurationMapperBuilder<TEntity>, IConfiguration>? _mappingBuilder;
    private Action<TableQueryConfiguration>? _queryConfiguration;

    /// <summary>
    /// Configures the factory function used to create a TableClient instance for the provider.
    /// </summary>
    /// <param name="tableClientFactory">A delegate that takes an IConfiguration and returns a TableClient instance. Cannot be null.</param>
    /// <returns>The current ProviderConfigurationBuilder<TEntity> instance for method chaining.</returns>
    /// <summary>
    /// Configures the factory used to create a TableClient from an IConfiguration instance.
    /// </summary>
    /// <param name="tableClientFactory">A function that produces a <see cref="TableClient"/> given an <see cref="IConfiguration"/>.</param>
    /// <returns>The current <see cref="ProviderConfigurationBuilder{TEntity}"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tableClientFactory"/> is null.</exception>
    public ProviderConfigurationBuilder<TEntity> ConfigureTableFactory(Func<IConfiguration, TableClient> tableClientFactory)
    {
        ArgumentNullException.ThrowIfNull(tableClientFactory);

        _tableClientFactory = tableClientFactory;
        return this;
    }

    /// <summary>
    /// Configures the entity mapping using the specified mapping builder action.
    /// </summary>
    /// <param name="mappingBuilder">An action that receives a configuration mapper builder and the current configuration, allowing custom mapping
    /// logic to be defined. Cannot be null.</param>
    /// <returns>The current <see cref="ProviderConfigurationBuilder{TEntity}"/> instance for method chaining.</returns>
    /// <summary>
    /// Sets the delegate that configures how configuration values map to a TEntity instance.
    /// </summary>
    /// <param name="mappingBuilder">A delegate that configures a <see cref="ConfigurationMapperBuilder{TEntity}"/> using an <see cref="IConfiguration"/>.</param>
    /// <returns>The same <see cref="ProviderConfigurationBuilder{TEntity}"/> instance to allow method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="mappingBuilder"/> is null.</exception>
    public ProviderConfigurationBuilder<TEntity> ConfigureMapping(Action<ConfigurationMapperBuilder<TEntity>, IConfiguration> mappingBuilder)
    {
        ArgumentNullException.ThrowIfNull(mappingBuilder);

        _mappingBuilder = mappingBuilder;
        return this;
    }

    /// <summary>
    /// Configures query options for the current entity provider using the specified configuration action.
    /// </summary>
    /// <param name="queryConfiguration">An action that receives a <see cref="TableQueryConfiguration"/> object to customize query behavior. Cannot be
    /// null.</param>
    /// <returns>The current <see cref="ProviderConfigurationBuilder{TEntity}"/> instance for method chaining.</returns>
    /// <summary>
    /// Sets the optional table query configuration to apply when building the provider.
    /// </summary>
    /// <param name="queryConfiguration">An action that configures a <see cref="TableQueryConfiguration"/> instance.</param>
    /// <returns>The same <see cref="ProviderConfigurationBuilder{TEntity}"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="queryConfiguration"/> is null.</exception>
    public ProviderConfigurationBuilder<TEntity> ConfigureQuery(Action<TableQueryConfiguration> queryConfiguration)
    {
        // The query configuration is optional but if they're calling this method, then it shouldn't be null.
        ArgumentNullException.ThrowIfNull(queryConfiguration);
        _queryConfiguration = queryConfiguration;
        return this;
    }

    /// <summary>
    /// Creates a ProviderConfiguration<TEntity> from the configured table client factory, mapping builder, and optional query configuration.
    /// </summary>
    /// <returns>The constructed ProviderConfiguration&lt;TEntity&gt; containing the configured TableClientFactory, MappingBuilder, and QueryConfiguration (if provided).</returns>
    /// <exception cref="InvalidOperationException">Thrown if TableClientFactory or MappingBuilder have not been configured; the exception message lists the missing configuration(s).</exception>
    internal ProviderConfiguration<TEntity> Build()
    {
        if (_tableClientFactory is not null && _mappingBuilder is not null)
        {
            return new()
            {
                MappingBuilder = _mappingBuilder,
                TableClientFactory = _tableClientFactory,
                QueryConfiguration = _queryConfiguration
            };
        }

        StringBuilder sb = new();

        var tableFactoryMissing = _tableClientFactory is null;
        var mappingMissing = _mappingBuilder is null;

        if (tableFactoryMissing)
        {
            sb.Append(_configureTableFactoryMessage);
        }

        if (tableFactoryMissing && mappingMissing)
        {
            sb.Append(' ');
        }

        if (mappingMissing)
        {
            sb.Append(_configureMappingMessage);
        }

        throw new InvalidOperationException(sb.ToString());
    }

    private const string _configureTableFactoryMessage = $"{nameof(ProviderConfiguration<TEntity>.TableClientFactory)} is not configured. Please call {nameof(ConfigureTableFactory)}.";
    private const string _configureMappingMessage = $"{nameof(ProviderConfiguration<TEntity>.MappingBuilder)} is not configured. Please call {nameof(ConfigureMapping)}.";
}

internal sealed class ProviderConfiguration<TEntity> where TEntity : class, ITableEntity
{
    public required Func<IConfiguration, TableClient> TableClientFactory { get; init; }
    public required Action<ConfigurationMapperBuilder<TEntity>, IConfiguration> MappingBuilder { get; init; }
    public Action<TableQueryConfiguration>? QueryConfiguration { get; init; }

    /// <summary>
    /// Deconstructs the provider configuration into its constituent factory, mapping builder, and optional query configuration.
    /// </summary>
    /// <param name="tableClientFactory">Receives the configured function that creates a TableClient from an IConfiguration.</param>
    /// <param name="mappingBuilder">Receives the configured mapping action that populates a ConfigurationMapperBuilder for TEntity using an IConfiguration.</param>
    /// <param name="queryConfiguration">Receives the optional action that configures TableQueryConfiguration, or null if not configured.</param>
    public void Deconstruct(out Func<IConfiguration, TableClient> tableClientFactory,
        out Action<ConfigurationMapperBuilder<TEntity>, IConfiguration> mappingBuilder,
        out Action<TableQueryConfiguration>? queryConfiguration)
    {
        tableClientFactory = TableClientFactory;
        mappingBuilder = MappingBuilder;
        queryConfiguration = QueryConfiguration;
    }
}