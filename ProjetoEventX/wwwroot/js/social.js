(function () {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    const overlay = document.querySelector('.js-post-actions-overlay');
    const sheet = overlay ? overlay.querySelector('.js-post-actions-sheet') : null;
    const list = overlay ? overlay.querySelector('.js-post-actions-list') : null;

    const toast = (message) => {
        let el = document.querySelector('.social-toast');
        if (!el) {
            el = document.createElement('div');
            el.className = 'social-toast';
            document.body.appendChild(el);
        }
        el.textContent = message;
        el.classList.add('is-visible');
        window.setTimeout(() => el.classList.remove('is-visible'), 1800);
    };

    const closeActions = () => {
        if (!overlay) return;
        overlay.classList.add('d-none');
        overlay.setAttribute('aria-hidden', 'true');
        list && (list.innerHTML = '');
    };

    const csrfHeaders = () => tokenInput ? { 'RequestVerificationToken': tokenInput.value } : {};

    const postAction = async (url, method = 'POST') => {
        const response = await fetch(url, {
            method,
            headers: { 'X-Requested-With': 'XMLHttpRequest', ...csrfHeaders() }
        });
        return response.ok ? response.json() : { ok: false };
    };

    const buildActionButton = (cfg) => {
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = `post-actions-item ${cfg.destructive ? 'is-destructive' : ''}`;
        btn.innerHTML = `<i class="${cfg.icon}"></i><span>${cfg.label}</span>`;
        btn.addEventListener('click', cfg.onClick);
        return btn;
    };

    const openActions = (trigger) => {
        if (!overlay || !sheet || !list) return;
        const isOwner = trigger.dataset.isOwner === '1';
        const saveUrl = trigger.dataset.saveUrl;
        const postUrl = trigger.dataset.postUrl;
        const editUrl = trigger.dataset.editUrl;
        const pinUrl = trigger.dataset.pinUrl;
        const archiveUrl = trigger.dataset.archiveUrl;
        const deleteUrl = trigger.dataset.deleteUrl;
        const toggleLikesUrl = trigger.dataset.toggleLikesUrl;
        const toggleSharesUrl = trigger.dataset.toggleSharesUrl;
        const toggleCommentsUrl = trigger.dataset.toggleCommentsUrl;
        const eventUrl = trigger.dataset.eventUrl;
        const profileUrl = trigger.dataset.profileUrl;
        list.innerHTML = '';

        const base = [
            { icon: 'far fa-bookmark', label: 'Salvar publicação', onClick: async () => { const r = await postAction(saveUrl); toast(r.message || 'Publicação salva.'); closeActions(); } },
            { icon: 'fas fa-share-nodes', label: 'Compartilhar', onClick: async () => { if (navigator.share) { try { await navigator.share({ url: postUrl }); } catch { } } else { await navigator.clipboard.writeText(postUrl); toast('Link copiado para compartilhamento.'); } closeActions(); } },
            { icon: 'fas fa-link', label: 'Copiar link', onClick: async () => { await navigator.clipboard.writeText(postUrl); toast('Link copiado.'); closeActions(); } }
        ];

        if (isOwner) {
            base.push(
                { icon: 'fas fa-pen', label: 'Editar publicação', onClick: () => { window.location.href = editUrl; } },
                { icon: 'fas fa-thumbtack', label: 'Fixar no perfil', onClick: async () => { const r = await postAction(pinUrl); toast(r.message || 'Estado atualizado.'); closeActions(); } },
                { icon: 'fas fa-calendar-days', label: 'Vincular/trocar evento', onClick: () => { window.location.href = editUrl; } },
                { icon: 'fas fa-heart-slash', label: 'Ocultar número de curtidas', onClick: async () => { const r = await postAction(toggleLikesUrl); toast(r.message || 'Estado atualizado.'); closeActions(); } },
                { icon: 'fas fa-share-from-square', label: 'Ocultar compartilhamentos', onClick: async () => { const r = await postAction(toggleSharesUrl); toast(r.message || 'Estado atualizado.'); closeActions(); } },
                { icon: 'fas fa-comments-slash', label: 'Desativar comentários', onClick: async () => { const r = await postAction(toggleCommentsUrl); toast(r.message || 'Estado atualizado.'); closeActions(); } },
                { icon: 'fas fa-box-archive', label: 'Arquivar publicação', onClick: async () => { const r = await postAction(archiveUrl); toast(r.message || 'Publicação arquivada.'); closeActions(); window.location.reload(); } },
                { icon: 'fas fa-trash', label: 'Excluir publicação', destructive: true, onClick: async () => { const ok = window.confirm('Excluir esta publicação?'); if (!ok) return; const r = await postAction(deleteUrl); toast(r.message || 'Publicação excluída.'); closeActions(); window.location.reload(); } }
            );
        } else {
            base.push(
                { icon: 'fas fa-calendar-check', label: 'Ver evento relacionado', onClick: () => { if (eventUrl) window.location.href = eventUrl; else toast('Sem evento relacionado.'); } },
                { icon: 'fas fa-user-plus', label: 'Seguir perfil', onClick: () => { if (profileUrl) window.location.href = profileUrl; } },
                { icon: 'fas fa-volume-xmark', label: 'Silenciar perfil', onClick: () => { toast('Perfil silenciado.'); closeActions(); } },
                { icon: 'fas fa-flag', label: 'Denunciar publicação', destructive: true, onClick: () => { toast('Denúncia registrada.'); closeActions(); } }
            );
        }

        base.forEach((cfg) => list.appendChild(buildActionButton(cfg)));
        overlay.classList.remove('d-none');
        overlay.setAttribute('aria-hidden', 'false');
    };

    document.querySelectorAll('.js-post-actions-open').forEach((btn) => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            openActions(btn);
        });
    });

    overlay?.addEventListener('click', (e) => {
        if (e.target === overlay) closeActions();
    });
    document.querySelectorAll('.js-post-actions-close').forEach((btn) => btn.addEventListener('click', closeActions));
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') closeActions();
    });
})();

(function () {
    const input = document.querySelector('[data-social-preview-input]');
    const target = document.querySelector('[data-social-preview-target]');

    if (input && target) {
        input.addEventListener('change', function (event) {
            const file = event.target.files && event.target.files[0];
            if (!file) {
                target.classList.add('d-none');
                target.removeAttribute('src');
                return;
            }

            const reader = new FileReader();
            reader.onload = function (e) {
                target.src = e.target?.result;
                target.classList.remove('d-none');
            };
            reader.readAsDataURL(file);
        });
    }

    const chips = document.querySelectorAll('.social-category-chips .chip');
    chips.forEach((chip) => {
        chip.addEventListener('click', () => {
            chips.forEach((c) => c.classList.remove('active'));
            chip.classList.add('active');
        });
    });
})();

(function () {
    const searchInput = document.querySelector('[data-explore-search-input]');
    if (searchInput) {
        searchInput.addEventListener('keydown', (event) => {
            if (event.key !== 'Escape') return;
            searchInput.value = '';
        });
    }

    const chips = document.querySelectorAll('[data-explore-chip]');
    chips.forEach((chip) => {
        chip.addEventListener('click', () => {
            chips.forEach((c) => c.classList.remove('is-active'));
            chip.classList.add('is-active');
        });
    });

    document.querySelectorAll('.explore-visual-card, .explore-event-card, .explore-template-card, .explore-supplier-card')
        .forEach((card) => {
            card.addEventListener('pointerdown', () => card.classList.add('is-pressed'));
            card.addEventListener('pointerup', () => card.classList.remove('is-pressed'));
            card.addEventListener('pointerleave', () => card.classList.remove('is-pressed'));
        });

    const grid = document.querySelector('.js-explore-grid');
    const loadMoreBtn = document.querySelector('.js-explore-load-more');
    const skeleton = document.querySelector('.js-explore-skeleton');
    if (grid && loadMoreBtn) {
        const bindCardInteractions = (root) => {
            root.querySelectorAll('.explore-visual-card, .explore-event-card, .explore-template-card, .explore-supplier-card')
                .forEach((card) => {
                    card.addEventListener('pointerdown', () => card.classList.add('is-pressed'));
                    card.addEventListener('pointerup', () => card.classList.remove('is-pressed'));
                    card.addEventListener('pointerleave', () => card.classList.remove('is-pressed'));
                });
        };

        const loadMore = async () => {
            if (loadMoreBtn.dataset.busy === '1') return;
            loadMoreBtn.dataset.busy = '1';
            loadMoreBtn.disabled = true;
            if (skeleton) skeleton.classList.remove('d-none');

            try {
                const url = grid.dataset.exploreLoadUrl;
                const busca = grid.dataset.exploreBusca || '';
                const categoria = grid.dataset.exploreCategoria || '';
                const skip = parseInt(grid.dataset.exploreSkip || '0', 10);
                const take = parseInt(grid.dataset.exploreTake || '12', 10);
                const qs = new URLSearchParams({ skip: String(skip), take: String(take), busca, categoria });
                const response = await fetch(`${url}?${qs.toString()}`, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
                if (!response.ok) return;

                const html = await response.text();
                if (!html.trim()) {
                    loadMoreBtn.remove();
                    return;
                }

                const tmp = document.createElement('div');
                tmp.innerHTML = html;
                const cards = Array.from(tmp.querySelectorAll('.explore-visual-card'));
                if (!cards.length) {
                    loadMoreBtn.remove();
                    return;
                }

                cards.forEach((card, index) => {
                    card.classList.add('is-entering');
                    grid.appendChild(card);
                    window.setTimeout(() => card.classList.remove('is-entering'), 220 + (index * 30));
                });
                bindCardInteractions(grid);
                grid.dataset.exploreSkip = String(skip + cards.length);
                if (cards.length < take) loadMoreBtn.remove();
            } finally {
                if (skeleton) skeleton.classList.add('d-none');
                if (document.body.contains(loadMoreBtn)) {
                    loadMoreBtn.dataset.busy = '0';
                    loadMoreBtn.disabled = false;
                }
            }
        };

        loadMoreBtn.addEventListener('click', loadMore);
    }
})();

(function () {
    const tabsRoot = document.querySelector('.js-profile-tabs');
    if (tabsRoot) {
        const tabs = Array.from(tabsRoot.querySelectorAll('.profile-tab'));
        const indicator = tabsRoot.querySelector('.profile-tab-indicator');
        const panels = Array.from(document.querySelectorAll('.profile-tab-panel'));
        const stage = document.querySelector('.profile-grid-stage');

        const moveIndicator = (index) => {
            if (!indicator) return;
            indicator.style.transform = `translateX(${index * 100}%)`;
        };

        const activate = (tabName) => {
            let activeIndex = 0;
            tabs.forEach((tab, index) => {
                const active = tab.dataset.tab === tabName;
                tab.classList.toggle('is-active', active);
                if (active) activeIndex = index;
            });
            panels.forEach((panel) => {
                panel.classList.toggle('is-active', panel.dataset.panel === tabName);
            });
            moveIndicator(activeIndex);
        };

        tabs.forEach((tab) => {
            tab.addEventListener('click', () => {
                activate(tab.dataset.tab);
            });
        });

        if (stage) {
            stage.classList.add('is-loading');
            window.setTimeout(() => stage.classList.remove('is-loading'), 280);
        }

        const active = tabs.find((t) => t.classList.contains('is-active')) || tabs[0];
        if (active?.dataset.tab) activate(active.dataset.tab);
    }
})();

(function () {
    const followBtn = document.querySelector('.js-profile-follow');
    if (!followBtn) return;

    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    followBtn.addEventListener('click', async () => {
        if (followBtn.dataset.busy === '1') return;
        followBtn.dataset.busy = '1';

        const perfilId = followBtn.dataset.perfilId;
        if (!perfilId) {
            followBtn.dataset.busy = '0';
            return;
        }

        try {
            const body = new URLSearchParams();
            body.append('perfilId', perfilId);

            const response = await fetch('/Social/Perfil/AlternarFollow', {
                method: 'POST',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    ...(token ? { 'RequestVerificationToken': token.value } : {})
                },
                body
            });

            if (!response.ok) return;
            const data = await response.json();
            if (!data?.ok) return;

            const seguindo = !!data.seguindo;
            followBtn.dataset.following = seguindo ? '1' : '0';
            followBtn.textContent = seguindo ? 'Seguindo' : 'Seguir';
            followBtn.classList.toggle('secondary', seguindo);
            followBtn.classList.toggle('is-following', seguindo);
            followBtn.classList.toggle('primary', !seguindo);

            const countTarget = document.querySelector('[data-seguidores-count]');
            if (countTarget && Number.isFinite(Number(data.totalSeguidores))) {
                countTarget.textContent = String(data.totalSeguidores);
            }
        } finally {
            followBtn.dataset.busy = '0';
        }
    });
})();

(function () {
    const feedSkeleton = document.getElementById('socialFeedSkeleton');
    const feedStack = document.getElementById('socialFeedStack');
    if (feedSkeleton && feedStack && !feedSkeleton.classList.contains('d-none')) {
        window.setTimeout(() => {
            feedSkeleton.classList.add('d-none');
            feedStack.classList.remove('d-none');
        }, 500);
    }

    async function postForm(form) {
        const token = form.querySelector('input[name="__RequestVerificationToken"]');
        const body = new URLSearchParams(new FormData(form));
        const response = await fetch(form.action, {
            method: 'POST',
            headers: {
                ...(token ? { 'RequestVerificationToken': token.value } : {}),
                'X-Requested-With': 'XMLHttpRequest'
            },
            body
        });
        return response;
    }

    document.querySelectorAll('.js-like-form').forEach((form) => {
        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            const button = form.querySelector('.js-like-btn');
            if (!button) return;
            if (button.dataset.busy === '1') return;
            button.dataset.busy = '1';
            try {
                const response = await postForm(form);
                if (!response.ok) return;
                const data = await response.json();
                if (!data || !data.ok) return;

                const postId = button.dataset.postId;
                const countTarget = document.querySelector(`[data-like-count="${postId}"]`);
                if (countTarget) countTarget.textContent = String(data.totalCurtidas);

                button.classList.toggle('btn-danger', data.usuarioCurtiu);
                button.classList.toggle('btn-outline-danger', !data.usuarioCurtiu);
                button.classList.toggle('is-liked', data.usuarioCurtiu);
            } finally {
                button.dataset.busy = '0';
            }
        });
    });

    document.querySelectorAll('.social-post-image-link').forEach((link) => {
        let clickCount = 0;
        let clickTimer = null;
        link.addEventListener('click', (e) => {
            clickCount += 1;
            if (clickCount === 1) {
                clickTimer = window.setTimeout(() => {
                    clickCount = 0;
                }, 220);
                return;
            }

            if (clickTimer) window.clearTimeout(clickTimer);
            clickCount = 0;
            e.preventDefault();
            const card = link.closest('.social-post-card');
            const form = card ? card.querySelector('.js-like-form') : null;
            if (form) form.requestSubmit();
        });
    });

    document.querySelectorAll('.js-inline-comment-form').forEach((form) => {
        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            const input = form.querySelector('input[name="texto"]');
            if (!input) return;
            const texto = input.value.trim();
            if (!texto) return;

            const response = await postForm(form);
            if (!response.ok) return;
            const data = await response.json();
            if (!data || !data.ok) return;

            input.value = '';
            const card = form.closest('.social-post-card');
            if (!card) return;

            const postId = form.getAttribute('action')?.split('/').pop();
            if (postId) {
                const countTarget = document.querySelector(`[data-comment-count="${postId}"]`);
                if (countTarget) countTarget.textContent = String(data.totalComentarios);
            }

            let list = card.querySelector('.social-comment-preview-list');
            if (!list) {
                list = document.createElement('div');
                list.className = 'social-comment-preview-list';
                form.before(list);
            }

            const p = document.createElement('p');
            const strong = document.createElement('strong');
            strong.textContent = data.nomeAutor + ' ';
            p.appendChild(strong);
            p.appendChild(document.createTextNode(data.texto));
            list.prepend(p);
            while (list.querySelectorAll('p').length > 2) {
                list.querySelector('p:last-of-type')?.remove();
            }
        });
    });

    document.querySelectorAll('.js-share-btn').forEach((button) => {
        button.addEventListener('click', async () => {
            const url = button.dataset.shareUrl;
            if (!url) return;
            if (navigator.share) {
                try {
                    await navigator.share({ url });
                    return;
                } catch { }
            }
            await navigator.clipboard.writeText(url);
            const original = button.innerHTML;
            button.innerHTML = '<i class="fas fa-check"></i> copiado';
            setTimeout(() => { button.innerHTML = original; }, 1300);
        });
    });
})();

(function () {
    const overlay = document.getElementById('storyViewerOverlay');
    if (!overlay) return;

    const img = document.getElementById('storyViewerImage');
    const avatar = document.getElementById('storyViewerAvatar');
    const name = document.getElementById('storyViewerName');
    const time = document.getElementById('storyViewerTime');
    const text = document.getElementById('storyViewerText');
    const progress = document.getElementById('storyViewerProgress');
    const closeBtn = document.getElementById('storyViewerClose');
    const prevBtn = document.getElementById('storyViewerPrev');
    const nextBtn = document.getElementById('storyViewerNext');
    const deleteForm = document.getElementById('storyViewerDeleteForm');
    const deleteId = document.getElementById('storyViewerDeleteId');

    let stories = [];
    let current = -1;
    let timer = null;
    const durationMs = 4500;

    function relTime(dateText) {
        const created = new Date(dateText);
        const diff = Math.floor((Date.now() - created.getTime()) / 1000);
        if (diff < 60) return 'agora';
        if (diff < 3600) return `${Math.floor(diff / 60)} min`;
        if (diff < 86400) return `${Math.floor(diff / 3600)} h`;
        return `${Math.floor(diff / 86400)} d`;
    }

    function clearTimer() {
        if (timer) {
            clearTimeout(timer);
            timer = null;
        }
    }

    function setProgress() {
        if (!progress) return;
        progress.style.transition = 'none';
        progress.style.width = '0%';
        void progress.offsetWidth;
        progress.style.transition = `width ${durationMs}ms linear`;
        progress.style.width = '100%';
    }

    async function markSeen(storyId) {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        if (!tokenInput) return;
        try {
            await fetch('/Social/MarcarStatusComoVisualizado/' + storyId, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': tokenInput.value
                }
            });
        } catch { }
    }

    function render(index) {
        if (index < 0 || index >= stories.length) return;
        current = index;
        const s = stories[current];
        img.src = s.imagemUrl;
        avatar.src = s.fotoPerfilUrl || '/uploads/social/defaults/default-profile.svg';
        name.textContent = s.nomePerfil || 'Perfil';
        time.textContent = relTime(s.criadoEm);
        text.textContent = s.textoOverlay || '';

        if (deleteForm && deleteId) {
            if (s.userId && window.__eventxUserId && Number(window.__eventxUserId) === Number(s.userId)) {
                deleteId.value = s.id;
                deleteForm.classList.remove('d-none');
            } else {
                deleteForm.classList.add('d-none');
            }
        }

        setProgress();
        clearTimer();
        markSeen(s.id);
        timer = setTimeout(() => {
            if (current + 1 < stories.length) render(current + 1);
            else close();
        }, durationMs);
    }

    function close() {
        clearTimer();
        overlay.classList.add('d-none');
        overlay.setAttribute('aria-hidden', 'true');
        current = -1;
        stories = [];
    }

    async function open(storyId, perfilId) {
        const resp = await fetch('/Social/FeedStories');
        if (!resp.ok) return;
        const all = await resp.json();
        stories = all.filter((x) => Number(x.perfilId) === Number(perfilId));
        if (!stories.length) return;
        const start = stories.findIndex((x) => Number(x.id) === Number(storyId));
        overlay.classList.remove('d-none');
        overlay.setAttribute('aria-hidden', 'false');
        render(start >= 0 ? start : 0);
    }

    document.querySelectorAll('.js-story-open').forEach((el) => {
        el.addEventListener('click', (e) => {
            e.preventDefault();
            open(el.dataset.storyId, el.dataset.perfilId);
        });
    });

    closeBtn?.addEventListener('click', close);
    overlay.addEventListener('click', (e) => {
        if (e.target === overlay) close();
    });
    prevBtn?.addEventListener('click', () => {
        if (current > 0) render(current - 1);
    });
    nextBtn?.addEventListener('click', () => {
        if (current + 1 < stories.length) render(current + 1);
        else close();
    });
})();
