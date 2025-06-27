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
        public async Task<IActionResult> Index(string searchTerm, int? categoriaId, int pageNumber = 1, int pageSize = 9) {
            
            // Query para obter todos os itens, incluindo a categoria associada
            var query = _context.Itens.Include(i => i.Categoria).AsQueryable();
            
            // Verifica se há uma string de pesquisa e filtra os itens
            if (!string.IsNullOrEmpty(searchTerm)) {
                searchTerm = searchTerm.ToLower();
                // Filtra os itens com base na string de pesquisa
                query = query.Where(i =>
                    i.Nome.ToLower().Contains(searchTerm) ||
                    i.Marca.ToLower().Contains(searchTerm) ||
                    i.Categoria.Nome.ToLower().Contains(searchTerm));
            }
            
            // Verifica se há um ID de categoria e filtra os itens por categoria
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
        /// <summary>
        /// Mostra os detalhes de um item.
        /// </summary>
        /// <param name="id">ID do item.</param>
        /// <returns>View com os detalhes do item.</returns>
        public async Task<IActionResult> Details(int? id) {
            // Verifica se o ID é nulo
            if (id == null) {
                // Se o ID for nulo, retorna NotFound
                return NotFound();
            }
            
            // Procurar o item pelo ID, incluindo a categoria associada
            var item = await _context.Itens
                .Include(i => i.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            // Verifica se o item foi encontrado
            if (item == null) {
                // Se o item não existir, retorna NotFound
                return NotFound();
            }
            
            // Retorna a View com os detalhes do item
            return View(item);
        }

        // GET: Itens/Create
        /// <summary>
        /// Exibe o formulário para criar um novo item.
        /// </summary>
        /// <returns>View para criar um item.</returns>
        [Authorize(Roles = "Administrator,Moderator")]
        public IActionResult Create() {
            // Preenche o ViewData com a lista de categorias para o dropdown
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
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Create([Bind("Id,Nome,Marca,Descricao,CategoriaId,AttrsJson")] Item item, IFormFile imagem) {
            
            // Retira a validação do campo imagem do ModelState
            ModelState.Remove("imagem");

            // Validação fo JSON
            try
            {
                // Verifica se o JSON de atributos não está vazio 
                if (!string.IsNullOrWhiteSpace(item.AttrsJson))
                {
                    using var doc = JsonDocument.Parse(item.AttrsJson); // Exceção se inválido
                    // Serializa o JSON para garantir que não está minifica ("espandido") ou formatado de forma inadequada
                    item.AttrsJson = JsonSerializer.Serialize(doc.RootElement); 
                }
            }
            catch (JsonException)
            {
                // Adiciona um erro ao ModelState se o JSON não for válido
                ModelState.AddModelError("AttrsJson", "O conteúdo não é um JSON válido.");
            }
            
            // Tenta obter o valor do campo Preco do formulário
            var precoStr = Request.Form["Preco"];
            // Tenta converter o valor do campo Preco para decimal
            if (decimal.TryParse(precoStr, NumberStyles.Any, CultureInfo.CurrentCulture, out var precoParsed)) {
                if (precoParsed < 0) {
                    ModelState.AddModelError("Preco", "O preço deve ser positivo.");
                } else {
                    item.Preco = precoParsed;
                }
            } else {
                ModelState.AddModelError("Preco", "Preço inválido.");
            }
            
            // Verifica se o ModelState é válido
            if (ModelState.IsValid) {
                // Verifica se a imagem não é nula 
                if (imagem != null) {
                    // Tenta guardar a imagem e obter o nome do arquivo
                    var nomeImagem = await GuardarImagemAsync(imagem);
                    // Verifica se o nome da imagem não é nulo
                    if (nomeImagem != null) {
                        // Atribui o nome da imagem ao item
                        item.FotoItem = nomeImagem;
                    }
                    else {
                        // Se o nome da imagem for nulo, define FotoItem como uma string vazia
                        item.FotoItem = string.Empty;
                    }
                }
                else {
                    // Se a imagem for nula, define FotoItem como uma string vazia
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
        /// <summary>
        /// Exibe o formulário para editar um item.
        /// </summary>
        /// <param name="id">ID do item.</param>
        /// <returns>View para editar o item.</returns>
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Edit(int? id) {
            // Verifica se o ID é nulo
            if (id == null) {
                // Se o ID for nulo, retorna NotFound
                return NotFound();
            }
            
            // Procura o item pelo ID
            var item = await _context.Itens.FindAsync(id);
            // Verifica se o item foi encontrado
            if (item == null) {
                // Se o item não existir, retorna NotFound
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
        /// <returns>Redireciona para a lista de itens se bem-sucedido, senão retorna a view de edição.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Marca,Descricao,CategoriaId,AttrsJson")] Item item, IFormFile imagem) {
            
            // Verifica se o ID do item no formulário corresponde ao ID passado por parâmetro
            if (id != item.Id) {
                // Se não corresponder, retorna NotFound
                return NotFound();
            }
            
            // Obtem o item original da base de dados
            var itemExistente = await _context.Itens.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
            // Verifica se o item existe
            if (itemExistente == null) {
                // Se o item não existir, retorna NotFound
                return NotFound();
            }
            
            // Remove a validação do campo imagem do ModelState
            ModelState.Remove("imagem");
            
            // validacao json
            try
            {
                // Verifica se o JSON de atributos não está vazio
                if (!string.IsNullOrWhiteSpace(item.AttrsJson))
                {
                    // Tenta analisar o JSON para verificar se é válido
                    using var doc = JsonDocument.Parse(item.AttrsJson); // Exceção se inválido
                    // Serializa o JSON para garantir que não está minificado ("expandido") ou formatado de forma inadequada
                    item.AttrsJson = JsonSerializer.Serialize(doc.RootElement); 
                }
            }
            catch (JsonException)
            {   
                // Adiciona um erro ao ModelState se o JSON não for válido
                ModelState.AddModelError("AttrsJson", "O conteúdo não é um JSON válido.");
            }
            
            // Remove a validação do campo Preco do ModelState
            ModelState.Remove("Preco");
            // Tenta obter o valor do campo Preco do formulário
            var precoStr = Request.Form["Preco"];
            // Tenta converter o valor do campo Preco para decimal
            if (decimal.TryParse(precoStr, NumberStyles.Any, CultureInfo.CurrentCulture, out var precoParsed))
            {
                // Se a conversão for bem-sucedida, atribui o valor ao item
                item.Preco = precoParsed;
            }
            else
            {
                // Se a conversão falhar, adiciona um erro ao ModelState
                ModelState.AddModelError("Preco", "Preço inválido.");
            }
            
            // Verifica se o ModelState é válido
            if (ModelState.IsValid) {
                try {
                    // Verifica se a imagem não é nula e tem tamanho maior que zero
                    if (imagem != null && imagem.Length > 0) {
                        // Tenta guardar a imagem e obter o nome do arquivo
                        var nomeImagem = await GuardarImagemAsync(imagem);
                        // Verifica se o nome da imagem não é nulo
                        if (nomeImagem != null) {
                            // Atribui o nome da imagem ao item
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
                // Captura exceções de concorrência
                catch (DbUpdateConcurrencyException) {
                    // Verifica se o item ainda existe na base de dados
                    if (!ItemExists(item.Id)) {
                        // Se o item não existir, retorna NotFound
                        return NotFound();
                    }
                    else {
                        // Lancça uma execeção
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
        /// <summary>
        /// Exibe a confirmação para eliminar um item.
        /// </summary>
        /// <param name="id">ID do item.</param>
        /// <returns>View de confirmação de eliminação.</returns>
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id) {
            // Verifica se o ID é nulo
            if (id == null) {
                // Se o ID for nulo, retorna NotFound
                return NotFound();
            }
            
            // Procura o item pelo ID, incluindo a categoria associada
            var item = await _context.Itens
                .Include(i => i.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);
            // Verifica se o item foi encontrado
            if (item == null) {
                // Se o item não existir, retorna NotFound
                return NotFound();
            }
            
            // Retorna a View de confirmação de eliminação com o item encontrado
            return View(item);
        }

        // POST: Itens/Delete/5
        /// <summary>
        /// Elimina um item.
        /// </summary>
        /// <param name="id">ID do item.</param>
        /// <returns>Redireciona para a lista de itens.</returns>
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            // Procura o item pelo ID
            var item = await _context.Itens.FindAsync(id);
            // Verifica se o item foi encontrado
            if (item != null) {
                // Se o item existir, remove-o do contexto
                _context.Itens.Remove(item);
            }
            
            // Salva as alterações no contexto
            await _context.SaveChangesAsync();
            
            // Redireciona para a ação Index após a eliminação bem-sucedida
            return RedirectToAction(nameof(Index));
        }
        
        /// <summary>
        /// Verifica se um item existe.
        /// </summary>
        /// <param name="id">ID do item.</param>
        /// <returns>True se existir, false caso contrário.</returns>
        private bool ItemExists(int id) {
            // Verifica se existe algum item com o ID fornecido
            return _context.Itens.Any(e => e.Id == id);
        }
        
        /// <summary>
        /// Guarda uma imagem enviada e retorna o nome do ficheiro.
        /// </summary>
        /// <param name="ficheiro">Arquivo de imagem enviado.</param>
        /// <returns>Nome do ficheiro guardado ou null se inválido.</returns>
        private async Task<string> GuardarImagemAsync(IFormFile ficheiro) {
            if (ficheiro != null && ficheiro.Length > 0) {
                // Obtém a extensão do ficheiro 
                var extensao = Path.GetExtension(ficheiro.FileName).ToLower();
                // Lista de extensões permitidas
                var permitidas = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                
                // Verifica se a extensão é permitida
                if (!permitidas.Contains(extensao)) 
                    // Se não for permitida, retorna null
                    return null;
                
                // Verifica se o tipo de conteúdo começa com "image/"
                if (!ficheiro.ContentType.StartsWith("image/")) 
                    // Se não for uma imagem, retorna null
                    return null;
                
                // Verifica se o tamanho do ficheiro é maior que 5MB
                if (ficheiro.Length > 5 * 1024 * 1024) return null; // Limite de 5MB
                
                // Gera um nome único para o ficheiro usando Guid e combina com a extensão
                var nomeUnico = Guid.NewGuid().ToString() + extensao;
                // Define o caminho onde as imagens serão guardadas
                var pastaUploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "itens_Imagens");
                
                // Verifica se a pasta de uploads existe, se não existir, cria-a
                if (!Directory.Exists(pastaUploads)) {
                    // Cria a pasta de uploads
                    Directory.CreateDirectory(pastaUploads);
                }
                
                // Combina o caminho da pasta de uploads com o nome único do ficheiro
                var caminho = Path.Combine(pastaUploads, nomeUnico);
                
                // Abre um FileStream para escrever o ficheiro na pasta de uploads
                using var stream = new FileStream(caminho, FileMode.Create);
                // Copia o conteúdo do ficheiro enviado para o FileStream
                await ficheiro.CopyToAsync(stream);
                
                // Retorna o nome único do ficheiro guardado
                return nomeUnico;
            }
            
            // Se o ficheiro for nulo ou inválido, retorna null
            return null;
        }
    }
}