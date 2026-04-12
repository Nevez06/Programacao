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
        public DbSet<TemplateConvite> TemplatesConvites { get; set; }
        public DbSet<ConviteRascunho> ConvitesRascunhos { get; set; }
        public DbSet<Notificacao> Notificacoes { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<MensagemChat> MensagemChats { get; set; }
        public DbSet<LogsAcesso> LogsAcessos { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<ChecklistEvento> ChecklistEventos { get; set; }
        public DbSet<TimelineEvento> TimelineEventos { get; set; }
        public DbSet<OrcamentoSimulado> OrcamentosSimulados { get; set; }
        public DbSet<AvaliacaoFornecedor> AvaliacoesFornecedores { get; set; }
        public DbSet<SolicitacaoOrcamento> SolicitacoesOrcamento { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<QuoteMessage> QuoteMessages { get; set; }
        public DbSet<FornecedorRanking> FornecedorRankings { get; set; }
        public DbSet<NegociacaoHistorico> NegociacaoHistoricos { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<EventLog> EventLogs { get; set; }
        public DbSet<PublicacaoFeed> PublicacoesFeed { get; set; }
        public DbSet<ComentarioFeed> ComentariosFeed { get; set; }
        public DbSet<PerfilSocial> PerfisSociais { get; set; }
        public DbSet<SocialPost> SocialPosts { get; set; }
        public DbSet<SocialCurtida> SocialCurtidas { get; set; }
        public DbSet<SocialComentario> SocialComentarios { get; set; }
        public DbSet<SocialStatus> SocialStatus { get; set; }
        public DbSet<SocialStatusVisualizacao> SocialStatusVisualizacoes { get; set; }
        public DbSet<SocialPostSalvo> SocialPostsSalvos { get; set; }
        public DbSet<Story> Stories { get; set; }
        public DbSet<StoryView> StoryViews { get; set; }
        public DbSet<StoryReaction> StoryReactions { get; set; }
        public DbSet<StoryHighlight> StoryHighlights { get; set; }
        public DbSet<StoryHighlightItem> StoryHighlightItems { get; set; }
        public DbSet<StoryReply> StoryReplies { get; set; }
        public DbSet<StoryMention> StoryMentions { get; set; }
        public DbSet<PerfilSocialFollow> PerfilSocialFollows { get; set; }

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

            builder.Entity<Despesa>()
                .HasOne(d => d.Pedido)
                .WithMany()
                .HasForeignKey(d => d.PedidoId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Pedido>().Property(p => p.DespesaGerada).HasDefaultValue(false);

            // AvaliacaoFornecedor
            builder.Entity<AvaliacaoFornecedor>()
                .HasOne(a => a.Fornecedor)
                .WithMany(f => f.Avaliacoes)
                .HasForeignKey(a => a.FornecedorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AvaliacaoFornecedor>()
                .HasOne(a => a.Organizador)
                .WithMany()
                .HasForeignKey(a => a.OrganizadorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AvaliacaoFornecedor>()
                .HasOne(a => a.Evento)
                .WithMany()
                .HasForeignKey(a => a.EventoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AvaliacaoFornecedor>()
                .HasIndex(a => new { a.FornecedorId, a.OrganizadorId, a.EventoId })
                .IsUnique();

            // SolicitacaoOrcamento
            builder.Entity<SolicitacaoOrcamento>()
                .HasOne(s => s.Fornecedor)
                .WithMany(f => f.SolicitacoesRecebidas)
                .HasForeignKey(s => s.FornecedorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SolicitacaoOrcamento>()
                .HasOne(s => s.Organizador)
                .WithMany()
                .HasForeignKey(s => s.OrganizadorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SolicitacaoOrcamento>()
                .HasOne(s => s.Evento)
                .WithMany()
                .HasForeignKey(s => s.EventoId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<SolicitacaoOrcamento>()
                .Property(s => s.Status)
                .HasDefaultValue("Pendente");

            // Quote (Orçamentos)
            builder.Entity<Quote>()
                .HasOne(q => q.Event)
                .WithMany()
                .HasForeignKey(q => q.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Quote>()
                .HasOne(q => q.Supplier)
                .WithMany()
                .HasForeignKey(q => q.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Quote>()
                .HasOne(q => q.Organizador)
                .WithMany()
                .HasForeignKey(q => q.OrganizadorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Quote>()
                .HasOne(q => q.PedidoGerado)
                .WithMany()
                .HasForeignKey(q => q.PedidoGeradoId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Quote>()
                .Property(q => q.Status)
                .HasDefaultValue("Pendente");

            builder.Entity<Quote>()
                .HasIndex(q => q.EventId);

            builder.Entity<ConviteRascunho>()
                .HasOne(r => r.Evento)
                .WithMany()
                .HasForeignKey(r => r.EventoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ConviteRascunho>()
                .HasOne(r => r.Template)
                .WithMany()
                .HasForeignKey(r => r.TemplateId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ConviteRascunho>()
                .HasIndex(r => new { r.EventoId, r.UpdatedAt });

            builder.Entity<Quote>()
                .HasIndex(q => q.SupplierId);

            // QuoteMessage (Chat de Orçamentos)
            builder.Entity<QuoteMessage>()
                .HasOne(m => m.Quote)
                .WithMany()
                .HasForeignKey(m => m.QuoteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<QuoteMessage>()
                .HasIndex(m => m.QuoteId);

            builder.Entity<QuoteMessage>()
                .Property(m => m.IsRead)
                .HasDefaultValue(false);

            // NegociacaoHistorico
            builder.Entity<NegociacaoHistorico>()
                .HasOne(n => n.Quote)
                .WithMany()
                .HasForeignKey(n => n.QuoteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<NegociacaoHistorico>()
                .HasIndex(n => n.QuoteId);

            builder.Entity<NegociacaoHistorico>()
                .HasIndex(n => new { n.QuoteId, n.Rodada });

            // FornecedorRanking
            builder.Entity<FornecedorRanking>()
                .HasOne(r => r.Fornecedor)
                .WithMany()
                .HasForeignKey(r => r.FornecedorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FornecedorRanking>()
                .HasIndex(r => r.FornecedorId)
                .IsUnique();

            builder.Entity<FornecedorRanking>()
                .HasIndex(r => r.PontuacaoGeral);

            // Notification
            builder.Entity<Notification>()
                .HasIndex(n => n.UserId);

            builder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead });

            builder.Entity<Notification>()
                .Property(n => n.IsRead)
                .HasDefaultValue(false);

            // EventX Social
            builder.Entity<PerfilSocial>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<PerfilSocial>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PerfilSocial>()
                .HasIndex(p => p.UserId)
                .IsUnique();

            builder.Entity<SocialPost>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SocialPost>()
                .HasOne(p => p.PerfilSocial)
                .WithMany(perfil => perfil.Posts)
                .HasForeignKey(p => p.PerfilSocialId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SocialPost>()
                .HasOne(p => p.Evento)
                .WithMany()
                .HasForeignKey(p => p.EventoId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<SocialPost>()
                .Property(p => p.CommentsEnabled)
                .HasDefaultValue(true);

            builder.Entity<SocialPost>()
                .HasIndex(p => new { p.PerfilSocialId, p.IsPinned, p.PinnedOrder });

            builder.Entity<SocialCurtida>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Curtidas)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SocialCurtida>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SocialCurtida>()
                .HasIndex(c => new { c.PostId, c.UserId })
                .IsUnique();

            builder.Entity<SocialComentario>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comentarios)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SocialComentario>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SocialComentario>()
                .HasOne(c => c.PerfilSocial)
                .WithMany()
                .HasForeignKey(c => c.PerfilSocialId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<SocialStatus>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SocialStatus>()
                .HasOne(s => s.PerfilSocial)
                .WithMany()
                .HasForeignKey(s => s.PerfilSocialId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SocialStatus>()
                .HasIndex(s => new { s.PerfilSocialId, s.ExpiraEm, s.Ativo });

            builder.Entity<SocialStatusVisualizacao>()
                .HasOne(v => v.SocialStatus)
                .WithMany(s => s.Visualizacoes)
                .HasForeignKey(v => v.SocialStatusId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SocialStatusVisualizacao>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SocialStatusVisualizacao>()
                .HasIndex(v => new { v.SocialStatusId, v.UserId })
                .IsUnique();

            builder.Entity<SocialPostSalvo>()
                .HasOne(s => s.Post)
                .WithMany(p => p.Salvos)
                .HasForeignKey(s => s.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SocialPostSalvo>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SocialPostSalvo>()
                .HasIndex(s => new { s.PostId, s.UserId })
                .IsUnique();

            builder.Entity<Story>()
                .HasIndex(s => s.UserId);

            builder.Entity<Story>()
                .HasIndex(s => s.ExpireAt);

            builder.Entity<Story>()
                .HasOne(s => s.PerfilSocial)
                .WithMany()
                .HasForeignKey(s => s.PerfilSocialId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Story>()
                .HasOne(s => s.SharedPost)
                .WithMany()
                .HasForeignKey(s => s.SharedPostId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Story>()
                .HasOne(s => s.Evento)
                .WithMany()
                .HasForeignKey(s => s.EventoId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<StoryView>()
                .HasOne(v => v.Story)
                .WithMany(s => s.Views)
                .HasForeignKey(v => v.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StoryView>()
                .HasIndex(v => new { v.StoryId, v.UserId })
                .IsUnique();

            builder.Entity<StoryReaction>()
                .HasOne(r => r.Story)
                .WithMany(s => s.Reactions)
                .HasForeignKey(r => r.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StoryReaction>()
                .Property(r => r.ReactionType)
                .HasMaxLength(16);

            builder.Entity<StoryReaction>()
                .HasIndex(r => new { r.StoryId, r.UserId })
                .IsUnique();

            builder.Entity<StoryReply>()
                .HasOne(r => r.Story)
                .WithMany(s => s.Replies)
                .HasForeignKey(r => r.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StoryReply>()
                .Property(r => r.Message)
                .HasMaxLength(1000);

            builder.Entity<StoryReply>()
                .HasIndex(r => new { r.StoryId, r.CreatedAt });

            builder.Entity<StoryMention>()
                .HasOne(m => m.Story)
                .WithMany(s => s.Mentions)
                .HasForeignKey(m => m.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StoryMention>()
                .HasIndex(m => new { m.StoryId, m.MentionedPerfilId });

            builder.Entity<StoryHighlight>()
                .Property(h => h.Nome)
                .HasMaxLength(100);

            builder.Entity<StoryHighlight>()
                .HasIndex(h => h.UserId);

            builder.Entity<StoryHighlightItem>()
                .HasOne(i => i.Highlight)
                .WithMany(h => h.Items)
                .HasForeignKey(i => i.HighlightId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StoryHighlightItem>()
                .HasOne(i => i.Story)
                .WithMany(s => s.HighlightItems)
                .HasForeignKey(i => i.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StoryHighlightItem>()
                .HasIndex(i => new { i.HighlightId, i.StoryId })
                .IsUnique();

            builder.Entity<PerfilSocialFollow>()
                .HasOne(f => f.SeguidorPerfil)
                .WithMany()
                .HasForeignKey(f => f.SeguidorPerfilId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PerfilSocialFollow>()
                .HasOne(f => f.SeguindoPerfil)
                .WithMany()
                .HasForeignKey(f => f.SeguindoPerfilId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PerfilSocialFollow>()
                .HasIndex(f => new { f.SeguidorPerfilId, f.SeguindoPerfilId })
                .IsUnique();

            // Restrições para status
            builder.Entity<Evento>().Property(e => e.StatusEvento).HasDefaultValue("Planejado");
            builder.Entity<Pedido>().Property(p => p.StatusPedido).HasDefaultValue("Pendente");
            builder.Entity<Pagamento>().Property(p => p.StatusPagamento).HasDefaultValue("Pendente");
            builder.Entity<TarefaEvento>().Property(t => t.StatusConclusao).HasDefaultValue("Pendente");

            // Índices
            builder.Entity<Evento>().HasIndex(e => e.OrganizadorId);
            builder.Entity<Evento>().HasIndex(e => e.Slug).IsUnique().HasFilter("\"Slug\" IS NOT NULL");
            builder.Entity<Pedido>().HasIndex(p => p.EventoId);
            builder.Entity<EventLog>().HasIndex(e => e.EventId);
            builder.Entity<EventLog>().HasIndex(e => e.CreatedAt);

            // Conversão automática de DateTime para UTC
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                            v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                        ));
                    }
                }
            }

        }
    }
}
