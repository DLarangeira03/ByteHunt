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
        /// <summary>
        /// Lista todos os itens, com suporte a pesquisa por nome, marca ou categoria e paginação.
        /// </summary>
        /// <param name="searchTerm">Termo de pesquisa para filtrar os itens.</param>
        /// <param name="categoriaId">ID da categoria para filtrar os itens.</param>
        /// <param name="pageNumber">Número da página atual.</param>
        /// <param name="pageSize">Quantidade de itens por página.</param>
        /// <returns>View com a lista de itens filtrados e paginados.</returns>
        public async Task<IActionResult> Index(string searchTerm, int? categoriaId, int pageNumber = 1,
            int pageSize = 9) {
            var query = _context.Itens.Include(i => i.Categoria).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm)) {
                searchTerm = searchTerm.ToLower();
                // Filtra os itens com base na string de pesquisa
                query = query.Where(i =>
                    i.Nome.ToLower().Contains(searchTerm) ||
                    i.Marca.ToLower().Contains(searchTerm) ||
                    i.Categoria.Nome.ToLower().Contains(searchTerm));
            }

            if (categoriaId.HasValue && categoriaId.Value > 0) {
                query = query.Where(i => i.CategoriaId == categoriaId.Value);
            }
            
            // Conta o total de itens que correspondem aos critérios de pesquisa
            int totalItems = await query.CountAsync();
            
            // Pagina os resultados com base no número da página e no tamanho da página
            var itens = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            // Configura os dados de paginação na ViewData
            ViewData["CurrentPage"] = pageNumber;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalItems / pageSize);
            
            // Configura os dados de pesquisa na ViewData
            ViewData["SearchTerm"] = searchTerm ;
            ViewData["CategoriaId"] = categoriaId ?? 0;
            ViewData["Categorias"] = new SelectList(await _context.Categorias.ToListAsync(), "Id", "Nome");
            
            // Retorna a View com a lista de itens filtrados e paginados
            return View(itens);
        }

        // GET: Itens/Details/5
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }
            
            // Procurar o item pelo ID, incluindo a categoria associada
            var item = await _context.Itens
                .Include(i => i.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) {
                return NotFound();
            }
            
            // Retorna a View com os detalhes do item
            return View(item);
        }

        // GET: Itens/Create
        public IActionResult Create() {
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome");
            
            // Retorna a View para criação de um novo item
            return View();
        }

        // POST: Itens/Create
        /// <summary>
        /// Cria um novo item com os dados fornecidos.
        /// </summary>
        /// <param name="item">Objeto Item com os dados do novo item.</param>
        /// <param name="imagem">Arquivo de imagem do item.</param>
        /// <returns>Redireciona para a lista de itens se bem-sucedido, senão retorna a view de criação.</returns>
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
                
                // Adiciona o novo item ao contexto e salva as alterações
                _context.Add(item);
                // Salva as alterações no contexto
                await _context.SaveChangesAsync();
                // Redireciona para a ação Index após a criação bem-sucedida
                return RedirectToAction(nameof(Index));
            }
            
            // Se o ModelState não for válido, preenche o ViewData com a lista de categorias para o dropdown
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome", item.CategoriaId);
            // Retorna a View de criação com o item atual
            return View(item);
        }

        // GET: Itens/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }
            
            // Procura o item pelo ID
            var item = await _context.Itens.FindAsync(id);
            if (item == null) {
                return NotFound();
            }
            
            // Preenche o ViewData com a lista de categorias para o dropdown
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome", item.CategoriaId);
            
            // Retorna a View de edição com o item encontrado
            return View(item);
        }

        // POST: Itens/Edit/5
        /// <summary>
        ///  Edita um item existente com os dados fornecidos.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <param name="imagem"></param>
        /// <returns></returns>
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
                    
                    // Atualiza o item no contexto
                    _context.Update(item);
                    // Salva as alterações no contexto
                    await _context.SaveChangesAsync();
                    // Redireciona para a ação Index após a edição bem-sucedida
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
            
            // Se o ModelState não for válido, preenche o ViewData com a lista de categorias para o dropdown
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nome", item.CategoriaId);
            // Retorna a View de edição com o item atualizado
            return View(item);
        }

        // GET: Itens/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }
            
            // Procura o item pelo ID, incluindo a categoria associada
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