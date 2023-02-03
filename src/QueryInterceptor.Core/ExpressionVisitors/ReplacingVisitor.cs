using JetBrains.Annotations;
using QueryInterceptor.Core.Validation;
using System.Linq.Expressions;

namespace QueryInterceptor.Core.ExpressionVisitors {
    /// <summary>
    /// copied from https://gist.github.com/mcintyre321/6294588
    /// </summary>
    public class ReplacingVisitor : ExpressionVisitor {
        private readonly Func<Expression, bool> _match;
        private readonly Func<Expression, Expression> _createReplacement;

        public ReplacingVisitor([NotNull] Expression from, [NotNull] Expression to) {
            Check.NotNull(from, nameof(from));
            Check.NotNull(to, nameof(to));

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