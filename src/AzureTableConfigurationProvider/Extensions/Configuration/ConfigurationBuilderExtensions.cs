using Azure.Data.Tables;
using AzureTable.Provider.Configuration;
using Microsoft.Extensions.Configuration;

namespace AzureTable.Provider.Extensions.Configuration;

public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds Azure Table Storage as a configuration source to the specified configuration builder.
    /// </summary>
    /// <remarks>Use this method to load configuration values from Azure Table Storage, enabling dynamic
    /// configuration updates from a table. This method supports custom entity types for advanced mapping
    /// scenarios.</remarks>
    /// <typeparam name="TEntity">The type of the table entity used to map configuration data. Must implement the ITableEntity interface.</typeparam>
    /// <param name="builder">The configuration builder to which the Azure Table Storage configuration source will be added.</param>
    /// <param name="configure">A delegate to configure the Azure Table Storage provider options for the specified entity type.</param>
    /// <returns>The configuration builder with the Azure Table Storage configuration source added.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the configuration source cannot be added due to misconfiguration.</exception>
    public static IConfigurationBuilder AddAzureTableConfiguration<TEntity>(
        this IConfigurationBuilder builder,
        Action<ProviderConfigurationBuilder<TEntity>> configure)
        where TEntity : class, ITableEntity
        => builder.Add(new AzureTableConfigurationSource<TEntity>(configure));
}