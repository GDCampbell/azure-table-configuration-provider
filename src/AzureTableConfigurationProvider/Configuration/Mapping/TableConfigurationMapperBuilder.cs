using Azure.Data.Tables;
using AzureTable.Provider.Configuration.Mapping.Elements;

namespace AzureTable.Provider.Configuration.Mapping;

public interface ITableConfigurationMapper<TEntity> : IConfigurationMapper<TEntity> where TEntity : class, ITableEntity;

internal sealed class TableConfigurationMapperBuilder<TEntity> : BaseConfigurationMapperBuilder<TEntity>, ITableConfigurationMapper<TEntity>
    where TEntity : class, ITableEntity
{
    internal TableConfigurationMapperBuilder() : base() { }
    private TableConfigurationMapperBuilder(MappingKeySection<TEntity> section) : base(section) { }

    protected override BaseConfigurationMapperBuilder<TEntity> CreateBuilder(MappingKeySection<TEntity> section)
        => new TableConfigurationMapperBuilder<TEntity>(section);
}
