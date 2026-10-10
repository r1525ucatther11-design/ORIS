# Домашнее задание №4 — CustomHttpServer

Я сделала учебный HTTP-сервер на C# (.NET 10). Он принимает запросы
от браузера, сам находит нужный контроллер и метод, а потом возвращает
сверстанную HTML-страницу или JSON.

Сервер работает на цепочке обработчиков (Chain of Responsibility).

## Из чего состоит

### Цепочка обработчиков

Handler — это абстрактный класс. Внутри есть свойство Successor —
ссылка на следующий обработчик, и абстрактный метод HandleRequest.
Каждый обработчик решает, обработать запрос самому или передать дальше.

StaticFileHandler отвечает за статику (HTML, CSS, JS, картинки).
Он смотрит, есть ли в URL точка. Если есть — значит это файл, читает
его и отправляет. Если точки нет — передаёт запрос дальше по цепочке.

ControllerHandler обрабатывает контроллеры. Он разбирает URL на части,
через рефлексию находит нужный класс контроллера (по атрибуту
HttpController), находит нужный метод (по атрибуту Get или Post),
собирает параметры из URL, query-строки и тела POST-формы, вызывает
метод и отправляет ответ.

В HttpServer я строю цепочку так:
staticFileHandler.Successor = controllerHandler;
и потом вызываю HandleRequest.

### Атрибуты

- HttpController("имя") — навешивается на класс контроллера.
- Get("route") — навешивается на GET-метод.
- Post("route") — навешивается на POST-метод.
- FromForm — параметр берётся из тела POST-формы.
- FromQuery — параметр берётся из query-строки.

### Ответы

- IHttpResponseTypeResult — интерфейс с методом Execute(), который
  возвращает массив байтов.
- HtmlResult — HTML-ответ.
- JsonResult — JSON-ответ.
- ControllerBase — базовый класс для контроллеров. Внутри есть методы
  Html() и Json(), которые создают нужный результат.

### Контроллеры

DashboardController (атрибут HttpController("Dashboard")):
- Get("profile") — метод GetProfile(int userId). Читает profile.html,
  подставляет userId и возвращает страницу.
- Get("activities") — метод GetActivities(int userId). Читает
  activities.html, подставляет userId и список активностей.
- Get("json") — метод GetJson(). Возвращает JSON.

AuthController (атрибут HttpController("auth")):
- Get("login") — возвращает форму логина.
- Post("login") — принимает login и password, выводит их в консоль.

SearchController (атрибут HttpController("Search")):
- Get("go") — принимает query из строки запроса и возвращает нужную
  страницу.

### Ошибки

- Если контроллер не найден — возвращаю 404.
- Если метод не найден — возвращаю 404.
- Если файл не найден — отдаю 404.html.
- Если внутри метода произошла ошибка — возвращаю 500.

## Как запустить

Открыть терминал в папке проекта и выполнить:

dotnet build
dotnet run

Сервер запустится на http://127.0.0.1:8888/

Чтобы остановить — написать stop в терминале и нажать Enter.

## Какие адреса открывать

- http://127.0.0.1:8888/search-engine.html — поисковик
- http://127.0.0.1:8888/Dashboard/profile/1 — профиль пользователя №1
- http://127.0.0.1:8888/Dashboard/profile/999 — профиль пользователя №999
- http://127.0.0.1:8888/Dashboard/activities/42 — активности №42
- http://127.0.0.1:8888/Dashboard/activities/999 — активности №999
- http://127.0.0.1:8888/Dashboard/json — JSON-ответ
- http://127.0.0.1:8888/auth/login — форма логина
- http://127.0.0.1:8888/Search/go?query=steam — поиск «steam»
(satisfactory пока что нет)

Ещё можно писать /Dashboard/profile/user/1 — работает так же,
как /Dashboard/profile/1.

## Как это работает по шагам

Например, я открываю /Dashboard/profile/1:

1. Запрос приходит в HttpServer.
2. Он передаёт его в StaticFileHandler.
3. В URL нет точки — значит это не файл. Передаём дальше.
4. ControllerHandler разбирает URL на части: Dashboard, profile, 1.
5. Через рефлексию находит класс DashboardController.
6. Находит метод GetProfile с атрибутом Get("profile").
7. Передаёт в метод userId = 1.
8. Метод читает profile.html, подставляет userId и возвращает HtmlResult.
9. ControllerHandler вызывает Execute() и отправляет результат в браузер.


## Что использовала

- C# / .NET 10
- HttpListener — встроенный HTTP-сервер
- Рефлексию, чтобы искать контроллеры и методы
- HTML / CSS
- fetch — чтобы отправлять POST-форму из JS