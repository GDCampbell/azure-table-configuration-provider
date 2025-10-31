using AzureTable.Provider.Configuration.Mapping.Elements;
using System.Linq.Expressions;

namespace AzureTable.Provider.Configuration.Mapping;

public static class ConfigurationMapperValueExtensions
{
    public static TMapper WithValue<T, TMapper, TValue>(this TMapper mapper, Expression<Func<T, TValue>> valueExpression, string key)
        where T : class
        where TMapper : BaseConfigurationMapper<T>
        where TValue : struct, IFormattable
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var element = ValueElement<T>.CreateFrom(valueExpression, key);
        mapper.Container.Add(element);

        return mapper;
    }

    public static TMapper WithValue<T, TMapper, TValue>(this TMapper mapper, Expression<Func<T, TValue?>> valueExpression, string key)
        where T : class
        where TMapper : BaseConfigurationMapper<T>
        where TValue : struct, IFormattable
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var element = ValueElement<T>.CreateFrom(valueExpression, key);
        mapper.Container.Add(element);

        return mapper;
    }

    public static TMapper WithValue<T, TMapper, TValue>(this TMapper mapper, Expression<Func<T, TValue>> valueExpression)
        where T : class
        where TMapper : BaseConfigurationMapper<T>
        where TValue : struct, IFormattable
    {
        var element = ValueElement<T>.CreateFrom(valueExpression);
        mapper.Container.Add(element);

        return mapper;
    }

    public static TMapper WithValue<T, TMapper, TValue>(this TMapper mapper, Expression<Func<T, TValue?>> valueExpression)
        where T : class
        where TMapper : BaseConfigurationMapper<T>
        where TValue : struct, IFormattable
    {
        var element = ValueElement<T>.CreateFrom(valueExpression);
        mapper.Container.Add(element);

        return mapper;
    }

    public static TMapper WithValue<T, TMapper>(this TMapper mapper, Expression<Func<T, string?>> valueExpression, string key)
        where T : class
        where TMapper : BaseConfigurationMapper<T>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var element = ValueElement<T>.CreateFrom(valueExpression);
        mapper.Container.Add(element);

        return mapper;
    }

    public static TMapper WithValue<T, TMapper>(this TMapper mapper, Expression<Func<T, string?>> valueExpression)
        where T : class
        where TMapper : BaseConfigurationMapper<T>
    {
        var element = ValueElement<T>.CreateFrom(valueExpression);
        mapper.Container.Add(element);

        return mapper;
    }
}