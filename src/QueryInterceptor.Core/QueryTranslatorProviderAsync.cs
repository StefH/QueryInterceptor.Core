using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using QueryInterceptor.Core.Validation;
using System.Reflection;

namespace QueryInterceptor.Core
{
    internal class QueryTranslatorProviderAsync : QueryTranslatorProvider, IDbAsyncQueryProvider
    {
        private readonly IEnumerable<ExpressionVisitor> _visitors;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryTranslatorProviderAsync"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="visitors">The visitors.</param>
        public QueryTranslatorProviderAsync(IQueryable source, IEnumerable<ExpressionVisitor> visitors)
            : base(source)
        {
            // ReSharper disable PossibleMultipleEnumeration
            Check.NotNull(visitors, nameof(visitors));

            _visitors = visitors;
            // ReSharper restore PossibleMultipleEnumeration
        }

        /// <summary>
        /// Creates the query.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns>IQueryable{TElement}</returns>
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            return new QueryTranslator<TElement>(Source, expression, _visitors);
        }

        /// <summary>
        /// Constructs an <see cref="T:System.Linq.IQueryable" /> object that can evaluate the query represented by a specified expression tree.
        /// </summary>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>An <see cref="T:System.Linq.IQueryable" /> that can evaluate the query represented by the specified expression tree.</returns>
        public IQueryable CreateQuery(Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            Type elementType = expression.Type.GetGenericArguments().First();
            return (IQueryable)Activator.CreateInstance(typeof(QueryTranslator<>).MakeGenericType(elementType), Source, expression, _visitors);
        }

        /// <summary>
        /// Executes the query represented by a specified expression tree.
        /// </summary>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>The value that results from executing the specified query.</returns>
        public TResult Execute<TResult>(Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var translated = VisitAll(expression);
            return Source.Provider.Execute<TResult>(translated);
        }

        /// <summary>
        /// Executes the query represented by a specified expression tree.
        /// </summary>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>The value that results from executing the specified query.</returns>
        public object Execute(Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            return Execute<object>(expression);
        }

#if NETSTANDARD
        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            Check.NotNull(expression, "expression");

            var provider = Source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                var translated = VisitAll(expression);
                return provider.ExecuteAsync<TResult>(translated);
            }

            // In case Source.Provider is not a IDbAsyncQueryProvider, just execute normal
            return (IAsyncEnumerable<TResult>) Execute<TResult>(expression);
        }
#else
        /// <summary>
        /// Asynchronously executes the query represented by a specified expression tree.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the value that results from executing the specified query.
        /// </returns>
        public Task<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            return ExecuteAsync<TResult>(expression, CancellationToken.None);
        }
#endif

        /// <summary>
        /// Asynchronously executes the query represented by a specified expression tree.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the value that results from executing the specified query.
        /// </returns>
        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            Check.NotNull(expression, nameof(expression));

            cancellationToken.ThrowIfCancellationRequested();

            var provider = Source.Provider as IDbAsyncQueryProvider;
            if (provider != null)
            {
                var translated = VisitAll(expression);
                return provider.ExecuteAsync<TResult>(translated, cancellationToken);
            }

            // In case Source.Provider is not a IDbAsyncQueryProvider, just start a new Task
            return Task.Factory.StartNew(() => Execute<TResult>(expression), cancellationToken);
        }

        /// <summary>
        /// Asynchronously executes the query represented by a specified expression tree.
        /// </summary>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the value that results from executing the specified query.
        /// </returns>
        public Task<object> ExecuteAsync(Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            return ExecuteAsync(expression, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously executes the query represented by a specified expression tree.
        /// </summary>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <param name="cancellationToken">A
        /// <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the value that results from executing the specified query.
        /// </returns>
        public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
        {
            Check.NotNull(expression, nameof(expression));

            return ExecuteAsync<object>(expression, cancellationToken);
        }

        internal IEnumerable ExecuteEnumerable(Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var translated = VisitAll(expression);
            return Source.Provider.CreateQuery(translated);
        }

        private Expression VisitAll(Expression expression)
        {
            // Run all visitors in order
            var visitors = new ExpressionVisitor[] { this }.Concat(_visitors);

            return visitors.Aggregate(expression, (expr, visitor) => visitor.Visit(expr));
        }

        /// <summary>
        /// Visits the <see cref="T:System.Linq.Expressions.ConstantExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            Check.NotNull(node, nameof(node));

            // Fix up the Expression tree to work with the underlying LINQ provider
            if (node.Type.GetTypeInfo().IsGenericType && node.Type.GetGenericTypeDefinition() == typeof(QueryTranslator<>))
            {
                var provider = ((IQueryable)node.Value).Provider as QueryTranslatorProvider;

                if (provider != null)
                {
                    return provider.Source.Expression;
                }

                return Source.Expression;
            }

            return base.VisitConstant(node);
        }
    }
}
