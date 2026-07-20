namespace TicketSystem.Application;

using System;
using TicketSystem.Application.Contracts;
using TicketSystem.Application.Exceptions;
using TicketSystem.Core.Entities;

public sealed class TicketManagementService : ITicketManagementService
/*
Оркестрирует прикладные сценарии работы с заявками
*/
{
    private readonly ITicketRepository _ticketRepo;
    private readonly IEmployeeRepository _employeeRepo;

    public TicketManagementService(ITicketRepository ticketRepo, IEmployeeRepository employeeRepo)
    {
        _ticketRepo = ticketRepo ?? throw new ArgumentNullException(nameof(ticketRepo));
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

            // Для новой заявки TryAssignExecutor всегда вернёт true (см. NewState),
            // но результат всё равно проверяем — не полагаемся на побочное знание
            // о текущей реализации состояния (Protected Variations)
            if (!ticket.TryAssignExecutor(executor))
            {
                throw new InvalidTicketStateTransitionException(
                    "Не удалось назначить исполнителя при создании заявки.");
            }
        }

        _ticketRepo.Save(ticket);
        return ticket;
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

    public void StartTicketWork(Guid ticketId)
    {
        var ticket = GetTicketOrThrow(ticketId);

        if (!ticket.TryStartWork())
        {
            throw new InvalidTicketStateTransitionException(
                $"Заявку {ticketId} нельзя перевести в статус \"В работе\" из её текущего статуса.");
        }

        _ticketRepo.Save(ticket);
    }

    public void CompleteTicket(Guid ticketId)
    {
        var ticket = GetTicketOrThrow(ticketId);

        if (!ticket.TryComplete())
        {
            // В частности сюда попадёт попытка перевести заявку
            // напрямую из "Новая" в "Выполнена" — правило из ТЗ.
            throw new InvalidTicketStateTransitionException(
                $"Заявку {ticketId} нельзя перевести в статус \"Выполнена\" из её текущего статуса.");
        }

        _ticketRepo.Save(ticket);
    }

    private Ticket GetTicketOrThrow(Guid ticketId)
    {
        return _ticketRepo.GetById(ticketId)
            ?? throw new EntityNotFoundException(nameof(Ticket), ticketId);
    }
}