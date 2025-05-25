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
        public async Task<IActionResult> Index()
        {
            return View(await _userManager.Users.ToListAsync());
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

            return View(utilizador);
        }

        private async Task<bool> UtilizadorExists(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            return user != null;
        }
    }
}
