using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using byte_hunt.Data;
using byte_hunt.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Globalization;

namespace byte_hunt.Controllers {
    public class ItensController : Controller {
        private readonly ApplicationDbContext _context;

        public ItensController(ApplicationDbContext context) {
            _context = context;
        }

        // GET: Itens
        public async Task<IActionResult> Index(string searchTerm, int? categoriaId, int pageNumber = 1,
            int pageSize = 9) {
            var query = _context.Itens.Include(i => i.Categoria).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm)) {
                searchTerm = searchTerm.ToLower();
                query = query.Where(i =>
                    i.Nome.ToLower().Contains(searchTerm) ||
                    i.Marca.ToLower().Contains(searchTerm) ||
                    i.Categoria.Nome.ToLower().Contains(searchTerm));
            }

            if (categoriaId.HasValue && categoriaId.Value > 0) {
                query = query.Where(i => i.CategoriaId == categoriaId.Value);
            }

            int totalItems = await query.CountAsync();

            var itens = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = pageNumber;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalItems"] = totalItems;

            ViewData["SearchTerm"] = searchTerm ?? "";
            ViewData["CategoriaId"] = categoriaId ?? 0;
            ViewData["Categorias"] = new SelectList(await _context.Categorias.ToListAsync(), "Id", "Nome");

            return View(itens);
        }

        // GET: Itens/Details/5
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            var item = await _context.Itens
                .Include(i => i.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) {
                return NotFound();
            }

            return View(item);
        }

        // GET: Itens/Create
        public IActionResult Create() {
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome");
            return View();
        }

        // POST: Itens/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,Marca,Descricao,CategoriaId,AttrsJson")] Item item,
            IFormFile imagem) {
            ModelState.Remove("imagem");

            // validacao json
            try
            {
                if (!string.IsNullOrWhiteSpace(item.AttrsJson))
                {
                    using var doc = JsonDocument.Parse(item.AttrsJson); // excecao se invalido
                    item.AttrsJson = JsonSerializer.Serialize(doc.RootElement); // minifica
                }
            }
            catch (JsonException)
            {
                ModelState.AddModelError("AttrsJson", "O conteúdo não é um JSON válido.");
            }
            
            ModelState.Remove("Preco");
            var precoStr = Request.Form["Preco"];
            if (decimal.TryParse(precoStr, NumberStyles.Any, CultureInfo.CurrentCulture, out var precoParsed))
            {
                item.Preco = precoParsed;
            }
            else
            {
                ModelState.AddModelError("Preco", "Preço inválido.");
            }
            
            if (ModelState.IsValid) {
                if (imagem != null) {
                    var nomeImagem = await GuardarImagemAsync(imagem);
                    if (nomeImagem != null) {
                        item.FotoItem = nomeImagem;
                    }
                    else {
                        item.FotoItem = string.Empty;
                    }
                }
                else {
                    Console.WriteLine("image  null");
                    item.FotoItem = string.Empty;
                }

                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome", item.CategoriaId);
            return View(item);
        }

        // GET: Itens/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            var item = await _context.Itens.FindAsync(id);
            if (item == null) {
                return NotFound();
            }

            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome", item.CategoriaId);
            return View(item);
        }

        // POST: Itens/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Nome,Marca,Descricao,CategoriaId,AttrsJson")] Item item, IFormFile imagem) {
            if (id != item.Id) {
                return NotFound();
            }

            // Obtem o item original da base de dados
            var itemExistente = await _context.Itens.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
            if (itemExistente == null) {
                return NotFound();
            }

            ModelState.Remove("imagem");
            
            // validacao json
            try
            {
                if (!string.IsNullOrWhiteSpace(item.AttrsJson))
                {
                    using var doc = JsonDocument.Parse(item.AttrsJson); // excecao se invalido
                    item.AttrsJson = JsonSerializer.Serialize(doc.RootElement); // minifica
                }
            }
            catch (JsonException)
            {
                ModelState.AddModelError("AttrsJson", "O conteúdo não é um JSON válido.");
            }
            
            ModelState.Remove("Preco");
            var precoStr = Request.Form["Preco"];
            if (decimal.TryParse(precoStr, NumberStyles.Any, CultureInfo.CurrentCulture, out var precoParsed))
            {
                item.Preco = precoParsed;
            }
            else
            {
                ModelState.AddModelError("Preco", "Preço inválido.");
            }
            
            if (ModelState.IsValid) {
                try {
                    if (imagem != null && imagem.Length > 0) {
                        var nomeImagem = await GuardarImagemAsync(imagem);
                        if (nomeImagem != null) {
                            item.FotoItem = nomeImagem;
                        }
                    }
                    else {
                        // Mantém a imagem anterior
                        item.FotoItem = itemExistente.FotoItem;
                    }

                    _context.Update(item);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException) {
                    if (!ItemExists(item.Id)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
            }

            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome", item.CategoriaId);
            return View(item);
        }

        // GET: Itens/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }

            var item = await _context.Itens
                .Include(i => i.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) {
                return NotFound();
            }

            return View(item);
        }

        // POST: Itens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var item = await _context.Itens.FindAsync(id);
            if (item != null) {
                _context.Itens.Remove(item);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ItemExists(int id) {
            return _context.Itens.Any(e => e.Id == id);
        }

        private async Task<string> GuardarImagemAsync(IFormFile ficheiro) {
            if (ficheiro != null && ficheiro.Length > 0) {
                var extensao = Path.GetExtension(ficheiro.FileName).ToLower();
                var permitidas = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                if (!permitidas.Contains(extensao)) return null;

                if (!ficheiro.ContentType.StartsWith("image/")) return null;

                if (ficheiro.Length > 5 * 1024 * 1024) return null; // Limite de 5MB

                var nomeUnico = Guid.NewGuid().ToString() + extensao;
                var pastaUploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "itens_Imagens");

                if (!Directory.Exists(pastaUploads)) {
                    Directory.CreateDirectory(pastaUploads);
                }

                var caminho = Path.Combine(pastaUploads, nomeUnico);

                using var stream = new FileStream(caminho, FileMode.Create);
                await ficheiro.CopyToAsync(stream);

                return nomeUnico;
            }

            return null;
        }
    }
}