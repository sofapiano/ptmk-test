namespace TicketSystem.Application.Queries;

using System;

/// <summary>
/// DTO для безопасной передачи данных о заявке на уровень UI.
/// </summary>
public record TicketListDto(
    Guid Id,
    string Number,
    string Description,
    DateTime CreatedAt,
    DateTime Deadline,
    string AuthorFullName,
    string? ExecutorFullName,
    string StatusName,
    bool IsOverdue
);