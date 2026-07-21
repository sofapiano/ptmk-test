namespace TicketSystem.Core.Entities;

using TicketSystem.Core.States;

public class Ticket
/*
    Сущность "заявка" в системе управления задачами.
    Инкапсулирует данные и бизнес-логику, связанную с жизненным циклом заявки.
    Внутри используется паттерн State для управления состояниями заявки.
*/
{
    public Guid Id { get; }
    public string Number { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public string Description { get; private set; }
    public DateTime Deadline { get; private set; }
    public Employee Author { get; }
    public Employee? Executor { get; private set; }

    private ITicketState _state;

    private Ticket(Guid id, string number, DateTime createdAt, string description,
        DateTime deadline, Employee author)
    {
        Id = id;
        Number = number;
        CreatedAt = createdAt;
        Description = description;
        Deadline = deadline;
        Author = author ?? throw new ArgumentNullException(nameof(author));
        _state = new NewState();
    }

    public Ticket(Guid id, string number, string description, DateTime deadline, Employee author)
        : this(id, number, DateTime.UtcNow, description, deadline, author)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Описание заявки не может быть пустым", nameof(description));

        if (deadline <= DateTime.UtcNow)
            throw new ArgumentException("Срок выполнения должен быть в будущем", nameof(deadline));
    }

    /// <summary>
    /// Фабричный метод восстановления заявки из хранилища (без валидации).
    /// </summary>
    public static Ticket Reconstruct(
        Guid id, string number, DateTime createdAt, string description,
        DateTime deadline, Employee author, Employee? executor,
        ITicketState state)
    {
        var ticket = new Ticket(id, number, createdAt, description, deadline, author);
        ticket.Executor = executor;
        ticket._state = state;
        return ticket;
    }

    public TicketStatus Status => _state.Status;

    public void AssignExecutor(Employee executor)
    {
        Executor = executor ?? throw new ArgumentNullException(nameof(executor));
    }

    public void ChangeState(ITicketState newState)
    {
        _state = newState ?? throw new ArgumentNullException(nameof(newState));
    }

    public bool TryAssignExecutor(Employee executor)
    {
        if (executor == null)
            throw new ArgumentNullException(nameof(executor));

        return _state.TryAssignExecutor(this, executor);
    }

    internal void InternalSetExecutor(Employee executor)
    {
        Executor = executor;
    }

    public bool TryStartWork()
    {
        return _state.TryStartWork(this);
    }

    public bool TryComplete()
    {
        return _state.TryComplete(this);
    }

    public bool IsOverdue(DateTime currentUtcTime)
    {
        return currentUtcTime > Deadline;
    }
}
