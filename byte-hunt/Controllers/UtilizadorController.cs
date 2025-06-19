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
    public class UtilizadorController : Controller
    {
        private readonly UserManager<Utilizador> _userManager;

        public UtilizadorController(UserManager<Utilizador> userManager)
        {
            _userManager = userManager;
        }

        // GET: Utilizador
        /// <summary>
        /// Index que lista todos os utilizadores com paginação e pesquisa.
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns>Lista de Utilizadores</returns>
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Index(string searchString, int page = 1, int pageSize = 10)
        {
            // Query para obter todos os utilizadores
            var query = _userManager.Users.AsQueryable();
            
            // Verifica se há uma string de pesquisa e filtra os utilizadores
            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(u => u.Nome.Contains(searchString) 
                                         || u.UserName.Contains(searchString)
                                         || u.Email.Contains(searchString));
            
            // Contagem total de utilizadores
            var totalCount = await query.CountAsync();
            
            // Paginação: aplica Skip e Take para obter apenas a página atual
            var utilizadoresFinal = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            // Prepara os dados para a View
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewData["SearchString"] = searchString;
            
            // Retorna a View com a lista de utilizadores
            return View(utilizadoresFinal);
        }

        // GET: Utilizador/Details/5
        /// <summary>
        /// Detalhes de um utilizador específico.
        /// </summary>
        /// <param name="id"></param>
        /// <returns> Detalhes de um utilizador por ID</returns>
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Details(string id)
        {
            // Verifica se o ID é nulo ou vazio
            if (id == null)
            {
                // Se o ID for nulo, retorna NotFound
                return NotFound();
            }
            
            // Busca o utilizador pelo ID
            var utilizador = await _userManager.FindByIdAsync(id);
            // Se o utilizador não for encontrado, retorna NotFound
            if (utilizador == null)
            {
                // Se o utilizador não existir, retorna NotFound
                return NotFound();
            }
            
            // Obtém as roles do utilizador
            var roles = await _userManager.GetRolesAsync(utilizador);
            
            // Passar as roles para a ViewData (ou podes criar ViewModel)
            ViewData["UserRoles"] = roles;
            
            // Retorna a View com o utilizador encontrado
            return View(utilizador);
        }
        
        /// <summary>
        /// Utiliza este método para verificar se um utilizador existe com base no ID fornecido.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>True se o utilizador existir, False se não existir</returns>
        private async Task<bool> UtilizadorExists(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            return user != null;
        }
    }
}
