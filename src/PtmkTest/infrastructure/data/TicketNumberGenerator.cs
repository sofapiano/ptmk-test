namespace TicketSystem.Infrastructure.Data;

using System;
using System.Data;
using Dapper;
using Npgsql;
using TicketSystem.Application.Contracts;

/// <summary>
/// Генератор номеров заявок на основе PostgreSQL SEQUENCE.
/// Гарантирует уникальность и монотонность номеров даже при конкурентных
/// вставках (генерация номера происходит атомарно на стороне СУБД).
/// </summary>
public class TicketNumberGenerator : ITicketNumberGenerator
{
    private readonly string _connectionString;

    public TicketNumberGenerator(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public string GenerateNext()
    {
        using IDbConnection db = new NpgsqlConnection(_connectionString);

        const string sql = "SELECT nextval('ticket_number_seq')";

        long next = db.ExecuteScalar<long>(sql);

        return $"T-{next}";
    }
}