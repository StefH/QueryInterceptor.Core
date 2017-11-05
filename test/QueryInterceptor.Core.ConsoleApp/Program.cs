using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QueryInterceptor.Core.ConsoleApp.Database;

namespace QueryInterceptor.Core.ConsoleApp
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

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello QueryInterceptor.EntityFrameworkCore");

            Console.WriteLine("Enable ExpressionOptimizer");
            ExtensibilityPoint.QueryOptimizer = ExpressionOptimizer.visit;

            var visitor = new EqualsToNotEqualsVisitor();

            // doit

            IQueryable<int> query = Enumerable.Range(0, 10).AsQueryable().Where(n => n > 0 && n % 2 == 0);

            List<int> numbersEven = query.ToList();
            Console.WriteLine("numbersEven > 0 = {0}", string.Join(", ", numbersEven));

            
            List<int> numbersOdd = query.InterceptWith(visitor).Where(x => x >= 0).ToList();
            Console.WriteLine("numbersOdd  > 0 = {0}", string.Join(", ", numbersOdd));

            var context = new NorthwindModel();

            if (context.Database.EnsureCreated())
            {
                context.Cars.Add(new Car { Brand = "Ford", Color = "Blue" });
                context.Cars.Add(new Car { Brand = "Fiat", Color = "Red" });
                context.Cars.Add(new Car { Brand = "Alfa", Color = "Black" });
                context.SaveChanges();
            }

            var carFirstOrDefault = context.Cars.AsQueryable().InterceptWith(visitor).Where(x => x.Color == "Blue").FirstOrDefault();
            Console.WriteLine("carFirstOrDefault {0}", JsonConvert.SerializeObject(carFirstOrDefault, Formatting.Indented));

            var carFirstOrDefaultAsync = context.Cars.AsQueryable().InterceptWith(visitor).Where(x => x.Color == "Blue").FirstOrDefaultAsync();
            Console.WriteLine("carAsync {0}", JsonConvert.SerializeObject(carFirstOrDefaultAsync.Result, Formatting.Indented));

            var carToListAsync = context.Cars.AsQueryable().InterceptWith(visitor).Where(x => x.Color == "Blue").ToListAsync();
            Console.WriteLine("carToListAsync {0}", JsonConvert.SerializeObject(carToListAsync.Result, Formatting.Indented));

            Console.WriteLine("Press key...");
            Console.ReadLine();
        }
    }
}