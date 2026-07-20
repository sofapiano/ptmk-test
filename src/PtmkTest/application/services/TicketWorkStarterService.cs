namespace TicketSystem.Application.Services;

using System;
using TicketSystem.Application.Contracts;
using TicketSystem.Application.Exceptions;
using TicketSystem.Core.Entities;

public sealed class TicketWorkStarterService : TicketOperationBase, ITicketWorkStarter
{
    public TicketWorkStarterService(ITicketRepository ticketRepo)
        : base(ticketRepo)
    {
    }

    public void StartTicketWork(Guid ticketId)
    {
        var ticket = GetTicketOrThrow(ticketId);

        if (!ticket.TryStartWork())
        {
            throw new InvalidTicketStateTransitionException(
                $"Заявку {ticketId} нельзя перевести в статус \"В работе\" из её текущего статуса.");
        }

        _ticketRepo.Save(ticket);
    }
}
