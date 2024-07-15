using SchoolPCScanner.Models;

namespace SchoolPCScanner.Services.Interfaces
{
    public interface IDamageRegistrationService
    {
        string GenerateEmailHtml(DamageRegistration damageRegistration);
        Task CreateDamageRegistrationAsync(DamageRegistration damageRegistration);
        Task<DamageRegistration> GetDamageRegistrationByIdAsync(int? id);
        Task UpdateDamageRegistrationAsync(DamageRegistration damageRegistration);
        DamageRegistration CreateDamageRegistrationCopy(DamageRegistration damageRegistration);
    }
}
