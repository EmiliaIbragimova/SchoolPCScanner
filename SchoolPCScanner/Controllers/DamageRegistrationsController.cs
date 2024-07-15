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
using SchoolPCScanner.Services;
using SchoolPCScanner.Services.Interfaces;

namespace SchoolPCScanner.Controllers
{
    public class DamageRegistrationsController : Controller
    {
        private readonly SchoolPCScannerDbContext _context;
        private readonly UserManager<IdentityUser> _userManager1;
        private readonly IDeviceService _deviceService;
        private readonly IDamageTypeService _damageTypeService;
        private readonly IStudentService _studentService;
        private readonly ILogService _logService;
        private readonly IEmailService _emailService;
        private readonly IDamageRegistrationService _damageRegistrationService;

        public DamageRegistrationsController(SchoolPCScannerDbContext context, UserManager<IdentityUser> userManager1, IDeviceService deviceService, IDamageTypeService damageTypeService, IStudentService studentService, ILogService logService, IEmailService emailService, IDamageRegistrationService damageRegistrationService)
        {
            _context = context;
            _userManager1 = userManager1;
            _deviceService = deviceService;
            _damageTypeService = damageTypeService;
            _studentService = studentService;
            _logService = logService;
            _emailService = emailService;
            _damageRegistrationService = damageRegistrationService;
        }

        // GET: DamageRegistrations
        public async Task<IActionResult> Index()
        {
            // Damageregistraties per device
            var damageRegistrationsPerDevice = await _context.DamageRegistrations
                .Include(d => d.Device)
                .Include(d => d.Student)
                .Include(d => d.User)
                .Include(d => d.DamageTypes)
                .OrderByDescending(l => l.RegistrationDate)
               // .AsNoTracking()
                .ToListAsync();

            return View(damageRegistrationsPerDevice);
        }

        // GET: DamageRegistrations/RegistrationsByDevice/{id}
        public async Task<IActionResult> RegistrationsByDevice(int id)
        {
            var damageRegistrationsPerDevice = await _context.DamageRegistrations
                .Where(d => d.DeviceId == id)
                .Include(d => d.Device)
                .Include(d => d.Student)
                .Include(d => d.User)
                .Include(d => d.DamageTypes)
                .OrderByDescending(l => l.RegistrationDate)
                //.AsNoTracking() 
                .ToListAsync();

            return View("Index", damageRegistrationsPerDevice);
        }

        public async Task<IActionResult> SendEmailMessage()
        {
            return View();
        }

        // GET: DamageRegistrations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var damageRegistration = await _context.DamageRegistrations
                .Include(d => d.DamageTypes)
                .Include(d => d.Device)
                .Include(d => d.Student)
                .Include(d => d.Device.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (damageRegistration == null)
            {
                return NotFound();
            }

            return View(damageRegistration);
        }

        // GET: DamageRegistrations/Create
        public async Task<IActionResult> Create(int? id, DeviceStatus? status)
        {
            if (id == null)
            {
                return BadRequest("Apparaat-ID is vereist");
            }


            var device = await _deviceService.GetDeviceByIdAsync(id.Value);
            var student = await _studentService.GetStudentByIdAsync(device.StudentId);

            if (device == null)
            {
                return NotFound("Apparaat niet gevonden");
            }
            var currentUserName = User.Identity.Name;

            var damageType = await _damageTypeService.GetAllDamageTypesAsync();

            if (status.HasValue)
            {
                ViewData["Status"] = status;
            }
            // Maak een DamageRegistration-object aan en wijs de gegevens van het device toe
            var damageRegistration = new DamageRegistration
            {
                DeviceId = device.Id,
                StudentId = student.Id,
                Device = device,
                Student = student,
                UserName = currentUserName,
                RegistrationDate = DateTime.Now,
                DamageTypes = damageType.ToList(),
                SendEmail = false
            };


            return View(damageRegistration);
        }

        // POST: DamageRegistrations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RegistrationDate,StudentId,DeviceId,UserName,Note,SendEmail")] DamageRegistration damageRegistration, List<int> selectedDamageTypeIds, List<string> changes)
        {
            if (ModelState.IsValid)
            {

                damageRegistration.RegistrationDate = DateTime.Now;

                damageRegistration.DamageTypes = new List<DamageType>();

                foreach (var damageTypeId in selectedDamageTypeIds)
                {
                    var damageType = await _context.DamageTypes.FindAsync(damageTypeId);
                    if (damageType != null)
                    {
                        damageRegistration.DamageTypes.Add(damageType);
                    }
                }

                var device = await _deviceService.GetDeviceByIdAsync(damageRegistration.DeviceId);
                var student = await _studentService.GetStudentByIdAsync(damageRegistration.StudentId);
                var user = await _userManager1.FindByNameAsync(User.Identity.Name);
                damageRegistration.Device = device;
                damageRegistration.Student = student;
                damageRegistration.User = user;


                try
                {
                    await _context.AddAsync(damageRegistration);
                    await _context.SaveChangesAsync();

                    if (damageRegistration.SendEmail)
                    {
                        await _emailService.SendEmailAsync("emiliniya.87@gmail.com", "Schadegeval", _damageRegistrationService.GenerateEmailHtml(damageRegistration));
                    }

                    await _logService.AddLogForDamageRegistrationAsync(damageRegistration, ControllerContext, selectedDamageTypeIds.Select(id => _context.DamageTypes.Find(id).TypeName).ToList());

                    return RedirectToAction(nameof(Index));
                    //await _logService.AddLogForDamageRegistrationAsync(damageRegistration, ControllerContext, selectedDamageTypeIds.Select(id => _context.DamageTypes.Find(id).TypeName).ToList());
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Error occurred while logging the damage registration.");
                    //ViewData["DeviceId"] = new SelectList(_context.Devices, "Id", "Id", damageRegistration.DeviceId);
                    //ViewData["StudentId"] = new SelectList(_context.Students, "Id", "Id", damageRegistration.StudentId);
                    //ViewData["UserName"] = User.Identity.Name;
                    return View(damageRegistration);
                }

            }

            return View(damageRegistration);
        }

        // GET: DamageRegistrations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var damageRegistration = await _context.DamageRegistrations.FindAsync(id);
            var damageRegistration = await _damageRegistrationService.GetDamageRegistrationByIdAsync(id.Value);

            var damageTypes = await _damageTypeService.GetAllDamageTypesAsync();
            damageRegistration.DamageTypes = damageTypes.ToList();

            if (damageRegistration == null)
            {
                return NotFound();
            }

            ViewData["DeviceId"] = new SelectList(_context.Devices, "Id", "Id", damageRegistration.DeviceId);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "Id", damageRegistration.StudentId);
            return View(damageRegistration);
        }

        // POST: DamageRegistrations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RegistrationDate,StudentId,DeviceId,UserName,Note,SendEmail")] DamageRegistration damageRegistration, List<int> selectedDamageTypeIds)
        {
            if (id != damageRegistration.Id)
            {
                return NotFound();
            }
            var user = await _userManager1.FindByNameAsync(User.Identity.Name);
            var existingDamageRegistration = await _damageRegistrationService.GetDamageRegistrationByIdAsync(id);
            
            if (ModelState.IsValid)
            {
                //var user = await _userManager1.FindByNameAsync(User.Identity.Name);
                try
                {
                    var existingRegistrationCopy = _damageRegistrationService.CreateDamageRegistrationCopy(existingDamageRegistration);

                    // 1. Update the damage registration
                    existingDamageRegistration.RegistrationDate = DateTime.Now;
                    existingDamageRegistration.StudentId = damageRegistration.StudentId;
                    existingDamageRegistration.DeviceId = damageRegistration.DeviceId;
                    existingDamageRegistration.UserName = User.Identity.Name;
                    existingDamageRegistration.Note = damageRegistration.Note;
                    existingDamageRegistration.SendEmail = damageRegistration.SendEmail;

                    // 2. Update the damage types
                    var selectedDamageTypes = await _damageTypeService.GetDamageTypesByIdsAsync(selectedDamageTypeIds);
                    existingDamageRegistration.DamageTypes = selectedDamageTypes.ToList();



                    await _damageRegistrationService.UpdateDamageRegistrationAsync(existingDamageRegistration);

                    if (existingDamageRegistration.SendEmail)
                    {
                        await _emailService.SendEmailAsync("emiliniya.87@gmail.com", "Schadegeval", _damageRegistrationService.GenerateEmailHtml(existingDamageRegistration));
                    }

                    var changes = _logService.GetChangesDamageTypes(existingDamageRegistration, existingRegistrationCopy);
                    var log = _logService.CreateLogDamageRegistration(existingDamageRegistration, user, changes, ControllerContext);
                    await _logService.AddLogAsync(log);

                    //_context.Update(damageRegistration);
                    //await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DamageRegistrationExists(damageRegistration.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DeviceId"] = new SelectList(_context.Devices, "Id", "Id", damageRegistration.DeviceId);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "Id", damageRegistration.StudentId);
            return View(damageRegistration);
        }

        // GET: DamageRegistrations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var damageRegistration = await _context.DamageRegistrations
                .Include(d => d.Device)
                .Include(d => d.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (damageRegistration == null)
            {
                return NotFound();
            }

            return View(damageRegistration);
        }

        // POST: DamageRegistrations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var damageRegistration = await _context.DamageRegistrations.FindAsync(id);
            if (damageRegistration != null)
            {
                _context.DamageRegistrations.Remove(damageRegistration);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DamageRegistrationExists(int id)
        {
            return _context.DamageRegistrations.Any(e => e.Id == id);
        }
    }
}
