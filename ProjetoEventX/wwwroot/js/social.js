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
