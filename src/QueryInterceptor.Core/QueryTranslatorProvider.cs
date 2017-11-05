//using System.Linq;
//using System.Linq.Expressions;
//using QueryInterceptor.Core.Validation;
//using JetBrains.Annotations;

//namespace QueryInterceptor.Core
//{
//    internal abstract class QueryTranslatorProvider : ExpressionVisitor
//    {
//        /// <summary>
//        /// Initializes a new instance of the <see cref="QueryTranslatorProvider"/> class.
//        /// </summary>
//        /// <param name="source">The source.</param>
//        protected QueryTranslatorProvider([NotNull] IQueryable source)
//        {
//            Check.NotNull(source, nameof(source));
//            Check.NotNull(source.Provider, nameof(source.Provider));

//            Source = source;
//            OriginalProvider = source.Provider;
//        }

//        internal IQueryable Source { get; }

//        public IQueryProvider OriginalProvider { get; private set; }
//    }
//}