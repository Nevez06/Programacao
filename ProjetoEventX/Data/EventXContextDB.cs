using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ProjetoEventX.Models
{
    public class EventXContext : IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>
    {
        public EventXContext(DbContextOptions<EventXContext> options) : base(options) { }

        public DbSet<Pessoa> Pessoas { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<Organizador> Organizadores { get; set; }
        public DbSet<Convidado> Convidados { get; set; }
        public DbSet<Evento> Eventos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<ItemPedido> ItensPedidos { get; set; }
        public DbSet<Pagamento> Pagamentos { get; set; }
        public DbSet<Administracao> Administracoes { get; set; }
        public DbSet<TemplateEvento> TemplatesEventos { get; set; }
        public DbSet<TarefaEvento> TarefasEventos { get; set; }
        public DbSet<AssistenteVirtual> AssistentesVirtuais { get; set; }
        public DbSet<Local> Locais { get; set; }
        public DbSet<ListaConvidado> ListasConvidados { get; set; }
        public DbSet<Notificacao> Notificacoes { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<MensagemChat> MensagemChats { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurar chaves int para Identity
            builder.Entity<IdentityUser<int>>().ToTable("AspNetUsers").HasKey(u => u.Id);
            builder.Entity<IdentityRole<int>>().ToTable("AspNetRoles").HasKey(r => r.Id);

            // Configurar herança/composição para usuários
            builder.Entity<Fornecedor>().HasOne(f => f.Pessoa).WithOne().HasForeignKey<Fornecedor>(f => f.PessoaId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Organizador>().HasOne(o => o.Pessoa).WithOne().HasForeignKey<Organizador>(o => o.PessoaId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Convidado>().HasOne(c => c.Pessoa).WithOne().HasForeignKey<Convidado>(c => c.PessoaId).OnDelete(DeleteBehavior.Restrict);

            // Configurar relacionamentos para MensagemChat
            builder.Entity<MensagemChat>()
                .HasOne(m => m.Remetente)
                .WithMany()
                .HasForeignKey(m => m.RemetenteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MensagemChat>()
                .HasOne(m => m.Destinatario)
                .WithMany()
                .HasForeignKey(m => m.DestinatarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MensagemChat>()
                .HasOne(m => m.Evento)
                .WithMany()
                .HasForeignKey(m => m.EventoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrições para status
            builder.Entity<Evento>().Property(e => e.StatusEvento).HasDefaultValue("Planejado");
            builder.Entity<Pedido>().Property(p => p.Status).HasDefaultValue("Pendente");
            builder.Entity<Pagamento>().Property(p => p.StatusPagamento).HasDefaultValue("Pendente");
            builder.Entity<TarefaEvento>().Property(t => t.StatusConclusao).HasDefaultValue("Pendente");

            // Índices
            builder.Entity<Evento>().HasIndex(e => e.OrganizadorId);
            builder.Entity<Pedido>().HasIndex(p => p.EventoId);
        }
    }
}