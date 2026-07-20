namespace TicketSystem.Infrastructure.Data;

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using TicketSystem.Application.Contracts;
using TicketSystem.Application.Queries;
using TicketSystem.Core;

public class ReportingService : IReportingService
{
    private readonly string _connectionString;

    public ReportingService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<TicketSummaryReport> GetSummaryReportAsync()
    {
        using IDbConnection db = new NpgsqlConnection(_connectionString);

        // 1. Считаем статусы (читаем сырой код, имя мапим в C#)
        const string statusSql = @"
            SELECT Status as StatusCode, COUNT(*) as Count 
            FROM Tickets 
            GROUP BY Status";
        var statusRows = await db.QueryAsync<StatusCountRow>(statusSql);

        var statuses = statusRows
            .Select(r => new StatusReportItem(
                TicketStatusNames.Get((TicketStatus)r.StatusCode),
                r.Count))
            .ToList();

        // 2. Считаем просроченные
        const string overdueSql = "SELECT COUNT(*) FROM Tickets WHERE Deadline < @Now";
        var overdueCount = await db.ExecuteScalarAsync<int>(overdueSql, new { Now = DateTime.UtcNow });

        // 3. Считаем выполненные по исполнителям
        const string executorSql = @"
            SELECT e.Id as ExecutorId, e.FullName as ExecutorFullName, COUNT(t.Id) as CompletedCount
            FROM Tickets t
            JOIN Employees e ON t.ExecutorId = e.Id
            WHERE t.Status = @CompletedStatus
            GROUP BY e.Id, e.FullName";
        var executors = await db.QueryAsync<ExecutorReportItem>(
            executorSql, new { CompletedStatus = (int)TicketStatus.Completed });

        return new TicketSummaryReport(
            statuses,
            overdueCount,
            executors.ToList()
        );
    }

    /// <summary>
    /// Служебная модель строки для агрегации по статусам (Dapper).
    /// </summary>
    private sealed class StatusCountRow
    {
        public int StatusCode { get; set; }
        public int Count { get; set; }
    }
}