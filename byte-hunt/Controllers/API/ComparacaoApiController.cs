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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IEnumerable<Comparacao>>> GetComparacoes()
        {
            // Obtém o nome do utilizador autenticado e verifica se é Admin ou Mod
            var userIdAtualApi = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rolesAtualApi = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            
            // Cria uma consulta para obter as comparações
            var query = _context.Comparacoes
                .Include(c => c.Utilizador)
                .AsQueryable();
            
            // Verifica se o utilizador é Admin ou Mod
            if (rolesAtualApi.Count == 1 && rolesAtualApi.Contains("User"))
            {
                //  Filtra só as contribuições do utilizador atual
                query = query.Where(c => c.Utilizador.Id == userIdAtualApi);
            }
            
            // Executa a consulta e obtém a lista de comparações
            var comparacoes = await query.ToListAsync();
            
            // Mapeia as comparações para a nova estrutura de dados de Comparacao
            var comparacoesDto = comparacoes.Select(c => new ComparacaoDto
            {
                // Mapeia os campos necessários para a nova comparação 
                Id = c.Id,
                data = c.Data,
                Utilizador = new UtilizadorDtoComparacao
                {
                    Id = c.Utilizador.Id,
                    Nome = c.Utilizador.Nome
                }
            }).ToList();
            
            // Retorna a lista de comparações nova
            return Ok(comparacoesDto);
        }

        // GET: api/ComparacaoApi/5
        /// <summary>
        /// Obtém a comparação por id caso tenha sido o utilizador que a realizou se for um utiliazdor autenticado.
        /// Obtém a comparação por id caso seja um Admin ou um Mod.
        /// </summary>
        /// <param name="id">ID da comparação a obter</param>
        /// <returns>Comparação</returns>
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Comparacao>> GetComparacao(int id)
        {
            // Obtém o nome do utilizador autenticado 
            var userIdAtualApi = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rolesAtualApi = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            
            // Busca a comparação pelo ID e inclui o utilizador associado
            var comparacao = await _context.Comparacoes.Include(c => c.Utilizador)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            // Verifica se a comparação existe
            if (comparacao == null)
                return NotFound();
            
            // Verifica se o utilizador é Admin ou Mod
            if (rolesAtualApi.Count == 1 && rolesAtualApi.Contains("User"))
            {
                // Se for um utilizador normal, verifica se a contribuição é do utilizador autenticado
                if (comparacao.Utilizador.Id != userIdAtualApi)
                    // Se não for do utilizador autenticado, nega acesso
                    return Forbid(); 
            }
            
            // Mapeia as comparações para a nova estrutura de dados de Comparacao
            var comparacaoDto = new ComparacaoDto
            {
                // Mapeia os campos necessários para a nova comparação
                Id = comparacao.Id,
                data = comparacao.Data,
                Utilizador = new UtilizadorDtoComparacao
                {
                    Id = comparacao.Utilizador.Id,
                    Nome = comparacao.Utilizador.Nome
                }
            };
            
            // Retorna a nova comparação 
            return Ok(comparacaoDto);
        }

        // PUT: api/ComparacaoApi/5
        /// <summary>
        /// Atualiza a comparação com base no ID.
        /// </summary>
        /// <param name="id">ID da comparação a atualizar</param>
        /// <param name="comparacao">Dados da comparação a atualizar</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator,Moderator")]
        public async Task<IActionResult> PutComparacao(int id, Comparacao comparacao)
        {
            // Verifica se o ID da comparação corresponde ao ID do parâmetro
            if (id != comparacao.Id)
            {
                // Se não corresponder, retorna BadRequest
                return BadRequest("ID da comparação não corresponde ao ID da comparação que quer editar.");
            }
            
            // Diz ao Entity Framework que o estado da entidade foi modificado
            _context.Entry(comparacao).State = EntityState.Modified;    
            
            // Tenta salvar as alterações no contexto
            try
            {
                // Salva as alterações no contexto
                await _context.SaveChangesAsync();
            }
            // Captura a exceção de concorrência se ocorrer
            catch (DbUpdateConcurrencyException)
            {
                // Verifica se a comparação ainda existe
                if (!ComparacaoExists(id))
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

        // POST: api/ComparacaoApi
        /// <summary>
        /// Cria uma nova comparação.
        /// </summary>
        /// <param name="comparacao">Dados da comparação a criar</param>
        /// <returns>Nova comparação</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator,Moderator")]
        public async Task<ActionResult<Comparacao>> PostComparacao(Comparacao comparacao)
        {
            // Adiciona a comparação ao contexto
            _context.Comparacoes.Add(comparacao);
            // Salva as alterações no contexto
            await _context.SaveChangesAsync();
            
            // Mapeia a comparação para a nova estrutura de dados de nova comparação
            var comparacaoDto = new ComparacaoDto
            {
                // Mapeia os campos necessários para a nova comparação
                Id = comparacao.Id,
                data = comparacao.Data,
                Utilizador = new UtilizadorDtoComparacao
                {
                    Id = comparacao.Utilizador.Id,
                    Nome = comparacao.Utilizador.Nome
                }
            };
            
            // Retorna a nova comparação
            return CreatedAtAction("GetComparacao", new { id = comparacao.Id }, comparacaoDto);
        }

        // DELETE: api/ComparacaoApi/5
        /// <summary>
        /// Elimina uma comparação.
        /// </summary>
        /// <param name="id"> ID da comparação a eliminar</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<IActionResult> DeleteComparacao(int id)
        {
            // Procura a comparação pelo ID
            var comparacao = await _context.Comparacoes.FindAsync(id);
            // Verifica se a comparação existe
            if (comparacao == null)
            {
                // Se não existir, retorna NotFound
                return NotFound();
            }
            
            // Remove a comparação do contexto
            _context.Comparacoes.Remove(comparacao);
            // Salva as alterações no contexto
            await _context.SaveChangesAsync();
            
            // Se tudo correr bem, retorna NoContent
            return NoContent();
        }
        
        /// <summary>
        /// Verifica se uma comparação existe com base no ID fornecido.
        /// </summary>
        /// returns>True se existir, caso contrário, false.</returns>
        private bool ComparacaoExists(int id)
        {
            // Verifica se existe uma comparação com o ID fornecido
            return _context.Comparacoes.Any(e => e.Id == id);
        }
    }
}

/// <summary>
/// Classe que permite transformar os dados de uma comparação noutro formato para mostrar
/// Classe que representa a comparação com os dados necessários para mostrar
/// </summary>
public class ComparacaoDto
{
    public int Id { get; set; }
    public DateTime data { get; set; }  
    public UtilizadorDtoComparacao Utilizador { get; set; }
}

/// <summary>
/// Classe que representa o utilizador com os dados necessários para poder associar à comparação sem que existem ciclos infinitos
/// </summary>
public class UtilizadorDtoComparacao
{
    public String Id { get; set; }
    public string Nome { get; set; }
}
