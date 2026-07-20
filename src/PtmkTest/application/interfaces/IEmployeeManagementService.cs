namespace TicketSystem.Application.Contracts;

using System;
using System.Collections.Generic;
using TicketSystem.Application.Commands;
using TicketSystem.Application.Queries;

public interface IEmployeeManagementService
{
    Guid CreateEmployee(CreateEmployeeDto dto);
    void UpdateEmployee(UpdateEmployeeDto dto);
    IReadOnlyCollection<EmployeeDto> GetAllEmployees();
    EmployeeDto GetEmployeeById(Guid id);
}