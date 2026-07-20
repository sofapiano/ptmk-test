namespace TicketSystem.Infrastructure.Data;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Npgsql;
using TicketSystem.Application.Contracts;
using TicketSystem.Core;
using TicketSystem.Core.Entities;
using TicketSystem.Core.States;

public class SqlTicketRepository : ITicketRepository
{
    private readonly string _connectionString;

    public SqlTicketRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Save(Ticket ticket)
    {
        using IDbConnection db = new NpgsqlConnection(_connectionString);

        // Используем UPSERT (ON CONFLICT) для вставки или обновления
        const string sql = @"
            INSERT INTO Tickets (Id, CreatedAt, Description, Deadline, AuthorId, ExecutorId, Status)
            VALUES (@Id, @CreatedAt, @Description, @Deadline, @AuthorId, @ExecutorId, @Status)
            ON CONFLICT (Id) DO UPDATE 
            SET ExecutorId = EXCLUDED.ExecutorId,
                Status = EXCLUDED.Status;";

        TicketStatus status = ticket.Status;

        db.Execute(sql, new
        {
            ticket.Id,
            ticket.CreatedAt,
            ticket.Description,
            ticket.Deadline,
            AuthorId = ticket.Author.Id,
            ExecutorId = ticket.Executor?.Id,
            Status = (int)status
        });
    }

    public Ticket? GetById(Guid id)
    {
        // В реальном проекте здесь будет JOIN с таблицей Employees, 
        // затем вызов фабричного метода Ticket.Reconstruct()
        throw new NotImplementedException("Реализация чтения с JOIN опущена для краткости");
    }

    public IReadOnlyCollection<Ticket> GetFiltered(
        TicketStatus? status = null, Guid? executorId = null, string? department = null, bool onlyOverdue = false)
    {
        using IDbConnection db = new NpgsqlConnection(_connectionString);
        var builder = new SqlBuilder();
        var template = builder.AddTemplate("SELECT * FROM Tickets t INNER JOIN Employees e ON t.ExecutorId = e.Id /**where**/");

        if (status.HasValue) builder.Where("t.Status = @Status", new { Status = (int)status.Value });
        if (executorId.HasValue) builder.Where("t.ExecutorId = @ExecutorId", new { ExecutorId = executorId.Value });
        if (!string.IsNullOrEmpty(department)) builder.Where("e.Department = @Department", new { Department = department });
        if (onlyOverdue) builder.Where("t.Deadline < @Now", new { Now = DateTime.UtcNow });

        // Возвращаем отфильтрованные данные (маппинг опущен для простоты)
        return new List<Ticket>();
    }
}