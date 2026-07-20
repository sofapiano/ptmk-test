namespace TicketSystem.Application.Contracts;

using System;
using TicketSystem.Core.Entities;

public interface ITicketCompleter
{
    void CompleteTicket(Guid ticketId);
}
