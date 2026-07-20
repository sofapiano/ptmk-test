namespace TicketSystem.Application.DTOs;

using System;

/// <summary>
/// Данные, необходимые для создания новой заявки.
/// Исполнитель на момент создания опционален — заявка может быть создана
/// "в общей очереди" и назначена позже через TicketManagementService.AssignTicket.
/// </summary>
public sealed record CreateTicketDto(
    string Description,
    DateTime Deadline,
    Guid AuthorId,
    Guid? ExecutorId = null);