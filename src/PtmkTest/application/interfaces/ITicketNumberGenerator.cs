namespace TicketSystem.Application.Contracts;

public interface ITicketNumberGenerator
{
    /// Генерирует следующий уникальный номер заявки (например, "T-1001")
    string GenerateNext();
}