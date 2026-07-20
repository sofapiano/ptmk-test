namespace TicketSystem.ConsoleApp;

using System;
using Microsoft.Extensions.DependencyInjection;
using TicketSystem.Application.Commands;
using TicketSystem.Application.Contracts;
using TicketSystem.Application.Exceptions;
using TicketSystem.Core;

public static class Program
{
    public static void Main(string[] args)
    {
        // В реальном проекте — из appsettings.json / переменной окружения
        var connectionString = Environment.GetEnvironmentVariable("TICKETSYSTEM_CONNECTION")
            ?? "Host=localhost;Database=ticketsystem;Username=postgres;Password=postgres";

        var provider = CompositionRoot.Build(connectionString);

        var employeeService = provider.GetRequiredService<IEmployeeManagementService>();
        var ticketCreator = provider.GetRequiredService<ITicketCreator>();
        var ticketAssigner = provider.GetRequiredService<ITicketAssigner>();
        var ticketWorkStarter = provider.GetRequiredService<ITicketWorkStarter>();
        var ticketCompleter = provider.GetRequiredService<ITicketCompleter>();
        var ticketQueryService = provider.GetRequiredService<ITicketQueryService>();
        var reportingService = provider.GetRequiredService<IReportingService>();

        RunMenuLoop(
            employeeService, ticketCreator, ticketAssigner,
            ticketWorkStarter, ticketCompleter, ticketQueryService, reportingService);
    }

    private static void RunMenuLoop(
        IEmployeeManagementService employeeService,
        ITicketCreator ticketCreator,
        ITicketAssigner ticketAssigner,
        ITicketWorkStarter ticketWorkStarter,
        ITicketCompleter ticketCompleter,
        ITicketQueryService ticketQueryService,
        IReportingService reportingService)
    {
        while (true)
        {
            PrintMenu();
            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1": CreateEmployee(employeeService); break;
                    case "2": ListEmployees(employeeService); break;
                    case "3": CreateTicket(ticketCreator, employeeService); break;
                    case "4": AssignTicket(ticketAssigner); break;
                    case "5": StartTicketWork(ticketWorkStarter); break;
                    case "6": CompleteTicket(ticketCompleter); break;
                    case "7": ListTickets(ticketQueryService); break;
                    case "8": ShowReport(reportingService).GetAwaiter().GetResult(); break;
                    case "0": return;
                    default: Console.WriteLine("Неизвестная команда."); break;
                }
            }
            catch (EntityNotFoundException ex)
            {
                Console.WriteLine($"[Ошибка] {ex.Message}");
            }
            catch (InvalidTicketStateTransitionException ex)
            {
                Console.WriteLine($"[Ошибка бизнес-правила] {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"[Ошибка ввода] {ex.Message}");
            }

            Console.WriteLine();
        }
    }

    private static void PrintMenu()
    {
        Console.WriteLine("=== Система учёта заявок сотрудников ===");
        Console.WriteLine("1. Добавить сотрудника");
        Console.WriteLine("2. Список сотрудников");
        Console.WriteLine("3. Создать заявку");
        Console.WriteLine("4. Назначить исполнителя");
        Console.WriteLine("5. Взять заявку в работу");
        Console.WriteLine("6. Завершить заявку");
        Console.WriteLine("7. Список заявок (с фильтрами)");
        Console.WriteLine("8. Отчёт");
        Console.WriteLine("0. Выход");
        Console.Write("> ");
    }

    private static void CreateEmployee(IEmployeeManagementService service)
    {
        Console.Write("ФИО: ");
        var fullName = Console.ReadLine() ?? "";
        Console.Write("Подразделение: ");
        var department = Console.ReadLine() ?? "";
        Console.Write("Должность: ");
        var position = Console.ReadLine() ?? "";

        var id = service.CreateEmployee(new CreateEmployeeDto(fullName, department, position));
        Console.WriteLine($"Сотрудник создан. Id = {id}");
    }

    private static void ListEmployees(IEmployeeManagementService service)
    {
        var employees = service.GetAllEmployees();
        foreach (var e in employees)
            Console.WriteLine($"{e.Id} | {e.FullName} | {e.Department} | {e.Position}");
    }

    private static void CreateTicket(ITicketCreator creator, IEmployeeManagementService employeeService)
    {
        Console.Write("Описание: ");
        var description = Console.ReadLine() ?? "";

        Console.Write("Срок выполнения (yyyy-MM-dd HH:mm): ");
        var deadline = DateTime.Parse(Console.ReadLine() ?? "");

        Console.Write("Id автора: ");
        var authorId = Guid.Parse(Console.ReadLine() ?? "");

        Console.Write("Id исполнителя (Enter — пропустить): ");
        var executorInput = Console.ReadLine();
        Guid? executorId = string.IsNullOrWhiteSpace(executorInput) ? null : Guid.Parse(executorInput);

        var ticket = creator.CreateTicket(new CreateTicketDto(description, deadline, authorId, executorId));
        Console.WriteLine($"Заявка создана: {ticket.Number} (Id = {ticket.Id})");
    }

    private static void AssignTicket(ITicketAssigner assigner)
    {
        Console.Write("Id заявки: ");
        var ticketId = Guid.Parse(Console.ReadLine() ?? "");
        Console.Write("Id нового исполнителя: ");
        var executorId = Guid.Parse(Console.ReadLine() ?? "");

        assigner.AssignTicket(ticketId, executorId);
        Console.WriteLine("Исполнитель назначен.");
    }

    private static void StartTicketWork(ITicketWorkStarter workStarter)
    {
        Console.Write("Id заявки: ");
        var ticketId = Guid.Parse(Console.ReadLine() ?? "");

        workStarter.StartTicketWork(ticketId);
        Console.WriteLine("Заявка переведена в статус \"В работе\".");
    }

    private static void CompleteTicket(ITicketCompleter completer)
    {
        Console.Write("Id заявки: ");
        var ticketId = Guid.Parse(Console.ReadLine() ?? "");

        completer.CompleteTicket(ticketId);
        Console.WriteLine("Заявка переведена в статус \"Выполнена\".");
    }

    private static void ListTickets(ITicketQueryService queryService)
    {
        Console.Write("Статус (0-Новая, 1-В работе, 2-Выполнена, Enter — все): ");
        var statusInput = Console.ReadLine();
        TicketStatus? status = string.IsNullOrWhiteSpace(statusInput)
            ? null
            : (TicketStatus)int.Parse(statusInput);

        Console.Write("Id исполнителя (Enter — все): ");
        var executorInput = Console.ReadLine();
        Guid? executorId = string.IsNullOrWhiteSpace(executorInput) ? null : Guid.Parse(executorInput);

        Console.Write("Подразделение (Enter — все): ");
        var department = Console.ReadLine();
        department = string.IsNullOrWhiteSpace(department) ? null : department;

        Console.Write("Только просроченные? (y/n): ");
        var onlyOverdue = (Console.ReadLine() ?? "").Trim().Equals("y", StringComparison.OrdinalIgnoreCase);

        var tickets = queryService.GetFilteredTickets(status, executorId, department, onlyOverdue);

        foreach (var t in tickets)
        {
            var overdueMark = t.IsOverdue ? " [ПРОСРОЧЕНА]" : "";
            Console.WriteLine(
                $"{t.Number} | {t.StatusName} | {t.Deadline:yyyy-MM-dd} | " +
                $"Автор: {t.AuthorFullName} | Исполнитель: {t.ExecutorFullName ?? "—"}{overdueMark}");
        }
    }

    private static async Task ShowReport(IReportingService reportingService)
    {
        var report = await reportingService.GetSummaryReportAsync();

        Console.WriteLine("--- Заявки по статусам ---");
        foreach (var s in report.StatusCounts)
            Console.WriteLine($"{s.StatusName}: {s.Count}");

        Console.WriteLine($"\nПросроченных заявок: {report.OverdueCount}");

        Console.WriteLine("\n--- Выполнено по исполнителям ---");
        foreach (var e in report.ExecutorCompletedCounts)
            Console.WriteLine($"{e.ExecutorFullName}: {e.CompletedCount}");
    }
}