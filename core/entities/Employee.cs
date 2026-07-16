namespace TicketSystem.Core.Entities;

public class Employee
/*
    Сущность "сотрудник" в системе управления задачами.
    Инкапсулирует данные и бизнес-логику, связанную с сотрудником.
    Внутри используется паттерн Value Object для управления неизменяемыми свойствами.
*/
{
    public Guid Id { get; }

    public string FullName { get; private set; }
    public string Department { get; private set; }
    public string Position { get; private set; }

    public Employee(Guid id, string fullName, string department, string position)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Идентификатор сотрудника не может быть пустым", nameof(id));

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("ФИО сотрудника не может быть пустым", nameof(fullName));

        if (string.IsNullOrWhiteSpace(department))
            throw new ArgumentException("Подразделение не может быть пустым", nameof(department));

        if (string.IsNullOrWhiteSpace(position))
            throw new ArgumentException("Должность не может быть пустой", nameof(position));

        Id = id;
        FullName = fullName.Trim();
        Department = department.Trim();
        Position = position.Trim();
    }

    // Смена должности/отдела
    public void TransferToDepartment(string newDepartment)
    {
        if (string.IsNullOrWhiteSpace(newDepartment))
            throw new ArgumentException("Новое подразделение не может быть пустым", nameof(newDepartment));

        Department = newDepartment.Trim();
    }

    public void PromoteToPosition(string newPosition)
    {
        if (string.IsNullOrWhiteSpace(newPosition))
            throw new ArgumentException("Новая должность не может быть пустой", nameof(newPosition));

        Position = newPosition.Trim();
    }
}