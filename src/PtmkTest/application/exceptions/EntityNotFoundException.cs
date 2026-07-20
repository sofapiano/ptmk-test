namespace TicketSystem.Application.Exceptions;

public sealed class EntityNotFoundException : Exception
/*
    Выбрасывается, когда сущность с указанным идентификатором не найдена
    в соответствующем репозитории. Отдельный тип исключения позволяет
    вызывающему коду (например, контроллеру) отличить "не найдено" (404)
    от прочих ошибок и не заставляет сервисы возвращать null / кортежи ошибок
*/
{
    public string EntityName { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityName, object entityId)
        : base($"{entityName} с идентификатором '{entityId}' не найден(а).")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}