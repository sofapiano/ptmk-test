namespace TicketSystem.Infrastructure.Data;

using System;

/// <summary>
/// Служебная модель строки таблицы Tickets, используемая только для
/// Dapper multi-mapping в SqlTicketRepository. Не является доменной сущностью.
/// </summary>
internal sealed class TicketRow
{
    public Guid Id { get; set; }
    public string Number { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public string Description { get; set; } = default!;
    public DateTime Deadline { get; set; }
    public Guid AuthorId { get; set; }
    public Guid? ExecutorId { get; set; }
    public int Status { get; set; }
}