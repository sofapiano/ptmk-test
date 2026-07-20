namespace TicketSystem.Core.States;

using TicketSystem.Core.Entities;

public interface ITicketState
/*
    Интерфейс состояния заявки (паттерн State).
    Определяет методы, которые должны реализовывать конкретные состояния заявки,
    каждое состояние инкапсулирует логику переходов и действий, доступных в этом состоянии.
*/
{
    TicketStatus Status { get; }

    bool TryStartWork(Ticket ticket);
    bool TryComplete(Ticket ticket);
    bool TryAssignExecutor(Ticket ticket, Employee executor);
}