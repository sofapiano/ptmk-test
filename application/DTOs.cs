namespace TicketSystem.Application.DTOs;

using System;

/// <summary>
/// Элемент отчёта, отражающий количество заявок в конкретном статусе.
/// </summary>
public record StatusReportItem(string StatusName, int Count);

/// <summary>
/// Элемент отчёта, отражающий количество выполненных заявок конкретным исполнителем.
/// </summary>
public record ExecutorReportItem(Guid ExecutorId, string ExecutorFullName, int CompletedCount);

/// <summary>
/// Полный агрегированный отчёт по заявкам системы.
/// </summary>
public record TicketSummaryReport(
    IReadOnlyCollection<StatusReportItem> StatusCounts,
    int OverdueCount,
    IReadOnlyCollection<ExecutorReportItem> ExecutorCompletedCounts);