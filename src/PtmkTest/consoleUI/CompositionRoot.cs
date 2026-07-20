namespace TicketSystem.ConsoleApp;

using Microsoft.Extensions.DependencyInjection;
using TicketSystem.Application.Contracts;
using TicketSystem.Application.Services;
using TicketSystem.Infrastructure.Data;

/// <summary>
/// Единая точка сборки зависимостей приложения (Composition Root).
/// Только здесь Application-слой связывается с конкретными Infrastructure-реализациями.
/// </summary>
public static class CompositionRoot
{
    public static IServiceProvider Build(string connectionString)
    {
        var services = new ServiceCollection();

        // Infrastructure
        services.AddSingleton<ITicketRepository>(_ => new SqlTicketRepository(connectionString));
        services.AddSingleton<IEmployeeRepository>(_ => new SqlEmployeeRepository(connectionString));
        services.AddSingleton<ITicketNumberGenerator>(_ => new TicketNumberGenerator(connectionString));
        services.AddSingleton<IReportingService>(_ => new ReportingService(connectionString));

        // Application
        services.AddSingleton<IEmployeeManagementService, EmployeeManagementService>();
        services.AddSingleton<ITicketCreator, TicketCreatorService>();
        services.AddSingleton<ITicketAssigner, TicketAssignerService>();
        services.AddSingleton<ITicketWorkStarter, TicketWorkStarterService>();
        services.AddSingleton<ITicketCompleter, TicketCompleterService>();
        services.AddSingleton<ITicketQueryService, TicketQueryService>();

        return services.BuildServiceProvider();
    }
}