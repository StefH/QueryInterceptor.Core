using QueryInterceptor.Core.Validation;
using System.Linq.Expressions;

namespace QueryInterceptor.Core.ExpressionVisitors {
    public class ReplacingVisitor : ExpressionVisitor {
        readonly Func<Expression, bool> _match;
        readonly Func<Expression, Expression> _createReplacement;

        public ReplacingVisitor(Expression from, Expression to) {
            _match = node => from == node;
            _createReplacement = node => to;
        }

        public override Expression Visit(Expression? node) {
            Check.NotNull(node, nameof(node));
            if (_match(node)) {
                return _createReplacement(node);
            }
            return base.Visit(node);
        }
    }
}