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
    public class ItensApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ItensApiController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        // GET: api/ItensApi
        /// <summary>
        /// Obtém o nome, marca, preço e categoria de cada item.
        /// Apenas utilizadores não autenticados
        /// </summary>
        /// <returns>Item com nome, marca, preço e categoria</returns>
        [HttpGet("publico")]
        public async Task<ActionResult<IEnumerable<Item>>> GetItensPublic()
        {
            var itensPublicos = await _context.Itens
                .Include(i => i.Categoria)
                .Select(i => new ItemPublicDTO {
                    Nome = i.Nome,
                    Marca = i.Marca,
                    Preco = i.Preco,
                    Categoria = i.Categoria.Nome
                })
                .ToListAsync();

            return Ok(itensPublicos);
        }
        
        // GET: api/ItensApi
        /// <summary>
        /// Obtém uma lista de todos os itens com todas as informações.
        /// Apenas utiliazdores autenticados têm acesso a este endpoint.
        /// </summary>
        /// <returns>Lista de Itens</returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Item>>> GetItens()
        {
            return await _context.Itens.ToListAsync();
        }

        // GET: api/ItensApi/5
        /// <summary>
        /// Obtém um item filtrado por ID.
        /// Apenas utiliazdores autenticados têm acesso a este endpoint.
        /// </summary>
        /// <returns>Item</returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Item>> GetItem(int id)
        {
            var item = await _context.Itens.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return item;
        }

        // PUT: api/ItensApi/5
        /// <summary>
        /// Edita um item com base no seu ID.
        /// Apenas utiliazdores autenticados e com estatuto de Admin ou Mod.
        /// </summary>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> PutItem(int id, Item item)
        {
            if (id != item.Id)
            {
                return BadRequest();
            }

            _context.Entry(item).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemExists(id))
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

        // POST: api/ItensApi
        /// <summary>
        /// Cria um novo item.
        /// Apenas utiliazdores autenticados e com estatuto de Admin ou Mod.
        /// </summary>
        /// <returns>Novo Item</returns>
        [HttpPost]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<ActionResult<Item>> PostItem(Item item)
        {
            _context.Itens.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetItem", new { id = item.Id }, item);
        }

        // DELETE: api/ItensApi/5
        /// <summary>
        /// Elimina um item.
        /// Apenas utiliazdores autenticados e com estatuto de Admin.
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.Itens.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            _context.Itens.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ItemExists(int id)
        {
            return _context.Itens.Any(e => e.Id == id);
        }
    }
}

//Classe que permite transformar os dados de um item noutro item para mostrar
public class ItemPublicDTO
{
    public string Nome { get; set; }
    public string Marca { get; set; }
    public decimal? Preco { get; set; }
    public string Categoria { get; set; }
}
