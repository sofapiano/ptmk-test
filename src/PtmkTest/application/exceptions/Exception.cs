namespace TicketSystem.Application.Exceptions;

public abstract class ApplicationLayerException : Exception
/*
    Базовый класс для всех исключений прикладного (Application) уровня
*/
{
    protected ApplicationLayerException(string message) : base(message)
    {
    }

    protected ApplicationLayerException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}