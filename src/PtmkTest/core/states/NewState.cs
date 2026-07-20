namespace TicketSystem.Core.States;

using TicketSystem.Core.Entities;

public sealed class NewState : ITicketState
/*
    Состояние заявки"Новая"
    Определяет поведение заявки, когда она находится в состоянии "Новая".
    В этом состоянии заявка может быть переведена в состояние "В работе", но не может быть завершена напрямую.
*/
{
    public bool TryStartWork(Ticket ticket)
    {
        ticket.ChangeState(new InProgressState());
        return true;
    }

    public bool TryComplete(Ticket ticket)
    {
        // напрямую завершить новую заявку нельзя по бизнес-требованиям
        return false;
    }
    public bool TryAssignExecutor(Ticket ticket, Employee executor)
    {
        ticket.InternalSetExecutor(executor);
        return true;
    }
}