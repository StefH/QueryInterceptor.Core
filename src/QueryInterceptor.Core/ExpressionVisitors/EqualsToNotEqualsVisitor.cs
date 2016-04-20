using System.Linq.Expressions;
using QueryInterceptor.Core.Validation;

namespace QueryInterceptor.Core.ExpressionVisitors
{
    public class EqualsToNotEqualsVisitor : ExpressionVisitor
    {
        protected override Expression VisitBinary(BinaryExpression node)
        {
            Check.NotNull(node, nameof(node));

            if (node.NodeType == ExpressionType.Equal)
            {
                // Change == to !=
                return Expression.NotEqual(node.Left, node.Right);
            }

            return base.VisitBinary(node);
        }
    }
}