using Azure.Data.Tables;
using AzureTable.Provider.Configuration.Mapping.Elements;
using AzureTable.Provider.Query;
using Microsoft.Extensions.Configuration;

namespace AzureTable.Provider;

internal sealed class AzureTableConfigurationProvider<TEntity>(
    MappingRootSection<TEntity> mappingRoot,
    TableClient tableClient,
    TableQueryConfiguration? query)
    : ConfigurationProvider
    where TEntity : class, ITableEntity
{

    public override void Load()
    {
        LoadEntitiesAsync().GetAwaiter().GetResult();
    }

    private async Task LoadEntitiesAsync()
    {
        var results = query?.Filter switch
        {
            StringFilter stringFilter => tableClient.QueryAsync<TEntity>(stringFilter.Filter, query.MaxPerPage, query.Select),
            PredicateFilter<TEntity> predicateFilter => tableClient.QueryAsync(predicateFilter.Filter, query.MaxPerPage, query.Select),
            _ => tableClient.QueryAsync<TEntity>(maxPerPage: query?.MaxPerPage, select: query?.Select)
        };

        await foreach (var entity in results.ConfigureAwait(false))
        {
            foreach (var (key, value) in mappingRoot.ExtractFrom(entity))
            {
                Data[key] = value;
            }
        }
    }
}
