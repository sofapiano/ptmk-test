namespace TicketSystem.Application.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using TicketSystem.Application.Contracts;
using TicketSystem.Application.Queries;
using TicketSystem.Core;
using TicketSystem.Core.Entities;

public sealed class TicketQueryService : ITicketQueryService
{
    private readonly ITicketRepository _ticketRepo;

    public TicketQueryService(ITicketRepository ticketRepo)
    {
        _ticketRepo = ticketRepo ?? throw new ArgumentNullException(nameof(ticketRepo));
    }

    public IReadOnlyCollection<TicketListDto> GetFilteredTickets(
        TicketStatus? status = null,
        Guid? executorId = null,
        string? department = null,
        bool onlyOverdue = false)
    {
        // 1. Получаем данные из репозитория
        var tickets = _ticketRepo.GetFiltered(status, executorId, department, onlyOverdue);

        // 2. Маппим доменные сущности в DTO
        var dtos = tickets.Select(MapToDto).ToList();

        return dtos;
    }

    // Приватный метод для маппинга инкапсулирует логику преобразования
    private TicketListDto MapToDto(Ticket ticket)
    {
        // В реальном проекте статус можно определять гибче, 
        // но здесь мы можем опираться на тип состояния (паттерн State)
        return new TicketListDto(
        Id: ticket.Id,
        Number: ticket.Number,
        Description: ticket.Description,
        CreatedAt: ticket.CreatedAt,
        Deadline: ticket.Deadline,
        AuthorFullName: ticket.Author.FullName,
        ExecutorFullName: ticket.Executor?.FullName,
        StatusName: TicketStatusNames.Get(ticket.Status),
        IsOverdue: ticket.IsOverdue(DateTime.UtcNow)
        );
    }
}