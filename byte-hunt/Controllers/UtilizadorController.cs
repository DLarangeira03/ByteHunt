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
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Index(string searchString, int page = 1, int pageSize = 10)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(u => u.Nome.Contains(searchString) 
                                         || u.UserName.Contains(searchString)
                                         || u.Email.Contains(searchString));

            var totalCount = await query.CountAsync();
            var utilizadoresFinal = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewData["SearchString"] = searchString;

            return View(utilizadoresFinal);
        }

        // GET: Utilizador/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utilizador = await _userManager.FindByIdAsync(id);
            if (utilizador == null)
            {
                return NotFound();
            }
            
            var roles = await _userManager.GetRolesAsync(utilizador);

            // Passar as roles para a ViewData (ou podes criar ViewModel)
            ViewData["UserRoles"] = roles;

            return View(utilizador);
        }

        private async Task<bool> UtilizadorExists(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            return user != null;
        }
    }
}
