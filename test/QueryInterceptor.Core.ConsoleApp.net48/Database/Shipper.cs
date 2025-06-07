using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QueryInterceptor.Core.ConsoleApp.net48.Database;

public class Shipper
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Shipper()
    {
        Orders = new HashSet<Order>();
    }

    public int ShipperID { get; set; }

    [Required]
    [StringLength(40)]
    public string CompanyName { get; set; }

    [StringLength(24)]
    public string Phone { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public virtual ICollection<Order> Orders { get; set; }
}