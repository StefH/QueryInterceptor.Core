using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace QueryInterceptor.Core {
    internal class QueryTranslatorProviderAsync : ExpressionVisitor
#if EFCORE
        , System.Data.Entity.Infrastructure.IDbAsyncQueryProvider
#else
        , IQueryProvider
#endif
    {
        static readonly TraceSource s_traceSource = new(typeof(QueryTranslatorProviderAsync).Name);

        public IQueryProvider OriginalProvider { get; }

        internal IQueryable Source { get; }

        readonly IEnumerable<ExpressionVisitor> _visitors;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryTranslatorProviderAsync"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="visitors">The visitors.</param>
        public QueryTranslatorProviderAsync(IQueryable source, IEnumerable<ExpressionVisitor> visitors) {
            Source = source;
            OriginalProvider = source.Provider;
            _visitors = visitors;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) {
            return new QueryTranslator<TElement>(Source, expression, _visitors);
        }

        public IQueryable CreateQuery(Expression expression) {
            Type elementType = expression.Type.GetGenericArguments().First();
            var queryable = Activator.CreateInstance(typeof(QueryTranslator<>).MakeGenericType(elementType), Source, expression, _visitors) as IQueryable ?? throw new InvalidOperationException("Can't create IQueryable");

            return queryable;
        }

        public TResult Execute<TResult>(Expression expression) {
            var translated = VisitAllAndOptimize(expression);
            return Source.Provider.Execute<TResult>(translated);
        }

        public object Execute(Expression expression) {
            return Execute<object>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
#if EFCORE
            if (Source.Provider is Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryProvider entityQueryProvider) {
                var translated = VisitAllAndOptimize(expression);
#pragma warning disable EF1001
                return entityQueryProvider.ExecuteAsync<TResult>(translated, cancellationToken);
#pragma warning restore EF1001
            }
#endif
            // In case Source.Provider is not a IDbAsyncQueryProvider or EntityQueryProvider, just start a new Task
            return Task.Factory.StartNew(() => Execute<TResult>(expression), cancellationToken).Result;
        }

        public object ExecuteAsync(Expression expression) {
            return ExecuteAsync(expression, CancellationToken.None);
        }

        public object ExecuteAsync(Expression expression, CancellationToken cancellationToken) {
            return ExecuteAsync<object>(expression, cancellationToken);
        }

        internal IEnumerable ExecuteEnumerable(Expression expression) {
            var translated = VisitAllAndOptimize(expression);
            return Source.Provider.CreateQuery(translated);
        }

        Expression VisitAllAndOptimize(Expression expression) {
            var visitors = new ExpressionVisitor[] { this }.Concat(_visitors);
            var translated = visitors.Aggregate(expression, (expr, visitor) => visitor.Visit(expr));
            return OptimizeExpression(translated);
        }

        /// <summary>
        /// Visits the <see cref="T:System.Linq.Expressions.ConstantExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitConstant(ConstantExpression node) {
            if (node.Type.GetTypeInfo().IsGenericType && node.Type.GetGenericTypeDefinition() == typeof(QueryTranslator<>)) {
                if (node.Value is IQueryable { Provider: QueryTranslatorProviderAsync provider }) {
                    return provider.Source.Expression;
                }
                return Source.Expression;
            }
            return base.VisitConstant(node);
        }

        static Expression OptimizeExpression(Expression expression) {
            if (ExtensibilityPoint.QueryOptimizer != null) {
                var optimized = ExtensibilityPoint.QueryOptimizer(expression);
                if (optimized != expression) {
                    s_traceSource.TraceEvent(TraceEventType.Verbose, 0, "Expression before : {0}", expression);
                    s_traceSource.TraceEvent(TraceEventType.Verbose, 0, "Expression after  : {0}", optimized);
                }
                return optimized;
            }
            return expression;
        }
    }
}