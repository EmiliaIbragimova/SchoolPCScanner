using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolPCScanner.Models
{
    public class Log
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        [Display(Name = "Toestel")]
        public virtual Device Device { get; set; }
        public DeviceStatus Status { get; set; }
        public string? IdentityUserId { get; set; }
        [Display(Name = "Medewerker")]
        public virtual IdentityUser IdentityUser { get; set; }
        [Display(Name = "Actie")]
        public string Action { get; set; }
        public int? StudentId { get; set; }
        [Display(Name = "Leerling")]
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
        [Display(Name = "Datum")]
        public DateTime Date { get; set; }
        [Display(Name = "Opmerking")]
        public string? Note { get; set; } // opmerkingen
    }
}
