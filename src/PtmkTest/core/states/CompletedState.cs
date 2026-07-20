namespace TicketSystem.Core.States;

using TicketSystem.Core.Entities;

public sealed class CompletedState : ITicketState
/*
    Состояние заявки "Выполнена"
    В этом состоянии заявка не может быть переведена в другие состояния
*/
{
    public TicketStatus Status => TicketStatus.Completed;

    public bool TryStartWork(Ticket ticket)
    {
        return false;
    }

    public bool TryComplete(Ticket ticket)
    {
        return false;
    }
    public bool TryAssignExecutor(Ticket ticket, Employee executor)
    {
        return false;
    }
}