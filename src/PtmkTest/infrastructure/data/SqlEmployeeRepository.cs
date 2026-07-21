namespace TicketSystem.Infrastructure.Data;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Npgsql;
using TicketSystem.Application.Contracts;
using TicketSystem.Core.Entities;

public class SqlEmployeeRepository : IEmployeeRepository
{
    private readonly string _connectionString;

    public SqlEmployeeRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public Employee? GetById(Guid id)
    {
        using IDbConnection db = new NpgsqlConnection(_connectionString);

        const string sql = @"
            SELECT Id, FullName, Department, Position
            FROM Employees
            WHERE Id = @Id";

        var row = db.QuerySingleOrDefault<EmployeeRow>(sql, new { Id = id });

        return row is null
            ? null
            : new Employee(row.Id, row.FullName, row.Department, row.Position);
    }

    public IReadOnlyCollection<Employee> GetAll()
    {
        using IDbConnection db = new NpgsqlConnection(_connectionString);

        const string sql = @"
            SELECT Id, FullName, Department, Position
            FROM Employees";

        var rows = db.Query<EmployeeRow>(sql);

        return rows
            .Select(r => new Employee(r.Id, r.FullName, r.Department, r.Position))
            .ToList();
    }

    public void Add(Employee employee)
    {
        if (employee is null)
            throw new ArgumentNullException(nameof(employee));

        using IDbConnection db = new NpgsqlConnection(_connectionString);

        const string sql = @"
            INSERT INTO Employees (Id, FullName, Department, Position)
            VALUES (@Id, @FullName, @Department, @Position)";

        db.Execute(sql, new
        {
            employee.Id,
            employee.FullName,
            employee.Department,
            employee.Position
        });
    }

    public void Update(Employee employee)
    {
        if (employee is null)
            throw new ArgumentNullException(nameof(employee));

        using IDbConnection db = new NpgsqlConnection(_connectionString);

        const string sql = @"
            UPDATE Employees
            SET FullName = @FullName, Department = @Department, Position = @Position
            WHERE Id = @Id";

        db.Execute(sql, new
        {
            employee.Id,
            employee.FullName,
            employee.Department,
            employee.Position
        });
    }

    /// <summary>
    /// Служебная модель строки таблицы Employees для Dapper.
    /// </summary>
    private sealed class EmployeeRow
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = default!;
        public string Department { get; set; } = default!;
        public string Position { get; set; } = default!;
    }
}