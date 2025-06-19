using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using byte_hunt.Data;
using byte_hunt.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IEnumerable<Contribuicao>>> GetContribuicoes()
        {
            // Obtém o nome do utilizador autenticado e verifica se é Admin ou Mod
            var userIdAtualApi = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rolesAtualApi = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            
            // Cria a query para obter as contribuições, incluindo os utilizadores e responsáveis
            var query = _context.Contribuicoes.Include(c => c.Utilizador)
                .Include(c => c.Responsavel).AsQueryable();
            
            // Verifica se o utilizador é Admin ou Mod
            if (rolesAtualApi.Count == 1 && rolesAtualApi.Contains("User"))
            {
                //  Filtra só as contribuições do utilizador atual
                query = query.Where(c => c.Utilizador.Id == userIdAtualApi);
            }
            
            // Converte a query para uma lista de contribuições
            var contribuicoes = await query.ToListAsync();
            
            // Mapeia as contribuições para a nova estrutura Contribuição
            var contribuicoesDto = contribuicoes.Select(c => new ContribuicaoDto
            {
                // Mapeia os campos necessários para a nova Contribuição
                Id= c.Id,
                DetalhesContribuicao = c.DetalhesContribuicao,
                DataContribuicao = c.DataContribuicao,
                DataReviewDto = c.DataReview,
                Utilizador = c.Utilizador != null
                    ? new UtilizadorDtoContribuicao
                    {
                        Id = c.Utilizador.Id,
                        Nome = c.Utilizador.Nome
                    }
                    : new UtilizadorDtoContribuicao
                    {
                        Id = "0", 
                        Nome = "Sem utilizador"
                    },

                Responsavel = c.Responsavel != null
                    ? new UtilizadorDtoContribuicao
                    {
                        Id = c.Responsavel.Id,
                        Nome = c.Responsavel.Nome
                    }
                    : new UtilizadorDtoContribuicao
                    {
                        Id = "0", 
                        Nome = "Sem responsável"
                    }
            }).ToList();
            
            // Retorna a lista de contribuições 
            return Ok(contribuicoesDto);
        }

        // GET: api/ContribuicaoApi/5
        /// <summary>
        /// Obtém a contribuição por ID do utilizador que realizou o request.
        /// Obtém a contribuição por ID caso seja um Admin ou um Mod. 
        /// </summary>
        /// <param name="id">ID da contribuição</param>
        /// <returns>Contribuição</returns>
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Contribuicao>> GetContribuicao(int id)
        {
            // Obtém o nome do utilizador autenticado 
            var userIdAtualApi = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rolesAtualApi = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            
            // Busca a contribuição pelo ID, incluindo os utilizadores e responsáveis
            var contribuicao = await _context.Contribuicoes.Include(c => c.Utilizador).Include(c => c.Responsavel)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            // Verifica se a contribuição foi encontrada
            if (contribuicao == null)
                // Se não encontrar a contribuição, retorna NotFound
                return NotFound();
            
            // Verifica se o utilizador é Admin ou Mod
            if (rolesAtualApi.Count == 1 && rolesAtualApi.Contains("User"))
            {
                // Se for um utilizador normal, verifica se a contribuição é do utilizador autenticado
                if (contribuicao.Utilizador.Id != userIdAtualApi)
                    // Se não for do utilizador autenticado, nega acesso
                    return Forbid(); 
            }
            // Mapeia a contribuição para a nova estrutura ContribuiçãoDto
            var contribuicoesDto = new ContribuicaoDto
            {
                // Mapeia os campos necessários para a nova ContribuiçãoDto
                Id = contribuicao.Id,
                DetalhesContribuicao = contribuicao.DetalhesContribuicao,
                DataContribuicao = contribuicao.DataContribuicao,
                DataReviewDto = contribuicao.DataReview,
                Utilizador = contribuicao.Utilizador != null
                    ? new UtilizadorDtoContribuicao
                    {
                        Id = contribuicao.Utilizador.Id,
                        Nome = contribuicao.Utilizador.Nome
                    }
                    : new UtilizadorDtoContribuicao
                    {
                        Id = "0", 
                        Nome = "Sem utilizador"
                    },

                Responsavel = contribuicao.Responsavel != null
                    ? new UtilizadorDtoContribuicao
                    {
                        Id = contribuicao.Responsavel.Id,
                        Nome = contribuicao.Responsavel.Nome
                    }
                    : new UtilizadorDtoContribuicao
                    {
                        Id = "0", 
                        Nome = "Sem responsável"
                    }
            };
            
            // Retorna a contribuição mapeada
            return Ok(contribuicoesDto);
        }

        // PUT: api/ContribuicaoApi/5
        /// <summary>
        /// Atualiza a contribuição com base no ID.
        /// </summary>
        /// <param name="id">ID da contribuição</param>
        /// <param name="contribuicao">Contribuição a ser atualizada</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator,Moderator")]
        public async Task<IActionResult> PutContribuicao(int id, Contribuicao contribuicao)
        {
            // Verifica se o ID da contribuição fornecido corresponde ao ID do objeto contribuicao
            if (id != contribuicao.Id)
            {
                // Se não corresponder, retorna BadRequest
                return BadRequest("O ID fornecido não corresponde ao ID da contribuição.");
            }
            
            // Notifica o contexto que a entidade contribuicao foi modificada
            _context.Entry(contribuicao).State = EntityState.Modified;
            
            // Tenta salvar as alterações no contexto
            try
            {
                //Tenta salvar as alterações na base de dados
                await _context.SaveChangesAsync();
            }
            // Captura exceção de concorrência se ocorrer
            catch (DbUpdateConcurrencyException)
            {
                // Verifica se a contribuição ainda existe
                if (!ContribuicaoExists(id))
                {
                    // Se não existir, retorna NotFound
                    return NotFound();
                }
                else
                {
                    // Se existir, lança uma exceção
                    throw;
                }
            }
            
            // Se tudo correr bem, retorna NoContent
            return NoContent();
        }

        // POST: api/ContribuicaoApi
        /// <summary>
        /// Cria uma nova contribuição.
        /// </summary>
        /// <param name="contribuicao">Contribuição a ser criada</param>
        /// <returns>Nova contribuição</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator,Moderator")]
        public async Task<ActionResult<Contribuicao>> PostContribuicao(Contribuicao contribuicao)
        {
            // Adiciona a nova contribuição ao contexto
            _context.Contribuicoes.Add(contribuicao);
            // Salva as alterações no contexto
            await _context.SaveChangesAsync();
            
            // Cria uma nova estrutura Contribuição para ser retornada
            var contribuicoesDto = new ContribuicaoDto
            {
                // Mapeia os campos necessários para a nova Contribuição
                Id = contribuicao.Id,
                DetalhesContribuicao = contribuicao.DetalhesContribuicao,
                DataContribuicao = contribuicao.DataContribuicao,
                DataReviewDto = contribuicao.DataReview,
                Utilizador = contribuicao.Utilizador != null
                    ? new UtilizadorDtoContribuicao
                    {
                        Id = contribuicao.Utilizador.Id,
                        Nome = contribuicao.Utilizador.Nome
                    }
                    : new UtilizadorDtoContribuicao
                    {
                        Id = "0", 
                        Nome = "Sem utilizador"
                    },

                Responsavel = contribuicao.Responsavel != null
                    ? new UtilizadorDtoContribuicao
                    {
                        Id = contribuicao.Responsavel.Id,
                        Nome = contribuicao.Responsavel.Nome
                    }
                    : new UtilizadorDtoContribuicao
                    {
                        Id = "0", 
                        Nome = "Sem responsável"
                    }
            };
            
            // Retorna a nova contribuição criada 
            return CreatedAtAction("GetContribuicao", new { id = contribuicao.Id }, contribuicoesDto);
        }

        // DELETE: api/ContribuicaoApi/5
        /// <summary>
        /// Elimina uma contribuição.
        /// </summary>
        /// <param name="id">ID da contribuição a ser eliminada</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<IActionResult> DeleteContribuicao(int id)
        {
            // Busca a contribuição pelo ID
            var contribuicao = await _context.Contribuicoes.FindAsync(id);
            // Verifica se a contribuição foi encontrada
            if (contribuicao == null)
            {
                // Se não encontrar a contribuição, retorna NotFound
                return NotFound();
            }
            
            // Remove a contribuição do contexto
            _context.Contribuicoes.Remove(contribuicao);
            // Salva as alterações no contexto
            await _context.SaveChangesAsync();
            
            // Retorna NoContent se a exclusão for bem-sucedida
            return NoContent();
        }
        
        /// <summary>
        /// Verifica se uma contribuição existe com base no ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool ContribuicaoExists(int id)
        {
            return _context.Contribuicoes.Any(e => e.Id == id);
        }
    }
}

/// <summary>
/// Classe que representa a estrutura de dados de uma contribuição.
/// </summary>
public class ContribuicaoDto
{
    public int Id { get; set; }
    public string DetalhesContribuicao { get; set; }
    public DateTime DataContribuicao { get; set; }
    public DateTime? DataReviewDto { get; set; }
    public UtilizadorDtoContribuicao Utilizador { get; set; }
    public UtilizadorDtoContribuicao Responsavel { get; set; }
}

/// <summary>
/// Classe que representa a estrutura de dados de um utilizador associado a uma contribuição.
/// </summary>
public class UtilizadorDtoContribuicao
{
    public String Id { get; set; }
    public string Nome { get; set; }
}