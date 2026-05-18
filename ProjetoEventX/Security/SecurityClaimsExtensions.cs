using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjetoEventX.Security
{
    public static class SecurityClaimsExtensions
    {
        private static bool IsAuthenticated(ClaimsPrincipal user)
        {
            return user?.Identity?.IsAuthenticated == true;
        }

        public static async Task<bool> IsOrganizadorAsync(this ClaimsPrincipal user, UserManager<Models.ApplicationUser> userManager)
        {
            if (!IsAuthenticated(user))
                return false;

            var appUser = await userManager.GetUserAsync(user);
            return appUser?.TipoUsuario == "Organizador";
        }

        public static async Task<bool> IsFornecedorAsync(this ClaimsPrincipal user, UserManager<Models.ApplicationUser> userManager)
        {
            if (!IsAuthenticated(user))
                return false;

            var appUser = await userManager.GetUserAsync(user);
            return appUser?.TipoUsuario == "Fornecedor";
        }

        public static async Task<bool> IsConvidadoAsync(this ClaimsPrincipal user, UserManager<Models.ApplicationUser> userManager)
        {
            if (!IsAuthenticated(user))
                return false;

            var appUser = await userManager.GetUserAsync(user);
            return appUser?.TipoUsuario == "Convidado";
        }

        public static async Task<string> GetUserTipoAsync(this ClaimsPrincipal user, UserManager<Models.ApplicationUser> userManager)
        {
            if (!IsAuthenticated(user))
                return "Anônimo";

            var appUser = await userManager.GetUserAsync(user);
            return appUser?.TipoUsuario ?? "Desconhecido";
        }

        public static async Task<bool> IsOwnerOfEventoAsync(this ClaimsPrincipal user, UserManager<Models.ApplicationUser> userManager, int eventoId, Data.EventXContext context)
        {
            if (!IsAuthenticated(user))
                return false;

            var appUser = await userManager.GetUserAsync(user);
            if (appUser == null || appUser.TipoUsuario != "Organizador")
                return false;

            var evento = await context.Eventos.FindAsync(eventoId);
            return evento?.OrganizadorId == appUser.Id;
        }

        public static async Task<bool> CanAccessEventoAsync(this ClaimsPrincipal user, UserManager<Models.ApplicationUser> userManager, int eventoId, Data.EventXContext context)
        {
            if (!IsAuthenticated(user))
                return false;

            var appUser = await userManager.GetUserAsync(user);
            if (appUser == null)
                return false;

            // Organizadores podem acessar seus próprios eventos
            if (appUser.TipoUsuario == "Organizador")
            {
                var evento = await context.Eventos.FindAsync(eventoId);
                return evento?.OrganizadorId == appUser.Id;
            }

            // Convidados podem acessar eventos onde estão convidados
            if (appUser.TipoUsuario == "Convidado")
            {
                var convidado = await context.Convidados.FindAsync(appUser.Id);
                if (convidado == null)
                    return false;

                var estaConvidado = await context.ListasConvidados
                    .AnyAsync(l => l.EventoId == eventoId && l.ConvidadoId == convidado.Id);
                
                return estaConvidado;
            }

            return false;
        }

        public static async Task<Models.ApplicationUser?> GetApplicationUserAsync(this ClaimsPrincipal user, UserManager<Models.ApplicationUser> userManager)
        {
            if (!IsAuthenticated(user))
                return null;

            return await userManager.GetUserAsync(user);
        }

        public static bool HasRole(this ClaimsPrincipal user, string role)
        {
            return IsAuthenticated(user) && user.IsInRole(role);
        }

        public static async Task<bool> IsAccountActiveAsync(this ClaimsPrincipal user, UserManager<Models.ApplicationUser> userManager)
        {
            if (!IsAuthenticated(user))
                return false;

            var appUser = await userManager.GetUserAsync(user);
            if (appUser == null)
                return false;

            // Verificar se a conta está bloqueada
            var isLockedOut = await userManager.IsLockedOutAsync(appUser);
            if (isLockedOut)
                return false;

            // Verificar se o email está confirmado
            var emailConfirmed = await userManager.IsEmailConfirmedAsync(appUser);
            return emailConfirmed;
        }

        public static async Task<bool> RequireTwoFactorAsync(this ClaimsPrincipal user, UserManager<Models.ApplicationUser> userManager)
        {
            if (!IsAuthenticated(user))
                return false;

            var appUser = await userManager.GetUserAsync(user);
            if (appUser == null)
                return false;

            // Verificar se 2FA está habilitado
            return await userManager.GetTwoFactorEnabledAsync(appUser);
        }
    }
}