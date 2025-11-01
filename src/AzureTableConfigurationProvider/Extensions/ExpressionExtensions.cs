using System.Linq.Expressions;

namespace AzureTable.Provider.Extensions;

internal static class ExpressionExtensions
{
    public static string GetMemberName<T, U>(this Expression<Func<T, U>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression unaryMemberExpression)
        {
            return unaryMemberExpression.Member.Name;
        }

        if (expression.Body is MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.Name == "get_Item" &&
                methodCallExpression.Arguments is [var argument])
            {
                return argument switch
                {
                    ConstantExpression constantExpression when constantExpression.Value is string str => str,
                    _ => throw new ArgumentException("Unsupported argument type in indexer expression.", nameof(expression))
                };
            }

            throw new ArgumentException($"Unsupported method call expression: {methodCallExpression.Method.Name}", nameof(expression));
        }

        throw new ArgumentException("Invalid expression. Expected member access, unary expression, or indexer access.", nameof(expression));
    }

    public static Func<T, string?> ToValueFactory<T, TValue>(this Expression<Func<T, TValue>> expression) where T : class
    {
        var compiled = expression.Compile();
        
        string? ValueFactory(T instance)
            => compiled(instance).ToInvariantString();

        return ValueFactory;
    }

    public static string? ToInvariantString<T>(this T? value)
        => value switch
        {
            null => null,
            string str => str,
            IFormattable formattable => formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
}
