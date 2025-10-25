using System.Linq.Expressions;
using Azure.Data.Tables;
using AzureTable.Provider.Configuration.Mapping.Elements;

namespace AzureTable.Provider.Configuration.Mapping;





public interface IConfigurationMapperOld<TEntity> where TEntity : class, ITableEntity
{
    /// <summary>
    /// Adds a new configuration section with the specified key and applies the provided configuration action to it.
    /// </summary>
    /// <remarks>
    /// <para>Note: Mapped configuration keys may include ':' to represent hierarchy; this is intentional and supported by the configuration system.</para>
    /// </remarks>
    /// <param name="key">The key of the configuration section to add. Cannot be null or empty.</param>
    /// <param name="configure">An action that configures the newly added section. Cannot be null.</param>
    /// <returns>The current <see cref="IConfigurationMapper{TEntity}"/> instance to allow method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is null, empty, or white space.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="configure"/> is null.</exception>
    IConfigurationMapper<TEntity> AddSection(string key, Action<IConfigurationMapper<TEntity>> configure);
    /// <summary>
    /// Adds a configuration section for the specified key and applies additional configuration to it.
    /// </summary>
    /// <remarks>Use this method to organize configuration by logical sections based on a key property. This
    /// is useful for grouping related configuration settings for different parts of an entity.
    /// <para>Note: Mapped configuration keys may include ':' to represent hierarchy; this is intentional and supported by the configuration system.</para>
    /// </remarks>
    /// <param name="keyExpression">An expression that specifies the key property of the entity. 
    /// The property value will be extracted from each entity instance  
    /// and used as the configuration section key. Cannot be null.</param>
    /// <param name="configure">An action that configures the newly added section. The provided mapper is used to define additional
    /// configuration for the section. Cannot be null.</param>
    /// <returns>The current configuration mapper instance, allowing for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="keyExpression"/> or <paramref name="configure"/> is null.</exception>
    IConfigurationMapper<TEntity> AddSection(Expression<Func<TEntity, string>> keyExpression, Action<IConfigurationMapper<TEntity>> configure);
    /// <summary>
    /// Configures the mapper to use the specified value expression for mapping an entity property, optionally
    /// overriding the default property name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Property values are converted to strings using culture-invariant formatting to ensure consistent 
    /// configuration values that can be parsed reliably across different locales. For types implementing 
    /// <see cref="IFormattable"/> (such as <see cref="decimal"/>, <see cref="double"/>, 
    /// <see cref="DateTime"/>), <see cref="CultureInfo.InvariantCulture"/> is used. Null values are 
    /// preserved as null in the configuration.
    /// </para>
    /// </remarks>
    /// <typeparam name="TValue">The type of the property value to be mapped.</typeparam>
    /// <param name="valueExpression">An expression that specifies the property of the entity to map.</param>
    /// <param name="nameOverride">An optional name to use instead of the default property name. If null, the property name from the expression is
    /// used.</param>
    /// <returns>An updated configuration mapper with the specified value mapping applied.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="valueExpression"/> is null.</exception>
    IConfigurationMapper<TEntity> WithValue<TValue>(Expression<Func<TEntity, TValue>> valueExpression, string name) where TValue : struct, IFormattable;
    IConfigurationMapper<TEntity> WithValue<TValue>(Expression<Func<TEntity, TValue?>> valueExpression, string name) where TValue : struct, IFormattable;
    IConfigurationMapper<TEntity> WithValue(Expression<Func<TEntity, string?>> valueExpression, string name);

    IConfigurationMapper<TEntity> WithValue<TValue>(Expression<Func<TEntity, TValue>> valueExpression) where TValue : struct, IFormattable;
    IConfigurationMapper<TEntity> WithValue<TValue>(Expression<Func<TEntity, TValue?>> valueExpression) where TValue : struct, IFormattable;
    IConfigurationMapper<TEntity> WithValue(Expression<Func<TEntity, string?>> valueExpression);
}



public interface IPrimitiveArrayMapperBuilder<TEntity, TValue>
    where TEntity : class, ITableEntity
    where TValue : struct, IFormattable
{
    IPrimitiveArrayMapperBuilder<TEntity, TValue> AddItem(Func<TEntity, TValue> valueFactory);
    IPrimitiveArrayMapperBuilder<TEntity, TValue> AddItem(Func<TEntity, TValue?> valueFactory);
}

public interface IStringArrayMapperBuilder<TEntity> where TEntity : class, ITableEntity
{
    IStringArrayMapperBuilder<TEntity> AddItem(Func<TEntity, string?> valueFactory);
}


internal abstract class BasePrimitiveArrayMapperBuilder<TEntity> where TEntity : class, ITableEntity
{
    private readonly PrimitiveArrayElement<TEntity> _section;

    protected BasePrimitiveArrayMapperBuilder(string key) => _section = new(key);
    protected BasePrimitiveArrayMapperBuilder(Expression<Func<TEntity, string>> keyExpression) => _section = new(keyExpression);

    protected void AddItemInternal<TValue>(Func<TEntity, TValue> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(valueFactory);
        _section.AddItem(valueFactory);
    }

    public PrimitiveArrayElement<TEntity> Build() => _section;
}

internal sealed class StringArrayMapperBuilder<TEntity> : BasePrimitiveArrayMapperBuilder<TEntity>, IStringArrayMapperBuilder<TEntity>
    where TEntity : class, ITableEntity
{
    internal StringArrayMapperBuilder(string key) : base(key) { }
    internal StringArrayMapperBuilder(Expression<Func<TEntity, string>> keyExpression) : base(keyExpression) { }
    public IStringArrayMapperBuilder<TEntity> AddItem(Func<TEntity, string?> valueFactory)
    {
        AddItemInternal(valueFactory);
        return this;
    }
}

internal sealed class PrimitiveArrayMapperBuilder<TEntity, TValue> : BasePrimitiveArrayMapperBuilder<TEntity>, IPrimitiveArrayMapperBuilder<TEntity, TValue>
    where TEntity : class, ITableEntity
    where TValue : struct, IFormattable
{

    internal PrimitiveArrayMapperBuilder(string key) : base(key) { }
    internal PrimitiveArrayMapperBuilder(Expression<Func<TEntity, string>> keyExpression) : base(keyExpression) { }

    public IPrimitiveArrayMapperBuilder<TEntity, TValue> AddItem(Func<TEntity, TValue> valueFactory)
    {
        AddItemInternal(valueFactory);

        return this;
    }

    public IPrimitiveArrayMapperBuilder<TEntity, TValue> AddItem(Func<TEntity, TValue?> valueFactory)
    {
        AddItemInternal(valueFactory);
        return this;
    }
}


