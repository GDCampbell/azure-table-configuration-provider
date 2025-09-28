using System.Linq.Expressions;

namespace AzureTable.Provider.Extensions;

internal static class ExpressionExtensions
{
    /// <summary>
    /// Extracts the member or indexer key name referenced by the provided lambda expression.
    /// </summary>
    /// <param name="expression">A lambda expression that accesses a member or an indexer (e.g., x => x.Property or x => x["key"]).</param>
    /// <returns>The member name or the indexer key string referenced by the expression.</returns>
    /// <exception cref="ArgumentException">Thrown when the expression is not a member access, a unary-wrapped member access, or a supported indexer access, or when an indexer argument is not a string constant.</exception>
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
}
