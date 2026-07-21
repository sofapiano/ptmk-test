-- =========================================================
-- 003_seed_tickets.sql
-- Тестовые данные: 1 000 000 заявок.
--
-- Номера генерируются через ту же последовательность
-- ticket_number_seq, что использует приложение в реальной работе,
-- так тестовые данные не создают пересечений с номерами,
-- сгенерированными позже через TicketNumberGenerator.
--
-- Deadline специально разбросан в диапазоне +-90 дней от NOW(),
-- чтобы в данных гарантированно присутствовала значимая доля
-- просроченных заявок (Deadline < NOW()) для показательного
-- замера целевого запроса.
-- =========================================================

INSERT INTO Tickets (Id, Number, CreatedAt, Description, Deadline, AuthorId, ExecutorId, Status)
SELECT
    gen_random_uuid(),
    'T-' || nextval('ticket_number_seq'),
    NOW() - (random() * INTERVAL '180 days'),
    'Заявка №' || i,
    NOW() - INTERVAL '90 days' + (random() * INTERVAL '180 days'),
    emp_ids.author_id,
    emp_ids.executor_id,
    (ARRAY[0, 1, 2])[1 + floor(random() * 3)]
FROM generate_series(1, 1000000) AS i
CROSS JOIN LATERAL (
    SELECT
        (SELECT Id FROM Employees OFFSET floor(random() * 1000) LIMIT 1) AS author_id,
        (SELECT Id FROM Employees OFFSET floor(random() * 1000) LIMIT 1) AS executor_id
) AS emp_ids;