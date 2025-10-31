using Azure.Data.Tables;
using AzureTable.Provider.Configuration.Mapping.Elements;
using AzureTable.Provider.Query;
using Microsoft.Extensions.Configuration;

namespace AzureTable.Provider;

internal sealed class AzureTableConfigurationProvider<TEntity>(
    RootElement<TEntity> mappingRoot,
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
        var arrayPathToIndexx = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        await foreach (var entity in results.ConfigureAwait(false))
        {
            MapEntity(entity, data, arrayPathToIndexx);
        }

        return data;
    }

    private void MapEntity(TEntity entity, Dictionary<string, string?> data, Dictionary<string, int> arrayPathToIndex)
    {
        foreach (var element in mappingRoot.Elements)
        {
            MapEntity(element, entity, SectionPath.Root, data, arrayPathToIndex);
        }
    }

    private static void MapEntity(IElement<TEntity> element, TEntity entity, SectionPath sectionPath, Dictionary<string, string?> data, Dictionary<string, int> arrayKeyToIndex)
    {
        switch (element)
        {
            case ValueElement<TEntity> valueElement:
                var keyFromEntity = valueElement.GetKey(entity);

                data[sectionPath + keyFromEntity] = valueElement.Value.MapFrom(entity);
                break;
            case NamedObjectElement<TEntity> namedObjectElement:
                var namedKey = namedObjectElement.GetKey(entity);

                MapEntity(namedObjectElement.AsObjectElement(), entity, sectionPath + namedKey, data, arrayKeyToIndex);
                break;
            case ObjectElement<TEntity> objectElement:
                foreach (var objChild in objectElement.Elements)
                {
                    MapEntity(objChild, entity, sectionPath, data, arrayKeyToIndex);
                }
                break;
            case SimpleArrayElement<TEntity> simpleArrayElement:
                {
                    var arrayKey = simpleArrayElement.GetKey(entity);
                    var arrayPath = sectionPath + arrayKey;

                    var lastIndex = arrayKeyToIndex.GetValueOrDefault(arrayPath, 0);

                    foreach (var item in simpleArrayElement.Values)
                    {
                        var itemKey = arrayPath + lastIndex++.ToString();
                        data[itemKey] = item.MapFrom(entity);
                    }

                    arrayKeyToIndex[arrayPath] = lastIndex;
                }

                break;

            case ComplexArrayElement<TEntity> complexArrayElement:
                {
                    var arrayKey = complexArrayElement.GetKey(entity);
                    var arrayPath = sectionPath + arrayKey;

                    var lastIndex = arrayKeyToIndex.GetValueOrDefault(arrayPath, 0);

                    foreach (var item in complexArrayElement.Elements)
                    {
                        var itemPath = arrayPath + lastIndex++.ToString();
                        MapEntity(item, entity, itemPath, data, arrayKeyToIndex);
                    }
                    arrayKeyToIndex[arrayPath] = lastIndex;
                }

                break;

        }
    }
}
