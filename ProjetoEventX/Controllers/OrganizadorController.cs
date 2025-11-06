<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
=======
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
>>>>>>> 9c557d6 (feat: adiciona controladores e atualiza modelos e views)
using Microsoft.EntityFrameworkCore;
using ProjetoEventX.Models;

namespace ProjetoEventX.Controllers
{
<<<<<<< HEAD
    public class OrganizadorController : Controller
    {
        private readonly EventXContext _context;

        public OrganizadorController(EventXContext context)
        {
            _context = context;
        }

        // GET: Organizador
        public async Task<IActionResult> Index()
        {
            var eventXContext = _context.Organizadores.Include(o => o.Pessoa);
            return View(await eventXContext.ToListAsync());
        }

        // GET: Organizador/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organizador = await _context.Organizadores
                .Include(o => o.Pessoa)
                .FirstOrDefaultAsync(m => m.Id == id);
=======
    [Authorize]
    public class OrganizadorController : Controller
    {
        private readonly EventXContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrganizadorController(EventXContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
            {
                return RedirectToAction("LoginOrganizador", "Auth");
            }

            var organizador = await _context.Organizadores
                .Include(o => o.Eventos)
                .FirstOrDefaultAsync(o => o.Id == user.Id);

>>>>>>> 9c557d6 (feat: adiciona controladores e atualiza modelos e views)
            if (organizador == null)
            {
                return NotFound();
            }

            return View(organizador);
        }

<<<<<<< HEAD
        // GET: Organizador/Create
        public IActionResult Create()
        {
            ViewData["PessoaId"] = new SelectList(_context.Pessoas, "Id", "Email");
            return View();
        }

        // POST: Organizador/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PessoaId,DataCadastro,CreatedAt,UpdatedAt,Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] Organizador organizador)
        {
            if (ModelState.IsValid)
            {
                _context.Add(organizador);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PessoaId"] = new SelectList(_context.Pessoas, "Id", "Email", organizador.PessoaId);
            return View(organizador);
        }

        // GET: Organizador/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organizador = await _context.Organizadores.FindAsync(id);
            if (organizador == null)
            {
                return NotFound();
            }
            ViewData["PessoaId"] = new SelectList(_context.Pessoas, "Id", "Email", organizador.PessoaId);
            return View(organizador);
        }

        // POST: Organizador/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PessoaId,DataCadastro,CreatedAt,UpdatedAt,Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] Organizador organizador)
        {
            if (id != organizador.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(organizador);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrganizadorExists(organizador.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PessoaId"] = new SelectList(_context.Pessoas, "Id", "Email", organizador.PessoaId);
            return View(organizador);
        }

        // GET: Organizador/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
=======
        [HttpGet]
        public IActionResult CriarEvento()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarEvento(Evento evento)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null && user.TipoUsuario == "Organizador")
                {
                    var organizador = await _context.Organizadores.FirstOrDefaultAsync(o => o.Id == user.Id);
                    if (organizador != null)
                    {
                        evento.OrganizadorId = organizador.Id;
                        evento.CreatedAt = DateTime.Now;
                        evento.UpdatedAt = DateTime.Now;

                        _context.Eventos.Add(evento);
                        await _context.SaveChangesAsync();

                        return RedirectToAction("Dashboard");
                    }
                }
            }
            return View(evento);
        }

        [HttpGet]
        public async Task<IActionResult> MeusEventos()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.TipoUsuario != "Organizador")
            {
                return RedirectToAction("LoginOrganizador", "Auth");
            }

            var eventos = await _context.Eventos
                .Where(e => e.OrganizadorId == user.Id)
                .OrderByDescending(e => e.DataEvento)
                .ToListAsync();

            return View(eventos);
        }

        [HttpGet]
        public async Task<IActionResult> DetalhesEvento(int id)
        {
            var evento = await _context.Eventos
                .Include(e => e.Pedidos)
                .Include(e => e.ListasConvidados)
                .ThenInclude(l => l.Convidado)
                .ThenInclude(c => c.Pessoa)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evento == null)
>>>>>>> 9c557d6 (feat: adiciona controladores e atualiza modelos e views)
            {
                return NotFound();
            }

<<<<<<< HEAD
            var organizador = await _context.Organizadores
                .Include(o => o.Pessoa)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (organizador == null)
            {
                return NotFound();
            }

            return View(organizador);
        }

        // POST: Organizador/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var organizador = await _context.Organizadores.FindAsync(id);
            if (organizador != null)
            {
                _context.Organizadores.Remove(organizador);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrganizadorExists(int id)
        {
            return _context.Organizadores.Any(e => e.Id == id);
        }
    }
}
=======
            return View(evento);
        }
    }
}



>>>>>>> 9c557d6 (feat: adiciona controladores e atualiza modelos e views)
