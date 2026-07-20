namespace TicketSystem.Application.Commands;

using System;

public record UpdateEmployeeDto(Guid Id, string? NewDepartment, string? NewPosition);