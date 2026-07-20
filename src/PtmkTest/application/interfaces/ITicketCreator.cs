namespace TicketSystem.Application.Contracts;

using System;
using TicketSystem.Core.Entities;

public interface ITicketCreator
{
    Ticket CreateTicket(CreateTicketDto dto);
}
