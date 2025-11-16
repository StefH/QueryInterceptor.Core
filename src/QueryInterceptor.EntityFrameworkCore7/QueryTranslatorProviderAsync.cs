using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using QueryInterceptor.Core.Validation;

namespace QueryInterceptor.Core;

internal class QueryTranslatorProviderAsync : ExpressionVisitor, System.Data.Entity.Infrastructure.IDbAsyncQueryProvider
{
    private static readonly TraceSource TraceSource = new(typeof(QueryTranslatorProviderAsync).Name);
    private readonly IEnumerable<ExpressionVisitor> _visitors;

    internal IQueryable Source { get; }

    public IQueryProvider OriginalProvider { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryTranslatorProviderAsync"/> class.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="visitors">The visitors.</param>
    public QueryTranslatorProviderAsync([NotNull] IQueryable source, [NotNull] IEnumerable<ExpressionVisitor> visitors)
    {
        Check.NotNull(source, nameof(source));
        Check.NotNull(visitors, nameof(visitors));

        Source = source;
        OriginalProvider = source.Provider;
        _visitors = visitors;
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        Check.NotNull(expression, nameof(expression));

        return new QueryTranslator<TElement>(Source, expression, _visitors);
    }

    [PublicAPI]
    public IQueryable CreateQuery(Expression expression)
    {
        Check.NotNull(expression, nameof(expression));

        Type elementType = expression.Type.GetGenericArguments().First();
        return (IQueryable)Activator.CreateInstance(typeof(QueryTranslator<>).MakeGenericType(elementType), Source, expression, _visitors)!;
    }

    [PublicAPI]
    public TResult Execute<TResult>(Expression expression)
    {
        Check.NotNull(expression, nameof(expression));

        var translated = VisitAllAndOptimize(expression);

        return Source.Provider.Execute<TResult>(translated);
    }

    [PublicAPI]
    public object Execute(Expression expression)
    {
        Check.NotNull(expression, nameof(expression));

        return Execute<object>(expression);
    }

    [PublicAPI]
    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        Check.NotNull(expression, nameof(expression));

        if (Source.Provider is Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryProvider entityQueryProvider)
        {
            var translated = VisitAllAndOptimize(expression);
#pragma warning disable EF1001
            return entityQueryProvider.ExecuteAsync<TResult>(translated);
#pragma warning restore EF1001
        }

        return Execute<TResult>(expression);
    }

    internal IEnumerable ExecuteEnumerable(Expression expression)
    {
        Check.NotNull(expression, nameof(expression));

        var translated = VisitAllAndOptimize(expression);

        return Source.Provider.CreateQuery(translated);
    }

    private Expression VisitAllAndOptimize(Expression expression)
    {
        // Run all visitors in order
        var visitors = new ExpressionVisitor[] { this }.Concat(_visitors);

        var translated = visitors.Aggregate(expression, (expr, visitor) => visitor.Visit(expr));

        var optimized = OptimizeExpression(translated);

        return optimized;
    }

    /// <summary>
    /// Visits the <see cref="T:System.Linq.Expressions.ConstantExpression" />.
    /// </summary>
    /// <param name="node">The expression to visit.</param>
    /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
    protected override Expression VisitConstant(ConstantExpression node)
    {
        Check.NotNull(node, nameof(node));

        // Fix up the Expression tree to work with the underlying LINQ provider
        if (node.Type.GetTypeInfo().IsGenericType && node.Type.GetGenericTypeDefinition() == typeof(QueryTranslator<>))
        {
            var provider = (node.Value as IQueryable)?.Provider as QueryTranslatorProviderAsync;
            if (provider != null)
            {
                return provider.Source.Expression;
            }

            return Source.Expression;
        }

        return base.VisitConstant(node);
    }

    private static Expression OptimizeExpression(Expression expression)
    {
        if (ExtensibilityPoint.QueryOptimizer != null)
        {
            var optimized = ExtensibilityPoint.QueryOptimizer(expression);

            if (optimized != expression)
            {
                TraceSource.TraceEvent(TraceEventType.Verbose, 0, "Expression before : {0}", expression);
                TraceSource.TraceEvent(TraceEventType.Verbose, 0, "Expression after  : {0}", optimized);
            }

            return optimized;
        }

        return expression;
    }
}