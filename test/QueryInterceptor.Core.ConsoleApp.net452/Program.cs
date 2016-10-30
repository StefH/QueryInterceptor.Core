using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace QueryInterceptor.Core.ConsoleApp.net452
{
    public class EqualsToNotEqualsVisitor : ExpressionVisitor
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

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello QueryInterceptor.Core.ConsoleApp.net452");

            IQueryable<int> query = Enumerable.Range(0, 10).AsQueryable().Where(n => n > 0 && n % 2 == 0);

            List<int> numbersEven = query.ToList();
            Console.WriteLine("numbersEven > 0 = {0}", string.Join(", ", numbersEven));

            var visitor = new EqualsToNotEqualsVisitor();
            List<int> numbersOdd = query.InterceptWith(visitor).Where(x => x >= 0).ToList();
            Console.WriteLine("numbersOdd  > 0 = {0}", string.Join(", ", numbersOdd));


            Console.WriteLine(new String('-', 80));


            Console.WriteLine("Enable ExpressionOptimizer");
            ExtensibilityPoint.QueryOptimizer = ExpressionOptimizer.visit;


            numbersEven = query.ToList();
            Console.WriteLine("numbersEven > 0 = {0}", string.Join(", ", numbersEven));

            numbersOdd = query.InterceptWith(visitor).Where(x => x >= 0).ToList();
            Console.WriteLine("numbersOdd  > 0 = {0}", string.Join(", ", numbersOdd));


            Console.WriteLine("Press key...");
            Console.ReadLine();
        }
    }
}