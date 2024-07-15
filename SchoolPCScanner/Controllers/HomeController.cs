using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolPCScanner.Models;
using SchoolPCScanner.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;

namespace SchoolPCScanner.Controllers
{
    public class HomeController : Controller
    {
        //private readonly SchoolPCScannerDbContext _context;
        private readonly IDeviceService _deviceService;
        private readonly ILogService _logService;
        private readonly UserManager<IdentityUser> _userManager1;
        private readonly IStudentService _studentService;

        public HomeController(SchoolPCScannerDbContext context, IDeviceService deviceService, ILogService logService, UserManager<IdentityUser> userManager, IStudentService studentService)
        {
            //_context = context;
            _deviceService = deviceService;
            _logService = logService;
            _userManager1 = userManager;
            _studentService = studentService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> BinnenBrengenView()
        {
            return View();
        }

        public async Task<IActionResult> OphalenView(string searchItem, string barcode)
        {
            ViewData["SearchStudent"] = searchItem;
            ViewData["Barcode"] = barcode;

            var students = from s in await _studentService.GetAllStudentsAsync()
                           select s;
            
            if (!string.IsNullOrEmpty(searchItem))
            {
                students = students.Where(s => s.Firstname.Contains(searchItem) || s.Lastname.Contains(searchItem) || (s.Firstname + " " + s.Lastname).Contains(searchItem));
            }

            // Zoek de apparaatstatus
            if (!string.IsNullOrEmpty(barcode))
            {
                var device = await _deviceService.GetDeviceByBarcodeAsync(barcode);
                if (device != null)
                {
                    //TODO check of serienumber niet null is
                    var status = device.Status;
                    var statusDisplayName = _deviceService.GetDisplayName(status);
                    var serienumber = device.Serienumber;
                    ViewData["DeviceStatus"] = statusDisplayName;
                    ViewData["DeviceSerienumber"] = serienumber;
                }
                else
                {
                    ViewData["DeviceStatus"] = "Apparaat niet gevonden";
                }
            }
            
            return View(students);
        }
        
        // GET: Device
        public async Task<IActionResult> DeviceIndex(string barcode, DeviceStatus? status)
        {
            if (string.IsNullOrEmpty(barcode))
            {
                // Barcode is leeg, geef een foutmelding of redirect terug naar de indexpagina
                return BadRequest("Barcode of serienummer is vereist");
            }
            var user = await _userManager1.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            //var device = await _deviceService.GetDeviceByBarcodeAsync(barcode);
            var device = await _deviceService.GetDeviceByBarcodeOrSerieNumberAsync(barcode);

            //var deviceCopy = _deviceService.CreateDeviceCopy(device);

            if (device == null)
            {
                // Geen apparaat gevonden voor de opgegeven barcode, geef een foutmelding of redirect terug naar de indexpagina
                return NotFound("Apparaat niet gevonden");
            }

            // Haal de student op basis van het apparaat
            var student = await _studentService.GetStudentByIdAsync(device.StudentId);
            if (student == null)
            {
                return BadRequest("Er is geen student gekoppeld aan dit apparaat");
            }

            if (status != null)
            {
                device.Status = status.Value;
                await _deviceService.UpdateDeviceAsync(device);

                //var changes = _logService.GetChanges(deviceCopy, device);
                //var log = _logService.CreateLog(device, user, changes, ControllerContext);
                //await _logService.AddLogAsync(log);

                return status.Value switch
                {
                    DeviceStatus.Damage => RedirectToAction("Create", "DamageRegistrations", new { id = device.Id, status, grade = student.Grade }),
                    DeviceStatus.Termination => RedirectToAction("Create", "TerminationRegistrations", new { id = device.Id, status }),
                    DeviceStatus.InUse => RedirectToAction("Details", "Devices", new { id = device.Id, status }),
                    _ => RedirectToAction("Index", "Home", new { id = device.Id, status })
                };
            }
            
            return RedirectToAction("Index", "Home", new { id = device.Id, status = status });

        }

        
        public async Task<IActionResult> LinkStudentToDevice(int studentId, string barcode)
        {
            if (string.IsNullOrEmpty(barcode))
            {
                // Barcode is leeg, geef een foutmelding of redirect terug naar de indexpagina
                return BadRequest("Barcode is vereist");
            }

            // Haal het apparaat op basis van de barcode
            var device = await _deviceService.GetDeviceByBarcodeAsync(barcode);

            if (device == null)
            {
                return NotFound("Apparaat niet gevonden");
            }

            // Haal de student op basis van het studentId
            var student = await _studentService.GetStudentByIdAsync(studentId);
            if (student == null)
            {
                return NotFound("Student niet gevonden");
            }

            // Controleer of het apparaat al aan een student is gekoppeld
            if (device.StudentId != null)
            {
                return BadRequest("Apparaat is al aan een student gekoppeld");
            }

            // Controleer of het apparaat beschikbaar is
            if (device.Status != DeviceStatus.Available)
            {
                return BadRequest("Apparaat is niet beschikbaar");
            }

            var allDevicesByStudent = await _deviceService.GetDevicesByStudentIdAsync(studentId);
            student.Devices = allDevicesByStudent;

            // Koppel de student aan het apparaat
            device.StudentId = student.Id;
            //TODO vragen of dit nodig is, want als student ontkoppeld is, zegt het dat hij/zij weg van school is. Of is het wel nodig in het geval van vergissing???
            if (!student.IsActive)
            {
                student.IsActive = true;
            }
            if (student.Devices == null)
            {
                student.Devices = new List<Device>();
            }
            student.Devices.Add(device);
            Console.WriteLine($"Student has {student.Devices.Count} devices.");
            device.Status = DeviceStatus.InUse;
            await _deviceService.UpdateDeviceAsync(device);

            return RedirectToAction("OphalenView", new { searchItem = student.FullName, barcode });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        
    }
}
