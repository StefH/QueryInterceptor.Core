using System.Linq;
using System.Linq.Expressions;
using QueryInterceptor.Core.Validation;

namespace QueryInterceptor.Core
{
    internal abstract class QueryTranslatorProvider : ExpressionVisitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryTranslatorProvider"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        protected QueryTranslatorProvider(IQueryable source)
        {
            Check.NotNull(source, nameof(source));

            Source = source;
        }

        internal IQueryable Source { get; }
    }
}