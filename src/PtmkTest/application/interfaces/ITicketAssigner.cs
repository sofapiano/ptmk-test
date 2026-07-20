namespace TicketSystem.Application.Contracts;

using System;
using TicketSystem.Core.Entities;

public interface ITicketAssigner
{
    void AssignTicket(Guid ticketId, Guid executorId);
}
