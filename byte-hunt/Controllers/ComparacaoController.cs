using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using byte_hunt.Data;
using byte_hunt.Models;
using byte_hunt.Models.Comparador;
using Microsoft.AspNetCore.Identity;

namespace byte_hunt.Controllers {
    public class ComparacaoController : Controller {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;
        private readonly SignInManager<Utilizador> _signInManager;

        public ComparacaoController(ApplicationDbContext context, UserManager<Utilizador> userManager,
            SignInManager<Utilizador> signInManager) {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult CompareSelect() {
            var items = _context.Itens.Include(i => i.Categoria).ToList();
            return View(new ItemCompareSelectViewModel { AllItems = items });
        }

        [HttpPost]
        public async Task<IActionResult> RunCompare(List<string> itemNames) {
            var items = _context.Itens
                .Include(i => i.Categoria)
                .Where(i => itemNames.Contains(i.Nome))
                .ToList();

            if (items.Count < 2 || items.Count > 4)
                return BadRequest("Selecione de 2 a 4 itens por favor");

            var categoryId = items.First().CategoriaId;
            if (items.Any(i => i.CategoriaId != categoryId))
                return BadRequest("Todos os itens devem pertencer à mesma categoria");

            if (_signInManager.IsSignedIn(User)) {
                var user = await _userManager.GetUserAsync(User);
                var comparacao = new Comparacao {
                    Data = DateTime.Now,
                    UtilizadorId = user.Id,
                    Itens = items
                };
                _context.Comparacoes.Add(comparacao);
                await _context.SaveChangesAsync();
            }

            var model = BuildComparisonViewModel(items);
            return View("Compare", model);
        }
        
        private ItemComparisonViewModel BuildComparisonViewModel(List<Item> items) {
            var attrMap = new Dictionary<string, List<string>>();

            for (int i = 0; i < items.Count; i++) {
                var attrs =
                    System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(items[i].AttrsJson ?? "{}");

                foreach (var kv in attrs) {
                    var key = kv.Key.Trim();
                    if (!attrMap.ContainsKey(key))
                        attrMap[key] = Enumerable.Repeat("---", items.Count).ToList();
                    attrMap[key][i] = kv.Value;
                }
            }

            var rows = new List<AttrComparisonRow>();

            foreach (var kvp in attrMap) {
                var key = kvp.Key;
                var values = kvp.Value;

                var rule = AttributeRulesRegistry.Rules
                               .FirstOrDefault(r => string.Equals(r.Key, key, StringComparison.OrdinalIgnoreCase)).Value
                           ?? new AttributeComparer.AttributeRule {
                               Key = key,
                               Direction = AttributeComparer.ComparisonDirection.HigherIsBetter
                           };

                List<HighlightType> highlights;

                if (values.Any(v => v == "---")) {
                    highlights = values.Select(_ => HighlightType.None).ToList();
                }
                else {
                    highlights = AttributeComparer.Compare(values, rule);
                }

                rows.Add(new AttrComparisonRow {
                    Key = key,
                    Values = values,
                    Highlights = highlights
                });
            }

            return new ItemComparisonViewModel {
                Items = items,
                AttrRows = rows
            };
        }

        public async Task<IActionResult> HistoricoComparacao() {
            if (!_signInManager.IsSignedIn(User)) return RedirectToAction("Login", "Account");

            var user = await _userManager.GetUserAsync(User);
            var comparacoes = await _context.Comparacoes
                .Where(c => c.UtilizadorId == user.Id)
                .Include(c => c.Itens)
                .OrderByDescending(c => c.Data)
                .ToListAsync();

            return View(comparacoes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletarHistorico(int id) {
            var user = await _userManager.GetUserAsync(User);
            var comp = await _context.Comparacoes
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.Id == id && c.UtilizadorId == user.Id);

            if (comp != null) {
                _context.Comparacoes.Remove(comp);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("HistoricoComparacao");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletarHistoricoAll() {
            var user = await _userManager.GetUserAsync(User);
            var all = _context.Comparacoes.Where(c => c.UtilizadorId == user.Id);

            _context.Comparacoes.RemoveRange(all);
            await _context.SaveChangesAsync();

            return RedirectToAction("HistoricoComparacao");
        }
    }
}