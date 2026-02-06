using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppServices.Importer;

/// <summary>
/// Interface for writing objects to the database
/// </summary>
public interface IGrammyImportDatabaseWriter
{
    /// <summary>
    /// Clears all existing Dummy records from the database
    /// </summary>
    Task ClearAllAsync();

    /// <summary>
    /// Writes a stage to its database
    /// </summary>
    /// <param name="stage">Stage to write</param>
    Task WriteStageAsync(GrammyStage stage);

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
public class GrammyImportDatabaseWriter(ApplicationDataContext context) : IGrammyImportDatabaseWriter
{
    private IDbContextTransaction? _transaction;

    public async Task ClearAllAsync()
    {
        // Delete in order to respect foreign key constraints
        await context.Performances.ExecuteDeleteAsync();
        await context.Categories.ExecuteDeleteAsync();
        await context.Artists.ExecuteDeleteAsync();
        await context.Stages.ExecuteDeleteAsync();
    }

    public async Task WriteStageAsync(GrammyStage stage)
    {
        // Create the Stage entity
        var stageEntity = new Stage
        {
            Id = Guid.NewGuid(),
            Name = stage.Name
        };

        // Dictionary to track artists by name to avoid duplicates
        var artistsByName = new Dictionary<string, Artist>();

        // Process each category
        foreach (var grammyCategory in stage.Categories)
        {
            var categoryEntity = new Category
            {
                Id = Guid.NewGuid(),
                Name = grammyCategory.Name,
                Priority = grammyCategory.PriorityLevel,
                Budget = grammyCategory.Budget,
                StageId = stageEntity.Id,
                Stage = stageEntity
            };

            // Process each artist in the category
            foreach (var grammyArtist in grammyCategory.Artists)
            {
                // Get or create artist (avoid duplicates across categories)
                if (!artistsByName.TryGetValue(grammyArtist.Name, out var artistEntity))
                {
                    artistEntity = new Artist
                    {
                        Id = Guid.NewGuid(),
                        Name = grammyArtist.Name
                    };
                    artistsByName[grammyArtist.Name] = artistEntity;
                }

                // Link artist to category (many-to-many)
                categoryEntity.Artists.Add(artistEntity);
                artistEntity.Categories.Add(categoryEntity);

                // Create performance if it exists
                if (grammyArtist.Performance != null)
                {
                    var performanceEntity = new Performance
                    {
                        Id = Guid.NewGuid(),
                        UsedBudget = grammyArtist.Performance.UsedBudget ?? 0,
                        DateTime = grammyArtist.Performance.Date,
                        ArtistId = artistEntity.Id,
                        CategoryId = categoryEntity.Id,
                        Artist = artistEntity,
                        Category = categoryEntity
                    };

                    artistEntity.Performance = performanceEntity;
                    artistEntity.PerformanceId = performanceEntity.Id;

                    await context.Performances.AddAsync(performanceEntity);
                }
            }

            stageEntity.Categories.Add(categoryEntity);
        }

        // Add the stage and all related entities to the context
        await context.Stages.AddAsync(stageEntity);
        
        // Add all artists to the context
        await context.Artists.AddRangeAsync(artistsByName.Values);
        
        // Save all changes to the database
        await context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
