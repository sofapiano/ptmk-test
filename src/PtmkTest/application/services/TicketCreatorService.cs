namespace TicketSystem.Application;

using System;
using TicketSystem.Application.Contracts;
using TicketSystem.Application.Exceptions;
using TicketSystem.Core.Entities;

public sealed class TicketCreatorService : TicketOperationBase, ITicketCreator
{
    private readonly IEmployeeRepository _employeeRepo;

    public TicketCreatorService(ITicketRepository ticketRepo, IEmployeeRepository employeeRepo)
        : base(ticketRepo)
    {
        _employeeRepo = employeeRepo ?? throw new ArgumentNullException(nameof(employeeRepo));
    }

    public Ticket CreateTicket(CreateTicketDto dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        var author = _employeeRepo.GetById(dto.AuthorId)
            ?? throw new EntityNotFoundException(nameof(Employee), dto.AuthorId);

        var ticket = new Ticket(Guid.NewGuid(), dto.Description, dto.Deadline, author);

        if (dto.ExecutorId is { } executorId)
        {
            var executor = _employeeRepo.GetById(executorId)
                ?? throw new EntityNotFoundException(nameof(Employee), executorId);

            if (!ticket.TryAssignExecutor(executor))
            {
                throw new InvalidTicketStateTransitionException(
                    "Не удалось назначить исполнителя при создании заявки.");
            }
        }

        _ticketRepo.Save(ticket);
        return ticket;
    }
}
