using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QueryInterceptor.Core.ConsoleApp.net48.Database;

public class Territory
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Territory()
    {
        Employees = new HashSet<Employee>();
    }

    [StringLength(20)]
    public string TerritoryID { get; set; }

    [Required]
    [StringLength(50)]
    public string TerritoryDescription { get; set; }

    public int RegionID { get; set; }

    public virtual Region Region { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public virtual ICollection<Employee> Employees { get; set; }
}