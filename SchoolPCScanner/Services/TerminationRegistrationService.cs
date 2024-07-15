using SchoolPCScanner.Models;
using SchoolPCScanner.Services.Interfaces;

namespace SchoolPCScanner.Services
{
    public class TerminationRegistrationService: ITerminationRegistrationService
    {
        private readonly SchoolPCScannerDbContext _context;

        public TerminationRegistrationService(SchoolPCScannerDbContext context)
        {
            _context = context;
        }

        public TerminationRegistration TerminationRegistrationCopy(TerminationRegistration existingRegistration)
        {
            try
            {
                if (existingRegistration == null)
                {
                    throw new ArgumentNullException(nameof(existingRegistration), "existingRegistration cannot be null");
                }

                return new TerminationRegistration
                {
                    Id = existingRegistration.Id,
                    Device = existingRegistration.Device,
                    DeviceId = existingRegistration.DeviceId,
                    Student = existingRegistration.Student,
                    StudentId = existingRegistration.StudentId,
                    IsCompleted = existingRegistration.IsCompleted,
                    ReturnedCase = existingRegistration.ReturnedCase,
                    ReturnedCharger = existingRegistration.ReturnedCharger,
                    TerminationDate = existingRegistration.TerminationDate,
                    Note = existingRegistration?.Note
                };
            }
            catch (Exception ex)
            {
                throw new Exception("TerminationRegistrationService > TerminationRegistrationCopy: An error occurred while coping the TerminationRegistration", ex);
            }
        }
    }
}
