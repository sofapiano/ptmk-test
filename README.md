# Инструкция по запуску и проверке проекта

## 1. Предварительные требования

- Docker + Docker Compose
- .NET SDK 8.0+ (`dotnet --version`)
- (опционально) `psql` клиент или любой GUI (DBeaver/pgAdmin) для ручных проверок и `EXPLAIN ANALYZE`

## 2. Структура проекта

```
/
├── docker-compose.yml
├── db/
│   └── migrations/
│       ├── 001_schema.sql
│       ├── 002_seed_employees.sql
│       └── 003_seed_tickets.sql
└── src/
    ├── TicketSystem.Core/
    ├── TicketSystem.Application/
    ├── TicketSystem.Infrastructure/
    └── TicketSystem.ConsoleApp/
```

## 3. Поднятие базы данных

```bash
docker compose up -d
```

Дождаться готовности контейнера:

```bash
docker compose ps
```

В колонке `STATUS` должно появиться `healthy` (это займёт ~10-20 секунд — за это время Postgres успевает инициализироваться и выполнить все `.sql`-файлы из `db/migrations`, включая генерацию 1000 сотрудников и 1 000 000 заявок).

Первая инициализация с генерацией 1 млн заявок может занять от нескольких секунд до пары минут — это нормально, следить за прогрессом можно через:

```bash
docker compose logs -f postgres
```

## 4. Проверка загрущки данных

```bash
docker exec -it ticketsystem-postgres psql -U postgres -d ticketsystem -c "SELECT count(*) FROM Employees;"
docker exec -it ticketsystem-postgres psql -U postgres -d ticketsystem -c "SELECT count(*) FROM Tickets;"
```

Ожидаемый результат: `1000` и `1000000` соответственно.

## 5. Сборка и запуск консольного приложения

Из корня проекта:

```bash
cd src/TicketSystem.ConsoleApp
dotnet restore
dotnet build
```

Задать строку подключения (по умолчанию в `Program.cs` уже указана строка, совпадающая с `docker-compose.yml`, но лучше явно):

**Linux/macOS:**
```bash
export TICKETSYSTEM_CONNECTION="Host=localhost;Database=ticketsystem;Username=postgres;Password=postgres"
```

**Windows (PowerShell):**
```powershell
$env:TICKETSYSTEM_CONNECTION="Host=localhost;Database=ticketsystem;Username=postgres;Password=postgres"
```

Запуск:

```bash
dotnet run
```

Должно появиться интерактивное меню:

```
=== Система учёта заявок сотрудников ===
1. Добавить сотрудника
2. Список сотрудников
3. Создать заявку
4. Назначить исполнителя
5. Взять заявку в работу
6. Завершить заявку
7. Список заявок (с фильтрами)
8. Отчёт
0. Выход
>
```

## 6. Проверка функциональности (сценарий)

Рекомендуемый порядок ручной проверки, покрывающий все требования docs.txt:

1. **Пункт 1** — создать сотрудника (запомнить выведенный `Id`, понадобится далее).
2. **Пункт 2** — убедиться, что созданный сотрудник появился в списке.
3. **Пункт 3** — создать заявку, указав `Id` автора из шага 1. Оставить исполнителя пустым (Enter). Запомнить `Id` заявки из вывода.
4. **Пункт 7** — вывести список заявок без фильтров, убедиться, что новая заявка в статусе "Новая".
5. **Пункт 4** — назначить исполнителя на заявку (можно того же сотрудника или любой существующий `Id` из seed-данных — их можно достать через `SELECT Id FROM Employees LIMIT 5;` в psql).
6. **Пункт 6** — попытаться сразу завершить свежесозданную заявку (минуя "В работе") — ожидается **ошибка бизнес-правила** (`InvalidTicketStateTransitionException`): заявку нельзя перевести в "Выполнена" из "Новая". Это ключевая проверка правила из docs.txt.
7. **Пункт 5** — перевести заявку в "В работе" — должно пройти успешно.
8. **Пункт 6** — повторно завершить заявку — теперь должно пройти успешно.
9. **Пункт 7** — проверить фильтры по очереди: по статусу (`2` — Выполнена), по `executorId`, по `department`, по флагу "только просроченные" (`y`) — используя seed-данные, где просроченные заявки заведомо есть.
10. **Пункт 8** — вывести отчёт, убедиться, что:
    - сумма `StatusCounts` по всем статусам совпадает с `1 000 001` (1 млн seed + созданная вручную заявка);
    - `OverdueCount` больше нуля;
    - `ExecutorCompletedCounts` содержит хотя бы вашего исполнителя с `CompletedCount = 1`.

### 7. Замер производительности целевого запроса

Подключиться к БД напрямую:

```bash
docker exec -it ticketsystem-postgres psql -U postgres -d ticketsystem
```

Получить `Id` реального исполнителя, у которого есть заявки в статусе "В работе":

```sql
SELECT ExecutorId, count(*) 
FROM Tickets 
WHERE Status = 1 AND ExecutorId IS NOT NULL 
GROUP BY ExecutorId 
ORDER BY count(*) DESC 
LIMIT 1;
```

**Замер "до" оптимизации** — временно удалить индекс:

```sql
DROP INDEX IF EXISTS IX_Tickets_Executor_Status_Deadline;

EXPLAIN ANALYZE
SELECT *
FROM Tickets
WHERE ExecutorId = '<подставить-guid-из-предыдущего-запроса>'
  AND Status = 1
  AND Deadline < NOW()
ORDER BY Deadline;
```

Сохранить вывод (обратить внимание на `Seq Scan`, `Execution Time`).

**Замер "после" оптимизации** — вернуть индекс и повторить тот же запрос:

```sql
CREATE INDEX IX_Tickets_Executor_Status_Deadline
    ON Tickets (ExecutorId, Status, Deadline);

EXPLAIN ANALYZE
SELECT *
FROM Tickets
WHERE ExecutorId = '<тот-же-guid>'
  AND Status = 1
  AND Deadline < NOW()
ORDER BY Deadline;
```

Сохранить вывод (ожидается `Index Scan` вместо `Seq Scan`, значительно меньший `Execution Time`).

Оба вывода `EXPLAIN ANALYZE` (до/после) вставить в отчёт как есть — это и есть требуемое docs.txt "описание оптимизации и результатов замеров".

### 8. Остановка и полная очистка

Остановить без удаления данных:
```bash
docker compose down
```

Остановить и **полностью удалить БД** (в т.ч. чтобы seed-скрипты выполнились заново при следующем `up`):
```bash
docker compose down -v
<<<<<<< HEAD
```
=======
```
>>>>>>> d5f68da4743fcc7ebeeb27ed822ef8e61d59833b
