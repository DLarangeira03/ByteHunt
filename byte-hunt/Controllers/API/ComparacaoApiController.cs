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
    public class ComparacaoApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ComparacaoApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ComparacaoApi
        /// <summary>
        /// Obtém todas as comparações disponíveis para o utilizador que realizou o request.
        /// Obtém todas as comparação disponíveis caso seja um Admin ou um Mod.
        /// </summary>
        /// <returns>Lista de comparações</returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Comparacao>>> GetComparacoes()
        {
            var userName = User.Identity.Name;
            var isAdmin = User.IsInRole("Administrator");
            var isMod = User.IsInRole("Moderator");

            var query = _context.Comparacoes
                .Include(c => c.Utilizador)
                .AsQueryable();

            if (!isAdmin && !isMod)
            {
                query = query.Where(c => c.Utilizador.Nome == userName);
            }

            var comparacoes = await query.ToListAsync();

            var comparacoesDto = comparacoes.Select(c => new ComparacaoDto
            {
                Id = c.Id,
                data = c.Data,
                Utilizador = new UtilizadorDtoComparacao
                {
                    Id = c.Utilizador.Id,
                    Nome = c.Utilizador.Nome
                }
            }).ToList();

            return Ok(comparacoesDto);
        }

        // GET: api/ComparacaoApi/5
        /// <summary>
        /// Obtém a comparação por id caso tenha sido o utilizador que a realizou se for um utiliazdor autenticado.
        /// Obtém a comparação por id caso seja um Admin ou um Mod.
        /// </summary>
        /// <returns>Comparação</returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Comparacao>> GetComparacao(int id)
        {
            
            var userName = User.Identity.Name;

            var comparacao = await _context.Comparacoes.Include(c => c.Utilizador)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comparacao == null)
                return NotFound();
            
            var isAdmin = User.IsInRole("Administrator");
            var isMod = User.IsInRole("Moderator");
            
            //verifica se é um user normal autenticado
            if (!isAdmin && !isMod)
            {
                //verifica se a comparacao pertence ao utilizador que fez request 
                if (comparacao.Utilizador.Nome != userName)
                    return Forbid();  // Se não for do utilizador autenticado, nega acesso
            }
            
            var comparacaoDto = new ComparacaoDto
            {
                Id = comparacao.Id,
                data = comparacao.Data,
                Utilizador = new UtilizadorDtoComparacao
                {
                    Id = comparacao.Utilizador.Id,
                    Nome = comparacao.Utilizador.Nome
                }
            };

            return Ok(comparacaoDto);
        }

        // PUT: api/ComparacaoApi/5
        /// <summary>
        /// Atualiza a comparação com base no ID.
        /// </summary>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> PutComparacao(int id, Comparacao comparacao)
        {
            if (id != comparacao.Id)
            {
                return BadRequest();
            }

            _context.Entry(comparacao).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ComparacaoExists(id))
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

        // POST: api/ComparacaoApi
        /// <summary>
        /// Cria uma nova comparação.
        /// </summary>
        /// <returns>Nova comparação</returns>
        [HttpPost]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<ActionResult<Comparacao>> PostComparacao(Comparacao comparacao)
        {
            _context.Comparacoes.Add(comparacao);
            await _context.SaveChangesAsync();
            
            var comparacaoDto = new ComparacaoDto
            {
                Id = comparacao.Id,
                data = comparacao.Data,
                Utilizador = new UtilizadorDtoComparacao
                {
                    Id = comparacao.Utilizador.Id,
                    Nome = comparacao.Utilizador.Nome
                }
            };

            return CreatedAtAction("GetComparacao", new { id = comparacao.Id }, comparacaoDto);
        }

        // DELETE: api/ComparacaoApi/5
        /// <summary>
        /// Elimina uma comparação.
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteComparacao(int id)
        {
            var comparacao = await _context.Comparacoes.FindAsync(id);
            if (comparacao == null)
            {
                return NotFound();
            }

            _context.Comparacoes.Remove(comparacao);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ComparacaoExists(int id)
        {
            return _context.Comparacoes.Any(e => e.Id == id);
        }
    }
}

public class ComparacaoDto
{
    public int Id { get; set; }
    public DateTime data { get; set; }  
    public UtilizadorDtoComparacao Utilizador { get; set; }
}
public class UtilizadorDtoComparacao
{
    public String Id { get; set; }
    public string Nome { get; set; }
}
