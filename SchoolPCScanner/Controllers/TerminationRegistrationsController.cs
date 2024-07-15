using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolPCScanner.Models;
using SchoolPCScanner.Services.Interfaces;

namespace SchoolPCScanner.Controllers
{
    public class TerminationRegistrationsController : Controller
    {
        private readonly SchoolPCScannerDbContext _context;
        private readonly IDeviceService _deviceService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IStudentService _studentService;
        private readonly ILogService _logService;
        private readonly ITerminationRegistrationService _terminationRegistrationService;

        public TerminationRegistrationsController(SchoolPCScannerDbContext context, IDeviceService deviceService, UserManager<IdentityUser> userManager, IStudentService studentService, ILogService logService, ITerminationRegistrationService terminationRegistrationService)
        {
            _context = context;
            _deviceService = deviceService;
            _userManager = userManager;
            _studentService = studentService;
            _logService = logService;
            _terminationRegistrationService = terminationRegistrationService;
        }


        // GET: TerminationRegistrations
        public async Task<IActionResult> Index()
        {
            var terminationDetails = await _context.TerminationRegistrations.Include(td => td.Device).Include(td => td.Student).OrderByDescending(l => l.TerminationDate).ToListAsync();
            return View(terminationDetails);
            //var schoolPCScannerDbContext = _context.TerminationRegistrations.Include(t => t.Device).Where(t => t.DeviceId == id);
            //return View(await schoolPCScannerDbContext.ToListAsync());
        }

        // GET: TerminationRegistrationsPerDevice
        public async Task<IActionResult> TerminationRegistrationsByDevice(int? id)
        {
            var terminationDetailsByDevice = await _context.TerminationRegistrations.Include(td => td.Device).Where(d => d.DeviceId == id).Include(td => td.Student).OrderByDescending(l => l.TerminationDate).ToListAsync();
            return View("Index", terminationDetailsByDevice);
            //var schoolPCScannerDbContext = _context.TerminationRegistrations.Include(t => t.Device).Where(t => t.DeviceId == id);
            //return View(await schoolPCScannerDbContext.ToListAsync());
        }

        // GET: TerminationRegistrations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var terminationRegistration = await _context.TerminationRegistrations
                .Include(t => t.Device)
                .Include(t => t.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (terminationRegistration == null)
            {
                return NotFound();
            }

            return View(terminationRegistration);
        }

        // GET: TerminationRegistrations/Create
        public async Task<IActionResult> Create(int? id)
        {
            //Controleer of het ID geldig is
            if (id == null)
            {
                return BadRequest("Ongeldige aanvraag");
            }

            // Haal de benodigde apparaatgegevens op
            var device = await _deviceService.GetDeviceByIdAsync(id);

            // Controleer of het apparaat bestaat
            if (device == null)
            {
                return NotFound();
            }

            // Maak een DamageRegistration-object aan en wijs de gegevens van het device toe
            var terminationRegistration = new TerminationRegistration
            {
                DeviceId = device.Id,
                StudentId = device.StudentId,
                Student = device.Student,
                Device = device,
                TerminationDate = DateTime.Now,
            };
            return View(terminationRegistration);
            //ViewData["DeviceId"] = new SelectList(_context.Devices, "Id", "Id");
            //return View();
        }

        // POST: TerminationRegistrations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DeviceId,StudentId,IsCompleted,ReturnedCase,ReturnedCharger,TerminationDate,Note")] TerminationRegistration terminationRegistration, string confirmationResult)
        {
            if (ModelState.IsValid)
            {
                // TODO eigenschap User.Identity.Name toevoegen aan TerminationRegistration
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                terminationRegistration.TerminationDate = DateTime.Now;

                // Haal de student op
                var student = await _studentService.GetStudentByIdAsync(terminationRegistration.StudentId);
                student.Devices = await _deviceService.GetDevicesByStudentIdAsync(student.Id);
                var device = await _deviceService.GetDeviceByIdAsync(terminationRegistration.DeviceId);
                var originalDevice = _deviceService.CreateDeviceCopy(device); // Maak een kopie van het apparaat

                // Controleer of het apparaat bestaat
                if (device == null)
                {
                    return NotFound();
                }

                terminationRegistration.Device = device;
                terminationRegistration.Student = student;

                if (confirmationResult == "cancelled")
                {
                    ModelState.AddModelError("", "Registratie geannuleerd.");
                    ViewData["DeviceId"] = new SelectList(_context.Devices, "Id", "Id", terminationRegistration.DeviceId);
                    return View(terminationRegistration);
                }
                else
                {
                    try
                    {
                        // Controleer of alle vereiste eigenschappen zijn ingesteld
                        if (terminationRegistration.ReturnedCase && terminationRegistration.ReturnedCharger)
                        {
                            Console.WriteLine($"Student has {student.Devices.Count} devices.");
                            if (student.Devices != null && student.Devices.Count > 1)
                            {
                                // Verbreek de koppeling tussen het apparaat en de student
                                device.StudentId = null;
                            }
                            else
                            {
                                // Verbreek de koppeling tussen het apparaat en de student
                                device.StudentId = null;
                                // zet de status van student op inactief
                                student.IsActive = false;
                                // zet de status van niuwe student op null
                                student.IsNew = false;
                            }
                         
                            // zet de status van device op beschikbaar
                            device.Status = DeviceStatus.Available;
                            terminationRegistration.IsCompleted = true;

                            // Initialiseer de TerminationRegistrations-verzameling indien nodig
                            if (device.TerminationRegistrations == null)
                            {
                                device.TerminationRegistrations = new List<TerminationRegistration>();
                            }

                            // Wijs de TerminationRegistration toe aan het apparaat
                            device.TerminationRegistrations.Add(terminationRegistration);


                            _context.Update(device);
                            //_context.Update(terminationRegistration);
                            await _context.AddAsync(terminationRegistration);

                            // Update de wijzigingen in de database
                            await _context.SaveChangesAsync();
                            var changes = _logService.GetChanges(device, originalDevice);
                            var log = _logService.CreateLog(device, user, changes, ControllerContext);
                            await _logService.AddLogAsync(log);

                            return RedirectToAction(nameof(Index));
                        }



                        await _context.AddAsync(terminationRegistration);
                        await _context.SaveChangesAsync();

                        var changes1 = _logService.GetChanges(device, originalDevice);
                        var log1 = _logService.CreateLog(device, user, changes1, ControllerContext);
                        await _logService.AddLogAsync(log1);
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                        return View(terminationRegistration);
                    }

                }
            }
            ViewData["DeviceId"] = new SelectList(_context.Devices, "Id", "Id", terminationRegistration.DeviceId);
            return View(terminationRegistration);
        }

        // GET: TerminationRegistrations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var terminationRegistration = await _context.TerminationRegistrations.FindAsync(id);

            var device = await _deviceService.GetDeviceByIdAsync(terminationRegistration.DeviceId);
            terminationRegistration.Device = device;
            var student = await _studentService.GetStudentByIdAsync(terminationRegistration.StudentId);
            
            if (terminationRegistration == null)
            {
                return NotFound();
            }

            ViewData["DeviceId"] = new SelectList(_context.Devices, "Id", "Id", device.Id);
            return View(terminationRegistration);
        }

        // POST: TerminationRegistrations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DeviceId,StudentId,IsCompleted,ReturnedCase,ReturnedCharger,TerminationDate,Note")] TerminationRegistration terminationRegistration, string confirmationResult)
        {
            if (id != terminationRegistration.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRegistration = await _context.TerminationRegistrations
                        .Include(tr => tr.Device)
                        .Include(tr => tr.Student)
                        .FirstOrDefaultAsync(tr => tr.Id == id);

                    if (existingRegistration == null)
                    {
                        return NotFound();
                    }

                    var registrationCopy = _terminationRegistrationService.TerminationRegistrationCopy(existingRegistration);

                    // Update de waarden van de bestaande registratie met de nieuwe waarden
                    existingRegistration.DeviceId = terminationRegistration.DeviceId;
                    existingRegistration.StudentId = terminationRegistration.StudentId;
                    existingRegistration.ReturnedCase = terminationRegistration.ReturnedCase;
                    existingRegistration.ReturnedCharger = terminationRegistration.ReturnedCharger;
                    existingRegistration.Note = terminationRegistration.Note;

                    var device = await _deviceService.GetDeviceByIdAsync(terminationRegistration.DeviceId);
                    var student = await _studentService.GetStudentByIdAsync(terminationRegistration.StudentId);
                    student.Devices = await _deviceService.GetDevicesByStudentIdAsync(student.Id);
                    terminationRegistration.TerminationDate = DateTime.Now;

                  

                    if (confirmationResult == "cancelled")
                    {
                        ModelState.AddModelError("", "Registratie geannuleerd.");
                        ViewData["DeviceId"] = new SelectList(_context.Devices, "Id", "Id", terminationRegistration.DeviceId);
                        return View(terminationRegistration);
                    }
                    else
                    {
                        if (terminationRegistration.ReturnedCase && terminationRegistration.ReturnedCharger)
                        {
                            Console.WriteLine($"Student has {student.Devices.Count} devices.");

                            if (student.Devices != null && student.Devices.Count > 1)
                            {
                                // Verbreek de koppeling tussen het apparaat en de student
                                device.StudentId = null;
                            }
                            else
                            {
                                // Verbreek de koppeling tussen het apparaat en de student
                                device.StudentId = null;
                                // zet de status van student op inactief
                                student.IsActive = false;
                                // zet de status van niuwe student op null
                                student.IsNew = false;
                            }

                            // zet de status van device op beschikbaar
                            device.Status = DeviceStatus.Available;
                            terminationRegistration.IsCompleted = true;


                            // Initialiseer de TerminationRegistrations-verzameling indien nodig
                            if (device.TerminationRegistrations == null)
                            {
                                device.TerminationRegistrations = new List<TerminationRegistration>();
                            }

                            // Controleer of de registratie al aanwezig is in de lijst van TerminationRegistrations
                            var existingTermination = device.TerminationRegistrations.FirstOrDefault(tr => tr.Id == terminationRegistration.Id);

                            if (existingTermination == null)
                            {
                                // Wijs de TerminationRegistration toe aan het apparaat als deze nog niet bestaat in de lijst
                                device.TerminationRegistrations.Add(terminationRegistration);
                            }
                            else
                            {
                                // Als de registratie al bestaat, vervang deze door de bijgewerkte registratie
                                device.TerminationRegistrations.Remove(existingTermination);
                                device.TerminationRegistrations.Add(terminationRegistration);
                            }


                        }

                        _context.Update(existingRegistration);
                        await _context.SaveChangesAsync();
                        
                        var user = await _userManager.GetUserAsync(User);
                        var changes = _logService.GetChangesTermination(registrationCopy, terminationRegistration);
                        var log = _logService.CreateLogTerminationRegistration(terminationRegistration, user, changes, ControllerContext);
                        await _logService.AddLogAsync(log);

                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(terminationRegistration);
                }
            }

            ViewData["DeviceId"] = new SelectList(_context.Devices, "Id", "Id", terminationRegistration.DeviceId);
            return View(terminationRegistration);
            

        }

        // GET: TerminationRegistrations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var terminationRegistration = await _context.TerminationRegistrations
                .Include(t => t.Device)
                .Include(t => t.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (terminationRegistration == null)
            {
                return NotFound();
            }

            return View(terminationRegistration);
        }

        // POST: TerminationRegistrations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var terminationRegistration = await _context.TerminationRegistrations.FindAsync(id);
            if (terminationRegistration != null)
            {
                _context.TerminationRegistrations.Remove(terminationRegistration);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TerminationRegistrationExists(int id)
        {
            return _context.TerminationRegistrations.Any(e => e.Id == id);
        }
    }
}
