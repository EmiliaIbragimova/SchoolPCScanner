using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolPCScanner.Models;
using SchoolPCScanner.Services.Interfaces;

namespace SchoolPCScanner.Controllers
{
    public class LogsController : Controller
    {
        private readonly SchoolPCScannerDbContext _context;
        private readonly ILogService _logService;

        public LogsController(SchoolPCScannerDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: Logs
        public async Task<IActionResult> Index(int? id, string searchItem, DateTime? searchDate)
        {
            ViewData["SearchLog"] = searchItem;
            var logs = from l in await _logService.GetAllLogsAsync() select l;

            if (id == null)
            {
                
                if (!string.IsNullOrEmpty(searchItem))
                {
                    logs = logs.Where(l => l.Device.Serienumber == searchItem);
                }

                if (searchDate.HasValue)
                {
                    // Filter logs op de opgegeven datum
                    logs = logs.Where(l => l.Date.Date == searchDate.Value.Date);
                }

            }
            var result = await logs.ToListAsync();
            return View(result);
            //else
            //{
            //var logs = from l in await _logService.GetLogsByDeviceIdAsync(id.Value) select l;
            //if (!string.IsNullOrEmpty(searchItem))
            //{
            //    if (Enum.IsDefined(typeof(DeviceStatus), searchItem))
            //    {
            //        var status = (DeviceStatus)Enum.Parse(typeof(DeviceStatus), searchItem, true);
            //        logs = logs.Where(l => l.Device.Status == status);
            //    }
            //    //logs = logs.Where(l => l.Device.Status == searchItem);
            //}

            //if (searchDate.HasValue)
            //{
            //    // Filter logs op de opgegeven datum
            //    logs = logs.Where(l => l.Date.Date == searchDate.Value.Date);
            //}

            //var logsByDevice = await _logService.GetLogsByDeviceIdAsync(id.Value);
            //    var result = await logs.ToListAsync();
            //    return View(result);
            //}



            //var result = await logs.ToListAsync();
            //return View(result);

            //if (deviceId == null)
            //{
            //    return NotFound();
            //}

            //var logs = await _context.Logs.Include(l => l.Device).Where(l => l.DeviceId == deviceId).ToListAsync();

            //if (logs == null)
            //{
            //    return NotFound();
            //}

            //return View(logs);
        }

        // GET: Logs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var log = await _context.Logs
                .Include(l => l.Device).Include(l => l.Device.Student).Include(l => l.IdentityUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (log == null)
            {
                return NotFound();
            }

            return View(log);
        }

        // GET: Logs/Create
        public IActionResult Create()
        {
            ViewData["DeviceId"] = new SelectList(_context.Devices, "Id", "Id");
            return View();
        }

        // POST: Logs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DeviceId,Status,Action,StudentName,StudentId,Date,Note")] Log log)
        {
            if (ModelState.IsValid)
            {
                _context.Add(log);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DeviceId"] = new SelectList(_context.Devices, "Id", "Id", log.DeviceId);
            return View(log);
        }

        // GET: Logs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var log = await _context.Logs.FindAsync(id);
            if (log == null)
            {
                return NotFound();
            }
            ViewData["DeviceId"] = new SelectList(_context.Devices, "Id", "Id", log.DeviceId);
            return View(log);
        }

        // POST: Logs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DeviceId,Status,Action,StudentName,StudentId,Date,Note")] Log log)
        {
            if (id != log.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(log);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LogExists(log.Id))
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
            ViewData["DeviceId"] = new SelectList(_context.Devices, "Id", "Id", log.DeviceId);
            return View(log);
        }

        // GET: Logs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var log = await _context.Logs
                .Include(l => l.Device)
                .Include(l => l.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (log == null)
            {
                return NotFound();
            }

            return View(log);
        }

        // POST: Logs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var log = await _context.Logs.FindAsync(id);
            if (log != null)
            {
                _context.Logs.Remove(log);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LogExists(int id)
        {
            return _context.Logs.Any(e => e.Id == id);
        }
    }
}
