# Gatherer Clone - семестровая работа

Клон каталога карт Magic: The Gathering (gatherer.wizards.com) на собственном HTTP-сервере: маршрутизация через атрибуты, свой шаблонизатор, своя ORM поверх Npgsql/PostgreSQL, административная панель, Docker-развёртывание.

## Стек
- C# / .NET 10, консольное приложение + HttpListener
- PostgreSQL, Npgsql
- Собственные библиотеки: Framework (сервер, маршруты, результаты, сессии), HtmlTemplating (шаблонизатор), ORM (OrmContext, SqlBuilder, EntityMapper)
- HTML / CSS / JavaScript без фреймворков

## Быстрый старт (Docker)

    docker compose up --build

Открыть http://localhost:8888/cards/list
База инициализируется автоматически из sql/01_schema.sql и sql/02_seed.sql.

## Локальный запуск
1. Установить PostgreSQL и создать базу oriscards.
2. Выполнить sql/01_schema.sql, затем sql/02_seed.sql.
3. Проверить MiniHttpServer/settings.json (ConnectionString, AdminPasswordHash).
4. Запустить: dotnet run --project MiniHttpServer

## Административная панель
Адрес: http://localhost:8888/auth/login
Логин: admin. Пароль по умолчанию: admin123
В конфигурации хранится SHA256-хеш пароля, а не сам пароль.

## Маршруты
| URL | Метод | Обработчик | Назначение |
|---|---|---|---|
| /cards/list | GET | CardsController.List | каталог: пагинация, сортировка, поиск |
| /cards/details?id=N | GET | CardsController.Details | страница карты |
| /cards/search?q=... | GET | CardsController.Search | JSON для живого поиска (AJAX) |
| /auth/login | GET/POST | AuthController.Login | форма входа и проверка |
| /auth/logout | POST | AuthController.Logout | выход |
| /admin/index | GET | AdminController.Index | дашборд (требуется сессия) |
| /admin/create | GET/POST | AdminController.Create | создание карты |
| /admin/edit?id=N | GET/POST | AdminController.Edit | редактирование карты |
| /admin/delete | POST | AdminController.Delete | удаление карты |

## Архитектура обработки запроса

    HttpListener -> HttpServer -> LoggingHandler -> StaticFilesHandler ->
    ControllersHandler -> NotFoundHandler

    ControllersHandler: RouteResolver (атрибуты [Controller]/[Get]/[Post]) ->
    ParameterBinder -> проверка [Authorize] через UserSessionService ->
    метод контроллера -> Repository -> OrmContext -> PostgreSQL ->
    PageResult -> HtmlTemplateRenderer -> HTML-ответ

## Надёжность
- Любое исключение обработчика превращается в 500/503/409, процесс не падает.
- NpgsqlException (нет соединения) -> 503; PostgresException класса 23xxx -> 409.
- Лимит глубины рекурсии шаблона (защита от StackOverflow).
- Лимит размера тела запроса, таймаут подключения к БД.
- Глобальные обработчики UnhandledException и UnobservedTaskException.

## Валидация
Одни и те же правила на трёх уровнях: HTML5 pattern в формах,
static/js/validation.js (мгновенная подсветка),
Validation/CardValidator.cs (сервер, регулярки имени и мана-стоимости).

## Тесты

    dotnet test

122 теста: маршрутизация, привязка параметров, шаблонизатор, ORM, сессии.

## Данные
278 карт сета Magic: The Gathering | The Hobbit собраны со страниц Gatherer,
изображения хранятся локально в static/images/cards (001.webp ... 278.webp),
поэтому сайт работает без внешнего интернета.