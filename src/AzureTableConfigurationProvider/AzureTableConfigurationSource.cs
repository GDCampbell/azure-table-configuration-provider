using Azure.Data.Tables;
using AzureTable.Provider.Configuration;
using AzureTable.Provider.Configuration.Mapping;
using AzureTable.Provider.Query;
using Microsoft.Extensions.Configuration;

namespace AzureTable.Provider;

internal sealed class AzureTableConfigurationSource<TEntity>(
    Action<ProviderConfigurationBuilder<TEntity>> configure
    ) 
    : IConfigurationSource 
    where TEntity : class, ITableEntity
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var configuration = builder.Build();

        var providerBuilder = new ProviderConfigurationBuilder<TEntity>();
        configure(providerBuilder);

        var (tableFactory, mappingBuilderConfiguration, queryConfiguration) = providerBuilder.Build();

        var tableClient = tableFactory(configuration);
        
        var mappingBuilder = new ConfigurationMapperBuilder<TEntity>();

        mappingBuilderConfiguration(mappingBuilder, configuration);

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
