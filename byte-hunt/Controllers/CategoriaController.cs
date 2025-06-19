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
    public class CategoriaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categoria
        /// <summary>
        /// Lista todas as categorias, com suporte a pesquisa e paginação.
        /// </summary>
        /// <param name="searchString">String de pesquisa para filtrar as categorias.</param>
        /// <param name="page">Número da página atual.</param>
        /// <param name="pageSize">Quantidade de categorias por página.</param>
        /// <returns>View com a lista de categorias filtradas e paginadas.</returns>
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Index(string searchString, int page = 1, int pageSize = 10)
        {
            // Query para obter categorias 
            var query = _context.Categorias.AsQueryable();
            
            // Verifica se há uma string de pesquisa e aplica o filtro
            if (!string.IsNullOrEmpty(searchString))
            {
                // Atualiza a query para filtrar categorias pelo nome ou descrição
                query = query.Where(c =>
                    c.Nome.Contains(searchString) ||
                    c.Descricao.Contains(searchString));
            }
            
            // Conta o total de categorias que correspondem à pesquisa
            int totalItems = await query.CountAsync();
            
            // Pagina os resultados
            var categorias = await query
                .OrderBy(c => c.Nome)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            // Prepara os dados para a view
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewData["SearchString"] = searchString;
            
            // Retorna a view com as categorias filtradas e por pagainas
            return View(categorias);
        }

        // GET: Categoria/Details/5
        /// <summary>
        /// Mostra os detalhes de uma categoria específica.
        /// </summary>
        /// <param name="id">ID da categoria a visualizar.</param>
        /// <returns>View com os detalhes da categoria ou NotFound se não existir.</returns>
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Details(int? id)
        {
            // Verifica se o ID é nulo
            if (id == null)
            {
                // Retorna NotFound se o ID for nulo
                return NotFound();
            }
            
            // Busca a categoria pelo ID
            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(m => m.Id == id);
            // Verifica se a categoria foi encontrada
            if (categoria == null)
            {
                // Retorna NotFound se a categoria não existir
                return NotFound();
            }
            
            // Retorna a view com os detalhes da categoria
            return View(categoria);
        }

        // GET: Categoria/Create
        /// <summary>
        /// Exibe o formulário para criar uma nova categoria.
        /// </summary>
        /// <returns>View para criação de uma nova categoria.</returns>
        [Authorize(Roles = "Administrator,Moderator")]
        public IActionResult Create()
        {
            // Retorna a view para criar uma nova categoria 
            return View();
        }

        // POST: Categoria/Create
        /// <summary>
        /// Cria uma nova categoria com os dados fornecidos.
        /// </summary>
        /// <param name="categoria">Objeto categoria a ser criada.</param>
        /// <returns>Redireciona para a lista de categorias se bem-sucedido, senão retorna a view de criação.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Create([Bind("Id,Nome,Descricao")] Categoria categoria)
        {
            // Verifica se o modelo é válido
            if (ModelState.IsValid)
            {
                // Adiciona a nova categoria ao contexto
                _context.Add(categoria);
                // Salva as alterações na base de dados
                await _context.SaveChangesAsync();
                // Redireciona para a lista de categorias
                return RedirectToAction(nameof(Index));
            }
            // Se o modelo não for válido, retorna a view de criação com os dados inseridos
            return View(categoria);
        }

        // GET: Categoria/Edit/5
        /// <summary>
        /// Exibe o formulário para editar uma categoria existente.
        /// </summary>
        /// <param name="id">ID da categoria a editar.</param>
        /// <returns>View para edição da categoria ou NotFound se não existir.</returns>
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Edit(int? id)
        {
            // Verifica se o ID é nulo
            if (id == null)
            {
                // Retorna NotFound se o ID for nulo
                return NotFound();
            }
            
            // Busca a categoria pelo ID
            var categoria = await _context.Categorias.FindAsync(id);
            // Verifica se a categoria foi encontrada
            if (categoria == null)
            {
                // Retorna NotFound se a categoria não existir
                return NotFound();
            }
            // Retorna a view com os dados da categoria para edição
            return View(categoria);
        }

        // POST: Categoria/Edit/5
        /// <summary>
        /// Atualiza uma categoria existente com os dados fornecidos.
        /// </summary>
        /// <param name="id">ID da categoria a atualizar.</param>
        /// <param name="categoria">Objeto categoria com os novos dados.</param>
        /// <returns>Redireciona para a lista de categorias se bem-sucedido, senão retorna a view de edição.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Descricao")] Categoria categoria)
        {
            // Verifica se o ID da categoria corresponde ao ID fornecido
            if (id != categoria.Id)
            {
                // Retorna NotFound se os IDs não corresponderem
                return NotFound();
            }
            
            // Verifica se o modelo é válido
            if (ModelState.IsValid)
            {
                try
                {
                    // Marca a categoria como atualizada no contexto
                    _context.Update(categoria);
                    // Salva as alterações na base de dados
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Trata exceção de concorrência se a categoria não existir mais
                    if (!CategoriaExists(categoria.Id))
                    {
                        // Retorna NotFound se a categoria não existir
                        return NotFound();
                    }
                    else
                    {
                        // Lança uma exeção
                        throw;
                    }
                }
                // Redireciona para a lista de categorias após a atualização bem-sucedida
                return RedirectToAction(nameof(Index));
            }
            // Se o modelo não for válido, retorna a view de edição com os dados inseridos
            return View(categoria);
        }

        // GET: Categoria/Delete/5
        /// <summary>
        /// Exibe o formulário para confirmar a eliminação de uma categoria.
        /// </summary>
        /// <param name="id">ID da categoria a excluir.</param>
        /// <returns>View de confirmação de exclusão ou NotFound se não existir.</returns>
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            // Verifica se o ID é nulo
            if (id == null)
            {
                // Retorna NotFound se o ID for nulo
                return NotFound();
            }
            
            // Busca a categoria pelo ID
            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(m => m.Id == id);
            // Verifica se a categoria foi encontrada
            if (categoria == null)
            {
                // Retorna NotFound se a categoria não existir
                return NotFound();
            }
            // Retorna a view de confirmação de exclusão com os dados da categoria
            return View(categoria);
        }

        // POST: Categoria/Delete/5
        /// <summary>
        /// Remove uma categoria do sistema.
        /// </summary>
        /// <param name="id">ID da categoria a remover.</param>
        /// <returns>Redireciona para a lista de categorias após a exclusão.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Busca a categoria pelo ID
            var categoria = await _context.Categorias.FindAsync(id);
            // Verifica se a categoria foi encontrada
            if (categoria != null)
            {
                // Remove a categoria do contexto
                _context.Categorias.Remove(categoria);
            }
            
            // Salva as alterações na base de dados
            await _context.SaveChangesAsync();
            // Redireciona para a lista de categorias após a exclusão bem-sucedida
            return RedirectToAction(nameof(Index));
        }
        
        /// <summary>
        /// Verifica se uma categoria existe na base de dados.
        /// </summary>
        /// <param name="id">ID da categoria a verificar.</param>
        /// <returns>True se a categoria existe, caso contrário false.</returns>
        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.Id == id);
        }
    }
}
