namespace TicketSystem.Application.Contracts;

using System;
using System.Collections.Generic;
using TicketSystem.Core.Entities;
using TicketSystem.Core.Enums;

public interface ITicketRepository
/*
    Контракт доступа к хранилищу заявок
*/
{
    /// Сохраняет заявку (создание либо обновление — определяется реализацией
    /// по наличию записи с данным Id)
    void Save(Ticket ticket);

    /// Возвращает заявку по идентификатору или null, если не найдена
    Ticket? GetById(Guid id);

    /// <summary>
    /// Возвращает список заявок с фильтрацией. Все параметры опциональны
    /// и комбинируются по правилу "И" (AND), что покрывает функциональное
    /// требование "вывести список заявок с фильтрацией по статусу,
    /// исполнителю, подразделению, просроченности"
    /// </summary>
    /// <param name="status">Фильтр по статусу заявки.</param>
    /// <param name="executorId">Фильтр по идентификатору исполнителя.</param>
    /// <param name="department">Фильтр по подразделению исполнителя.</param>
    /// <param name="onlyOverdue">Если true — вернуть только просроченные заявки.</param>
    IReadOnlyCollection<Ticket> GetFiltered(
        TicketStatus? status = null,
        Guid? executorId = null,
        string? department = null,
        bool onlyOverdue = false);
}