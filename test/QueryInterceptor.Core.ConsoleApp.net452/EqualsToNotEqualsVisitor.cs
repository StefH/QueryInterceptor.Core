using System.Linq.Expressions;

namespace QueryInterceptor.Core.ConsoleApp.net452
{
    internal class EqualsToNotEqualsVisitor : ExpressionVisitor
    {
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.Equal)
            {
                // Change == to != and add dummy
                int seven = 7;

                return Expression.AndAlso(Expression.NotEqual(node.Left, node.Right), Expression.Equal(Expression.Constant(7), Expression.Constant(seven)));
            }

            return base.VisitBinary(node);
        }
    }
}