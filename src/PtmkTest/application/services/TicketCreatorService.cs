namespace TicketSystem.Application;

using System;
using TicketSystem.Application.Commands;
using TicketSystem.Application.Contracts;
using TicketSystem.Application.Exceptions;
using TicketSystem.Core.Entities;

public sealed class TicketCreatorService : TicketOperationBase, ITicketCreator
{
    private readonly IEmployeeRepository _employeeRepo;
    private readonly ITicketNumberGenerator _numberGenerator;

    public TicketCreatorService(
        ITicketRepository ticketRepo, 
        IEmployeeRepository employeeRepo,
        ITicketNumberGenerator numberGenerator)
        : base(ticketRepo)
    {
        _employeeRepo = employeeRepo ?? throw new ArgumentNullException(nameof(employeeRepo));
        _numberGenerator = numberGenerator ?? throw new ArgumentNullException(nameof(numberGenerator));
    }

    public Ticket CreateTicket(CreateTicketDto dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        var author = _employeeRepo.GetById(dto.AuthorId)
            ?? throw new EntityNotFoundException(nameof(Employee), dto.AuthorId);

        // Генерируем уникальный номер перед созданием доменного объекта
        var ticketNumber = _numberGenerator.GenerateNext();

        // Передаем сгенерированный номер в конструктор
        var ticket = new Ticket(Guid.NewGuid(), ticketNumber, dto.Description, dto.Deadline, author);

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