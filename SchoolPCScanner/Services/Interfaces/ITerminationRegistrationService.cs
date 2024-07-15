using SchoolPCScanner.Models;

namespace SchoolPCScanner.Services.Interfaces
{
    public interface ITerminationRegistrationService
    {
        TerminationRegistration TerminationRegistrationCopy(TerminationRegistration terminationRegistration);
    }
}
