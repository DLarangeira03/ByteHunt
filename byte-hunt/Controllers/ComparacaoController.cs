using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using byte_hunt.Data;
using byte_hunt.Models;
using Microsoft.AspNetCore.Identity;

namespace byte_hunt.Controllers
{
    public class ComparacaoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public ComparacaoController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Comparacao
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Comparacoes.Include(c => c.Utilizador);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Comparacao/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comparacao = await _context.Comparacoes
                .Include(c => c.Utilizador)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comparacao == null)
            {
                return NotFound();
            }

            return View(comparacao);
        }

        // GET: Comparacao/Create
        public IActionResult Create()
        {
            ViewData["UtilizadorId"] = new SelectList(_userManager.Users, "Id", "Nome");
            return View();
        }

        // POST: Comparacao/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Data,UtilizadorId")] Comparacao comparacao)
        {
            if (ModelState.IsValid)
            {
                _context.Add(comparacao);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UtilizadorId"] = new SelectList(_userManager.Users, "Id", "Nome", comparacao.UtilizadorId);
            return View(comparacao);
        }

        // GET: Comparacao/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comparacao = await _context.Comparacoes.FindAsync(id);
            if (comparacao == null)
            {
                return NotFound();
            }
            ViewData["UtilizadorId"] = new SelectList(_userManager.Users, "Id", "Nome", comparacao.UtilizadorId);
            return View(comparacao);
        }

        // POST: Comparacao/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Data,UtilizadorId")] Comparacao comparacao)
        {
            if (id != comparacao.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(comparacao);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComparacaoExists(comparacao.Id))
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
            ViewData["UtilizadorId"] = new SelectList(_userManager.Users, "Id", "Nome", comparacao.UtilizadorId);
            return View(comparacao);
        }

        // GET: Comparacao/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comparacao = await _context.Comparacoes
                .Include(c => c.Utilizador)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comparacao == null)
            {
                return NotFound();
            }

            return View(comparacao);
        }

        // POST: Comparacao/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comparacao = await _context.Comparacoes.FindAsync(id);
            if (comparacao != null)
            {
                _context.Comparacoes.Remove(comparacao);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComparacaoExists(int id)
        {
            return _context.Comparacoes.Any(e => e.Id == id);
        }
    }
}
