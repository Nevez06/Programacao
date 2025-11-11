using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Models;
namespace ProjetoEventX.Data
{
    public class EventXContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>

    {
        public EventXContext(DbContextOptions<EventXContext> options) : base(options) { }

        public DbSet<Pessoa> Pessoas { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<Organizador> Organizadores { get; set; }
        public DbSet<Convidado> Convidados { get; set; }

        public DbSet<Despesa> Despesas { get; set; }

        public DbSet<Evento> Eventos { get; set; }
        public DbSet<Produto> Produtos { get; set; }
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
        public DbSet<MensagemChat> MensagensChat { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Ignorar classes do Stripe que não devem ser mapeadas
            builder.Ignore<Stripe.StripeResponse>();
            builder.Ignore<Stripe.StripeRequest>();
            builder.Ignore<Stripe.StripeError>();
            builder.Ignore<Stripe.Checkout.Session>();
            builder.Ignore<Stripe.Event>();

            // Ignorar propriedades que podem causar problemas com interfaces
            // Manter relacionamentos principais; não ignorar coleções necessárias para o domínio

            // Ignorar propriedades de navegação em outras entidades
            // Manter navegações essenciais; evitar ignorar propriedades necessárias

            // Manter navegações essenciais de Organizador



            // Configurar herança/composição para usuários com navegações inversas explícitas
            builder.Entity<Fornecedor>()
                .HasOne(f => f.Pessoa)
                .WithOne(p => p.Fornecedor)
                .HasForeignKey<Fornecedor>(f => f.PessoaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Organizador>()
                .HasOne(o => o.Pessoa)
                .WithOne(p => p.Organizador)
                .HasForeignKey<Organizador>(o => o.PessoaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Convidado>()
                .HasOne(c => c.Pessoa)
                .WithOne(p => p.Convidado)
                .HasForeignKey<Convidado>(c => c.PessoaId)
                .OnDelete(DeleteBehavior.Restrict);

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

            // Configurar relacionamentos com tipos corretos
            // Produto -> Fornecedor (FornecedorId é int, não Guid)
            builder.Entity<Produto>()
                .HasOne(p => p.Fornecedor)
                .WithMany(f => f.Produtos)
                .HasForeignKey(p => p.FornecedorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Garantir que FornecedorId seja int
            builder.Entity<Produto>()
                .Property(p => p.FornecedorId)
                .HasColumnType("integer");

            builder.Entity<Pedido>()
                .HasOne(p => p.Evento)
                .WithMany(e => e.Pedidos)
                .HasForeignKey(p => p.EventoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Pedido>()
                .HasOne(p => p.Produto)
                .WithMany(pr => pr.Pedidos)
                .HasForeignKey(p => p.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Restrições para status
            builder.Entity<Evento>().Property(e => e.StatusEvento).HasDefaultValue("Planejado");
            builder.Entity<Pedido>().Property(p => p.StatusPedido).HasDefaultValue("Pendente");
            builder.Entity<Pagamento>().Property(p => p.StatusPagamento).HasDefaultValue("Pendente");
            builder.Entity<TarefaEvento>().Property(t => t.StatusConclusao).HasDefaultValue("Pendente");

            // Índices
            builder.Entity<Evento>().HasIndex(e => e.OrganizadorId);
            builder.Entity<Pedido>().HasIndex(p => p.EventoId);
        }
    }
}