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
        /// <summary>
        /// Obtém todas as contribuições diponíveis do utilizador que realizou o request.
        /// Obtém todas as contribuições disponíveis caso seja um Admin ou um Mod. 
        /// </summary>
        /// <returns>Contribuições</returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Contribuicao>>> GetContribuicoes()
        {
            var userName = User.Identity.Name; // Obtém o nome do utilizador autenticado
            var isAdmin = User.IsInRole("Administrator");
            var isMod = User.IsInRole("Moderator");

            var query = _context.Contribuicoes.Include(c => c.Utilizador).Include(c => c.Responsavel).AsQueryable();

            if (!isAdmin && !isMod)
            {
                // Se não for admin ou mod, filtra só as contribuições do utilizador atual
                query = query.Where(c => c.Utilizador.Nome == userName);
            }

            var contribuicoes = await query.ToListAsync();

            var contribuicoesDto = contribuicoes.Select(c => new ContribuicaoDto
            {
                Id= c.Id,
                DetalhesContribuicao = c.DetalhesContribuicao,
                DataContribuicao = c.DataContribuicao,
                DataReviewDto = c.DataReview,
                Utilizador = new UtilizadorDtoContribuicao
                {
                    Nome = c.Utilizador != null ? c.Utilizador.Nome : "Sem utilizador"
                },
                Responsavel = new UtilizadorDtoContribuicao
                {
                    Nome = c.Responsavel != null ? c.Responsavel.Nome : "Sem responsável"
                }
            }).ToList();

            return Ok(contribuicoesDto);
        }

        // GET: api/ContribuicaoApi/5
        /// <summary>
        /// Obtém a contribuição por ID do utilizador que realizou o request.
        /// Obtém a contribuição por ID caso seja um Admin ou um Mod. 
        /// </summary>
        /// <returns>Contribuição</returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Contribuicao>> GetContribuicao(int id)
        {
            var userName = User.Identity.Name;

            var contribuicao = await _context.Contribuicoes.Include(c => c.Utilizador).Include(c => c.Responsavel)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contribuicao == null)
                return NotFound();

            var isAdmin = User.IsInRole("Administrator");
            var isMod = User.IsInRole("Moderator");
            
            //verifica se é um user normal autenticado
            if (!isAdmin && !isMod)
            {
                //verifica se a comparacao pertence ao utilizador que fez request 
                if (contribuicao.Utilizador.Nome != userName)
                    return Forbid();  // Se não for do utilizador autenticado, nega acesso
            }

            var contribuicoesDto = new ContribuicaoDto
            {
                Id = contribuicao.Id,
                DetalhesContribuicao = contribuicao.DetalhesContribuicao,
                DataContribuicao = contribuicao.DataContribuicao,
                DataReviewDto = contribuicao.DataReview,
                Utilizador = new UtilizadorDtoContribuicao
                {
                    Id = contribuicao.Utilizador.Id,
                    Nome = contribuicao.Utilizador != null ? contribuicao.Utilizador.Nome : "Sem utilizador"
                },
                Responsavel = new UtilizadorDtoContribuicao
                {
                    Id = contribuicao.Responsavel.Id,
                    Nome = contribuicao.Responsavel != null ? contribuicao.Responsavel.Nome : "Sem responsável"
                }
            };

            return Ok(contribuicoesDto);
        }

        // PUT: api/ContribuicaoApi/5
        /// <summary>
        /// Atualiza a contribuição com base no ID.
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Cria uma nova contribuição.
        /// </summary>
        /// <returns>Nova contribuição</returns>
        [HttpPost]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<ActionResult<Contribuicao>> PostContribuicao(Contribuicao contribuicao)
        {
            _context.Contribuicoes.Add(contribuicao);
            await _context.SaveChangesAsync();
            
            var contribuicoesDto = new ContribuicaoDto
            {
                Id = contribuicao.Id,
                DetalhesContribuicao = contribuicao.DetalhesContribuicao,
                DataContribuicao = contribuicao.DataContribuicao,
                DataReviewDto = contribuicao.DataReview,
                Utilizador = new UtilizadorDtoContribuicao
                {
                    Id = contribuicao.Utilizador.Id,
                    Nome = contribuicao.Utilizador != null ? contribuicao.Utilizador.Nome : "Sem utilizador"
                },
                Responsavel = new UtilizadorDtoContribuicao
                {
                    Id = contribuicao.Responsavel.Id,
                    Nome = contribuicao.Responsavel != null ? contribuicao.Responsavel.Nome : "Sem responsável"
                }
            };

            return CreatedAtAction("GetContribuicao", new { id = contribuicao.Id }, contribuicoesDto);
        }

        // DELETE: api/ContribuicaoApi/5
        /// <summary>
        /// Elimina uma contribuição.
        /// </summary>
        /// <returns></returns>
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

public class ContribuicaoDto
{
    public int Id { get; set; }
    public string DetalhesContribuicao { get; set; }
    public DateTime DataContribuicao { get; set; }
    public DateTime? DataReviewDto { get; set; }
    public UtilizadorDtoContribuicao Utilizador { get; set; }
    public UtilizadorDtoContribuicao Responsavel { get; set; }
}

public class UtilizadorDtoContribuicao
{
    public String Id { get; set; }
    public string Nome { get; set; }
}