using SchoolPCScanner.Models;
using SchoolPCScanner.Services.Interfaces;

namespace SchoolPCScanner.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly SchoolPCScannerDbContext _context;

        public SupplierService(SchoolPCScannerDbContext context)
        {
            _context = context;
        }

        public async Task<List<Supplier>> GetAllSuppliersAsync()
        {
            try
            {
                return _context.Suppliers.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("SupplierService > GetAllSuppliersAsync: An error occurred while retrieving all suppliers", ex);
            }
        }

        public async Task<Supplier> GetSupplierByIdAsync(int? id)
        {
            try
            {
                return _context.Suppliers.FirstOrDefault(s => s.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception("SupplierService > GetSupplierByIdAsync: An error occurred while retrieving supplier by id", ex);

            }

        }
    }
}
