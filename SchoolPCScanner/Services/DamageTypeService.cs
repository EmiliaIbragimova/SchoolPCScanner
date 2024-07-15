using Microsoft.EntityFrameworkCore;
using SchoolPCScanner.Models;
using SchoolPCScanner.Services.Interfaces;

namespace SchoolPCScanner.Services
{
    public class DamageTypeService: IDamageTypeService
    {
        private readonly SchoolPCScannerDbContext _context;

        public DamageTypeService(SchoolPCScannerDbContext context)
        {
            _context = context;
        }
        public async Task<IQueryable<DamageType>> GetAllDamageTypesAsync()
        {
            try
            {
                return _context.DamageTypes.Include(d => d.DamageRegistrations);
            }
            catch (Exception ex)
            {
                throw new Exception("DamageTypeService > GetAllDamageTypesAsync: An error occurred while retrieving damage types", ex);
            }
        }

        //public async Task<DamageType> GetDamageTypeByIdAsync(int? id)
        //{
        //    return await _context.DamageTypes.FirstOrDefaultAsync(d => d.Id == id);
        //}

        public async Task<List<DamageType>> GetDamageTypesByIdsAsync(List<int> selectedDamageTypeIds)
        {
            try
            {
                return await _context.DamageTypes.Where(d => selectedDamageTypeIds.Contains(d.Id)).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("DamageTypeService > GetDamageTypesByIdsAsync: An error occurred while retrieving damage types by ids", ex);
            }
        }
    }
}
