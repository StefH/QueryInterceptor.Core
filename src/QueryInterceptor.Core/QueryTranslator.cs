using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using QueryInterceptor.Core.Validation;

namespace QueryInterceptor.Core
{
    internal class QueryTranslator : QueryTranslator<object>
    {
        public QueryTranslator(IQueryable source, IEnumerable<ExpressionVisitor> visitors) : base(source, visitors)
        {
        }

        public QueryTranslator(IQueryable source, Expression expression, IEnumerable<ExpressionVisitor> visitors) : base(source, expression, visitors)
        {
        }
    }

    internal class QueryTranslator<T> : IOrderedQueryable<T>
#if EF || EFCORE
        , IAsyncEnumerable<T>
#endif
#if EF
        , System.Data.Entity.Infrastructure.IDbAsyncEnumerable<T>
#endif
    {
        private readonly Expression _expression;
        private readonly QueryTranslatorProviderAsync _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryTranslator{T}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="visitors">The visitors.</param>
        public QueryTranslator(IQueryable source, IEnumerable<ExpressionVisitor> visitors)
        {
            Check.NotNull(source, nameof(source));

            Check.NotNull(visitors, nameof(visitors));

            _expression = Expression.Constant(this);
            _provider = new QueryTranslatorProviderAsync(source, visitors);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryTranslator{T}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="visitors">The visitors.</param>
        public QueryTranslator(IQueryable source, Expression expression, IEnumerable<ExpressionVisitor> visitors)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(expression, nameof(expression));

            Check.NotNull(visitors, nameof(visitors));

            _expression = expression;
            _provider = new QueryTranslatorProviderAsync(source, visitors);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_provider.ExecuteEnumerable(_expression)).GetEnumerator();
        }

#if EF
        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetEnumerator()
        {
            return _provider.ExecuteAsync<T>(_expression).GetEnumerator();
        }
#endif
#if EFCORE
        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetEnumerator()
        {
            return new QueryTranslatorDbAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }
#endif

#if EF
        public System.Data.Entity.Infrastructure.IDbAsyncEnumerator<T> GetAsyncEnumerator()
        {
            return new QueryTranslatorDbAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        System.Data.Entity.Infrastructure.IDbAsyncEnumerator System.Data.Entity.Infrastructure.IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return GetAsyncEnumerator();
        }
#endif

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _provider.ExecuteEnumerable(_expression).GetEnumerator();
        }

        /// <summary>
        /// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable" /> is executed.
        /// </summary>
        /// <returns>A <see cref="T:System.Type" /> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.</returns>
        public Type ElementType => typeof(T);

        /// <summary>
        /// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable" />.
        /// </summary>
        /// <returns>The <see cref="T:System.Linq.Expressions.Expression" /> that is associated with this instance of <see cref="T:System.Linq.IQueryable" />.</returns>
        public Expression Expression => _expression;

        /// <summary>
        /// Gets the query provider that is associated with this data source.
        /// </summary>
        /// <returns>The <see cref="T:System.Linq.IQueryProvider" /> that is associated with this data source.</returns>
        public IQueryProvider Provider => _provider;
    }
}