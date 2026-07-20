namespace TicketSystem.Core.States;

using TicketSystem.Core.Entities;

public sealed class InProgressState : ITicketState
/*
    Состояние заявки "В работе"
    В этом состоянии заявка может быть завершена, но не может быть переведена обратно в состояние "Новая".
*/
{
    public bool TryStartWork(Ticket ticket)
    {
        return false;
    }

    public bool TryComplete(Ticket ticket)
    {
        ticket.ChangeState(new CompletedState());
        return true;
    }
    public bool TryAssignExecutor(Ticket ticket, Employee executor)
    {
        ticket.InternalSetExecutor(executor);
        return true;
    }
}