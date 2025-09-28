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

    /// <summary>
    /// Loads configuration entries from Azure Table Storage and replaces the provider's data with them.
    /// </summary>
    /// <remarks>
    /// Clears the existing Data dictionary and populates it with the key/value pairs extracted from table entities.
    /// </remarks>
    public override void Load()
    {
        var data = LoadEntitiesAsync().GetAwaiter().GetResult();

        Data.Clear();

        foreach (var (key, value) in data)
        {
            Data[key] = value;
        }
    }

    /// <summary>
    /// Retrieves entities from the configured table, maps each entity into key/value configuration entries, and returns the combined results.
    /// </summary>
    /// <returns>A case-insensitive dictionary of configuration keys to their string values (values may be <c>null</c>) populated from the mapped table entities.</returns>
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
