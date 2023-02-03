using System.Linq.Expressions;

namespace QueryInterceptor.Core.ExpressionVisitors {
    public class EqualsToNotEqualsVisitor : ExpressionVisitor {
        protected override Expression VisitBinary(BinaryExpression node) {
            if (node.NodeType == ExpressionType.Equal) {
                // Change == to !=
                return Expression.NotEqual(node.Left, node.Right);
            }
            return base.VisitBinary(node);
        }
    }
}