namespace TicketSystem.Infrastructure.Data;

using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using TicketSystem.Application.Contracts;
using TicketSystem.Application.Queries;

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

        // 1. Считаем статусы
        const string statusSql = @"
            SELECT Status as StatusName, COUNT(*) as Count 
            FROM Tickets 
            GROUP BY Status";
        var statuses = await db.QueryAsync<StatusReportItem>(statusSql);

        // 2. Считаем просроченные
        const string overdueSql = "SELECT COUNT(*) FROM Tickets WHERE Deadline < @Now";
        var overdueCount = await db.ExecuteScalarAsync<int>(overdueSql, new { Now = DateTime.UtcNow });

        // 3. Считаем выполненные по исполнителям
        const string executorSql = @"
            SELECT e.Id as ExecutorId, e.FullName as ExecutorFullName, COUNT(t.Id) as CompletedCount
            FROM Tickets t
            JOIN Employees e ON t.ExecutorId = e.Id
            WHERE t.Status = 2 -- Completed
            GROUP BY e.Id, e.FullName";
        var executors = await db.QueryAsync<ExecutorReportItem>(executorSql);

        return new TicketSummaryReport(
            statuses.ToList(),
            overdueCount,
            executors.ToList()
        );
    }
}