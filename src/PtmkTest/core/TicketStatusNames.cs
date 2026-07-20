namespace TicketSystem.Core;

using System.Collections.Generic;

/// <summary>
/// Единая точка сопоставления TicketStatus человекочитаемым названиям.
/// Используется во всех слоях, где статус нужно показать пользователю
/// (отчёты, списки заявок), чтобы избежать дублирования и расхождения строк.
/// </summary>
public static class TicketStatusNames
{
    private static readonly Dictionary<TicketStatus, string> Names = new()
    {
        [TicketStatus.New] = "Новая",
        [TicketStatus.InProgress] = "В работе",
        [TicketStatus.Completed] = "Выполнена"
    };

    public static string Get(TicketStatus status) => Names[status];
}