using System.ComponentModel.DataAnnotations;

namespace QueryInterceptor.Core.ConsoleApp.Database {
    public class Car {
        [Key]
        public int Key { get; set; }

        [StringLength(8)]
        public string? Vin { get; set; }

        public string? Year { get; set; }

        [Required]
        public string Brand { get; set; } = string.Empty;

        [Required]
        public string Color { get; set; } = string.Empty;
    }
}
