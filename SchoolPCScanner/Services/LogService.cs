using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolPCScanner.Models;
using System.Linq;
using System.Threading.Channels;
using SchoolPCScanner.Services.Interfaces;
using Humanizer;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SchoolPCScanner.Services
{
    public class LogService : ILogService
    {
        private readonly SchoolPCScannerDbContext _context;
        private readonly IDeviceService _deviceService;
        private readonly UserManager<IdentityUser> _userManager1;
        private readonly IStudentService _studentService;

        public LogService(SchoolPCScannerDbContext context, IDeviceService deviceService)
        {
            _context = context;
            _deviceService = deviceService;
        }

        public async Task AddLogAsync(Log log)
        {
            try
            {
                // Voeg de log toe aan de Logs-collectie van het Device-object
                var device = await _deviceService.GetDeviceByIdAsync(log.DeviceId);
                if (device != null)
                {
                    Console.WriteLine($"DeviceId: {device.Id}, DeviceLogsCount: {device.Logs?.Count}");
                    device.Logs.Add(log);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Device niet gevonden
                    throw new Exception("Device not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("LogService > AddLogAsync: An error occurred while adding a log", ex);
            }

        }

        public async Task AddLogForDamageRegistrationAsync(DamageRegistration damageRegistration, ActionContext actionContext, List<string> changes)
        {
            try
            {

                if (damageRegistration != null)
                {
                    // Voeg een log toe voor de schade registratie
                    //damageRegistration = await _context.DamageRegistrations
                    //    .OrderByDescending(dr => dr.Id)
                    //.FirstOrDefaultAsync();

                    // Verkrijg de naam van de huidige actie
                    var controllerName = actionContext.RouteData.Values["controller"].ToString();
                    var actionName = actionContext.RouteData.Values["action"].ToString();
                    var fullActionName = $"{controllerName}.{actionName}";


                    changes = new List<string>();
                    // Verkrijg de wijzigingen in de schade registratie


                    if (damageRegistration.DamageTypes != null)
                    {
                        foreach (var damageType in damageRegistration.DamageTypes)
                        {
                            changes.Add($"{damageType.TypeName}");
                        }
                    }
                    var changeMessage = string.Join(", ", changes);

                    var log = new Log
                    {
                        //DeviceId = damageRegistration.DeviceId,
                        //IdentityUserId = damageRegistration.IdentityUserId,
                        //Date = DateTime.Now,
                        DeviceId = damageRegistration.DeviceId,
                        Status = damageRegistration.Device.Status,
                        IdentityUser = damageRegistration.User,
                        Action = GetActionMessage(fullActionName),
                        Student = damageRegistration.Student,
                        StudentId = damageRegistration.StudentId,
                        Date = damageRegistration.RegistrationDate,
                        Note = $"<strong>DamageType:</strong> {changeMessage}"
                    };

                    damageRegistration.Device.Logs.Add(log);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Schade registratie niet gevonden
                    throw new Exception("Damage registration not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("LogService > AddLogForDamageRegistrationAsync: An error occurred while adding a log for damage registration", ex);
            }
        }

        //public async Task AddLogForTerminationRegistrationAsync(TerminationRegistration terminationRegistration)
        //{
        //    try
        //    {
        //        if (terminationRegistration == null)
        //        {
        //            throw new ArgumentNullException(nameof(terminationRegistration), "Termination registration cannot be null.");
        //        }
        //        var actionContext = new ControllerContext();
        //        if (actionContext == null)
        //        {
        //            throw new ArgumentNullException(nameof(actionContext), "Action context cannot be null.");
        //        }

        //        var userName = actionContext.HttpContext.User?.Identity?.Name;
        //        if (string.IsNullOrEmpty(userName))
        //        {
        //            throw new Exception("User is not authenticated or user name is null.");
        //        }

        //        var user = await _userManager1.FindByNameAsync(userName);
        //        if (user == null)
        //        {
        //            throw new Exception("User not found.");
        //        }

        //        // Verkrijg de naam van de huidige actie
        //        var controllerName = actionContext.RouteData.Values["controller"]?.ToString();
        //        var actionName = actionContext.RouteData.Values["action"]?.ToString();

        //        if (string.IsNullOrEmpty(controllerName) || string.IsNullOrEmpty(actionName))
        //        {
        //            throw new Exception("Controller name or action name is null.");
        //        }

        //        var fullActionName = $"{controllerName}.{actionName}";

        //        // Initialiseer de Logs-verzameling indien nodig
        //        if (terminationRegistration.Device.Logs == null)
        //        {
        //            terminationRegistration.Device.Logs = new List<Log>();
        //        }
        //        var device = await _deviceService.GetDeviceByIdAsync(terminationRegistration.DeviceId);
        //        var student = await _studentService.GetStudentByIdAsync(device.StudentId);

        //        // Voeg een log toe voor de beëindiging registratie
        //        var log = new Log
        //        {
        //            DeviceId = terminationRegistration.DeviceId,
        //            Action = GetActionMessage(fullActionName),
        //            Student = student,
        //            StudentId = terminationRegistration.StudentId,
        //            Date = DateTime.Now,
        //            Note = terminationRegistration?.Note
        //        };

        //        terminationRegistration.Device.Logs.Add(log);
        //        await _context.SaveChangesAsync();

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("LogService > AddLogForTerminationRegistrationAsync: An error occurred while adding a log for termination registration", ex);
        //    }
        //}
        public async Task<IQueryable<Log>> GetAllLogsAsync()
        {
            try
            {
                return _context.Logs.Include(l => l.Device).Include(l => l.IdentityUser).Include(l => l.Device.Student).OrderByDescending(l => l.Date); // Sorteer op logdatum in omgekeerde volgorde (nieuwste eerst)

            }
            catch (Exception ex)
            {
                throw new Exception("DeviceService > GetAllDevicesAsync: An error occurred while retrieving all devices", ex);
            }
        }
        public async Task<IQueryable<Log>> GetLogsByDeviceIdAsync(int deviceId)
        {
            try
            {
                var logItemPerDevice = _context.Logs.Where(l => l.DeviceId == deviceId).Include(l => l.Device).ThenInclude(l => l.Student).Include(l => l.IdentityUser).OrderByDescending(l => l.Date);

                return logItemPerDevice;
            }
            catch (Exception ex)
            {
                throw new Exception("LogService > GetLogByDeviceId: An error occurred while adding a log\", ex");
            }

        }

        //public async Task<DateTime?> GetRecentLogDateAsync(int deviceId)
        //{
        //    try
        //    {
        //        // Haal de recente log op voor het opgegeven apparaat ID
        //        var recentLog = await _context.Logs
        //            .Where(l => l.DeviceId == deviceId)
        //            .OrderByDescending(l => l.Date)
        //            .FirstOrDefaultAsync();

        //        // Geef de datum van de recente log terug, indien aanwezig
        //        return recentLog?.Date;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handel eventuele fouten af
        //        throw new Exception("Error occurred while retrieving recent log date", ex);
        //    }
        //}

        // Hulpmethode om te controleren of een eigenschap van een Device-object is gewijzigd
        public List<string> GetChanges(Device originalDevice, Device existingDevice)
        {
            var changes = new List<string>();

            if (IsPropertyChanged(originalDevice, existingDevice, "Status"))
            {
                changes.Add($"Status is veranderd van '{originalDevice.Status}' naar '{existingDevice.Status}'");
                Console.WriteLine($"Status changed to '{existingDevice.Status}'");
            }

            if (IsPropertyChanged(originalDevice, existingDevice, "IsReserve"))
            {
                string originalStatus = originalDevice.IsReserve ? "Reserve" : "Gewoon toestel";
                string newStatus = existingDevice.IsReserve ? "Reserve" : "Gewoon toestel";

                changes.Add($"Status is veranderd van '{originalStatus}' naar '{newStatus}'");
                //changes.Add($"Status changed from '{originalDevice.IsReserve}' to '{existingDevice.IsReserve}'");
                Console.WriteLine($"Status changed to '{existingDevice.IsReserve}'");
            }

            if (IsPropertyChanged(originalDevice, existingDevice, "SupplierId"))
            {
                // Controleer op verandering in Supplier.Name alleen als beide Suppliers niet null zijn
                if (originalDevice.Supplier.Name != existingDevice.Supplier.Name)
                {
                    changes.Add($"Leverancier is veranderd van '{originalDevice.Supplier.Name}' naar '{existingDevice.Supplier.Name}'");
                    Console.WriteLine($"Status changed to '{existingDevice.Supplier.Name}'");
                }
            }

            //if (IsPropertyChanged(originalDevice, existingDevice, "Name"))
            //{
            //    changes.Add($"Status changed from '{originalDevice.Supplier?.Name}' to '{existingDevice.Supplier?.Name}'");
            //    Console.WriteLine($"Status changed to '{existingDevice.Supplier.Name}'");
            //}

            if (IsPropertyChanged(originalDevice, existingDevice, "Serienumber"))
            {
                changes.Add($"Serienummer is veranderd van '{originalDevice.Serienumber}' naar '{existingDevice.Serienumber}'");
                Console.WriteLine($"Status changed to '{existingDevice.Serienumber}'");
            }

            if (IsPropertyChanged(originalDevice, existingDevice, "Type"))
            {
                changes.Add($"Type is veranderd van '{originalDevice.Type}' naar '{existingDevice.Type}'");
                Console.WriteLine($"Status changed to '{existingDevice.Type}'");
            }

            //if (IsPropertyChanged(originalDevice, existingDevice, "StudentId"))
            //{
            //    changes.Add($"Student ID changed from '{originalDevice.StudentId}' to '{existingDevice.StudentId}'");
            //    Console.WriteLine($"Student ID changed to '{existingDevice.StudentId}'");
            //}

            if (IsPropertyChanged(originalDevice, existingDevice, "Barcode"))
            {
                changes.Add($"Barcode is veranderd van'{originalDevice.Barcode}' naar '{existingDevice.Barcode}'");
                Console.WriteLine($"Barcode changed to '{existingDevice.Barcode}'");
            }

            Console.WriteLine(changes.ToString());
            return changes;
        }

        public List<string> GetChangesDamageTypes(DamageRegistration damageRegistration, DamageRegistration existingDamageRegistration)
        {
            if (damageRegistration == null || existingDamageRegistration == null)
            {
                return null;
            }

            var changes = new List<string>();

            // Controleer wijzigingen in DamageTypes
            var originalDamageTypeNames = damageRegistration.DamageTypes.Select(dt => dt.TypeName).ToList();
            var existingDamageTypeNames = existingDamageRegistration.DamageTypes.Select(dt => dt.TypeName).ToList();

            if (originalDamageTypeNames.Any())
            {
                foreach (var newType in originalDamageTypeNames)
                {
                    changes.Add($"'{newType}'");
                }
            }

            //var addedDamageTypes = existingDamageTypeNames.Except(originalDamageTypeNames).ToList();
            //var removedDamageTypes = originalDamageTypeNames.Except(existingDamageTypeNames).ToList();

            //changes.Add($"Schadeomschrijving is veranderd van '{existingDamageRegistration}' naar '{originalDamageTypeNames}'");
            Console.WriteLine($"DamageDescription changed to '{originalDamageTypeNames}'");

            return changes;

            //if (damageRegistration.DamageTypes.
            //{
            //    changes.Add($"Schadetype is veranderd van '{damageRegistration.DamageType}' naar '{existingDamageRegistration.DamageType}'");
            //    Console.WriteLine($"DamageType changed to '{existingDamageRegistration.DamageType}'");
            //}
        }

        public List<string> GetChangesTermination(TerminationRegistration original, TerminationRegistration updated)
        {
            if (original == null || updated == null)
            {
                return null;
            }
            var changes = new List<string>();

            if (original.ReturnedCase != updated.ReturnedCase)
            {
                changes.Add($"Hoes is binnengebracht.");
            }

            if (original.ReturnedCharger != updated.ReturnedCharger)
            {
                changes.Add($"Lader is binnengebracht.");
            }

            return changes;
        }

        // Hulpmethode om een log te maken
        public Log CreateLog(Device device, IdentityUser user, List<string> changes, ActionContext actionContext)
        {
            // Verkrijg de naam van de huidige actie
            var controllerName = actionContext.RouteData.Values["controller"].ToString();
            var actionName = actionContext.RouteData.Values["action"].ToString();
            var fullActionName = $"{controllerName}.{actionName}";

            var changeMessage = string.Join(", ", changes);
            return new Log
            {
                DeviceId = device.Id,
                Status = device.Status,
                IdentityUserId = user.Id,
                IdentityUser = user,
                Action = GetActionMessage(fullActionName),
                Student = device.Student,
                StudentId = device.StudentId,
                Date = DateTime.Now,
                Note = $"<strong>Wijzigingen:</strong> {changeMessage}"
            };
        }

        public Log CreateLogDamageRegistration(DamageRegistration damageRegistration, IdentityUser user, List<string> changes, ActionContext actionContext)
        {
            // Verkrijg de naam van de huidige actie
            var controllerName = actionContext.RouteData.Values["controller"].ToString();
            var actionName = actionContext.RouteData.Values["action"].ToString();
            var fullActionName = $"{controllerName}.{actionName}";

            var changeMessage = string.Join(", ", changes);

            return new Log
            {
                DeviceId = damageRegistration.DeviceId,
                Status = damageRegistration.Device.Status,
                IdentityUserId = user.Id,
                IdentityUser = user,
                Action = GetActionMessage(fullActionName),
                Student = damageRegistration.Device.Student,
                StudentId = damageRegistration.Device.StudentId,
                Date = DateTime.Now,
                Note = $"<strong>Wijzigingen:</strong> {changeMessage}"
            };
        }

        public Log CreateLogTerminationRegistration(TerminationRegistration terminationRegistration, IdentityUser user, List<string> changes, ActionContext actionContext)
        {
            // Verkrijg de naam van de huidige actie
            var controllerName = actionContext.RouteData.Values["controller"].ToString();
            var actionName = actionContext.RouteData.Values["action"].ToString();
            var fullActionName = $"{controllerName}.{actionName}";

            var changeMessage = string.Join(", ", changes);

            return new Log
            {
                Device = terminationRegistration.Device,
                DeviceId = terminationRegistration.DeviceId,
                Status = terminationRegistration.Device.Status,
                IdentityUser = user,
                Action = GetActionMessage(fullActionName),
                Student = terminationRegistration.Student,
                Date = DateTime.Now,
                Note = $"<strong>Wijzigingen:</strong> {changeMessage}"
            };

        }
        public bool IsPropertyChanged(Device originalDevice, Device existingDevice, string propertyName)
        {
            var originalEntry = _context.Entry(originalDevice);
            var existingEntry = _context.Entry(existingDevice);

            var originalProperty = originalEntry.Property(propertyName);
            var existingProperty = existingEntry.Property(propertyName);

            var originalValue = originalProperty.OriginalValue;
            var existingValue = existingProperty.CurrentValue;

            Console.WriteLine($"Property: {propertyName}, OriginalValue: {originalValue}, ExistingValue: {existingValue}");
            return !EqualityComparer<object>.Default.Equals(originalValue, existingValue);
        }

        // Methode om het bericht op basis van de actienaam te verkrijgen
        public string GetActionMessage(string fullActionName)
        {

            Console.WriteLine(fullActionName);
            // Maak een mapping tussen actienamen en berichten
            var actionMessages = new Dictionary<string, string>
            {
                //{ "Home.DeviceIndex", "Stopzetting" },
                { "Devices.Edit", "Toestel bewerkt" },
                { "DamageRegistrations.Create", "Schade geregistreerd" },
                {"TerminationRegistrations.Create", "Stopzetting geregistreerd"},
                { "DamageRegistrations.Edit", "Schadegeval bewerkt" },
                {"TerminationRegistrations.Edit", "Stopzetting bewerkt"},
            };

            // Controleer of de actie in de mapping voorkomt
            if (actionMessages.ContainsKey(fullActionName))
            {
                return actionMessages[fullActionName];
            }
            else
            {
                return "Wijzigingen zijn aangebracht in een onbekende pagina";
            }
        }

    }
}
