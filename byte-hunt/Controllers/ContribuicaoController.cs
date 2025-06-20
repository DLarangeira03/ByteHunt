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
using Microsoft.AspNetCore.Identity;

namespace byte_hunt.Controllers
{
    public class ContribuicaoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public ContribuicaoController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Contribuicao
        /// <summary>
        /// Lista as contribuições, com filtros e paginação.
        /// </summary>
        /// <param name="dropDownId">ID do utilizador selecionado no filtro (ou null para todos).</param>
        /// <param name="page">Número da página atual.</param>
        /// <param name="pageSize">Número de itens por página.</param>
        /// <returns>View com a lista de contribuições filtrada e paginada.</returns>
        [Authorize]
        public async Task<IActionResult> Index(string? dropDownId, int page = 1, int pageSize = 10) {
            
            // Query para obter as contribuições, incluindo utilizador e responsável
            var query = _context.Contribuicoes
                .Include(c => c.Utilizador)
                .Include(c => c.Responsavel)
                .AsQueryable();
            
            // Se dropDownId não for nulo ou vazio, filtra as contribuições pelo ID do utilizador
            if (!string.IsNullOrEmpty(dropDownId) && dropDownId != "0")
            {
                // Verifica se o ID é válido e filtra as contribuições
                query = query.Where(c => c.UtilizadorId == dropDownId);
            }
            
            // Procura o utilizador atual e verifica se é um moderador ou administrador
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var isMod = _userManager.IsInRoleAsync(user, "Moderator").Result;
            var iaAdmin = _userManager.IsInRoleAsync(user, "Moderator").Result;
            
            // Se o utilizador não for um moderador ou administrador, filtra as contribuições pelo ID do utilizador
            if (!isMod && !iaAdmin) {
                // Se o utilizador não for um moderador ou administrador, filtra as contribuições pelo ID do utilizador
                query = query.Where(c => c.UtilizadorId == user.Id);
            }
            
            // Ordena as contribuições pela data de contribuição em ordem decrescente
            query = query.OrderByDescending(c => c.DataContribuicao);
            
            // Conta o total de contribuições para paginação
            int totalCount = await query.CountAsync();
            // Calcula o número total de páginas
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            // Procura a página atual
            var contribuicoes = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            // Prepara a lista de utilizadores para o dropdown
            var utilizadores = await _context.Users
                .Select(u => new SelectListItem { Value = u.Id, Text = u.Nome })
                .ToListAsync();
            
            // Adiciona a opção "Todos Utilizadores" no início da lista
            utilizadores.Insert(0, new SelectListItem { Value = "0", Text = "Todos Utilizadores" });
            
            // Define os dados da View
            ViewData["Utilizadores"] = utilizadores;
            ViewData["UtilizadorSelecionado"] = dropDownId ?? "0";
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = totalPages;
            
            // Retorna a View com as contribuições filtradas e por páginas
            return View(contribuicoes);
        }

        // GET: Contribuicao/Details/5
        /// <summary>
        /// Mostra os detalhes de uma contribuição.
        /// </summary>
        /// <param name="id">ID da contribuição.</param>
        /// <returns>View com os detalhes da contribuição.</returns>
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            // Verifica se o ID é nulo
            if (id == null) 
                // Se for nulo, retorna NotFound
                return NotFound();
            
            // Busca a contribuição pelo ID, incluindo o utilizador e o responsável
            var contribuicao = await _context.Contribuicoes
                .Include(c => c.Utilizador)
                .Include(c => c.Responsavel)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            // Se a contribuição não for encontrada, retorna NotFound
            if (contribuicao == null) 
                return NotFound();
            
            // Retorna a View com os detalhes da contribuição
            return View(contribuicao);
        }
        
        /// <summary>
        /// Permite a um utilizador reclamar a responsabilidade de uma contribuição.
        /// </summary>
        /// <param name="id">ID da contribuição.</param>
        /// <param name="returnUrl">URL de retorno após a ação (opcional).</param>
        /// <returns>Redireciona para a página anterior ou para o índice.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Claim(int id, string? returnUrl = null)
        {
            // Carrega o utilizador atual
            var user = await _userManager.GetUserAsync(User);
            // Carrega a contribuição pelo ID
            var contrib = await _context.Contribuicoes.FindAsync(id);
            
            // Verifica se a contribuição existe
            if (contrib == null) return 
                // Se não existir, retorna NotFound
                NotFound();
            
            // Atualiza a contribuição com o utilizador atual como responsável
            contrib.ResponsavelId = user.Id;
            // Atualiza a base de dados
            await _context.SaveChangesAsync();
            
            // Verifica se a URL de retorno é válida (local e nao nula/vazia)
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                // Redireciona para a URL de retorno se for válida
                return LocalRedirect(returnUrl);
            
            // Redireciona para a página de Contribuições
            return RedirectToAction("Index"); 
        }

        // GET: Contribuicao/Create
        /// <summary>
        /// Exibe o formulário para criar uma nova contribuição.
        /// </summary>
        /// <returns>View para criar uma contribuição.</returns>
        [Authorize]
        public IActionResult Create()
        {
           // Retorna a View para criar uma nova contribuição
            return View();
        }

        // POST: Contribuicao/Create
        /// <summary>
        /// Cria uma nova contribuição.
        /// </summary>
        /// <param name="form">Dados do formulário submetido.</param>
        /// <returns>Redireciona para o índice ou retorna a view em caso de erro.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            // Retira os dados do formulário
            var nome = form["Nome"];
            var marca = form["Marca"];
            var preco = form["Preco"];
            var descricao = form["Descricao"];
            // Atribui o ID do utilizador atual
            var utilizadorId = _userManager.GetUserId(User);
            
            // Constrói os detalhes da contribuição
            string detalhes = $"Nome: {nome}\nMarca: {marca}\nPreço: {preco}\nDescrição: {descricao}";
            
            // Cria uma nova contribuição com os detalhes e o ID do utilizador
            var contribuicao = new Contribuicao
            {
                DetalhesContribuicao = detalhes,
                DataContribuicao = DateTime.Now,
                Status = "Pendente",
                UtilizadorId = utilizadorId
            };
            
            // Verifica se o modelo é válido
            if (ModelState.IsValid)
            {
                // Adiciona a nova contribuição ao contexto
                _context.Add(contribuicao);
                // Salva as alterações no contexto
                await _context.SaveChangesAsync();
                // Redireciona para a página de Contribuições após a criação
                return RedirectToAction(nameof(Index));
            }
            
            // Se o modelo não for válido, retorna a View com os dados da contribuição
            return View(contribuicao);
        }

        // GET: Contribuicao/Edit/5
        /// <summary>
        /// Exibe o formulário para editar uma contribuição.
        /// </summary>
        /// <param name="id">ID da contribuição.</param>
        /// <returns>View para editar a contribuição.</returns>
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Edit(int? id)
        {
            // Verifica se o ID é nulo
            if (id == null) 
                // Se for nulo, retorna NotFound
                return NotFound();
            
            // Procurar a contribuição pelo ID
            var contribuicao = await _context.Contribuicoes.FindAsync(id);
            // Verifica se a contribuição foi encontrada
            if (contribuicao == null) 
                // Se não for encontrada, retorna NotFound
                return NotFound();
            
            // Dividir os detalhes da contribuição em partes
            var partes = contribuicao.DetalhesContribuicao?.Split("\n");
            // Atribuir os valores das partes às ViewBag para serem usados na view
            ViewBag.Nome = partes?.FirstOrDefault(p => p.StartsWith("Nome: "))?.Replace("Nome: ", "") ?? "";
            ViewBag.Marca = partes?.FirstOrDefault(p => p.StartsWith("Marca: "))?.Replace("Marca: ", "") ?? "";
            ViewBag.Preco = partes?.FirstOrDefault(p => p.StartsWith("Preço: "))?.Replace("Preço: ", "") ?? "";
            ViewBag.Descricao = partes?.FirstOrDefault(p => p.StartsWith("Descrição: "))?.Replace("Descrição: ", "") ?? "";
            
            // Retorna a View com a contribuição para edição
            return View(contribuicao);
        }

        // POST: Contribuicao/Edit/5
        /// <summary>
        /// Edita uma contribuição existente.
        /// </summary>
        /// <param name="id">ID da contribuição.</param>
        /// <param name="form">Dados do formulário submetido.</param>
        /// <returns>Redireciona para o índice ou retorna a view em caso de erro.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Edit(int id, IFormCollection form)
        {
            // Procurar a contribuição pelo ID
            var contribuicao = await _context.Contribuicoes.FindAsync(id);
            // Verifica se a contribuição foi encontrada
            if (contribuicao == null) 
                // Se não for encontrada, retorna NotFound
                return NotFound();
            
            // Retira os dados do formulário
            var nome = form["Nome"];
            var marca = form["Marca"];
            var preco = form["Preco"];
            var descricao = form["Descricao"];
            
            // Atualiza os detalhes da contribuição com os novos valores
            contribuicao.DetalhesContribuicao = $"Nome: {nome}\nMarca: {marca}\nPreço: {preco}\nDescrição: {descricao}";
            // Atualiza a data de edição
            contribuicao.DataEditada = DateTime.Now;
            
            // Verifica se o modelo é válido
            if (ModelState.IsValid)
            {
                // Atualiza a contribuição no contexto
                _context.Update(contribuicao);
                // Salva as alterações no contexto
                await _context.SaveChangesAsync();
                // Redireciona para a página de Contribuições após a edição
                return RedirectToAction(nameof(Index));
            }
            
            // Retorna a View com os dados da contribuição
            return View(contribuicao);
        }

        // GET: Contribuicao/Delete/5
        /// <summary>
        /// Exibe a confirmação para eliminar uma contribuição.
        /// </summary>
        /// <param name="id">ID da contribuição.</param>
        /// <returns>View de confirmação de eliminação.</returns>
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            // Verifica se o ID é nulo
            if (id == null) 
                // Se for nulo, retorna NotFound
                return NotFound();
        
            // Procurar a contribuição pelo ID, incluindo o utilizador e o responsável
            var contribuicao = await _context.Contribuicoes
                .Include(c => c.Utilizador)
                .Include(c => c.Responsavel)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            // Verifica se a contribuição foi encontrada
            if (contribuicao == null) 
                // Se não for encontrada, retorna NotFound
                return NotFound();
            
            // Retorna a View de confirmação de eliminação com os detalhes da contribuição
            return View(contribuicao);
        }

        // POST: Contribuicao/Delete/5
        /// <summary>
        /// Elimina uma contribuição.
        /// </summary>
        /// <param name="id">ID da contribuição.</param>
        /// <returns>Redireciona para o índice.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Procurar a contribuição pelo ID
            var contribuicao = await _context.Contribuicoes.FindAsync(id);
            // Verifica se a contribuição foi encontrada
            if (contribuicao != null)
                // Se for encontrada, remove a contribuição do contexto
                _context.Contribuicoes.Remove(contribuicao);
            
            // Salva as alterações no contexto
            await _context.SaveChangesAsync();
            
            // Redireciona para a página de Contribuições após a eliminação
            return RedirectToAction(nameof(Index));
        }
        
        /// <summary>
        /// Verifica se uma contribuição existe.
        /// </summary>
        /// <param name="id">ID da contribuição.</param>
        /// <returns>True se existir, false caso contrário.</returns>
        private bool ContribuicaoExists(int id)
        {
            // Verifica se existe alguma contribuição com o ID fornecido
            return _context.Contribuicoes.Any(e => e.Id == id);
        }
        
        /// <summary>
        /// Atualiza o estado de uma contribuição (aprovada, rejeitada, etc).
        /// </summary>
        /// <param name="Id">ID da contribuição.</param>
        /// <param name="status">Novo estado da contribuição.</param>
        /// <returns>Redireciona para o índice.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> UpdateStatus(int Id, string status)
        {
            // Procura a contribuição pelo ID
            var contribuicao = await _context.Contribuicoes.FindAsync(Id);
            // Verifica se a contribuição foi encontrada
            if (contribuicao == null) 
                // Se não for encontrada, retorna NotFound
                return NotFound();
            
            // Atribui o novo estado e atualiza a data de revisão
            contribuicao.Status = status;
            contribuicao.DataReview = DateTime.Now;
            // Atribui o ID do utilizador atual como responsável
            _context.Update(contribuicao);
            
            // Verifica se o estado é "Aprovado"
            if (status == "Aprovado") {
                // Cria um novo item a partir da contribuição aprovada
                var newItem = new Item();
                // Divide os detalhes da contribuição em partes
                var detalhesContribuicao = contribuicao.DetalhesContribuicao?.Split("\n");
                // Retira os detalhes necessários da contribuição
                var nome = detalhesContribuicao?.FirstOrDefault(p => p.StartsWith("Nome: "))?.Replace("Nome: ", "") ?? "SEM_NOME";
                var marca = detalhesContribuicao?.FirstOrDefault(p => p.StartsWith("Marca: "))?.Replace("Marca: ", "") ?? "SEM_MARCA";
                var preco = detalhesContribuicao?.FirstOrDefault(p => p.StartsWith("Preço: "))?.Replace("Preço: ", "") ?? "";
                var descricao = detalhesContribuicao?.FirstOrDefault(p => p.StartsWith("Descrição: "))?.Replace("Descrição: ", "") ?? "";
                
                //nome
                newItem.Nome = nome;
                //marca
                newItem.Marca = marca;
                //preco
                var strPreco = preco; 
                newItem.Preco = decimal.Parse(strPreco);
                //desricao
                newItem.Descricao = descricao;
                
                // Procura a categoria "Outros" na base de dados
                var categoriaOutros = await _context.Categorias.FirstOrDefaultAsync(c => c.Nome == "Outros");
                // Verifica se a categoria "Outros" foi encontrada
                if (categoriaOutros != null)
                {
                    // Se a categoria "Outros" existir, atribui o ID da categoria ao novo item
                    newItem.CategoriaId = categoriaOutros.Id;
                }
                else
                {
                    // Cria a categoria "Outros" se não existir
                    categoriaOutros = new Categoria { Nome = "Outros" };
                    // Adiciona a nova categoria ao contexto
                    _context.Categorias.Add(categoriaOutros);
                    // Salva as alterações no contexto para garantir que a categoria é criada
                    await _context.SaveChangesAsync(); 
                    // Atribui o ID da nova categoria "Outros" ao novo item
                    newItem.CategoriaId = categoriaOutros.Id;
                }
                // Adiciona o novo item ao contexto
                _context.Itens.Add(newItem);
            }
            
            // Salva as alterações 
            await _context.SaveChangesAsync();
            
            // Redireciona para a página de Contribuições após a atualização do estado
            return RedirectToAction(nameof(Index));
        }
    }
}