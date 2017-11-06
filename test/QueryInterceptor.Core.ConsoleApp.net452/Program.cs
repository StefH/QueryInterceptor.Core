using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Newtonsoft.Json;
using QueryInterceptor.Core.ConsoleApp.net452.Database;

namespace QueryInterceptor.Core.ConsoleApp.net452
{
    class Program
    {
        static void Main(string[] args)
        {
            var visitor = EqualsToNotEqualsVisitorFactory.EqualsToNotEqualsVisitor;

            Console.WriteLine("Hello QueryInterceptor.Core.ConsoleApp.net452");

            IQueryable<int> query = Enumerable.Range(0, 10).AsQueryable().Where(n => n > 0 && n % 2 == 0);

            List<int> numbersEven = query.ToList();
            Console.WriteLine("numbersEven > 0 = {0}", string.Join(", ", numbersEven));

            
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
            var carsAsyncToListAsync0 = ctx.Cars.Where(x => x.Color == "White").ToListAsync();
            Console.WriteLine("carsAsyncToListAsync {0}", JsonConvert.SerializeObject(carsAsyncToListAsync0.Result, Formatting.Indented));

            var carsAsyncToListAsync = ctx.Cars.InterceptWith(visitor).Where(x => x.Color == "White").ToListAsync();
            Console.WriteLine("carsAsyncToListAsync InterceptWith {0}", JsonConvert.SerializeObject(carsAsyncToListAsync.Result, Formatting.Indented));

            var carFirstOrDefault = ctx.Cars.InterceptWith(visitor).Where(x => x.Color == "White").FirstOrDefault();
            Console.WriteLine("carFirstOrDefault InterceptWith {0}", JsonConvert.SerializeObject(carFirstOrDefault, Formatting.Indented));

            var carFirstOrDefaultAsync = ctx.Cars.InterceptWith(visitor).Where(x => x.Color == "White").FirstOrDefaultAsync();
            Console.WriteLine("carFirstOrDefaultAsync InterceptWith {0}", JsonConvert.SerializeObject(carFirstOrDefaultAsync.Result, Formatting.Indented));

            Console.WriteLine("Press key...");
            Console.ReadLine();
        }
    }
}