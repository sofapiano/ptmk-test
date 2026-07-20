namespace TicketSystem.Application.Contracts;

using System;
using System.Collections.Generic;
using TicketSystem.Core.Entities;

public interface IEmployeeRepository
/*
    Контракт доступа к справочнику сотрудников
*/
{
    Employee? GetById(Guid id);

    IReadOnlyCollection<Employee> GetAll();

    void Add(Employee employee);
}