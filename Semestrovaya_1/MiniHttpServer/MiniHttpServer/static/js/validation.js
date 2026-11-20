(function () {
    const rules = {
        name: {
            pattern: /^[A-Za-z0-9\s\-',.]{3,100}$/,
            message: 'Название: 3–100 символов, буквы, цифры, пробелы и дефисы.'
        },
        manaCost: {
            pattern: /^(\d|[WUBRGCwubrgc]){0,10}$/,
            message: 'Мана: только цифры и символы WUBRGC, до 10 символов.'
        },
        login: {
            pattern: /^[a-zA-Z0-9_]{3,20}$/,
            message: 'Логин: 3–20 символов, латиница, цифры, подчёркивание.'
        }
    };

    document.querySelectorAll('form').forEach(function (form) {
        form.querySelectorAll('input[name], textarea[name]').forEach(function (field) {
            const rule = rules[field.name];

            const error = document.createElement('span');
            error.className = 'field-error';
            error.hidden = true;
            field.insertAdjacentElement('afterend', error);

            function check() {
                let message = '';
                if (field.required && field.value.trim().length === 0) {
                    message = 'Обязательное поле.';
                } else if (rule && field.value.length > 0 && !rule.pattern.test(field.value)) {
                    message = rule.message;
                }
                error.textContent = message;
                error.hidden = message.length === 0;
                field.classList.toggle('input--invalid', message.length > 0);
                return message.length === 0;
            }

            field.addEventListener('input', check);
            field.addEventListener('blur', check);
            field._check = check;

            const max = parseInt(field.getAttribute('maxlength') || '0', 10);
            if (max > 0) {
                const counter = document.createElement('span');
                counter.className = 'field-counter';
                field.insertAdjacentElement('afterend', counter);
                const update = function () {
                    counter.textContent = field.value.length + ' / ' + max;
                };
                field.addEventListener('input', update);
                update();
            }
        });

        form.addEventListener('submit', function (e) {
            let ok = true;
            form.querySelectorAll('input[name], textarea[name]').forEach(function (f) {
                if (f._check && !f._check()) ok = false;
            });
            if (!ok) e.preventDefault();
        });
    });
})();