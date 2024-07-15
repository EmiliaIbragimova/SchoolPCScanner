using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SchoolPCScanner.Models;
using SchoolPCScanner.Services.Interfaces;

namespace SchoolPCScanner.Controllers
{
    public class StudentsController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IDeviceService _deviceService;

        public StudentsController(SchoolPCScannerDbContext context, IStudentService studentService, IDeviceService deviceService)
        {
            _studentService = studentService;
            _deviceService = deviceService;
        }

        // GET: Students
        public async Task<IActionResult> Index(string searchItem)
        {
            ViewData["SearchStudent"] = searchItem;

            var students = from s in await _studentService.GetAllStudentsAsync()
                           select s;
            
            if (!string.IsNullOrEmpty(searchItem))
            {
                if (searchItem.ToLower() == "nieuw")
                {
                    students = students.Where(s => s.IsNew);
                }
                else if (searchItem.ToLower() == "beeindigd")
                {
                    students = students.Where(s => !s.IsActive);
                }
                else
                {
                    students = students.Where(s => s.Firstname.Contains(searchItem) || s.Lastname.Contains(searchItem) || s.Grade.Contains(searchItem) || (s.Firstname + " " + s.Lastname).Contains(searchItem));
                }


            }
            return View(await students.ToListAsync());
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var student = await _context.Students
            //    .FirstOrDefaultAsync(m => m.Id == id);
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            //student.Devices = _context.Devices.Where(d => d.StudentId == id).ToList();
            student.Devices = await _deviceService.GetDevicesByStudentIdAsync(id);

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Firstname,Lastname,Birthdate,Email,Grade,IsActive,IsNew,Phonenumber")] Student student)
        {
            if (ModelState.IsValid)
            {
                //_context.Add(student);
                //await _context.SaveChangesAsync();
                await _studentService.CreateStudentAsync(student);
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var student = await _context.Students.FindAsync(id);
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Firstname,Lastname,Birthdate,Email,Grade,IsActive,Phonenumber")] Student student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //_context.Update(student);
                    //await _context.SaveChangesAsync();
                    await _studentService.UpdateStudentAsync(student);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await StudentExists(student.Id))
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
            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var student = await _context.Students
            //    .FirstOrDefaultAsync(m => m.Id == id);
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //var student = await _context.Students.FindAsync(id);
            var student = await _studentService.GetStudentByIdAsync(id);
            var deviceByStudent = await _deviceService.GetDevicesByStudentIdAsync(id);

            if (deviceByStudent.Count > 0)
            {
                // Als de student wordt gebruikt in de Device-entiteit, geef dan een foutmelding terug
                return BadRequest("Deze student kan niet worden verwijderd omdat hij/zij wordt gebruikt in een apparaatrecord. Verwijder eerst het apparaatrecord van deze student.");
            }

            if (student != null)
            {
                await _studentService.DeleteStudentAsync(student);
            }
            
            //await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> StudentExists(int id)
        {
            //return _context.Students.Any(e => e.Id == id);
            return await _studentService.GetStudentByIdAsync(id) != null;
        }
    }
}
