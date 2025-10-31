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
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var providerBuilder = new ProviderConfigurationBuilder<TEntity>();
        configure(providerBuilder);

        var (tableFactory, mappingBuilderConfiguration, queryConfiguration) = providerBuilder.Build();

        var tableClient = tableFactory(configurationSnapshot);

        var mappingBuilder = new ConfigurationMapper<TEntity>();

        mappingBuilderConfiguration(mappingBuilder, configurationSnapshot);

        var mappingRoot = mappingBuilder.Element;

        if (!mappingRoot.HasElements)
        {
            throw new InvalidOperationException("At least one mapping element must be configured.");
        }

        TableQueryConfiguration? query = null;

        if (queryConfiguration is not null)
        {
            query = new();
            queryConfiguration(query);
        }

        return new AzureTableConfigurationProvider<TEntity>(mappingRoot, tableClient, query);
    }
}
