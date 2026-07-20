namespace TicketSystem.Application.Services;

using System;
using TicketSystem.Application.Contracts;
using TicketSystem.Application.Exceptions;
using TicketSystem.Core.Entities;

public abstract class TicketOperationBase
{
    protected readonly ITicketRepository _ticketRepo;

    protected TicketOperationBase(ITicketRepository ticketRepo)
    {
        _ticketRepo = ticketRepo ?? throw new ArgumentNullException(nameof(ticketRepo));
    }

    protected Ticket GetTicketOrThrow(Guid ticketId)
    {
        return _ticketRepo.GetById(ticketId)
            ?? throw new EntityNotFoundException(nameof(Ticket), ticketId);
    }
}
