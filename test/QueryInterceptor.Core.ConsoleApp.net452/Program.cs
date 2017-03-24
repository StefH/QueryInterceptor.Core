﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using QueryInterceptor.Core.ConsoleApp.net452.Database;

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


            Console.WriteLine(new string('-', 80));


            Console.WriteLine("Enable ExpressionOptimizer");
            ExtensibilityPoint.QueryOptimizer = ExpressionOptimizer.visit;


            numbersEven = query.ToList();
            Console.WriteLine("numbersEven > 0 = {0}", string.Join(", ", numbersEven));

            numbersOdd = query.InterceptWith(visitor).Where(x => x >= 0).ToList();
            Console.WriteLine("numbersOdd  > 0 = {0}", string.Join(", ", numbersOdd));

            var ctx = new NorthwindModel();

            var car = ctx.Cars.AsQueryable().InterceptWith(visitor).Where(x => x.Key >= 0).FirstOrDefault();
            Console.WriteLine("car      {0}", JsonConvert.SerializeObject(car));

            var carAsync = ctx.Cars.AsQueryable().InterceptWith(visitor).Where(x => x.Key >= 0).FirstOrDefaultAsync();
            Console.WriteLine("carAsync {0}", JsonConvert.SerializeObject(carAsync.Result));

            Console.WriteLine("Press key...");
            Console.ReadLine();
        }
    }
}