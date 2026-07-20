namespace TicketSystem.Application.Queries;

using System;

public record EmployeeDto(Guid Id, string FullName, string Department, string Position);