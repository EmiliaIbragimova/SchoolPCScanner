using System.ComponentModel.DataAnnotations;

namespace SchoolPCScanner.Models
{
    public class DamageType
    {
        public int Id { get; set; }
        [Display(Name = "Type schade")]
        public string TypeName { get; set; }
        public virtual ICollection<DamageRegistration> DamageRegistrations { get; set; } 
    }
}
