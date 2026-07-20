namespace TicketSystem.Application.Contracts;

using System.Threading.Tasks;
using TicketSystem.Application.Queries;

public interface IReportingService
/* 
    Контракт для сервиса формирования аналитической отчётности по заявкам сотрудников 
*/
{
    Task<TicketSummaryReport> GetSummaryReportAsync();
}