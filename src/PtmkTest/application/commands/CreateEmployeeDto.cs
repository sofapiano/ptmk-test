namespace TicketSystem.Application.Commands;

using System;

public record CreateEmployeeDto(string FullName, string Department, string Position);