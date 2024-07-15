using System.ComponentModel.DataAnnotations;

namespace SchoolPCScanner.Models
{
    public class Supplier // Leverancier
    {
        public int Id { get; set; }
        [Display(Name = "Leverancier")]
        public string Name { get; set; }
        
        // foreign key
        // collection of devices
        public virtual ICollection<Device> Devices { get; set; }
    }
}
