using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolPCScanner.Models;
using SchoolPCScanner.Services;
using SchoolPCScanner.Services.Interfaces;
using SchoolPCScanner.ViewModels;


namespace SchoolPCScanner.Controllers
{
    //TODO do context weg
    public class DevicesController : Controller
    {
        //private readonly SchoolPCScannerDbContext _context;
        private readonly IDeviceService _deviceService;
        private readonly ILogService _logService;
        private readonly UserManager<IdentityUser> _userManager1;
        private readonly IStudentService _studentService;
        private readonly ISupplierService _supplierService;

        public DevicesController(IDeviceService deviceService, SchoolPCScannerDbContext context, ILogService logService, UserManager<IdentityUser> userManager1, IStudentService studentService, ISupplierService supplierService)
        {
            //_context = context;
            _deviceService = deviceService;
            _logService = logService;
            _userManager1 = userManager1;
            _studentService = studentService;
            _supplierService = supplierService;
        }


        // GET: Devices
        public async Task<IActionResult> Index(string searchItem)
        {
            ViewData["SearchLog"] = searchItem;

            var devices = from d in await _deviceService.GetAllDevicesAsync() select d;



            if (!string.IsNullOrEmpty(searchItem))
            {
                if (searchItem.ToLower() == "reserve")
                {
                    devices = devices.Where(d => d.IsReserve);
                    //students = students.Where(s => s.IsNew);
                }
                else if (Enum.TryParse(searchItem, true, out DeviceStatus status))
                {
                    devices = devices.Where(d => d.Serienumber.Contains(searchItem) ||
                                                 d.Type.Contains(searchItem) ||
                                                 d.Supplier.Name.Contains(searchItem) ||
                                                 d.Barcode.Contains(searchItem) ||
                                                 d.Student.Firstname.Contains(searchItem) ||
                                                 d.Student.Lastname.Contains(searchItem) ||
                                                 (d.Student.Firstname + " " + d.Student.Lastname).Contains(searchItem) ||
                                                 d.Status == status);
                }
                else
                {
                    devices = devices.Where(d => d.Serienumber.Contains(searchItem) ||
                                                 d.Type.Contains(searchItem) ||
                                                 d.Supplier.Name.Contains(searchItem) ||
                                                 d.Barcode.Contains(searchItem) ||
                                                 d.Student.Firstname.Contains(searchItem) ||
                                                 d.Student.Lastname.Contains(searchItem) ||
                                                 (d.Student.Firstname + " " + d.Student.Lastname).Contains(searchItem));
                }
            }
            var result = await devices.ToListAsync();
            return View(result);
        }


        // GET: Devices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //if (device == null)
            //{
            //    return NotFound();
            //}
            var deviceViewModel = await _deviceService.GetDeviceByIdAsync(id);
            if (deviceViewModel == null)
            {
                return NotFound();
            }
            var supplier = await _supplierService.GetSupplierByIdAsync(deviceViewModel.SupplierId);


            var device = new Device
            {
                Id = deviceViewModel.Id,
                Serienumber = deviceViewModel.Serienumber,
                Student = deviceViewModel.Student,
                Supplier = supplier,
                Type = deviceViewModel.Type,
                IsReserve = deviceViewModel.IsReserve,
                Barcode = deviceViewModel.Barcode,
                Status = deviceViewModel.Status
            };
            return View(device);
        }

        // GET: Devices/Create
        public async Task<IActionResult> Create()
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync();

            var viewModel = new DeviceViewModel
            {
                Suppliers = suppliers.ToList()
            };


            return View(viewModel);
        }

        // POST: Devices/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create([Bind("Serienumber,SupplierId,Type,IsReserve,Barcode,Status")] DeviceViewModel deviceViewModel)
        {

            if (ModelState.IsValid)
            {
                if (deviceViewModel == null) { return NotFound(); }
                if (string.IsNullOrEmpty(deviceViewModel.Serienumber) ||
                    string.IsNullOrEmpty(deviceViewModel.Barcode) ||
                    string.IsNullOrEmpty(deviceViewModel.Type))
                {
                    ModelState.AddModelError(string.Empty, "Gelieve alle velden in te vullen.");
                    deviceViewModel.Suppliers = await _supplierService.GetAllSuppliersAsync();
                    return View(deviceViewModel);
                }
                var existingDeviceSNumber = await _deviceService.GetDeviceByBarcodeOrSerieNumberAsync(deviceViewModel.Serienumber);
                var existingDeviceBarcode = await _deviceService.GetDeviceByBarcodeOrSerieNumberAsync(deviceViewModel.Barcode);
                if (existingDeviceSNumber != null || existingDeviceBarcode != null)
                {
                    //ModelState.AddModelError("Serienumber", "Dit toestel bestaat al.");
                    //deviceViewModel.Suppliers = await _supplierService.GetAllSuppliersAsync();
                    //return View(deviceViewModel);
                    if (existingDeviceSNumber != null)
                    {
                        ModelState.AddModelError("Serienumber", "Dit serienummer bestaat al.");
                    }
                    if (existingDeviceBarcode != null)
                    {
                        ModelState.AddModelError("Barcode", "Deze barcode bestaat al.");
                    }
                    deviceViewModel.Suppliers = await _supplierService.GetAllSuppliersAsync();
                    return View(deviceViewModel);

                }

                var device = new Device
                {
                    Serienumber = deviceViewModel.Serienumber,
                    SupplierId = deviceViewModel.SupplierId,
                    Type = deviceViewModel.Type,
                    IsReserve = deviceViewModel.IsReserve,
                    Barcode = deviceViewModel.Barcode,
                    Status = deviceViewModel.Status
                };

                await _deviceService.CreateDeviceAsync(device);
                return RedirectToAction(nameof(Index));
            }
            deviceViewModel.Suppliers = await _supplierService.GetAllSuppliersAsync();
            return View(deviceViewModel);
        }

        // GET: Devices/Edit/5
        public async Task<IActionResult> Edit(int? id, IdentityUser user, string searchStudent)
        {
            ViewData["SearchStudent"] = searchStudent;

            if (id == null)
            {
                return NotFound();
            }

            var device = await _deviceService.GetDeviceByIdAsync(id);
            if (device == null)
            {
                return NotFound();
            }

            var allSuppliers = await _supplierService.GetAllSuppliersAsync();
            ViewData["Suppliers"] = new SelectList(allSuppliers, "Id", "Name", device.SupplierId);

            if (device.StudentId != null)
            {
                var student = await _studentService.GetStudentByIdAsync(device.StudentId);
                if (student != null)
                {
                    ViewData["StudentId"] = new SelectList(new List<Student> { student }, "Id", "Id", device.StudentId);
                    ViewData["StudentLastname"] = student.Lastname;
                }
            }

            return View(device);
        }

        // POST: Devices/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Serienumber,SupplierId,Type,IsReserve,Barcode,Status")] Device device)
        {
            //if (id != device.Id)
            //{
            //    return NotFound();
            //}
            var user = await _userManager1.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var supplier = await _supplierService.GetSupplierByIdAsync(device.SupplierId);
            var suppliers = await _supplierService.GetAllSuppliersAsync();

            if (ModelState.IsValid)
            {
                try
                {
                    if (device == null) { return NotFound(); }
                    if (string.IsNullOrEmpty(device.Serienumber) || string.IsNullOrEmpty(device.Barcode) || string.IsNullOrEmpty(device.Type))
                    {
                        ModelState.AddModelError("", "Gelieve alle velden in te vullen.");
                        
                        ViewData["Suppliers"] = new SelectList(suppliers, "Id", "Name", device.SupplierId);
                        return View(device);
                    }

                    // Maak een kopie van de waarden van het apparaat
                    var existingDevice = await _deviceService.GetDeviceByIdAsync(id);

                    var originalDevice = _deviceService.CreateDeviceCopy(existingDevice);

                    var student = await _studentService.GetStudentByIdAsync(existingDevice.StudentId);

                    // Als StudentId niet null is, voeg het toestel toe aan de lijst van toestellen van de student
                    if (device.StudentId != null && student != null)
                    {
                        // Controleer of de lijst van apparaten null is
                        if (student.Devices == null)
                        {
                            // Maak een nieuwe lijst van apparaten
                            student.Devices = new List<Device>();
                        }
                        // Voeg het apparaat toe aan de lijst van apparaten van de student
                        student.Devices.Add(existingDevice);
                    }

                    // Map de nieuwe waarden naar de bestaande entiteit
                    existingDevice.Serienumber = device.Serienumber;
                    existingDevice.Supplier = supplier;
                    existingDevice.Type = device.Type;
                    existingDevice.IsReserve = device.IsReserve;
                    existingDevice.Student = student;
                    existingDevice.StudentId = student?.Id;
                    existingDevice.Barcode = device.Barcode;
                    //existingDevice.Status = device.Status;

                    await _deviceService.UpdateDeviceAsync(existingDevice);
                    // Haal de wijzigingen op tussen het originele en het bijgewerkte apparaat
                    var changes = _logService.GetChanges(originalDevice, existingDevice);
                    // Maak een log aan van de wijzigingen
                    var log = _logService.CreateLog(existingDevice, user, changes, ControllerContext);
                    await _logService.AddLogAsync(log);

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await DeviceExists(device.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                //return Redirect(redirectUrl);
                return RedirectToAction(nameof(Index));

            }
            ViewData["Suppliers"] = new SelectList(await _supplierService.GetAllSuppliersAsync(), "Id", "Name", device.SupplierId);
            ViewData["StudentId"] = new SelectList(_studentService.GetAllStudentsAsync().Result, "Id", "Id", device.StudentId);
            ViewData["StudentLastname"] = new SelectList(_studentService.GetAllStudentsAsync().Result, "Lastname", "Lastname", device.StudentId);
            return View(device);
        }

        // GET: Devices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var device = await _deviceService.GetDeviceByIdAsync(id);

            if (device == null)
            {
                return NotFound();
            }

            return View(device);
        }

        // POST: Devices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var device = await _deviceService.GetDeviceByIdAsync(id);
            if(device == null)
            {
                return NotFound();
            }
            if (device.StudentId != null)
            {
                return BadRequest("Het apparaat is nog steeds toegewezen aan een student.");
            }
            else
            {
                await _deviceService.DeleteDeviceAsync(device);
            }
            
            return RedirectToAction(nameof(Index));
            
            
            //if (device != null)
            //{
            //    await _deviceService.DeleteDeviceAsync(device);
            //}
            //return RedirectToAction(nameof(Index));
        }

        private async Task<bool> DeviceExists(int id)
        {
            return await _deviceService.GetDeviceByIdAsync(id) != null;
        }

    }

}
