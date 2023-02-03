using QueryInterceptor.Core.Validation;
using System.Linq.Expressions;

namespace QueryInterceptor.Core.ExpressionVisitors {
    public class StringComparisonVisitor : ExpressionVisitor {
        const string DummyString = "_QCore_";

        readonly StringComparison _comparer;

        public StringComparisonVisitor(StringComparison comparer) {
            _comparer = comparer;
        }

        protected override Expression VisitBinary(BinaryExpression node) {
            if (node.Left.Type == typeof(string) && node.Right.Type == typeof(string)) {
                if (node.NodeType == ExpressionType.Equal) {
                    var exp = ((LambdaExpression)MakeExpression(s => s.Equals(DummyString, _comparer))).Body;
                    exp = new ReplacingVisitor(((dynamic)exp).Arguments[0], ((dynamic)node).Left).Visit(exp);
                    exp = new ReplacingVisitor(((dynamic)exp).Object, ((dynamic)node).Right).Visit(exp);
                    return exp;
                }

                if (node.NodeType == ExpressionType.NotEqual) {
                    var exp = ((LambdaExpression)MakeExpression(s => !s.Equals(DummyString, _comparer))).Body;
                    exp = new ReplacingVisitor(((dynamic)exp).Operand.Arguments[0], ((dynamic)node).Left).Visit(exp);
                    exp = new ReplacingVisitor(((dynamic)exp).Operand.Object, ((dynamic)node).Right).Visit(exp);
                    return exp;
                }
            }
            return base.VisitBinary(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node) {
            Check.NotNull(node, nameof(node));

            if (node.Method.DeclaringType == typeof(string)) {
                switch (node.Method.Name) {
                    case "Contains": {
                            var exp = ((LambdaExpression)MakeExpression(s => s.IndexOf(DummyString, _comparer) > -1)).Body;
                            exp = new ReplacingVisitor(((dynamic)exp).Left.Arguments[0], ((dynamic)node).Arguments[0]).Visit(exp);
                            exp = new ReplacingVisitor(((dynamic)exp).Left.Object, ((dynamic)node).Object).Visit(exp);
                            return exp;
                        }
                    case "StartsWith": {
                            var exp = ((LambdaExpression)MakeExpression(s => s.IndexOf(DummyString, _comparer) == 0)).Body;
                            exp = new ReplacingVisitor(((dynamic)exp).Left.Arguments[0], ((dynamic)node).Arguments[0]).Visit(exp);
                            exp = new ReplacingVisitor(((dynamic)exp).Left.Object, ((dynamic)node).Object).Visit(exp);
                            return exp;
                        }
                    case "EndsWith": {
                            var exp = ((LambdaExpression)MakeExpression(s => s.EndsWith(DummyString, _comparer))).Body;
                            exp = new ReplacingVisitor(((dynamic)exp).Arguments[0], ((dynamic)node).Arguments[0]).Visit(exp);
                            exp = new ReplacingVisitor(((dynamic)exp).Object, ((dynamic)node).Object).Visit(exp);
                            return exp;
                        }
                    case "Equals": {
                            var exp = ((LambdaExpression)MakeExpression(s => s.Equals(DummyString, _comparer))).Body;
                            exp = new ReplacingVisitor(((dynamic)exp).Arguments[0], ((dynamic)node).Arguments[0]).Visit(exp);
                            exp = new ReplacingVisitor(((dynamic)exp).Object, ((dynamic)node).Object).Visit(exp);
                            return exp;
                        }
                }
            }

            return base.VisitMethodCall(node);
        }

        private Expression MakeExpression(Expression<Func<string, bool>> exp) {
            return exp;
        }
    }
}