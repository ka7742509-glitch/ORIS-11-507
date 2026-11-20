(function () {
    const form = document.querySelector('.site-header__search');
    if (!form) return;
    const input = form.querySelector('input[name="q"]');
    if (!input) return;

    const box = document.createElement('div');
    box.className = 'search-dropdown';
    box.hidden = true;
    form.appendChild(box);

    let timer = null;
    let controller = null;

    input.addEventListener('input', function () {
        clearTimeout(timer);
        const q = input.value.trim();
        if (q.length < 2) {
            box.hidden = true;
            box.innerHTML = '';
            return;
        }
        timer = setTimeout(function () { load(q); }, 300);
    });

    async function load(q) {
        if (controller) controller.abort();
        controller = new AbortController();
        try {
            const res = await fetch('/cards/search?q=' + encodeURIComponent(q), { signal: controller.signal });
            if (!res.ok) return;
            render(await res.json());
        } catch (e) {
            if (e.name !== 'AbortError') console.error(e);
        }
    }

    function render(cards) {
        box.innerHTML = '';
        if (!cards.length) {
            const empty = document.createElement('div');
            empty.className = 'search-dropdown__empty';
            empty.textContent = 'Ничего не найдено';
            box.appendChild(empty);
            box.hidden = false;
            return;
        }
        cards.forEach(function (card) {
            const a = document.createElement('a');
            a.className = 'search-dropdown__item';
            a.href = '/cards/details?id=' + card.id;

            const num = document.createElement('span');
            num.className = 'search-dropdown__num';
            num.textContent = '#' + card.number;

            const name = document.createElement('span');
            name.textContent = card.name;

            a.appendChild(num);
            a.appendChild(name);
            box.appendChild(a);
        });
        box.hidden = false;
    }

    document.addEventListener('click', function (e) {
        if (!form.contains(e.target)) box.hidden = true;
    });

    input.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') box.hidden = true;
    });
})();