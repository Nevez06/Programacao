using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ProjetoEventX.Models
{
    public class SimpleEventXContext : IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>
    {
        public SimpleEventXContext(DbContextOptions<SimpleEventXContext> options) : base(options) { }

        public DbSet<Pessoa> Pessoas { get; set; }
        public DbSet<Organizador> Organizadores { get; set; }
        public DbSet<Evento> Eventos { get; set; }
        public DbSet<Local> Locais { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Ignorar classes do Stripe que não devem ser mapeadas
            builder.Ignore<Stripe.StripeResponse>();
            builder.Ignore<Stripe.StripeRequest>();
            builder.Ignore<Stripe.StripeError>();

            // Configurar chaves int para Identity
            builder.Entity<IdentityUser<int>>().ToTable("AspNetUsers").HasKey(u => u.Id);
            builder.Entity<IdentityRole<int>>().ToTable("AspNetRoles").HasKey(r => r.Id);

            // Configurar relacionamentos básicos
            builder.Entity<Organizador>().HasOne(o => o.Pessoa).WithOne().HasForeignKey<Organizador>(o => o.PessoaId).OnDelete(DeleteBehavior.Restrict);

            // Restrições para status
            builder.Entity<Evento>().Property(e => e.StatusEvento).HasDefaultValue("Planejado");

            // Índices
            builder.Entity<Evento>().HasIndex(e => e.OrganizadorId);
        }
    }
}

