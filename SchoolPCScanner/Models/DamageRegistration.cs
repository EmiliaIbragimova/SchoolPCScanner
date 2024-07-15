using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SchoolPCScanner.Models
{
    public class DamageRegistration
    {
        public int Id { get; set; }
        [Display(Name = "Datum")]
        public DateTime RegistrationDate { get; set; }
        public int StudentId { get; set; }
        [Display(Name = "Leerling")]
        public virtual Student Student { get; set; }
        public int DeviceId { get; set; }
        [Display(Name = "Toestel")]
        public virtual Device Device { get; set; }
        [Display(Name = "Medewerker")]
        public string UserName { get; set; }
        public virtual IdentityUser User { get; set; }
        [Display(Name = "Opmerking")]
        public string Note { get; set; }

        // Foreign key voor DamageType
        [Display(Name = "Schadegeval")]
        public virtual ICollection<DamageType> DamageTypes { get; set; }

        [Display(Name = "Verstuurd naar boekhouding")]
        public bool SendEmail { get; set; }
    }
}
