using Azure.Data.Tables;
using System.Linq.Expressions;

namespace AzureTable.Provider.Query;

public sealed class TableQueryConfiguration
{
    public IFilter? Filter { get; set; }
    public int? MaxPerPage { get; set; }
    public IEnumerable<string>? Select { get; set; }
}

public interface IFilter;

public sealed class StringFilter(string filter) : IFilter
{
    public string Filter { get; } = filter;
}

public sealed class PredicateFilter<TEntity>(Expression<Func<TEntity, bool>> filter) : IFilter where TEntity : class, ITableEntity
{
    public Expression<Func<TEntity, bool>> Filter { get; } = filter;
}