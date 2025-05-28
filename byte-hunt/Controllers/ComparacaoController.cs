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

namespace byte_hunt.Controllers {
    public class ComparacaoController : Controller {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public ComparacaoController(ApplicationDbContext context, UserManager<Utilizador> userManager) {
            _context = context;
            _userManager = userManager;
        }

        // GET: Comparacao
        public async Task<IActionResult> Index() {
            var applicationDbContext = _context.Comparacoes.Include(c => c.Utilizador);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Comparacao/Details/5
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            var comparacao = await _context.Comparacoes
                .Include(c => c.Utilizador)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comparacao == null) {
                return NotFound();
            }

            return View(comparacao);
        }

        // GET: Comparacao/Create
        public IActionResult Create() {
            ViewData["UtilizadorId"] = new SelectList(_userManager.Users, "Id", "Nome");
            return View();
        }

        // POST: Comparacao/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Data,UtilizadorId")] Comparacao comparacao) {
            if (ModelState.IsValid) {
                _context.Add(comparacao);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["UtilizadorId"] = new SelectList(_userManager.Users, "Id", "Nome", comparacao.UtilizadorId);
            return View(comparacao);
        }

        // GET: Comparacao/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            var comparacao = await _context.Comparacoes.FindAsync(id);
            if (comparacao == null) {
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Data,UtilizadorId")] Comparacao comparacao) {
            if (id != comparacao.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(comparacao);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) {
                    if (!ComparacaoExists(comparacao.Id)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["UtilizadorId"] = new SelectList(_userManager.Users, "Id", "Nome", comparacao.UtilizadorId);
            return View(comparacao);
        }

        // GET: Comparacao/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }

            var comparacao = await _context.Comparacoes
                .Include(c => c.Utilizador)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comparacao == null) {
                return NotFound();
            }

            return View(comparacao);
        }

        // POST: Comparacao/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var comparacao = await _context.Comparacoes.FindAsync(id);
            if (comparacao != null) {
                _context.Comparacoes.Remove(comparacao);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComparacaoExists(int id) {
            return _context.Comparacoes.Any(e => e.Id == id);
        }

        public IActionResult CompareSelect() {
            var items = _context.Itens.Include(i => i.Categoria).ToList();
            return View(new ItemCompareSelectViewModel { AllItems = items });
        }

        // preparar a comparacao e chamar o compare
        [HttpPost]
        public IActionResult RunCompare(List<string> itemNames) {
            var items = _context.Itens
                .Include(i => i.Categoria)
                .Where(i => itemNames.Contains(i.Nome))
                .ToList();

            if (items.Count < 2 || items.Count > 4)
                return BadRequest("Selecione de 2 a 4 itens por favor");

            var categoryId = items.First().CategoriaId;
            if (items.Any(i => i.CategoriaId != categoryId))
                return BadRequest("Todos os itens devem pertencer a mesma categoria");

            var model = BuildComparisonViewModel(items);
            return View("Compare", model);
        }

        private ItemComparisonViewModel BuildComparisonViewModel(List<Item> items) {
            var attrMap = new Dictionary<string, List<string>>();

            for (int i = 0; i < items.Count; i++) {
                var attrs =
                    System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(items[i].AttrsJson ?? "{}");
                
                //criar um mapa (dataset) que abrange todos os atributos de todos os itens
                foreach (var kv in attrs) {
                    if (!attrMap.ContainsKey(kv.Key))
                        attrMap[kv.Key] = Enumerable.Repeat("---", items.Count).ToList();
                    attrMap[kv.Key][i] = kv.Value;
                }
            }
            
            //definir cada row da comparacao
            var rows = attrMap.Select(kvp => new AttrComparisonRow {
                Key = kvp.Key,
                Values = kvp.Value
            }).ToList();
            
            // retorna model para a tabela
            return new ItemComparisonViewModel {
                Items = items,
                AttrRows = rows
            };
        }
    }
}