namespace TicketSystem.Application.Contracts;

using System;
using TicketSystem.Core.Entities;

public interface ITicketWorkStarter
{
    void StartTicketWork(Guid ticketId);
}
