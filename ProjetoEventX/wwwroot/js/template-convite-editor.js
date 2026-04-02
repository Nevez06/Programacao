(() => {
    const config = window.templateEditorConfig || {};
    const templateId = Number(config.templateId || 0);
    const canvas = document.getElementById('inviteCanvas');
    const grid = document.getElementById('canvasGrid');
    const layersList = document.getElementById('layersList');
    const uploadInput = document.getElementById('imageUploadInput');

    if (!canvas || !templateId) {
        return;
    }

    const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';

    const propText = document.getElementById('propText');
    const propFont = document.getElementById('propFont');
    const propFontSize = document.getElementById('propFontSize');
    const propColor = document.getElementById('propColor');
    const propBackground = document.getElementById('propBackground');
    const propWidth = document.getElementById('propWidth');
    const propHeight = document.getElementById('propHeight');
    const propX = document.getElementById('propX');
    const propY = document.getElementById('propY');
    const propImageUrl = document.getElementById('propImageUrl');
    const canvasBackground = document.getElementById('canvasBackground');

    const zoomOut = document.getElementById('zoomOut');
    const zoomIn = document.getElementById('zoomIn');
    const zoomReset = document.getElementById('zoomReset');
    const toggleGrid = document.getElementById('toggleGrid');
    const bringFront = document.getElementById('bringFront');
    const sendBack = document.getElementById('sendBack');
    const btnDelete = document.getElementById('btnDelete');
    const btnDuplicate = document.getElementById('btnDuplicate');
    const btnSave = document.getElementById('btnSave');
    const btnExportPng = document.getElementById('btnExportPng');
    const btnUploadImageTop = document.getElementById('btnUploadImageTop');
    const btnUploadImageProperty = document.getElementById('btnUploadImageProperty');

    let zoom = 1;
    let selectedId = null;
    let layerCounter = 1;
    let uploadTargetElementId = null;

    let state = {
        background: '#ffffff',
        width: 760,
        height: 1080,
        elements: []
    };

    if (config.layoutJson && typeof config.layoutJson === 'object') {
        state = { ...state, ...config.layoutJson };
    }

    if (!Array.isArray(state.elements)) {
        state.elements = [];
    }

    const nums = state.elements
        .map(e => parseInt(String(e.id || '').replace('el', ''), 10))
        .filter(n => !Number.isNaN(n));

    if (nums.length > 0) {
        layerCounter = Math.max(...nums);
    }

    function normalizeColor(value) {
        if (!value || value === 'transparent') return '#ffffff';
        if (String(value).startsWith('#')) return value;
        return '#ffffff';
    }

    function getNextZIndex() {
        return state.elements.length ? Math.max(...state.elements.map(e => Number(e.zIndex || 1))) + 1 : 1;
    }

    function makeBaseElement(type) {
        return {
            id: `el${++layerCounter}`,
            type,
            text: type === 'title' ? 'Novo título' : type === 'button' ? 'Novo botão' : type === 'image' ? '' : 'Novo texto',
            x: 100,
            y: 100 + (state.elements.length * 25),
            width: type === 'button' ? 240 : type === 'image' ? 320 : 280,
            height: type === 'button' ? 54 : type === 'image' ? 220 : type === 'title' ? 80 : 60,
            zIndex: getNextZIndex(),
            color: type === 'button' ? '#ffffff' : '#0f172a',
            background: type === 'button' ? '#992008' : 'transparent',
            fontSize: type === 'title' ? 42 : 24,
            fontFamily: "'Inter', sans-serif",
            fontWeight: type === 'title' ? '800' : '600',
            textAlign: 'center',
            imageUrl: ''
        };
    }

    function getSelected() {
        return state.elements.find(e => e.id === selectedId);
    }

    function createElementNode(item) {
        const el = document.createElement('div');
        el.className = `canvas-element canvas-${item.type}`;
        el.dataset.id = item.id;
        el.style.left = `${item.x}px`;
        el.style.top = `${item.y}px`;
        el.style.width = `${item.width}px`;
        el.style.height = `${item.height}px`;
        el.style.zIndex = item.zIndex;
        el.style.color = item.color;
        el.style.background = item.background;
        el.style.fontSize = `${item.fontSize}px`;
        el.style.fontFamily = item.fontFamily;
        el.style.fontWeight = item.fontWeight || '600';
        el.style.textAlign = item.textAlign || 'center';

        if (item.type === 'button' || item.type === 'title' || item.type === 'text') {
            el.classList.add(item.type === 'button' ? 'canvas-button' : 'canvas-text');
            el.textContent = item.text || '';
        }

        if (item.type === 'image') {
            if (item.imageUrl) {
                el.innerHTML = `<img src="${item.imageUrl}" alt="Imagem do convite" draggable="false" />`;
                el.style.background = 'transparent';
            } else {
                el.innerHTML = '<div class="placeholder">Imagem</div>';
            }
        }

        el.addEventListener('click', (e) => {
            e.stopPropagation();
            selectElement(item.id);
        });

        canvas.appendChild(el);
        makeInteractable(el);
    }

    function refreshSelection() {
        canvas.querySelectorAll('.canvas-element').forEach(el => {
            el.classList.toggle('selected', el.dataset.id === selectedId);
        });
    }

    function renderLayers() {
        layersList.innerHTML = '';
        [...state.elements]
            .sort((a, b) => Number(b.zIndex) - Number(a.zIndex))
            .forEach(item => {
                const row = document.createElement('div');
                row.className = `layer-item${item.id === selectedId ? ' active' : ''}`;
                row.innerHTML = `
                    <div class="layer-item-left">
                        <i class="fas fa-layer-group"></i>
                        <span>${item.text || item.type}</span>
                    </div>
                    <small>#${item.zIndex}</small>
                `;
                row.addEventListener('click', () => selectElement(item.id));
                layersList.appendChild(row);
            });
    }

    function fillProperties() {
        const item = getSelected();
        if (!item) return;

        propText.value = item.text || '';
        propFont.value = item.fontFamily || "'Inter', sans-serif";
        propFontSize.value = item.fontSize || 16;
        propColor.value = normalizeColor(item.color || '#000000');
        propBackground.value = normalizeColor(item.background || '#ffffff');
        propWidth.value = item.width || 100;
        propHeight.value = item.height || 40;
        propX.value = item.x || 0;
        propY.value = item.y || 0;
        propImageUrl.value = item.imageUrl || '';
    }

    function selectElement(id) {
        selectedId = id;
        refreshSelection();
        fillProperties();
        renderLayers();
    }

    function renderCanvas() {
        canvas.querySelectorAll('.canvas-element').forEach(el => el.remove());
        canvas.style.background = state.background || '#ffffff';
        canvas.style.width = `${state.width || 760}px`;
        canvas.style.height = `${state.height || 1080}px`;
        canvasBackground.value = normalizeColor(state.background || '#ffffff');

        state.elements
            .sort((a, b) => Number(a.zIndex) - Number(b.zIndex))
            .forEach(createElementNode);

        renderLayers();
        refreshSelection();
    }

    function updateSelectedFromProps() {
        const item = getSelected();
        if (!item) return;

        item.text = propText.value;
        item.fontFamily = propFont.value;
        item.fontSize = parseInt(propFontSize.value || '16', 10);
        item.color = propColor.value;
        item.background = propBackground.value;
        item.width = parseInt(propWidth.value || '100', 10);
        item.height = parseInt(propHeight.value || '40', 10);
        item.x = parseInt(propX.value || '0', 10);
        item.y = parseInt(propY.value || '0', 10);
        item.imageUrl = propImageUrl.value;

        renderCanvas();
        selectElement(item.id);
    }

    function makeInteractable(target) {
        interact(target)
            .draggable({
                listeners: {
                    move(event) {
                        const id = event.target.dataset.id;
                        const item = state.elements.find(e => e.id === id);
                        if (!item) return;

                        item.x += event.dx;
                        item.y += event.dy;
                        event.target.style.left = `${item.x}px`;
                        event.target.style.top = `${item.y}px`;

                        if (selectedId === id) {
                            propX.value = Math.round(item.x);
                            propY.value = Math.round(item.y);
                        }
                    }
                }
            })
            .resizable({
                edges: { left: true, right: true, bottom: true, top: true },
                listeners: {
                    move(event) {
                        const id = event.target.dataset.id;
                        const item = state.elements.find(e => e.id === id);
                        if (!item) return;

                        item.width = Math.max(40, event.rect.width);
                        item.height = Math.max(20, event.rect.height);
                        item.x += event.deltaRect.left;
                        item.y += event.deltaRect.top;

                        event.target.style.width = `${item.width}px`;
                        event.target.style.height = `${item.height}px`;
                        event.target.style.left = `${item.x}px`;
                        event.target.style.top = `${item.y}px`;

                        if (selectedId === id) {
                            propWidth.value = Math.round(item.width);
                            propHeight.value = Math.round(item.height);
                            propX.value = Math.round(item.x);
                            propY.value = Math.round(item.y);
                        }
                    }
                },
                modifiers: [
                    interact.modifiers.restrictSize({
                        min: { width: 40, height: 20 }
                    })
                ]
            });
    }

    async function uploadImageAndGetUrl() {
        const file = uploadInput.files?.[0];
        if (!file) return null;

        const formData = new FormData();
        formData.append('file', file);
        formData.append('templateId', String(templateId));

        const response = await fetch(config.uploadImageUrl, {
            method: 'POST',
            headers: { RequestVerificationToken: antiForgeryToken },
            body: formData
        });

        if (!response.ok) {
            throw new Error('Falha HTTP no upload.');
        }

        const result = await response.json();
        if (!result.success || !result.url) {
            throw new Error(result.message || 'Não foi possível fazer upload da imagem.');
        }

        return result.url;
    }

    async function exportCanvasToPng() {
        const previousTransform = canvas.style.transform;
        canvas.style.transform = 'none';

        try {
            const capture = await html2canvas(canvas, {
                useCORS: true,
                allowTaint: false,
                backgroundColor: null,
                scale: 3,
                logging: false
            });

            const link = document.createElement('a');
            link.href = capture.toDataURL('image/png', 1.0);
            link.download = `convite-template-${templateId}.png`;
            document.body.appendChild(link);
            link.click();
            link.remove();
        } finally {
            canvas.style.transform = previousTransform;
        }
    }

    document.querySelectorAll('[data-add]').forEach(btn => {
        btn.addEventListener('click', () => {
            const element = makeBaseElement(btn.dataset.add);
            state.elements.push(element);
            renderCanvas();
            selectElement(element.id);
        });
    });

    [propText, propFont, propFontSize, propColor, propBackground, propWidth, propHeight, propX, propY, propImageUrl].forEach(input => {
        input?.addEventListener('input', updateSelectedFromProps);
    });

    canvasBackground.addEventListener('input', () => {
        state.background = canvasBackground.value;
        canvas.style.background = state.background;
    });

    canvas.addEventListener('click', () => {
        selectedId = null;
        refreshSelection();
        renderLayers();
    });

    zoomIn.addEventListener('click', () => {
        zoom = Math.min(1.6, zoom + 0.1);
        canvas.style.transform = `scale(${zoom})`;
    });

    zoomOut.addEventListener('click', () => {
        zoom = Math.max(0.5, zoom - 0.1);
        canvas.style.transform = `scale(${zoom})`;
    });

    zoomReset.addEventListener('click', () => {
        zoom = 1;
        canvas.style.transform = 'scale(1)';
    });

    toggleGrid.addEventListener('click', () => {
        grid.style.display = grid.style.display === 'none' ? 'block' : 'none';
    });

    bringFront.addEventListener('click', () => {
        const item = getSelected();
        if (!item) return;
        item.zIndex = getNextZIndex();
        renderCanvas();
        selectElement(item.id);
    });

    sendBack.addEventListener('click', () => {
        const item = getSelected();
        if (!item) return;
        item.zIndex = 1;
        state.elements
            .filter(e => e.id !== item.id)
            .forEach(e => e.zIndex = Number(e.zIndex) + 1);
        renderCanvas();
        selectElement(item.id);
    });

    btnDelete.addEventListener('click', () => {
        if (!selectedId) return;
        state.elements = state.elements.filter(e => e.id !== selectedId);
        selectedId = null;
        renderCanvas();
    });

    btnDuplicate.addEventListener('click', () => {
        const item = getSelected();
        if (!item) return;
        const copy = {
            ...item,
            id: `el${++layerCounter}`,
            x: Number(item.x) + 25,
            y: Number(item.y) + 25,
            zIndex: getNextZIndex()
        };
        state.elements.push(copy);
        renderCanvas();
        selectElement(copy.id);
    });

    btnSave.addEventListener('click', async () => {
        const payload = {
            templateId,
            layoutJson: JSON.stringify(state)
        };

        const response = await fetch(config.saveLayoutUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                RequestVerificationToken: antiForgeryToken
            },
            body: JSON.stringify(payload)
        });

        const result = await response.json();
        if (!result.success) {
            alert(result.message || 'Erro ao salvar layout.');
            return;
        }

        alert('Layout salvo com sucesso.');
    });

    btnExportPng.addEventListener('click', async () => {
        try {
            await exportCanvasToPng();
        } catch (error) {
            console.error(error);
            alert('Não foi possível exportar o PNG.');
        }
    });

    btnUploadImageTop.addEventListener('click', () => {
        uploadTargetElementId = null;
        uploadInput.value = '';
        uploadInput.click();
    });

    btnUploadImageProperty.addEventListener('click', () => {
        const selected = getSelected();
        if (!selected) {
            alert('Selecione um elemento para aplicar a imagem.');
            return;
        }
        uploadTargetElementId = selected.id;
        uploadInput.value = '';
        uploadInput.click();
    });

    uploadInput.addEventListener('change', async () => {
        if (!uploadInput.files?.length) {
            return;
        }

        try {
            const url = await uploadImageAndGetUrl();
            if (!url) return;

            if (uploadTargetElementId) {
                const target = state.elements.find(e => e.id === uploadTargetElementId);
                if (target) {
                    target.type = 'image';
                    target.imageUrl = url;
                    target.text = '';
                    target.background = 'transparent';
                    renderCanvas();
                    selectElement(target.id);
                }
            } else {
                const imageElement = makeBaseElement('image');
                imageElement.imageUrl = url;
                state.elements.push(imageElement);
                renderCanvas();
                selectElement(imageElement.id);
            }
        } catch (error) {
            console.error(error);
            alert(error.message || 'Erro no upload da imagem.');
        }
    });

    renderCanvas();
    if (state.elements.length > 0) {
        selectElement(state.elements[0].id);
    }
})();
