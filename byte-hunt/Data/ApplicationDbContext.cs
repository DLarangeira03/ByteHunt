
using Microsoft.EntityFrameworkCore;
using byte_hunt.Models;


namespace byte_hunt.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Item> Itens { get; set; }
        public DbSet<Utilizador> Utilizadores { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Contribuicao> Contribuicoes { get; set; }
        public DbSet<Comparacao> Comparacoes { get; set; }
        
    }
}