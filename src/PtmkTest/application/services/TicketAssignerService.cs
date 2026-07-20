namespace TicketSystem.Application;

using System;
using TicketSystem.Application.Contracts;
using TicketSystem.Application.Exceptions;
using TicketSystem.Core.Entities;

public sealed class TicketAssignerService : TicketOperationBase, ITicketAssigner
{
    private readonly IEmployeeRepository _employeeRepo;

    public TicketAssignerService(ITicketRepository ticketRepo, IEmployeeRepository employeeRepo)
        : base(ticketRepo)
    {
        _employeeRepo = employeeRepo ?? throw new ArgumentNullException(nameof(employeeRepo));
    }

    public void AssignTicket(Guid ticketId, Guid executorId)
    {
        var ticket = GetTicketOrThrow(ticketId);

        var executor = _employeeRepo.GetById(executorId)
            ?? throw new EntityNotFoundException(nameof(Employee), executorId);

        if (!ticket.TryAssignExecutor(executor))
        {
            throw new InvalidTicketStateTransitionException(
                $"Заявку {ticketId} нельзя переназначить в её текущем статусе (заявка уже выполнена).");
        }

        _ticketRepo.Save(ticket);
    }
}
