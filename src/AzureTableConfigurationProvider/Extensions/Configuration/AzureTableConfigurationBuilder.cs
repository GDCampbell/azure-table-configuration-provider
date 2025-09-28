using Azure.Data.Tables;
using AzureTable.Provider.Configuration;
using Microsoft.Extensions.Configuration;

namespace AzureTable.Provider.Extensions.Configuration;

public sealed class AzureTableConfigurationBuilder
{
    internal AzureTableConfigurationBuilder() { }

    private readonly List<Func<IConfiguration, IConfigurationSource>> _sourceFactories = [];

    public AzureTableConfigurationBuilder AddTable<TEntity>(
        Action<ProviderConfigurationBuilder<TEntity>> configure)
        where TEntity : class, ITableEntity
    {
        ArgumentNullException.ThrowIfNull(configure);

        _sourceFactories.Add(configSnapshot =>
        {
            return new AzureTableConfigurationSource<TEntity>(configSnapshot, configure);
        });

        return this;
    }

    internal IEnumerable<IConfigurationSource> Build(IConfiguration configurationSnapshot)
        => _sourceFactories.Select(factory => factory(configurationSnapshot));
}