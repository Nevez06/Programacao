(function () {
    const input = document.querySelector('[data-social-preview-input]');
    const target = document.querySelector('[data-social-preview-target]');

    if (!input || !target) {
        return;
    }

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
})();

(function () {
    const chips = document.querySelectorAll('.social-category-chips .chip');
    if (!chips.length) return;

    chips.forEach((chip) => {
        chip.addEventListener('click', () => {
            chips.forEach((c) => c.classList.remove('active'));
            chip.classList.add('active');
        });
    });
})();
