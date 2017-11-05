using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using QueryInterceptor.Core.Validation;
using JetBrains.Annotations;

namespace QueryInterceptor.Core
{
    internal class QueryTranslatorProviderAsync : ExpressionVisitor
#if EF || EFCORE
        , System.Data.Entity.Infrastructure.IDbAsyncQueryProvider
#else
        , IQueryProvider
#endif
    {
        private static readonly TraceSource TraceSource = new TraceSource(typeof(QueryTranslatorProviderAsync).Name);
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
            return (IQueryable)Activator.CreateInstance(typeof(QueryTranslator<>).MakeGenericType(elementType), Source, expression, _visitors);
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

#if EF
        [PublicAPI]
        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var dbAsyncQueryProvider = Source.Provider as System.Data.Entity.Infrastructure.IDbAsyncQueryProvider;
            if (dbAsyncQueryProvider != null)
            {
                var translated = VisitAllAndOptimize(expression);
                return dbAsyncQueryProvider.ExecuteAsync<TResult>(translated, CancellationToken.None).ToAsyncEnumerable();
            }

            // In case Source.Provider is not a IDbAsyncQueryProvider, just execute normal
            return (IAsyncEnumerable<TResult>)Execute<TResult>(expression);
        }

#elif EFCORE
        [PublicAPI]
        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var entityQueryProvider = Source.Provider as Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryProvider;
            if (entityQueryProvider != null)
            {
                var translated = VisitAllAndOptimize(expression);
                return entityQueryProvider.ExecuteAsync<TResult>(translated);
            }

            // In case Source.Provider is not a EntityQueryProvider, just execute normal
            return (IAsyncEnumerable<TResult>)Execute<TResult>(expression);
        }
#else
        [PublicAPI]
        public Task<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            return ExecuteAsync<TResult>(expression, CancellationToken.None);
        }
#endif

        [PublicAPI]
        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            Check.NotNull(expression, nameof(expression));

            cancellationToken.ThrowIfCancellationRequested();

#if EF
            var dbAsyncQueryProvider = Source.Provider as System.Data.Entity.Infrastructure.IDbAsyncQueryProvider;
            if (dbAsyncQueryProvider != null)
            {
                var translated = VisitAllAndOptimize(expression);
                return dbAsyncQueryProvider.ExecuteAsync<TResult>(translated, cancellationToken);
            }
#elif EFCORE
            var entityQueryProvider = Source.Provider as Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryProvider;
            if (entityQueryProvider != null)
            {
                var translated = VisitAllAndOptimize(expression);
                return entityQueryProvider.ExecuteAsync<TResult>(translated, cancellationToken);
            }
#endif

            // In case Source.Provider is not a IDbAsyncQueryProvider or EntityQueryProvider, just start a new Task
            return Task.Factory.StartNew(() => Execute<TResult>(expression), cancellationToken);
        }

        [PublicAPI]
        public Task<object> ExecuteAsync(Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            return ExecuteAsync(expression, CancellationToken.None);
        }

        [PublicAPI]
        public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
        {
            Check.NotNull(expression, nameof(expression));

            return ExecuteAsync<object>(expression, cancellationToken);
        }

        internal IEnumerable ExecuteEnumerable([CanBeNull] Expression expression)
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
                var provider = ((IQueryable)node.Value).Provider as QueryTranslatorProviderAsync;

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
}