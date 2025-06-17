using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using byte_hunt.Data;
using byte_hunt.Models;
using Microsoft.AspNetCore.Authorization;

namespace byte_hunt.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContribuicaoApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContribuicaoApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ContribuicaoApi
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Contribuicao>>> GetContribuicoes()
        {
            var userName = User.Identity.Name; // Obtém o nome do utilizador autenticado

            var contribsDoUser = await _context.Contribuicoes
                .Where(c => c.Utilizador.Nome == userName)  // Filtra só as contribuições do utilizador atual
                .ToListAsync();

            return Ok(contribsDoUser);
        }

        // GET: api/ContribuicaoApi/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Contribuicao>> GetContribuicao(int id)
        {
            var userName = User.Identity.Name;

            var contribuicao = await _context.Contribuicoes.FindAsync(id);

            if (contribuicao == null)
                return NotFound();

            if (contribuicao.Utilizador.Nome != userName)
                return Forbid();  // Se não for do utilizador autenticado, nega acesso

            return Ok(contribuicao);
        }

        // PUT: api/ContribuicaoApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> PutContribuicao(int id, Contribuicao contribuicao)
        {
            if (id != contribuicao.Id)
            {
                return BadRequest();
            }

            _context.Entry(contribuicao).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContribuicaoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ContribuicaoApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<ActionResult<Contribuicao>> PostContribuicao(Contribuicao contribuicao)
        {
            _context.Contribuicoes.Add(contribuicao);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetContribuicao", new { id = contribuicao.Id }, contribuicao);
        }

        // DELETE: api/ContribuicaoApi/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteContribuicao(int id)
        {
            var contribuicao = await _context.Contribuicoes.FindAsync(id);
            if (contribuicao == null)
            {
                return NotFound();
            }

            _context.Contribuicoes.Remove(contribuicao);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ContribuicaoExists(int id)
        {
            return _context.Contribuicoes.Any(e => e.Id == id);
        }
    }
}
