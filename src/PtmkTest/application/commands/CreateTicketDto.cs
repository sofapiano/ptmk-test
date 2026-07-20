namespace TicketSystem.Application.Commands;

/// <summary>
/// DTO для создания новой заявки (Command).
/// </summary>
public record CreateTicketDto(string Description, DateTime Deadline, Guid AuthorId, Guid? ExecutorId = null);
