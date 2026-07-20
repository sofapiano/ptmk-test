namespace TicketSystem.Application.Exceptions;

public sealed class InvalidTicketStateTransitionException : Exception
/*
    Выбрасывается, когда доменный объект (Ticket через ITicketState) отклонил
    операцию как недопустимую в текущем статусе (например, попытка сразу
    перевести заявку из "Новая" в "Выполнена", либо назначить исполнителя
    на уже выполненную заявку)
*/
{
    public InvalidTicketStateTransitionException(string message) : base(message)
    {
    }
}