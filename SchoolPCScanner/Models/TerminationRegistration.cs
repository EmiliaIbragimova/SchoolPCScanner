using System.ComponentModel.DataAnnotations;

namespace SchoolPCScanner.Models
{
    public class TerminationRegistration
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        [Display(Name = "Toestel")]
        public virtual Device Device { get; set; }
        public int? StudentId { get; set; }
        [Display(Name = "Leerling")]
        public virtual Student Student { get; set; }
        [Display(Name = "Afgerond")]
        public bool IsCompleted { get; set; }
        [Display(Name = "Hoes")]
        public bool ReturnedCase { get; set; }
        [Display(Name = "Lader")]
        public bool ReturnedCharger { get; set; }
        [Display(Name = "Datum")]
        public DateTime TerminationDate { get; set; }
        [Display(Name = "Opmerking")]
        public string Note { get; set; }
    }
}
