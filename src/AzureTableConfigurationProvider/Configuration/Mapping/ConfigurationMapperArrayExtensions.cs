using System.Linq.Expressions;

namespace AzureTable.Provider.Configuration.Mapping;

public static class ConfigurationMapperArrayExtensions
{
    public static TMapper AddPrimitiveArray<T, TMapper, TValue>(this TMapper mapper, string name, Action<SimpleArrayMapper<T, TValue>> configure)
        where T : class
        where TMapper : BaseConfigurationMapper<T>
        where TValue : struct, IFormattable
    {
        ArgumentNullException.ThrowIfNull(configure);

        var arrayMapper = new SimpleArrayMapper<T, TValue>(name);

        configure(arrayMapper);

        mapper.Container.Add(arrayMapper.Element);

        return mapper;
    }

    public static TMapper AddPrimitiveArray<T, TMapper, TValue>(this TMapper mapper, string name, Action<SimpleArrayMapper<T, TValue?>> configure)
        where T : class
        where TMapper : BaseConfigurationMapper<T>
        where TValue : struct, IFormattable
    {
        ArgumentNullException.ThrowIfNull(configure);

        var arrayMapper = new SimpleArrayMapper<T, TValue?>(name);

        configure(arrayMapper);

        mapper.Container.Add(arrayMapper.Element);

        return mapper;
    }

    public static TMapper AddPrimitiveArray<T, TMapper, TValue>(this TMapper mapper, Expression<Func<T, string>> nameExpression, Action<SimpleArrayMapper<T, TValue>> configure)
        where T : class
        where TMapper : BaseConfigurationMapper<T>
        where TValue : struct, IFormattable
    {
        ArgumentNullException.ThrowIfNull(configure);

        var arrayMapper = new SimpleArrayMapper<T, TValue>(nameExpression);

        configure(arrayMapper);

        mapper.Container.Add(arrayMapper.Element);

        return mapper;
    }

    public static TMapper AddPrimitiveArray<T, TMapper, TValue>(this TMapper mapper, Expression<Func<T, string>> nameExpression, Action<SimpleArrayMapper<T, TValue?>> configure)
        where T : class
        where TMapper : BaseConfigurationMapper<T>
        where TValue : struct, IFormattable
    {
        ArgumentNullException.ThrowIfNull(configure);

        var arrayMapper = new SimpleArrayMapper<T, TValue?>(nameExpression);

        configure(arrayMapper);

        mapper.Container.Add(arrayMapper.Element);

        return mapper;
    }

    public static TMapper AddStringArray<T, TMapper>(this TMapper mapper, Expression<Func<T, string>> nameExpression, Action<SimpleArrayMapper<T, string?>> configure)
        where T : class
        where TMapper : BaseConfigurationMapper<T>
    {
        ArgumentNullException.ThrowIfNull(configure);

        var arrayMapper = new SimpleArrayMapper<T, string?>(nameExpression);

        configure(arrayMapper);

        mapper.Container.Add(arrayMapper.Element);

        return mapper;
    }

    public static TMapper AddStringArray<T, TMapper>(this TMapper mapper, string name, Action<SimpleArrayMapper<T, string?>> configure)
        where T : class
        where TMapper : BaseConfigurationMapper<T>
    {
        ArgumentNullException.ThrowIfNull(configure);

        var arrayMapper = new SimpleArrayMapper<T, string?>(name);

        configure(arrayMapper);

        mapper.Container.Add(arrayMapper.Element);

        return mapper;
    }

    public static TMapper AddComplexArray<T, TMapper>(this TMapper mapper, string name, Action<ComplexArrayMapper<T>> configure)
        where T : class
        where TMapper : BaseConfigurationMapper<T>
    {
        ArgumentNullException.ThrowIfNull(configure);

        var objectMapper = new ComplexArrayMapper<T>(name);

        configure(objectMapper);

        mapper.Container.Add(objectMapper.Element);
        return mapper;
    }

    public static TMapper AddComplexArray<T, TMapper>(this TMapper mapper, Expression<Func<T, string>> nameExpression, Action<ComplexArrayMapper<T>> configure)
        where T : class
        where TMapper : BaseConfigurationMapper<T>
    {
        ArgumentNullException.ThrowIfNull(configure);

        var objectMapper = new ComplexArrayMapper<T>(nameExpression);

        configure(objectMapper);

        mapper.Container.Add(objectMapper.Element);

        return mapper;
    }
}
