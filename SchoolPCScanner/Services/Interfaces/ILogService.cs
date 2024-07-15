using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolPCScanner.Models;

namespace SchoolPCScanner.Services.Interfaces
{
    public interface ILogService
    {
        Task AddLogAsync(Log log);
        Task<IQueryable<Log>> GetLogsByDeviceIdAsync(int deviceId);
        Task<IQueryable<Log>> GetAllLogsAsync();
        //Task<DateTime?> GetRecentLogDateAsync(int deviceId);
        Task AddLogForDamageRegistrationAsync(DamageRegistration damageRegistration, ActionContext actionContext, List<string> changes);
        //Task AddLogForTerminationRegistrationAsync(TerminationRegistration terminationRegistration);
        public List<string> GetChanges(Device device, Device originalDevice);
        public List<string> GetChangesDamageTypes(DamageRegistration damageRegistration, DamageRegistration originalDamageRegistration);
        List<string> GetChangesTermination(TerminationRegistration original, TerminationRegistration updated);
        
        public Log CreateLog(Device device, IdentityUser user, List<string> changes, ActionContext actionContext);
        Log CreateLogDamageRegistration(DamageRegistration damageRegistration, IdentityUser user, List<string> changes, ActionContext actionContext);
        Log CreateLogTerminationRegistration(TerminationRegistration terminationRegistration, IdentityUser user, List<string> changes, ActionContext actionContext);
        public bool IsPropertyChanged(Device device, Device device1, string propertyName);
        string GetActionMessage(string actionName);
    }
}
