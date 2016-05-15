using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace QueryInterceptor.Core.ConsoleApp
{
    public class EqualsToNotEqualsVisitor : ExpressionVisitor
    {
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.Equal)
            {
                // Change == to !=
                return Expression.NotEqual(node.Left, node.Right);
            }

            return base.VisitBinary(node);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello QueryInterceptor.Core");

            IQueryable<int> query = Enumerable.Range(0, 10).AsQueryable().Where(n => n > 0 && n % 2 == 0);

            List<int> numbersEven = query.ToList();
            Console.WriteLine("numbersEven > 0 = {0}", string.Join(", ", numbersEven));

            var visitor = new EqualsToNotEqualsVisitor();
            List<int> numbersOdd = query.InterceptWith(visitor).Where(x => x >= 0).ToList();
            Console.WriteLine("numbersOdd  > 0 = {0}", string.Join(", ", numbersOdd));

            Console.WriteLine("Press key...");
            Console.ReadLine();
        }
    }
}