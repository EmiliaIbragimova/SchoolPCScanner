using SchoolPCScanner.Models;
using System.ComponentModel.DataAnnotations;

namespace SchoolPCScanner.ViewModels
{
    public class DeviceViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Serienummer")]
        public string? Serienumber { get; set; }
        
        [Display(Name = "Leverancier")]
        public int? SupplierId { get; set; }
        public Supplier Supplier { get; set; }
        public List<Supplier> Suppliers { get; set; }
        public int? StudentId { get; set; }
        public Student Student { get; set; }
        public string? Type { get; set; }
        public bool IsReserve { get; set; }
        public string? Barcode { get; set; }
        public DeviceStatus Status { get; set; }
    }
}
