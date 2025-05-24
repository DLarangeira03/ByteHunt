using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using byte_hunt.Models;


namespace byte_hunt.Data
{
    public class ApplicationDbContext : IdentityDbContext<Utilizador>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Item> Itens { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Contribuicao> Contribuicoes { get; set; }
        public DbSet<Comparacao> Comparacoes { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationship for Responsavel (moderator)
            modelBuilder.Entity<Contribuicao>()
                .HasOne(c => c.Responsavel)
                .WithMany()  // No collection navigation in Utilizador for Responsavel
                .HasForeignKey(c => c.ResponsavelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure relationship for Utilizador (author of the contribution)
            modelBuilder.Entity<Contribuicao>()
                .HasOne(c => c.Utilizador)
                .WithMany(u => u.Contribuicoes)  // Utilizador has collection Contribuicoes
                .HasForeignKey(c => c.UtilizadorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
        
    }
}