namespace TicketSystem.Application.Services;

using System;
using TicketSystem.Application.Contracts;
using TicketSystem.Application.Exceptions;
using TicketSystem.Core.Entities;

public sealed class TicketCompleterService : TicketOperationBase, ITicketCompleter
{
    public TicketCompleterService(ITicketRepository ticketRepo)
        : base(ticketRepo)
    {
    }

    public void CompleteTicket(Guid ticketId)
    {
        var ticket = GetTicketOrThrow(ticketId);

        if (!ticket.TryComplete())
        {
            throw new InvalidTicketStateTransitionException(
                $"Заявку {ticketId} нельзя перевести в статус \"Выполнена\" из её текущего статуса.");
        }

        _ticketRepo.Save(ticket);
    }
}
