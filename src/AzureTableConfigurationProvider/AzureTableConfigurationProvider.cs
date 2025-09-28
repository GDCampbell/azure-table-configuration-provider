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
        var data = LoadEntitiesAsync().GetAwaiter().GetResult();

        Data.Clear();

        foreach (var (key, value) in data)
        {
            Data[key] = value;
        }
    }

    private async Task<IDictionary<string, string?>> LoadEntitiesAsync()
    {
        var results = query?.Filter switch
        {
            PredicateFilter<TEntity> predicateFilter => tableClient.QueryAsync(predicateFilter.Filter, query.MaxPerPage, query.Select),
            StringFilter stringFilter => tableClient.QueryAsync<TEntity>(stringFilter.Filter, query.MaxPerPage, query.Select),
            _ => tableClient.QueryAsync<TEntity>(maxPerPage: query?.MaxPerPage, select: query?.Select)
        };

        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        await foreach (var entity in results.ConfigureAwait(false))
        {
            foreach (var (key, value) in mappingRoot.ExtractFrom(entity))
            {
                data[key] = value;
            }
        }

        return data;
    }
}
