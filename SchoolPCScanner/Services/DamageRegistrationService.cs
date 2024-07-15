using Microsoft.EntityFrameworkCore;
using SchoolPCScanner.Models;
using SchoolPCScanner.Services.Interfaces;

namespace SchoolPCScanner.Services
{
    public class DamageRegistrationService : IDamageRegistrationService
    {
        private readonly SchoolPCScannerDbContext _context;
        private readonly IEmailService _emailService;

        public DamageRegistrationService(SchoolPCScannerDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task CreateDamageRegistrationAsync(DamageRegistration damageRegistration)
        {
            try
            {
                _context.DamageRegistrations.Add(damageRegistration);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("DamageRegistrationService > CreateDamageRegistrationAsync: An error occurred while creating the damage registration.", ex);
            }

        }

        public async Task<DamageRegistration> GetDamageRegistrationByIdAsync(int? id)
        {
            try
            {
                return _context.DamageRegistrations
                    .Include(dr => dr.Device)
                    .Include(dr => dr.Device.Supplier)
                    .Include(dr => dr.DamageTypes)
                    .Include(dr => dr.Student)
                    .FirstOrDefault(dr => dr.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception("DamageRegistrationService > GetDamageRegistrationByIdAsync: An error occurred while retrieving the damage registration.", ex);
            }
        }

        public async Task UpdateDamageRegistrationAsync(DamageRegistration damageRegistration)
        {
            try
            {
                _context.DamageRegistrations.Update(damageRegistration);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("DamageRegistrationService > UpdateDamageRegistration: An error occurred while updating the damage registration.", ex);
            }
        }

        public DamageRegistration CreateDamageRegistrationCopy(DamageRegistration damageRegistration)
        {
            var damageRegistrationCopy = new DamageRegistration
            {
                RegistrationDate = damageRegistration.RegistrationDate,
                Student = damageRegistration.Student,
                Device = damageRegistration.Device,
                User = damageRegistration.User,
                DamageTypes = damageRegistration.DamageTypes
            };

            return damageRegistrationCopy;
        }


        public string GenerateEmailHtml(DamageRegistration damageRegistration)
        {
            var damageTypes = damageRegistration.DamageTypes.Select(d => d.TypeName).ToList();
            var damageTypesString = string.Join(", ", damageTypes);

            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Schadegevalregistratie</title>
            </head>
            <body style='font-family: Arial, sans-serif'>

                <p style='color: #000000;'>Beste boekhouding,</p>
                <p style='color: #000000;'>De leerling heeft schade veroorzaakt aan zijn laptop.</p>
                <p style='color: #000000;'>Er mag €50 in rekening worden gebracht in het geval van een Chromebook, of €100 in het geval van een Laptop.</p>
                <p style='color: #000000;'Alvast bedankt,</p>
                <p style='color: #000000;'>ICT</p>
                <hr/>

                <h3 style='color: #000000;'>Nieuwe Schadegevalregistratie</h3>
                <br/>
                <p style='color: #000000;'>Datum: <strong>{damageRegistration.RegistrationDate}</strong></p>
                <p style='color: #000000;'>Leerling: <strong>{damageRegistration.Student.FullName}</strong></p>
                <p style='color: #000000;'>Klas: <strong>{damageRegistration.Student.Grade}</strong></p>
                <p style='color: #000000;'>Apparaat: <strong>{damageRegistration.Device.Serienumber}</strong></p>
                <p style='color: #000000;'>Type apparaat: <strong>{damageRegistration.Device.Type}</strong></p>
                <p style='color: #000000;'>Leverancier: <strong>{damageRegistration.Device.Supplier.Name}</strong></p>
                <p style='color: #000000;'>Geregistreerd door: <strong>{damageRegistration.UserName}</strong></p>
                <p style='color: #000000;'>Schadegeval: <strong>{damageTypesString}</strong></p>
                <p style='color: #000000;'>Opmerking: <strong>{damageRegistration.Note}</strong></p>
            </body>
            </html>";
        }
    }
}
