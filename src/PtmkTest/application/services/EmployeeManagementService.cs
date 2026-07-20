namespace TicketSystem.Application.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using TicketSystem.Application.Contracts;
using TicketSystem.Application.Commands;
using TicketSystem.Application.Queries;
using TicketSystem.Application.Exceptions;
using TicketSystem.Core.Entities;

public sealed class EmployeeManagementService : IEmployeeManagementService
{
    private readonly IEmployeeRepository _employeeRepo;

    public EmployeeManagementService(IEmployeeRepository employeeRepo)
    {
        _employeeRepo = employeeRepo ?? throw new ArgumentNullException(nameof(employeeRepo));
    }

    public Guid CreateEmployee(CreateEmployeeDto dto)
    {
        var employee = new Employee(Guid.NewGuid(), dto.FullName, dto.Department, dto.Position);
        _employeeRepo.Add(employee);

        return employee.Id;
    }

    public void UpdateEmployee(UpdateEmployeeDto dto)
    {
        // 1. Получаем сотрудника или выбрасываем специфичное исключение
        var employee = _employeeRepo.GetById(dto.Id)
            ?? throw new EntityNotFoundException(nameof(Employee), dto.Id);

        // 2. Используем бизнес-методы сущности для изменения состояния
        if (!string.IsNullOrWhiteSpace(dto.NewDepartment))
        {
            employee.TransferToDepartment(dto.NewDepartment);
        }

        if (!string.IsNullOrWhiteSpace(dto.NewPosition))
        {
            employee.PromoteToPosition(dto.NewPosition);
        }
    }

    public IReadOnlyCollection<EmployeeDto> GetAllEmployees()
    {
        // Получаем все записи и маппим их в плоские DTO
        return _employeeRepo.GetAll()
            .Select(e => new EmployeeDto(e.Id, e.FullName, e.Department, e.Position))
            .ToList();
    }

    public EmployeeDto GetEmployeeById(Guid id)
    {
        var employee = _employeeRepo.GetById(id)
            ?? throw new EntityNotFoundException(nameof(Employee), id);

        return new EmployeeDto(employee.Id, employee.FullName, employee.Department, employee.Position);
    }
}