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
        using IDbConnection db = new NpgsqlConnection(_connectionString);

        const string sql = @"
            SELECT t.*, a.*, ex.*
            FROM Tickets t
            JOIN Employees a ON t.AuthorId = a.Id
            LEFT JOIN Employees ex ON t.ExecutorId = ex.Id
            WHERE t.Id = @Id";

        var result = db.Query<TicketRow, Employee, Employee, Ticket>(
            sql,
            (row, author, executor) => Ticket.Reconstruct(
                row.Id, row.Number, row.CreatedAt, row.Description, row.Deadline,
                author, executor, ResolveState(row.Status)),
            new { Id = id },
            splitOn: "Id,Id");

        return result.FirstOrDefault();
    }

    private static ITicketState ResolveState(int status) => (TicketStatus)status switch
    {
        TicketStatus.New => new NewState(),
        TicketStatus.InProgress => new InProgressState(),
        TicketStatus.Completed => new CompletedState(),
        _ => throw new InvalidOperationException($"Неизвестный статус заявки: {status}")
    };

    public IReadOnlyCollection<Ticket> GetFiltered(
    TicketStatus? status = null, Guid? executorId = null, string? department = null, bool onlyOverdue = false)
    {
        using IDbConnection db = new NpgsqlConnection(_connectionString);
        var builder = new SqlBuilder();
        var template = builder.AddTemplate(@"
            SELECT t.*, a.*, ex.*
            FROM Tickets t
            JOIN Employees a ON t.AuthorId = a.Id
            LEFT JOIN Employees ex ON t.ExecutorId = ex.Id
            /**where**/
            ORDER BY t.Deadline");

        if (status.HasValue) builder.Where("t.Status = @Status", new { Status = (int)status.Value });
        if (executorId.HasValue) builder.Where("t.ExecutorId = @ExecutorId", new { ExecutorId = executorId.Value });
        if (!string.IsNullOrEmpty(department)) builder.Where("ex.Department = @Department", new { Department = department });
        if (onlyOverdue) builder.Where("t.Deadline < @Now", new { Now = DateTime.UtcNow });

        var tickets = db.Query<TicketRow, Employee, Employee, Ticket>(
            template.RawSql,
            (row, author, executor) => Ticket.Reconstruct(
                row.Id, row.Number, row.CreatedAt, row.Description, row.Deadline,
                author, executor, ResolveState(row.Status)),
            template.Parameters,
            splitOn: "Id,Id");

        return tickets.ToList();
    }
}