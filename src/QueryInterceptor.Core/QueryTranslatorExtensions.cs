using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using QueryInterceptor.Core.Validation;

namespace QueryInterceptor.Core
{
    public static class QueryTranslatorExtensions
    {
        /// <summary>
        /// An extension method on IQueryable{T} that lets you plug in arbitrary expression visitors.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="visitors">The visitors.</param>
        /// <returns>IQueryable{T}</returns>
        public static IQueryable<T> InterceptWith<T>([NotNull] this IQueryable<T> source, [NotNull] params ExpressionVisitor[] visitors)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(visitors, nameof(visitors));
            Check.HasNoNulls(visitors, nameof(visitors));

            return new QueryTranslator<T>(source, visitors);
        }

        /// <summary>
        /// An extension method on IQueryable that lets you plug in arbitrary expression visitors.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="visitors">The visitors.</param>
        /// <returns>IQueryable</returns>
        public static IQueryable InterceptWith([NotNull] this IQueryable source, [NotNull] params ExpressionVisitor[] visitors)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(visitors, nameof(visitors));
            Check.HasNoNulls(visitors, nameof(visitors));

            return new QueryTranslator(source, visitors);
        }
    }
}