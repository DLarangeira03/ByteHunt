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
        public async Task<IActionResult> Index(string? dropDownId, int page = 1, int pageSize = 10) {
            
            var query = _context.Contribuicoes
                .Include(c => c.Utilizador)
                .Include(c => c.Responsavel)
                .AsQueryable();

            if (!string.IsNullOrEmpty(dropDownId) && dropDownId != "0")
            {
                query = query.Where(c => c.UtilizadorId == dropDownId);
            }
            
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var isMod = _userManager.IsInRoleAsync(user, "Moderator").Result;
            var iaAdmin = _userManager.IsInRoleAsync(user, "Moderator").Result;

            if (!isMod && !iaAdmin) {
                query = query.Where(c => c.UtilizadorId == user.Id);
            }

            query = query.OrderByDescending(c => c.DataContribuicao);

            int totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var contribuicoes = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var utilizadores = await _context.Users
                .Select(u => new SelectListItem { Value = u.Id, Text = u.Nome })
                .ToListAsync();

            utilizadores.Insert(0, new SelectListItem { Value = "0", Text = "Todos Utilizadores" });

            ViewData["Utilizadores"] = utilizadores;
            ViewData["UtilizadorSelecionado"] = dropDownId ?? "0";
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;

            return View(contribuicoes);
        }

        // GET: Contribuicao/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var contribuicao = await _context.Contribuicoes
                .Include(c => c.Utilizador)
                .Include(c => c.Responsavel)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contribuicao == null) return NotFound();

            ViewBag.Utilizadores = new SelectList(_context.Users, "Id", "Nome");
            return View(contribuicao);
        }
        
        //atribuicao de responsavel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Claim(int id, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            var contrib = await _context.Contribuicoes.FindAsync(id);
            if (contrib == null) return NotFound();

            contrib.ResponsavelId = user.Id;
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index"); 
        }

        // GET: Contribuicao/Create
        public IActionResult Create()
        {
            ViewData["UtilizadorId"] = new SelectList(_context.Users, "Id", "Nome");
            ViewData["ResponsavelId"] = new SelectList(_context.Users, "Id", "Nome");
            return View();
        }

        // POST: Contribuicao/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            var nome = form["Nome"];
            var marca = form["Marca"];
            var preco = form["Preco"];
            var descricao = form["Descricao"];
            var utilizadorId = _userManager.GetUserId(User);

            string detalhes = $"Nome: {nome}\nMarca: {marca}\nPreço: {preco}\nDescrição: {descricao}";

            var contribuicao = new Contribuicao
            {
                DetalhesContribuicao = detalhes,
                DataContribuicao = DateTime.Now,
                Status = "Pending",
                UtilizadorId = utilizadorId
            };

            if (ModelState.IsValid)
            {
                _context.Add(contribuicao);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.UtilizadorId = new SelectList(_context.Users, "Id", "Nome", contribuicao.UtilizadorId);
            return View(contribuicao);
        }

        // GET: Contribuicao/Edit/5
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var contribuicao = await _context.Contribuicoes.FindAsync(id);
            if (contribuicao == null) return NotFound();

            var partes = contribuicao.DetalhesContribuicao?.Split("\n");
            ViewBag.Nome = partes?.FirstOrDefault(p => p.StartsWith("Nome: "))?.Replace("Nome: ", "") ?? "";
            ViewBag.Marca = partes?.FirstOrDefault(p => p.StartsWith("Marca: "))?.Replace("Marca: ", "") ?? "";
            ViewBag.Preco = partes?.FirstOrDefault(p => p.StartsWith("Preço: "))?.Replace("Preço: ", "") ?? "";
            ViewBag.Descricao = partes?.FirstOrDefault(p => p.StartsWith("Descrição: "))?.Replace("Descrição: ", "") ?? "";

            ViewData["UtilizadorId"] = new SelectList(_context.Users, "Id", "Nome", contribuicao.UtilizadorId);
            ViewData["ResponsavelId"] = new SelectList(_context.Users, "Id", "Nome", contribuicao.ResponsavelId);

            return View(contribuicao);
        }

        // POST: Contribuicao/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormCollection form)
        {
            var contribuicao = await _context.Contribuicoes.FindAsync(id);
            if (contribuicao == null) return NotFound();

            var nome = form["Nome"];
            var marca = form["Marca"];
            var preco = form["Preco"];
            var descricao = form["Descricao"];

            contribuicao.DetalhesContribuicao = $"Nome: {nome}\nMarca: {marca}\nPreço: {preco}\nDescrição: {descricao}";
            contribuicao.DataEditada = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Update(contribuicao);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(contribuicao);
        }

        // GET: Contribuicao/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var contribuicao = await _context.Contribuicoes
                .Include(c => c.Utilizador)
                .Include(c => c.Responsavel)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contribuicao == null) return NotFound();

            return View(contribuicao);
        }

        // POST: Contribuicao/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contribuicao = await _context.Contribuicoes.FindAsync(id);
            if (contribuicao != null)
                _context.Contribuicoes.Remove(contribuicao);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContribuicaoExists(int id)
        {
            return _context.Contribuicoes.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int Id, string status)
        {
            var contribuicao = await _context.Contribuicoes.FindAsync(Id);
            if (contribuicao == null) return NotFound();

            contribuicao.Status = status;
            contribuicao.DataReview = DateTime.Now;
            _context.Update(contribuicao);
            
            //se aprovada, adicionar o item
            if (status == "Approved") {
                //criar item
                var newItem = new Item();
                Console.WriteLine("===================================================");
                    Console.WriteLine("|" + contribuicao.DetalhesContribuicao + "|");
                Console.WriteLine("===================================================");
                //adicionar ao context
                var detalhesContribuicao = contribuicao.DetalhesContribuicao?.Split("\n");
                Console.WriteLine("===================================================");
                foreach (var k in detalhesContribuicao) {
                    Console.WriteLine("|" + k + "|");
                }
                Console.WriteLine("===================================================");
                var nome = detalhesContribuicao?.FirstOrDefault(p => p.StartsWith("Nome: "))?.Replace("Nome: ", "") ?? "SEM_NOME";
                var marca = detalhesContribuicao?.FirstOrDefault(p => p.StartsWith("Marca: "))?.Replace("Marca: ", "") ?? "SEM_MARCA";
                var preco = detalhesContribuicao?.FirstOrDefault(p => p.StartsWith("Preço: "))?.Replace("Preço: ", "") ?? "";
                var descricao = detalhesContribuicao?.FirstOrDefault(p => p.StartsWith("Descrição: "))?.Replace("Descrição: ", "") ?? "";
                Console.WriteLine("===================================================");
                Console.WriteLine("Nome: |" + nome +"|");
                Console.WriteLine("Marca: |" + marca +"|");
                Console.WriteLine("Preco: |" + preco +"|");
                Console.WriteLine("Descricao: |" + descricao +"|");
                Console.WriteLine("===================================================");
                
                //nome
                newItem.Nome = nome;
                //marca
                newItem.Marca = marca;
                //preco
                var strPreco = preco; 
                newItem.Preco = decimal.Parse(strPreco);
                //desricao
                newItem.Descricao = descricao;
                
                //add a categoria "Outros" (criar se nao existir)
                var categoriaOutros = await _context.Categorias.FirstOrDefaultAsync(c => c.Nome == "Outros");
                if (categoriaOutros != null)
                {
                    newItem.CategoriaId = categoriaOutros.Id;
                }
                else
                {
                    // criar categoria "Outros" se não existir
                    categoriaOutros = new Categoria { Nome = "Outros" };
                    _context.Categorias.Add(categoriaOutros);
                    await _context.SaveChangesAsync(); 
                    newItem.CategoriaId = categoriaOutros.Id;
                }
                _context.Itens.Add(newItem);
            }
            
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}


// if (User.IsInRole("admin"))
// {
//     
// }
// else
// {
//     query = query.Where(c => c.UtilizadorId == utilizadorId.Value);    
// }