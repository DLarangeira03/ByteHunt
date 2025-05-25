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
    // Atualizado: ContribuicaoController.cs
    public class ContribuicaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContribuicaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Contribuicao
        public async Task<IActionResult> Index(int? utilizadorId)
        {
            var query = _context.Contribuicoes
                .Include(c => c.Utilizador)
                .Include(c => c.Responsavel)
                .AsQueryable();

            if (utilizadorId.HasValue && utilizadorId.Value != 0)
            {
                query = query.Where(c => c.UtilizadorId == utilizadorId.Value);
            }

            var contribuicoes = await query.ToListAsync();

            var utilizadores = await _context.Utilizadores
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.Nome })
                .ToListAsync();

            utilizadores.Insert(0, new SelectListItem { Value = "0", Text = "Todos Utilizadores" });

            ViewData["Utilizadores"] = utilizadores;
            ViewData["UtilizadorSelecionado"] = utilizadorId ?? 0;

            return View(contribuicoes);
        }

        // GET: Contribuicao/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var contribuicao = await _context.Contribuicoes
                .Include(c => c.Utilizador)
                .Include(c => c.Responsavel)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contribuicao == null) return NotFound();

            ViewBag.Utilizadores = new SelectList(_context.Utilizadores, "Id", "Nome");
            return View(contribuicao);
        }

        // GET: Contribuicao/Create
        public IActionResult Create()
        {
            ViewData["UtilizadorId"] = new SelectList(_context.Utilizadores, "Id", "Nome");
            ViewData["ResponsavelId"] = new SelectList(_context.Utilizadores, "Id", "Nome");
            return View();
        }

        // POST: Contribuicao/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            var nome = form["Nome"];
            var marca = form["Marca"];
            var preco = form["Preco"];
            var descricao = form["Descricao"];
            var utilizadorIdStr = form["UtilizadorId"];
            int.TryParse(utilizadorIdStr, out var utilizadorId);

            string detalhes = $"Nome: {nome}\nMarca: {marca}\nPreço: {preco}\nDescrição: {descricao}";

            var contribuicao = new Contribuicao
            {
                DetalhesContribuicao = detalhes,
                DataContribuicao = DateTime.Now,
                Status = "Pending",
                UtilizadorId = utilizadorId
            };

            if (ModelState.IsValid)
            {
                _context.Add(contribuicao);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.UtilizadorId = new SelectList(_context.Utilizadores, "Id", "Nome", contribuicao.UtilizadorId);
            return View(contribuicao);
        }

        // GET: Contribuicao/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var contribuicao = await _context.Contribuicoes.FindAsync(id);
            if (contribuicao == null) return NotFound();

            ViewData["UtilizadorId"] =
                new SelectList(_context.Utilizadores, "Id", "Nome", contribuicao.UtilizadorId);
            ViewData["ResponsavelId"] =
                new SelectList(_context.Utilizadores, "Id", "Nome", contribuicao.ResponsavelId);
            return View(contribuicao);
        }

        // POST: Contribuicao/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,DetalhesContribuicao,Status,DataContribuicao,DataReview,ResponsavelId,UtilizadorId")]
            Contribuicao contribuicao)
        {
            if (id != contribuicao.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contribuicao);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContribuicaoExists(contribuicao.Id)) return NotFound();
                    else throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["UtilizadorId"] =
                new SelectList(_context.Utilizadores, "Id", "Nome", contribuicao.UtilizadorId);
            ViewData["ResponsavelId"] =
                new SelectList(_context.Utilizadores, "Id", "Nome", contribuicao.ResponsavelId);
            return View(contribuicao);
        }

        // GET: Contribuicao/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var contribuicao = await _context.Contribuicoes
                .Include(c => c.Utilizador)
                .Include(c => c.Responsavel)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contribuicao == null) return NotFound();

            return View(contribuicao);
        }

        // POST: Contribuicao/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contribuicao = await _context.Contribuicoes.FindAsync(id);
            if (contribuicao != null) _context.Contribuicoes.Remove(contribuicao);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContribuicaoExists(int id)
        {
            return _context.Contribuicoes.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int Id, int Responsavel, string status)
        {
            var contribuicao = await _context.Contribuicoes.FindAsync(Id);
            if (contribuicao == null) return NotFound();

            contribuicao.ResponsavelId = Responsavel;
            contribuicao.Status = status;
            contribuicao.DataReview = DateTime.Now;

            _context.Update(contribuicao);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}