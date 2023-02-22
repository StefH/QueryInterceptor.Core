using System;
using System.ComponentModel.DataAnnotations;
using Linq.PropertyTranslator.Core;

namespace QueryInterceptor.Core.ConsoleApp.Database
{
    public class Car
    {
        [Key]
        public int Key { get; set; }

        [Required]
        [StringLength(8)]
        public string Vin { get; set; }

        [Required]
        public string Year { get; set; }

        [Required]
        public string Brand { get; set; }

        [Required]
        public string Color { get; set; }

        private static readonly CompiledExpressionMap<Car, double> sqrtExpression
            = DefaultTranslationOf<Car>.Property(s => s.Sqrt).Is(s => Math.Sqrt(s.Key + 7));

        public double Sqrt => sqrtExpression.Evaluate(this);
    }
}
