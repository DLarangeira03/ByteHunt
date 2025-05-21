using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using byte_hunt.Data;
using byte_hunt.Models;

namespace byte_hunt.Controllers
{
    public class ContribuicaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContribuicaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Contribuicao
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Contribuicoes.Include(c => c.Utilizador);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Contribuicao/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contribuicao = await _context.Contribuicoes
                .Include(c => c.Utilizador)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contribuicao == null)
            {
                return NotFound();
            }

            return View(contribuicao);
        }

        // GET: Contribuicao/Create
        public IActionResult Create()
        {
            ViewData["UtilizadorId"] = new SelectList(_context.Utilizadores, "Id", "Nome");
            return View();
        }

        // POST: Contribuicao/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Descricao_Contribuicao,Data,UtilizadorId")] Contribuicao contribuicao)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contribuicao);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UtilizadorId"] = new SelectList(_context.Utilizadores, "Id", "Nome", contribuicao.UtilizadorId);
            return View(contribuicao);
        }

        // GET: Contribuicao/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contribuicao = await _context.Contribuicoes.FindAsync(id);
            if (contribuicao == null)
            {
                return NotFound();
            }
            ViewData["UtilizadorId"] = new SelectList(_context.Utilizadores, "Id", "Nome", contribuicao.UtilizadorId);
            return View(contribuicao);
        }

        // POST: Contribuicao/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Descricao_Contribuicao,Data,UtilizadorId")] Contribuicao contribuicao)
        {
            if (id != contribuicao.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contribuicao);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContribuicaoExists(contribuicao.Id))
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
            ViewData["UtilizadorId"] = new SelectList(_context.Utilizadores, "Id", "Nome", contribuicao.UtilizadorId);
            return View(contribuicao);
        }

        // GET: Contribuicao/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contribuicao = await _context.Contribuicoes
                .Include(c => c.Utilizador)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contribuicao == null)
            {
                return NotFound();
            }

            return View(contribuicao);
        }

        // POST: Contribuicao/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contribuicao = await _context.Contribuicoes.FindAsync(id);
            if (contribuicao != null)
            {
                _context.Contribuicoes.Remove(contribuicao);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContribuicaoExists(int id)
        {
            return _context.Contribuicoes.Any(e => e.Id == id);
        }
    }
}
