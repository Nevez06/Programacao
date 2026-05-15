using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Models;

namespace ProjetoEventX.Data;

public static class StartupDataSeeder
{
    private const string DefaultProfileImage = "/uploads/social/defaults/default-profile.svg";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<EventXContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var organizadorUser = await EnsureOrganizadorAsync(context, userManager);
        await EnsureFornecedorAsync(context, userManager);
        await EnsureConvidadoAsync(context, userManager);
        await EnsureEventoInicialAsync(context, organizadorUser);
        await EnsurePerfisSociaisAsync(context);
    }

    private static async Task<ApplicationUser> EnsureOrganizadorAsync(EventXContext context, UserManager<ApplicationUser> userManager)
    {
        const string email = "organizador@eventx.local";
        const string senha = "EventX@1234";

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                TipoUsuario = "Organizador"
            };

            var result = await userManager.CreateAsync(user, senha);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Falha ao criar usuário seed organizador: {string.Join("; ", result.Errors.Select(e => e.Description))}");
            }
        }
        else if (user.TipoUsuario != "Organizador")
        {
            user.TipoUsuario = "Organizador";
            await userManager.UpdateAsync(user);
        }

        var pessoa = await context.Pessoas.FirstOrDefaultAsync(p => p.Email == email);
        if (pessoa == null)
        {
            pessoa = new Pessoa
            {
                Nome = "Organizador Padrão",
                Email = email,
                Endereco = "Av. Central, 1000",
                Cpf = "12345678909",
                Telefone = "(11) 99999-0001",
                Cidade = "São Paulo",
                UF = "SP",
                FotoPerfilUrl = DefaultProfileImage,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Pessoas.Add(pessoa);
            await context.SaveChangesAsync();
        }

        var organizador = await context.Organizadores.FirstOrDefaultAsync(o => o.Email == email);
        if (organizador == null)
        {
            context.Organizadores.Add(new Organizador
            {
                Id = user.Id,
                PessoaId = pessoa.Id,
                Pessoa = pessoa,
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        return user;
    }

    private static async Task EnsureFornecedorAsync(EventXContext context, UserManager<ApplicationUser> userManager)
    {
        const string email = "fornecedor@eventx.local";
        const string senha = "EventX@1234";

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                TipoUsuario = "Fornecedor"
            };

            var result = await userManager.CreateAsync(user, senha);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Falha ao criar usuário seed fornecedor: {string.Join("; ", result.Errors.Select(e => e.Description))}");
            }
        }
        else if (user.TipoUsuario != "Fornecedor")
        {
            user.TipoUsuario = "Fornecedor";
            await userManager.UpdateAsync(user);
        }

        var pessoa = await context.Pessoas.FirstOrDefaultAsync(p => p.Email == email);
        if (pessoa == null)
        {
            pessoa = new Pessoa
            {
                Nome = "Fornecedor Padrão",
                Email = email,
                Endereco = "Rua Comercial, 200",
                Cpf = "98765432100",
                Telefone = "(11) 99999-0002",
                Cidade = "São Paulo",
                UF = "SP",
                FotoPerfilUrl = DefaultProfileImage,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Pessoas.Add(pessoa);
            await context.SaveChangesAsync();
        }

        var fornecedor = await context.Fornecedores.FirstOrDefaultAsync(f => f.Email == email);
        if (fornecedor == null)
        {
            context.Fornecedores.Add(new Fornecedor
            {
                PessoaId = pessoa.Id,
                Pessoa = pessoa,
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = true,
                Cnpj = "12.345.678/0001-90",
                TipoServico = "Buffet",
                Cidade = "São Paulo",
                UF = "SP",
                NomeNegocio = "Fornecedor Demo",
                Descricao = "Fornecedor inicial criado automaticamente.",
                Categoria = "Buffet",
                FaixaPreco = "$$",
                ContatoComercial = email,
                Disponibilidade = "Seg-Sex",
                FotoPerfilUrl = DefaultProfileImage,
                Regiao = "Sudeste",
                Telefone = "(11) 99999-0002",
                ServicosOferecidos = new List<string>(),
                Galeria = new List<string>(),
                TotalAvaliacoes = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }
    }

    private static async Task EnsureConvidadoAsync(EventXContext context, UserManager<ApplicationUser> userManager)
    {
        const string email = "convidado@eventx.local";
        const string senha = "EventX@1234";

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                TipoUsuario = "Convidado"
            };

            var result = await userManager.CreateAsync(user, senha);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Falha ao criar usuário seed convidado: {string.Join("; ", result.Errors.Select(e => e.Description))}");
            }
        }
        else if (user.TipoUsuario != "Convidado")
        {
            user.TipoUsuario = "Convidado";
            await userManager.UpdateAsync(user);
        }

        var pessoa = await context.Pessoas.FirstOrDefaultAsync(p => p.Email == email);
        if (pessoa == null)
        {
            pessoa = new Pessoa
            {
                Nome = "Convidado Padrão",
                Email = email,
                Endereco = "Rua dos Eventos, 300",
                Cpf = "11122233344",
                Telefone = "(11) 99999-0003",
                Cidade = "São Paulo",
                UF = "SP",
                FotoPerfilUrl = DefaultProfileImage,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Pessoas.Add(pessoa);
            await context.SaveChangesAsync();
        }

        var convidado = await context.Convidados.FirstOrDefaultAsync(c => c.Email == email);
        if (convidado == null)
        {
            context.Convidados.Add(new Convidado
            {
                PessoaId = pessoa.Id,
                Pessoa = pessoa,
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = true,
                ConfirmaPresenca = "Pendente",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }
    }

    private static async Task EnsureEventoInicialAsync(EventXContext context, ApplicationUser organizador)
    {
        var existe = await context.Eventos.AnyAsync(e => e.OrganizadorId == organizador.Id);
        if (existe)
        {
            return;
        }

        context.Eventos.Add(new Evento
        {
            NomeEvento = "Evento Inicial EventX",
            DataEvento = DateTime.UtcNow.AddDays(30),
            DescricaoEvento = "Evento exemplo criado automaticamente na inicialização.",
            TipoEvento = "Corporativo",
            HoraInicio = "19:00",
            HoraFim = "23:00",
            PublicoEstimado = 100,
            OrganizadorId = organizador.Id,
            StatusEvento = "Planejado",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }

    private static async Task EnsurePerfisSociaisAsync(EventXContext context)
    {
        var users = await context.Users.AsNoTracking().ToListAsync();
        if (users.Count == 0)
        {
            return;
        }

        var perfisPorUserId = await context.PerfisSociais
            .AsNoTracking()
            .ToDictionaryAsync(p => p.UserId);

        var novosPerfis = new List<PerfilSocial>();
        foreach (var user in users)
        {
            if (perfisPorUserId.ContainsKey(user.Id))
            {
                continue;
            }

            novosPerfis.Add(new PerfilSocial
            {
                UserId = user.Id,
                NomeExibicao = user.UserName ?? user.Email ?? $"Usuário {user.Id}",
                Username = user.UserName,
                TipoPerfil = user.TipoUsuario ?? "Convidado",
                FotoPerfilUrl = DefaultProfileImage,
                CriadoEm = DateTime.UtcNow,
                AtualizadoEm = DateTime.UtcNow
            });
        }

        if (novosPerfis.Count == 0)
        {
            return;
        }

        context.PerfisSociais.AddRange(novosPerfis);
        await context.SaveChangesAsync();
    }
}
