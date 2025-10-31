using AzureTable.Provider.Configuration.Mapping.Elements;
using System.Linq.Expressions;

namespace AzureTable.Provider.Configuration.Mapping;

public static class ConfigurationMapperObjectExtensions
{
    public static TMapper AddObject<T, TMapper>(this TMapper mapper, string name, Action<ObjectMapper<T>> configure)
        where T : class
        where TMapper : BaseConfigurationMapper<T>
    {
        var namedObjectElement = new NamedObjectElement<T>(name);
        var objectMapper = new ObjectMapper<T>(namedObjectElement);
        configure(objectMapper);
        mapper.Container.Add(namedObjectElement);
        return mapper;
    }

    public static TMapper AddObject<T, TMapper>(this TMapper mapper, Expression<Func<T, string>> nameExpression, Action<ObjectMapper<T>> configure)
        where T : class
        where TMapper : BaseConfigurationMapper<T>
    {
        var namedObjectElement = new NamedObjectElement<T>(nameExpression);
        var objectMapper = new ObjectMapper<T>(namedObjectElement);
        configure(objectMapper);
        mapper.Container.Add(namedObjectElement);
        return mapper;
    }

    public static TMapper AddTypedObject<T, TMapper, TObject>(this TMapper mapper, string name, Action<TypedObjectMapper<T, TObject>> configure)
        where T : class
        where TMapper : BaseConfigurationMapper<T>
        where TObject : class
    {
        var namedObjectElement = new NamedObjectElement<T>(name);
        var objectMapper = new TypedObjectMapper<T, TObject>(namedObjectElement);
        configure(objectMapper);
        mapper.Container.Add(namedObjectElement);
        return mapper;
    }

    public static TMapper AddTypedObject<T, TMapper, TObject>(this TMapper mapper, Expression<Func<T, string>> nameExpression, Action<TypedObjectMapper<T, TObject>> configure)
        where T : class
        where TMapper : BaseConfigurationMapper<T>
        where TObject : class
    {
        var namedObjectElement = new NamedObjectElement<T>(nameExpression);
        var objectMapper = new TypedObjectMapper<T, TObject>(namedObjectElement);
        configure(objectMapper);
        mapper.Container.Add(namedObjectElement);
        return mapper;
    }
}
