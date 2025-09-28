using Azure.Data.Tables;
using System.Linq.Expressions;

namespace AzureTable.Provider.Query;

public sealed class TableQueryConfiguration
{
    public IFilter? Filter { get; set; }
    public int? MaxPerPage 
    {
#pragma warning disable S1135 // Track uses of "TODO" tags
        // TODO: Conditionally use 'field' keyword when C# 14/.NET 10 is available.
#pragma warning restore S1135 // Track uses of "TODO" tags
        
        get => _maxPerPage;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "MaxPerPage must be greater than zero.");
            }
            _maxPerPage = value;
        }
    }
    public IEnumerable<string>? Select { get; set; }

    private int? _maxPerPage;
}

public interface IFilter;

public sealed class StringFilter(string filter) : IFilter
{
    public string Filter { get; } = !string.IsNullOrWhiteSpace(filter)
        ? filter
        : throw new ArgumentException("String Filter cannot be null, empty, or white space.", nameof(filter));
}

public sealed class PredicateFilter<TEntity>(Expression<Func<TEntity, bool>> filter) : IFilter where TEntity : class, ITableEntity
{
    public Expression<Func<TEntity, bool>> Filter { get; } = filter ?? throw new ArgumentNullException(nameof(filter));
}