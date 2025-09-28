using Azure.Data.Tables;
using AzureTable.Provider.Configuration;
using AzureTable.Provider.Configuration.Mapping;
using AzureTable.Provider.Query;
using Microsoft.Extensions.Configuration;

namespace AzureTable.Provider;

internal sealed class AzureTableConfigurationSource<TEntity>(
    IConfiguration configurationSnapshot,
    Action<ProviderConfigurationBuilder<TEntity>> configure
    ) 
    : IConfigurationSource 
    where TEntity : class, ITableEntity
{
    /// <summary>
    /// Builds an <see cref="IConfigurationProvider"/> that reads configuration from Azure Table storage for the configured <typeparamref name="TEntity"/> mappings.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to which the provider will be added.</param>
    /// <returns>An <see cref="IConfigurationProvider"/> configured with the resolved table client, mapping root, and optional query configuration.</returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var providerBuilder = new ProviderConfigurationBuilder<TEntity>();
        configure(providerBuilder);

        var (tableFactory, mappingBuilderConfiguration, queryConfiguration) = providerBuilder.Build();

        var tableClient = tableFactory(configurationSnapshot);
        
        var mappingBuilder = new ConfigurationMapperBuilder<TEntity>();

        mappingBuilderConfiguration(mappingBuilder, configurationSnapshot);

        var mappingRoot = mappingBuilder.Build();

        TableQueryConfiguration? query = null;

        if (queryConfiguration is not null)
        {
            query = new();
            queryConfiguration(query);
        }

        return new AzureTableConfigurationProvider<TEntity>(mappingRoot, tableClient, query);
    }
}
