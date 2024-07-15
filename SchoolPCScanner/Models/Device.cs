using System.ComponentModel.DataAnnotations;

namespace SchoolPCScanner.Models
{
    public enum DeviceStatus
    {
        [Display(Name = "In gebruik")]
        InUse,// 0
        [Display(Name = "Beschadigd")]
        Damage,// 1
        [Display(Name = "Niet afgerond")]
        Termination,// 2
        [Display(Name = "Beschikbaar")] 
        Available,// 3
    }
    public class Device
    {
        public int Id { get; set; }
        [Display(Name = "Serienummer")]
        public string? Serienumber { get; set; }
        [Display(Name = "Leverancier")]
        public int? SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }
        public string? Type { get; set; }
        public string? Barcode { get; set; }
        public DeviceStatus Status { get; set; }
        public int? StudentId { get; set; }

        [Display(Name = "Leerling")]
        public virtual Student Student { get; set; }
        [Display(Name = "Reserve")]
        public bool IsReserve { get; set; }
        [Display(Name = "Verwijderd")]
        public bool IsDeleted { get; set; }

        // collection of logs for this device
        public virtual ICollection<Log> Logs { get; set; }
        // collection of terminations for this device
        public virtual ICollection<TerminationRegistration> TerminationRegistrations { get; set; }


    }


}
