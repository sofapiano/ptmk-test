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
    public DateTime CreatedAt { get; }
    
    public string Description { get; private set; }
    public DateTime Deadline { get; private set; }
    public Employee Author { get; }
    public Employee? Executor { get; private set; }
    
    // внутреннее поле для хранения текущего состояния (паттерн State)
    private ITicketState _state;

    public Ticket(Guid id, string description, DateTime deadline, Employee author)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Описание заявки не может быть пустым", nameof(description));
            
        if (deadline <= DateTime.UtcNow)
            throw new ArgumentException("Срок выполнения должен быть в будущем", nameof(deadline));

        Id = id;
        CreatedAt = DateTime.UtcNow;
        Description = description;
        Deadline = deadline;
        Author = author ?? throw new ArgumentNullException(nameof(author));
        
        // По умолчанию новая заявка создается в состоянии "Новая"
        _state = new NewState();
    }

    // Бизнес-метод назначения исполнителя
    public void AssignExecutor(Employee executor)
    {
        Executor = executor ?? throw new ArgumentNullException(nameof(executor));
    }

    // Метод смены состояния, вызываемый из классов состояний (ITicketState)
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

    // Вспомогательный метод, доступный ТОЛЬКО внутри нашего проекта/сборки.
    // Это нужно, чтобы классы состояний (ITicketState) могли записать значение,
    // но при этом внешний код (контроллеры, UI) не мог напрямую поменять исполнителя.
    internal void InternalSetExecutor(Employee executor)
    {
        Executor = executor;
    }

    // Бизнес-действия, делегируемые объекту состояния
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