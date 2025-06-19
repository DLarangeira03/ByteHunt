using System;
using System.Collections.Generic;
using System.Linq;
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
    public class CategoriaApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriaApiController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        // GET: api/CategoriaApi
        /// <summary>
        /// Obtém o nome e a descrição de cada categoria.
        /// Apenas utilizadores não autenticados
        /// </summary>
        /// <returns>Categoria com nome e descrição</returns>
        [HttpGet("publico")]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategoriasPublic()
        {  
            // Obtém apenas o nome e a descrição de cada categoria para utilizadores não autenticados
            var categoriasPublicas = await _context.Categorias
                .Select(c => new CategoriaPublicDTO
                {
                    Nome = c.Nome,
                    Descricao = c.Descricao
                })
                .ToListAsync();
            
            // Retorna uma lista de categorias públicas
            return Ok(categoriasPublicas);
        }
        
        // GET: api/CategoriaApi
        /// <summary>
        /// Obtém uma lista de todas as categorias com todas as informações.
        /// Apenas utiliazdores autenticados têm acesso a este endpoint.
        /// </summary>
        /// <returns>Lista de Categorias</returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
        {
            // Retorna todas as categorias com todas as informações para utilizadores autenticados
            return await _context.Categorias.ToListAsync();
        }

        // GET: api/CategoriaApi/5
        /// <summary>
        /// Obtém uma categoria filtrada por ID.
        /// Apenas utiliazdores autenticados têm acesso a este endpoint.
        /// </summary>
        /// <param name="id">ID da categoria</param>
        /// <returns>Categoria</returns>
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Categoria>> GetCategoria(int id)
        {
            // Procura a categoria pelo ID fornecido
            var categoria = await _context.Categorias.FindAsync(id);
            
            //Verifica se a categoria existe
            if (categoria == null)
            {
                // Se não existir, retorna Not Found
                return NotFound();
            }
            
            // Retorna a categoria encontrada
            return categoria;
        }

        // PUT: api/CategoriaApi/5
        /// <summary>
        /// Edita uma categoria com base no seu ID.
        /// Apenas utiliazdores autenticados e com estatuto de Admin ou Mod.
        /// </summary>
        /// <param name="id">ID da categoria</param>
        /// <param name="categoria">Categoria com as novas informações</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator,Moderator")]
        public async Task<IActionResult> PutCategoria(int id, Categoria categoria)
        {
            // Verifica se o ID fornecido corresponde ao ID da categoria
            if (id != categoria.Id)
            {
                // Se não corresponder, retorna Bad Request
                return BadRequest("O ID da categoria não corresponde ao ID fornecido.");
            }
            
            // Identifica a categoria no contexto e marca como modificada
            _context.Entry(categoria).State = EntityState.Modified;
            
            // Tenta salvar as alterações no contexto
            try
            {
                // Salva as alterações na base de dados
                await _context.SaveChangesAsync();
            }
            // Captura exceções de concorrência durante a atualização
            catch (DbUpdateConcurrencyException)
            {
                // Verifica se a categoria ainda existe
                if (!CategoriaExists(id))
                {
                    // Se não existir, retorna Not Found
                    return NotFound();
                }
                else
                {
                    // Se existir, lança uma exceção
                    throw;
                }
            }
            
            // Se tudo correr bem, retorna No Content
            return NoContent();
        }

        // POST: api/CategoriaApi
        /// <summary>
        /// Cria uma nova categoria.
        /// Apenas utiliazdores autenticados e com estatuto de Admin ou Mod.
        /// </summary>
        /// <param name="categoria">Categoria a ser criada</param>
        /// <returns>Nova Categoria</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator,Moderator")]
        public async Task<ActionResult<Categoria>> PostCategoria(Categoria categoria)
        {
            // Adiciona a nova categoria ao contexto
            _context.Categorias.Add(categoria);
            // Tenta salvar as alterações no contexto
            await _context.SaveChangesAsync();
            
            // Retorna a categoria criada com o local onde pode ser obtida
            return CreatedAtAction("GetCategoria", new { id = categoria.Id }, categoria);
        }

        // DELETE: api/CategoriaApi/5
        /// <summary>
        /// Elimina uma categoria.
        /// Apenas utiliazdores autenticados e com estatuto de Admin.
        /// </summary>
        /// <param name="id">ID da categoria a ser eliminada</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            // Procura a categoria pelo ID fornecido
            var categoria = await _context.Categorias.FindAsync(id);
            // Verifica se a categoria existe
            if (categoria == null)
            {
                // Se não existir, retorna Not Found
                return NotFound();
            }
            
            // Remove a categoria do contexto
            _context.Categorias.Remove(categoria);
            // Salva as alterações no contexto
            await _context.SaveChangesAsync();
            
            // Se tudo correr bem, retorna No Content
            return NoContent();
        }
        
        /// <summary>
        /// Verifica se uma categoria existe com base no ID fornecido.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.Id == id);
        }
    }
}

/// <summary>
/// Categoria DTO para utilizadores não autenticados.
/// </summary>
public class CategoriaPublicDTO
{
    public string Nome { get; set; }
    public string Descricao { get; set; }
}