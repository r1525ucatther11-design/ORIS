// Отправляем форму через fetch, потом показываем alert
document.addEventListener('DOMContentLoaded', function () {
    const form = document.querySelector('form');
    if (!form) return;

    form.addEventListener('submit', async function (e) {
        e.preventDefault(); // отменяем стандартную отправку (иначе страница перезагрузится)

        // Собираем данные формы
        const formData = new FormData(form);
        const params = new URLSearchParams();
        for (const [key, value] of formData.entries()) {
            params.append(key, value);
        }

        // Отправляем на сервер POST-запрос
        try {
            await fetch(form.action, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: params.toString()
            });
        } catch (err) {
            console.error('Ошибка отправки:', err);
        }

        // Показываем алерт — как в прошлом задании
        alert('Вход выполнен!');
    });
});