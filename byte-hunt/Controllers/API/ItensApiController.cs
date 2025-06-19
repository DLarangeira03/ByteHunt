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
            //Criação de uma lista de novos itens com a informação dos itens já existentes
            var itensPublicos = await _context.Itens
                .Include(i => i.Categoria)
                .Select(i => new ItemPublicDTO {
                    Nome = i.Nome,
                    Marca = i.Marca,
                    Preco = i.Preco,
                    Categoria = i.Categoria.Nome
                })
                .ToListAsync();
            
            //Retorna a lista de Itens
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
            //Retorna uma lista com todos os itens e todas as suas informações
            return await _context.Itens.ToListAsync();
        }

        // GET: api/ItensApi/5
        /// <summary>
        /// Obtém um item filtrado por ID.
        /// Apenas utiliazdores autenticados têm acesso a este endpoint.
        /// </summary>
        /// <param id">ID do item a obter</param>
        /// <returns>Item</returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Item>> GetItem(int id)
        {
            //Identifica o item pretendido por ID
            var item = await _context.Itens.FindAsync(id);
            
            //Verifica se o item não está vazio (nulo)
            if (item == null)
            {
                //Caso seja nulo, retorna que o item não existe
                return NotFound();
            }
            
            //Retorna o item
            return item;
        }

        // PUT: api/ItensApi/5
        /// <summary>
        /// Edita um item com base no seu ID.
        /// Apenas utiliazdores autenticados e com estatuto de Admin ou Mod.
        /// </summary>
        /// <param name="id">ID do item a editar</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> PutItem(int id, Item item)
        {
            //Verfica se o ID do produto que pretende editar é igual ao que o utilizador pretende editar 
            if (id != item.Id)
            {
                //Caso não seja igual, devolve um mensagem de erro
                return BadRequest();
            }
            
            //Indica que a entidade 'item' foi alterada e deve ser atualizada na BD
            _context.Entry(item).State = EntityState.Modified;
            
            
            try
            {
                // Tenta salvar as alterações feitas no contexto para a base de dados de forma assíncrona
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Se ocorrer uma exceção de concorrência, verifica se o item ainda existe na base de dados
                if (!ItemExists(id))
                {
                    // Se o item não existir, retorna a dizer que não existe
                    return NotFound();
                }
                else
                {
                    // Se o item existir, lança uma exceção
                    throw;
                }
            }
            
            //Não tem conteúdo para retornar
            return NoContent();
        }

        // POST: api/ItensApi
        /// <summary>
        /// Cria um novo item.
        /// Apenas utiliazdores autenticados e com estatuto de Admin ou Mod.
        /// </summary>
        /// <param name="item">Item a criar</param>
        /// <returns>Novo Item</returns>
        [HttpPost]
        [Authorize(Roles = "Administrator,Moderator")]
        public async Task<ActionResult<Item>> PostItem(Item item)
        {
            //Adiciona o novo item ao contexto da base de dados
            _context.Itens.Add(item);
            //Salva as alterações de forma assíncrona
            await _context.SaveChangesAsync();
            
            //Retorna o novo item criado
            return CreatedAtAction("GetItem", new { id = item.Id }, item);
        }

        // DELETE: api/ItensApi/5
        /// <summary>
        /// Elimina um item.
        /// Apenas utiliazdores autenticados e com estatuto de Admin.
        /// </summary>
        /// <param name="id">ID do item a eliminar</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            //Procura o item na base de dados pelo ID
            var item = await _context.Itens.FindAsync(id);
            //Verifica se o item não está vazio (nulo)
            if (item == null)
            {
                //Caso seja nulo, retorna que o item não existe
                return NotFound();
            }
            
            //Remove o item do contexto da base de dados
            _context.Itens.Remove(item);
            //Salva as alterações de forma assíncrona
            await _context.SaveChangesAsync();
            
            //Não tem conteúdo para retornar
            return NoContent();
        }

        private bool ItemExists(int id)
        {
            //Verifica se existe algum item na base de dados com o ID indicado
            return _context.Itens.Any(e => e.Id == id);
        }
    }
}

//Classe que permite transformar os dados de um item noutro item para mostrar
// apenas as informações necessárias para o utilizador não autenticado
public class ItemPublicDTO
{
    public string Nome { get; set; }
    public string Marca { get; set; }
    public decimal? Preco { get; set; }
    public string Categoria { get; set; }
}
