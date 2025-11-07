using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppServices.Importer;

/// <summary>
/// Interface for writing objects to the database
/// </summary>
public interface IToDoDatabaseWriter
{
    /// <summary>
    /// Clears all existing Dummy records from the database
    /// </summary>
    Task ClearAllAsync();

    /// <summary>
    /// Writes a collection of Dummy objects to the database
    /// </summary>
    /// <param name="dummies">Dummies to write</param>
    Task WriteToDosAsync(IEnumerable<ToDo> toDos);

    /// <summary>
    /// Begins a database transaction
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    Task RollbackTransactionAsync();
}

/// <summary>
/// Implementation for writing objects to the database
/// </summary>
public class ToDoDatabaseWriter(ApplicationDataContext context) : IToDoDatabaseWriter
{
    private IDbContextTransaction? transaction;

    public async Task ClearAllAsync()
    {
        await context.ToDos.ExecuteDeleteAsync();
    }

    public async Task WriteToDosAsync(IEnumerable<ToDo> toDos)
    {
        await context.ToDos.AddRangeAsync(toDos);
        await context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (transaction != null)
        {
            await transaction.CommitAsync();
            await transaction.DisposeAsync();
            transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (transaction != null)
        {
            await transaction.RollbackAsync();
            await transaction.DisposeAsync();
            transaction = null;
        }
    }
}
