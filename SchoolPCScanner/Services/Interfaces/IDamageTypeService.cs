using SchoolPCScanner.Models;

namespace SchoolPCScanner.Services.Interfaces
{
    public interface IDamageTypeService
    {
        Task<IQueryable<DamageType>> GetAllDamageTypesAsync();
        //Task<DamageType> GetDamageTypeByIdAsync(int? id);
        Task<List<DamageType>> GetDamageTypesByIdsAsync(List<int> selectedDamageTypeIds);
    }
}
