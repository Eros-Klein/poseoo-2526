using AppServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneOf.Types;

namespace WebApi;

public static class ToDoEndpoints
{
    public static IEndpointRouteBuilder MapToDoEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/to-do");

        api.MapGet("/", HandleQuery)
            .Produces<List<ToDo>>(200);

        api.MapGet("/{id}", HandleIdQuery)
            .Produces(404)
            .Produces<ToDo>(200);

        api.MapPost("/", HandleToDoCreation)
            .Produces(400)
            .Produces<ToDo>(200);

        api.MapPut("/{id}", HandleCompletetionToggle)
            .Produces<ToDo>(200);

        api.MapPatch("/{id}", HandleToDoEntryUpdate)
            .Produces<ToDo>(200);

        api.MapDelete("/{id}", HandleToDoEntryDelete)
            .Produces(404)
            .Produces(204);

        return api;
    }

    public static async Task<IResult> HandleToDoCreation(ApplicationDataContext dbContext, ToDoCreationDto creationDto)
    {
        if (string.IsNullOrWhiteSpace(creationDto.Title) || string.IsNullOrWhiteSpace(creationDto.Assignee))
        {
            return Results.BadRequest("Both the assignee, and the title may not be empty or whitespace.");
        }

        var toDo = new ToDo
        {
            Title = creationDto.Title,
            Assignee = creationDto.Assignee
        };

        await dbContext.ToDos.AddAsync(toDo);

        await dbContext.SaveChangesAsync();

        return Results.Created($"/to-do/{toDo.Id}", toDo);
    }

    public static async Task<IResult> HandleToDoEntryDelete(int id, ApplicationDataContext dbContext)
    {
        var toDo = await dbContext.ToDos.FindAsync(id);

        if (toDo == null)
        {
            return Results.NotFound($"ToDo entry with id {id} was not found!");
        }

        dbContext.ToDos.Remove(toDo);

        await dbContext.SaveChangesAsync();

        return Results.NoContent();
    }

    public static async Task<IResult> HandleToDoEntryUpdate(int id, ApplicationDataContext dbContext, ToDoCreationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Assignee))
        {
            return Results.BadRequest("Both the assignee, and the title may not be empty or whitespace.");
        }

        var toDo = await dbContext.ToDos.FindAsync(id);

        if (toDo == null)
        {
            return Results.NotFound($"ToDo entry with id {id} was not found!");
        }

        toDo.Assignee = dto.Assignee;
        toDo.Title = dto.Title;

        dbContext.Update(toDo);

        await dbContext.SaveChangesAsync();

        return Results.Ok(toDo);
    }

    public static async Task<IResult> HandleCompletetionToggle(int id, ApplicationDataContext dbContext, ToDoLogic logic)
    {
        var toDo = await dbContext.ToDos.FindAsync(id);

        if (toDo == null)
        {
            return Results.NotFound($"ToDo entry with id {id} was not found!");
        }

        toDo.IsCompleted = logic.ToggleCompletedStatus(toDo);

        dbContext.Update(toDo);

        await dbContext.SaveChangesAsync();

        return Results.Ok(toDo);
    }

    public static async Task<IResult> HandleQuery(ApplicationDataContext dbContext, [FromQuery] string? assignee, [FromQuery] bool? completitionStatus)
    {
        var toDos = await dbContext.ToDos.ToListAsync();

        if (!string.IsNullOrWhiteSpace(assignee))
        {
            toDos = [.. toDos.Where(td => td.Assignee == assignee)];
        }

        if (completitionStatus != null)
        {
            toDos = [.. toDos.Where(td => td.IsCompleted == completitionStatus)];
        }

        return Results.Ok(toDos);
    }

    public static async Task<IResult> HandleIdQuery(int id, ApplicationDataContext dbContext)
    {
        var toDo = await dbContext.ToDos.FindAsync(id);

        if (toDo == null)
        {
            return Results.NotFound($"ToDo entry with id {id} was not found!");
        }

        return Results.Ok(toDo);
    }
}

public record ToDoCreationDto(string Title, string Assignee);