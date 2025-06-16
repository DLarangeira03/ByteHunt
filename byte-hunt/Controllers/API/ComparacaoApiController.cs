using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using byte_hunt.Data;
using byte_hunt.Models;

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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comparacao>>> GetComparacoes()
        {
            return await _context.Comparacoes.ToListAsync();
        }

        // GET: api/ComparacaoApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Comparacao>> GetComparacao(int id)
        {
            var comparacao = await _context.Comparacoes.FindAsync(id);

            if (comparacao == null)
            {
                return NotFound();
            }

            return comparacao;
        }

        // PUT: api/ComparacaoApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Comparacao>> PostComparacao(Comparacao comparacao)
        {
            _context.Comparacoes.Add(comparacao);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetComparacao", new { id = comparacao.Id }, comparacao);
        }

        // DELETE: api/ComparacaoApi/5
        [HttpDelete("{id}")]
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
