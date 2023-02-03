using System.Linq.Expressions;

namespace QueryInterceptor.Core {
    /// <summary>
    /// Extensibility point: If you want to modify expanded queries before executing them
    /// set your own functionality to override empty QueryOptimizer
    /// </summary>
    public static class ExtensibilityPoint {
        /// <summary>
        /// Place to optimize your queries. Example: Add a reference to Nuget package Linq.Expression.Optimizer 
        /// and in your program initializers set Extensibility.QueryOptimizer = ExpressionOptimizer.visit;
        /// </summary>
        public static Func<Expression, Expression> QueryOptimizer = e => e;
    }
}