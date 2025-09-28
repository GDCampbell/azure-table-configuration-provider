using Azure.Data.Tables;
using AzureTable.Provider.Configuration;
using Microsoft.Extensions.Configuration;

namespace AzureTable.Provider.Extensions.Configuration;

public sealed class AzureTableConfigurationBuilder
{
    /// <summary>
/// Initializes a new instance of AzureTableConfigurationBuilder used to register Azure Table configuration sources.
/// </summary>
internal AzureTableConfigurationBuilder() { }

    private readonly List<Func<IConfiguration, IConfigurationSource>> _sourceFactories = [];

    /// <summary>
    /// Registers an Azure Table configuration source for the specified entity type using the provided configuration action.
    /// </summary>
    /// <typeparam name="TEntity">The table entity type; must implement <see cref="ITableEntity"/>.</typeparam>
    /// <param name="configure">An action that configures a <see cref="ProviderConfigurationBuilder{TEntity}"/> for the table entity.</param>
    /// <returns>The same <see cref="AzureTableConfigurationBuilder"/> instance to allow method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is null.</exception>
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

    /// <summary>
        /// Materializes registered configuration sources using the provided configuration snapshot.
        /// </summary>
        /// <param name="configurationSnapshot">The IConfiguration snapshot supplied to each factory when creating sources.</param>
        /// <returns>An enumerable of IConfigurationSource created by invoking each registered factory with the provided configuration snapshot.</returns>
        internal IEnumerable<IConfigurationSource> Build(IConfiguration configurationSnapshot)
        => _sourceFactories.Select(factory => factory(configurationSnapshot));
}