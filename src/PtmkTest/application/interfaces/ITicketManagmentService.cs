namespace TicketSystem.Application.Contracts;

using System;
using TicketSystem.Application.Commands;
using TicketSystem.Core.Entities;

public interface ITicketManagementService
/*
    Интерфейс сервиса управления заявками
*/
{
    Ticket CreateTicket(CreateTicketDto dto);

    void AssignTicket(Guid ticketId, Guid executorId);

    void StartTicketWork(Guid ticketId);

    void CompleteTicket(Guid ticketId);
}