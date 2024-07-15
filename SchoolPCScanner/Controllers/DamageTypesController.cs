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
    public class DamageTypesController : Controller
    {
        private readonly SchoolPCScannerDbContext _context;
        private readonly IDamageTypeService _damageTypeService;

        public DamageTypesController(SchoolPCScannerDbContext context, IDamageTypeService damageTypeService)
        {
            _context = context;
            _damageTypeService = damageTypeService;

        }

        // GET: DamageTypes
        public async Task<IActionResult> Index()
        {
            return View(await _damageTypeService.GetAllDamageTypesAsync());
        }

        // GET: DamageTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var damageType = await _context.DamageTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (damageType == null)
            {
                return NotFound();
            }

            return View(damageType);
        }

        // GET: DamageTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DamageTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TypeName")] DamageType damageType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(damageType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(damageType);
        }

        // GET: DamageTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var damageType = await _context.DamageTypes.FindAsync(id);
            if (damageType == null)
            {
                return NotFound();
            }
            return View(damageType);
        }

        // POST: DamageTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TypeName")] DamageType damageType)
        {
            if (id != damageType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(damageType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DamageTypeExists(damageType.Id))
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
            return View(damageType);
        }

        // GET: DamageTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var damageType = await _context.DamageTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (damageType == null)
            {
                return NotFound();
            }

            return View(damageType);
        }

        // POST: DamageTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var damageType = await _context.DamageTypes.FindAsync(id);
            if (damageType != null)
            {
                _context.DamageTypes.Remove(damageType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DamageTypeExists(int id)
        {
            return _context.DamageTypes.Any(e => e.Id == id);
        }
    }
}
