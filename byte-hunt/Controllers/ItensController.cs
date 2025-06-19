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

namespace byte_hunt.Controllers
{
    public class ItensController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ItensController(ApplicationDbContext context)
        {
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
            int pageSize = 9)
        {
            // Query para obter os itens, incluindo a categoria associada
            var query = _context.Itens.Include(i => i.Categoria).AsQueryable();
            
            // Verifica se a string de pesquisa não é nula ou vazia
            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Converte a string de pesquisa para minúsculas para comparação case-insensitive
                searchTerm = searchTerm.ToLower();
                // Filtra os itens com base na string de pesquisa
                query = query.Where(i =>
                    i.Nome.ToLower().Contains(searchTerm) ||
                    i.Marca.ToLower().Contains(searchTerm) ||
                    i.Categoria.Nome.ToLower().Contains(searchTerm));
            }
            
            // Verifica se o ID da categoria é fornecido e maior que zero
            if (categoriaId.HasValue && categoriaId.Value > 0)
            {
                // Filtra os itens pela categoria selecionada
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
        /// Mostra os detalhes de um item específico.
        /// </summary>
        /// <param name="id">ID do item a visualizar.</param>
        /// <returns>View com os detalhes do item.</returns>
        public async Task<IActionResult> Details(int? id)
        {
            // Verifica se o ID é nulo
            if (id == null)
            {
                // Retorna NotFound se o ID for nulo
                return NotFound();
            }
            
            // Procurar o item pelo ID, incluindo a categoria associada
            var item = await _context.Itens
                .Include(i => i.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            // Verifica se o item foi encontrado
            if (item == null)
            {
                // Retorna NotFound se o item não for encontrado
                return NotFound();
            }
            
            // Retorna a View com os detalhes do item
            return View(item);
        }

        // GET: Itens/Create
        /// <summary>
        /// Exibe o formulário para criar um novo item.
        /// </summary>
        /// <returns>View para criação de um novo item.</returns>
        public IActionResult Create()
        {
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
        public async Task<IActionResult> Create([Bind("Id,Nome,Marca,Descricao,Preco,CategoriaId")] Item item,
            IFormFile imagem)
        {
            // Remove a validação do campo "imagem" do ModelState para evitar erros de validação
            ModelState.Remove("imagem");
            
            // Verifica se o ModelState é válido
            if (ModelState.IsValid)
            {
                // Se a imagem não for nula, tenta guardar a imagem e atribui o nome à mesma
                if (imagem != null)
                {
                    // Chama o método GuardarImagemAsync para salvar a imagem e obter o nome do arquivo
                    var nomeImagem = await GuardarImagemAsync(imagem);
                    // Se o nome da imagem não for nulo, atribui ao item, caso contrário, define como string vazia
                    if (nomeImagem != null)
                    {
                        // Atribui ao atribuito FotoItem do item o nome da imagem
                        item.FotoItem = nomeImagem;
                    }
                    else
                    {
                        // Se o nomeImagem for nulo, define FotoItem como string vazia
                        item.FotoItem = string.Empty;
                    }
                }
                else
                {
                    // Se a imagem for nula, define FotoItem como string vazia
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
        /// Exibe o formulário para editar um item existente.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>View que permite editar um item já existente, através do seu ID</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            // Verifica se o ID é nulo
            if (id == null)
            {
                // Retorna NotFound se o ID for nulo
                return NotFound();
            }
            
            // Procura o item pelo ID
            var item = await _context.Itens.FindAsync(id);
            // Verifica se o item foi encontrado
            if (item == null)
            {
                // Retorna NotFound se o item não for encontrado
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Marca,Descricao,Preco,CategoriaId")] Item item, IFormFile imagem)
        {
            // Verifica se o ID do item no formulário corresponde ao ID do item na URL
            if (id != item.Id)
            {
                // Se não corresponder, retorna NotFound
                return NotFound();
            }
            
            // Obtem o item original da base de dados
            var itemExistente = await _context.Itens.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
            // Verifica se o item original existe
            if (itemExistente == null)
            {
                // Se o item original não existir, retorna NotFound
                return NotFound();
            }
            
            // Remove a validação do campo "imagem" do ModelState para evitar erros de validação
            ModelState.Remove("imagem");
            
            // Verifica se o ModelState é válido
            if (ModelState.IsValid)
            {
                try
                {
                    // Atualiza os dados do item com os valores do formulário
                    // Verfica se a imagem não é nula e se tem tamanho maior que zero
                    if (imagem != null && imagem.Length > 0)
                    {
                        // Chama o método GuardarImagemAsync para salvar a imagem e obter o nome do arquivo
                        var nomeImagem = await GuardarImagemAsync(imagem);
                        // Se o nome da imagem não for nulo, atribui ao item, caso contrário, mantém a imagem anterior
                        if (nomeImagem != null)
                        {
                            // Atribui ao atribuito FotoItem do item o nome da imagem
                            item.FotoItem = nomeImagem;
                        }
                    }
                    else
                    {
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
                // Captura exceções de concorrência ao atualizar o item
                catch (DbUpdateConcurrencyException)
                {
                    // Verifica se o item ainda existe na base de dados
                    if (!ItemExists(item.Id))
                    {
                        // Se o item não existir, retorna NotFound
                        return NotFound();
                    }
                    else
                    {
                        // Se o item existir, lança uma exceção 
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
        /// Apresenta a página de confirmação de exclusão de um item.
        /// </summary>
        /// <param name="id"></param>
        /// <returns> View com os dados do item identificado pronto a eliminar</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            // Verifica se o ID é nulo
            if (id == null)
            {
                // Retorna NotFound se o ID for nulo
                return NotFound();
            }
            
            // Procura o item pelo ID, incluindo a categoria associada
            var item = await _context.Itens
                .Include(i => i.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);
            // Verifica se o item foi encontrado
            if (item == null)
            {
                // Retorna NotFound se o item não for encontrado
                return NotFound();
            }
            
            // Retorna a View de exclusão com o item encontrado
            return View(item);
        }

        // POST: Itens/Delete/5
        /// <summary>
        /// Remove um item do sistema.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Redireciona para a lista de itens após a exclusão.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Procura o item pelo ID
            var item = await _context.Itens.FindAsync(id);
            // Verifica se o item foi encontrado
            if (item != null)
            {
                // Se o item foi encontrado, remove-o do contexto
                _context.Itens.Remove(item);
            }
            
            // Salva as alterações no contexto
            await _context.SaveChangesAsync();
            // Redireciona para a ação Index após a exclusão bem-sucedida
            return RedirectToAction(nameof(Index));
        }
        
        /// <summary>
        /// Verifica se um item existe na base de dados.
        /// </summary>
        /// <param name="id">ID do item a verificar.</param>
        /// <returns>True se o item existe, caso contrário false.</returns>
        private bool ItemExists(int id)
        {
            return _context.Itens.Any(e => e.Id == id);
        }
        
        /// <summary>
        /// Guarda a imagem enviada no servidor.
        /// </summary>
        /// <param name="ficheiro">Arquivo de imagem a ser guardado.</param>
        /// <returns>Nome do ficheiro guardado ou null se falhar.</returns>
        private async Task<string> GuardarImagemAsync(IFormFile ficheiro)
        {
            if (ficheiro != null && ficheiro.Length > 0)
            {
                var extensao = Path.GetExtension(ficheiro.FileName).ToLower();
                var permitidas = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                if (!permitidas.Contains(extensao)) return null;

                if (!ficheiro.ContentType.StartsWith("image/")) return null;

                if (ficheiro.Length > 5 * 1024 * 1024) return null; // Limite de 5MB

                var nomeUnico = Guid.NewGuid().ToString() + extensao;
                var pastaUploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "itens_Imagens");

                if (!Directory.Exists(pastaUploads))
                {
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