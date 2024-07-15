using SchoolPCScanner.Models;

namespace SchoolPCScanner.Services.Interfaces
{
    public interface ISupplierService
    {
        Task<List<Supplier>> GetAllSuppliersAsync();
        Task<Supplier> GetSupplierByIdAsync(int? id);
    }
}
