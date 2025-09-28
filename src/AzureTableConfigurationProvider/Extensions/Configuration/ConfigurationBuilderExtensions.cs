using Azure.Data.Tables;
using AzureTable.Provider.Configuration;
using Microsoft.Extensions.Configuration;

namespace AzureTable.Provider.Extensions.Configuration;

public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds Azure Table Storage as a configuration source to the specified configuration builder.
    /// </summary>
    /// <remarks>Use this method to load configuration values from Azure Table Storage.
    /// This method supports custom entity types for advanced mapping scenarios.
    /// 
    /// <strong>Important:</strong> Configuration providers must be added in the correct order. This method
    /// creates a snapshot of the current configuration state, so it can only access values from providers
    /// that were added before this call. Add any required configuration sources (JSON files, user secrets,
    /// etc.) before calling this method if you need to access their values in your table factory or mapping
    /// configuration.
    /// 
    /// Example:
    /// <code>
    /// builder.AddJsonFile("appsettings.json")              // Add first
    ///        .AddAzureTableConfiguration&lt;MyEntity&gt;(opts =&gt;   // Can access JSON values
    ///        {
    ///            opts.ConfigureTableFactory(config =&gt;
    ///                new TableClient(config.GetConnectionString("Storage"), "MyTable"));
    ///        })
    ///        .AddEnvironmentVariables();                  // Add last (overrides all)
    /// </code>
    /// 
    /// For multiple table configurations, use <see cref="AddAzureTableConfiguration(IConfigurationBuilder, Action{AzureTableConfigurationBuilder})"/> 
    /// instead to avoid redundant configuration reloads.
    /// </remarks>
    /// <typeparam name="TEntity">The type of the table entity used to map configuration data. Must implement the ITableEntity interface.</typeparam>
    /// <param name="builder">The configuration builder to which the Azure Table Storage configuration source will be added.</param>
    /// <param name="configure">A delegate to configure the Azure Table Storage provider options for the specified entity type.</param>
    /// <returns>The configuration builder with the Azure Table Storage configuration source added.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="configure"/> are null.</exception>
    /// <summary>
    /// Adds an Azure Table Storage configuration source for entities of type <typeparamref name="TEntity"/> to the configuration builder.
    /// </summary>
    /// <param name="builder">The configuration builder to add the Azure Table source to.</param>
    /// <param name="configure">A delegate that configures the table-to-configuration mapping and provider options for <typeparamref name="TEntity"/>.</param>
    /// <returns>The original <see cref="IConfigurationBuilder"/> with the Azure Table configuration source added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="configure"/> is null.</exception>
    public static IConfigurationBuilder AddAzureTableConfiguration<TEntity>(
        this IConfigurationBuilder builder,
        Action<ProviderConfigurationBuilder<TEntity>> configure)
        where TEntity : class, ITableEntity
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var configurationSnapshot = builder.Build();

        return builder.Add(new AzureTableConfigurationSource<TEntity>(configurationSnapshot, configure));
    }

    /// <summary>
    /// Adds multiple Azure Table Storage configurations as sources to the specified configuration builder.
    /// </summary>
    /// <remarks>
    /// Use this method to configure multiple table mappings efficiently. The configuration snapshot
    /// is created once and shared across all table mappings, preventing redundant configuration reloads.
    /// 
    /// <strong>Important:</strong> This method creates a single configuration snapshot that is shared
    /// across all table configurations. Only configuration sources added before this method call will
    /// be available to your table factory and mapping configurations. Add any required configuration
    /// sources (JSON files, user secrets, etc.) before calling this method.
    /// 
    /// All table configurations will see the same configuration state. If you need a table
    /// configuration to depend on values loaded by a previous table, use multiple calls to
    /// <see cref="AddAzureTableConfiguration{TEntity}(IConfigurationBuilder, Action{ProviderConfigurationBuilder{TEntity}})"/> instead.
    /// 
    /// Example:
    /// <code>
    /// builder.AddJsonFile("appsettings.json")              // Add first
    ///        .AddUserSecrets&lt;Program&gt;()                     // Add second  
    ///        .AddAzureTableConfiguration(tables =&gt;          // Can access JSON and secrets
    ///        {
    ///            tables.AddTable&lt;Entity1&gt;(opts =&gt; /* ... */)
    ///                  .AddTable&lt;Entity2&gt;(opts =&gt; /* ... */);
    ///        })
    ///        .AddEnvironmentVariables();                  // Add last (overrides all)
    /// </code>
    /// </remarks>
    /// <param name="builder">The configuration builder to which the Azure Table Storage configuration sources will be added.</param>
    /// <param name="configure">A delegate to configure multiple table mappings using the provided builder. Will throw 
    /// <see cref="InvalidOperationException"/> if no tables are added.</param>
    /// <returns>The configuration builder with all the Azure Table Storage configuration sources added.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="configure"/> are null.</exception>
    /// <summary>
    /// Adds one or more Azure Table configuration sources configured via the provided delegate to the given <see cref="IConfigurationBuilder"/>.
    /// </summary>
    /// <param name="configure">A delegate that populates an <see cref="AzureTableConfigurationBuilder"/> with table mappings to be added as configuration sources.</param>
    /// <returns>The original <see cref="IConfigurationBuilder"/> with the new Azure Table configuration sources added.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="configure"/> or the <paramref name="builder"/> instance is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the delegate does not produce any table configuration sources to add.</exception>
    public static IConfigurationBuilder AddAzureTableConfiguration(
        this IConfigurationBuilder builder,
        Action<AzureTableConfigurationBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var tableBuilder = new AzureTableConfigurationBuilder();
        configure(tableBuilder);

        var configurationSnapshot = builder.Build();

        var any = false;

        foreach (var source in tableBuilder.Build(configurationSnapshot))
        {
            any = true;
            builder.Add(source);
        }

        if (!any)
        {
            throw new InvalidOperationException($"No table configurations were added. Ensure at least one table is configured in the {nameof(configure)} delegate by calling {nameof(AzureTableConfigurationBuilder)}.{nameof(AzureTableConfigurationBuilder.AddTable)}.");
        }

        return builder;
    }
}
