namespace TicketSystem.Application.Contracts;

using System;
using System.Collections.Generic;
using TicketSystem.Application.Queries;
using TicketSystem.Core; // для TicketStatus

public interface ITicketQueryService
{
    /// <summary>
    /// Возвращает отфильтрованный список заявок.
    /// </summary>
    IReadOnlyCollection<TicketListDto> GetFilteredTickets(
        TicketStatus? status = null,
        Guid? executorId = null,
        string? department = null,
        bool onlyOverdue = false);
}