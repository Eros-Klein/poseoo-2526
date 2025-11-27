using System.Text.RegularExpressions;
using AppServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

public record EmployeeDto(string EmployeeId, string EmployeeName);
public record ProjectDto(string ProjectCode);
public record TimeEntryUpdateReq(DateOnly Date, TimeOnly StartTime, TimeOnly EndTime, string Description, EmployeeDto Employee, ProjectDto Project);

public static partial class TimeTrackingEndpoints
{
    [GeneratedRegex("[0-9]*", RegexOptions.IgnoreCase)]
    private static partial Regex Is_Number();

    public static IEndpointRouteBuilder MapTimeTrackingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/employees", HandleEmployeeQuerry)
        .WithName("Get all Employees")
        .WithDescription("Returns all persisted Employees")
        .Produces<Employee[]>();

        app.MapGet("/projects", HandleProjectQuerry)
        .WithName("Get all Projects")
        .WithDescription("Returns all persisted Projects")
        .Produces<Project[]>();

        app.MapGet("/timeentries", HandleTimeEntryQuerry)
        .WithName("Get Timeentries by optional filters: employeeId, projectId")
        .WithDescription("Returns filtered persisted Timeentries")
        .Produces<TimeEntry[]>();

        app.MapGet("/timeentries/{id}", HandleTimeentriesIdQuerry)
        .WithName("Get Timeentry by id")
        .WithDescription("Returns the found Timeentry or 404")
        .Produces<string>(StatusCodes.Status404NotFound)
        .Produces<TimeEntry>();

        app.MapDelete("/timeentries/{id}", HandleTimeentriesIdDelete)
        .WithName("Delete Timeentry by id")
        .WithDescription("Does what the name says")
        .Produces<string>(StatusCodes.Status404NotFound)
        .Produces<NoContentResult>();

        app.MapPut("/timeentries/{id}", HandleTimeentryIdUpdate)
        .WithName("Update Timeentry by id")
        .WithDescription("Everything except the Id can be updated")
        .Produces<string>(StatusCodes.Status404NotFound)
        .Produces<string>(StatusCodes.Status400BadRequest)
        .Produces<NoContentResult>();

        return app;
    }

    public static async Task<IResult> HandleEmployeeQuerry(ApplicationDataContext context)
    {
        var results = await context.Employees.ToListAsync();

        return Results.Ok(results);
    }

    public static async Task<IResult> HandleProjectQuerry(ApplicationDataContext context)
    {
        var results = await context.Projects.ToListAsync();

        return Results.Ok(results);
    }

    public static async Task<IResult> HandleTimeEntryQuerry(ApplicationDataContext context, [FromQuery] string? employeeId, [FromQuery] string? projectId)
    {
        IEnumerable<TimeEntry> results;

        if (!string.IsNullOrEmpty(employeeId) && !string.IsNullOrEmpty(projectId))
        {
            results = await context.TimeEntries.Include(te => te.Employee).Include(te => te.Project)
                .Where(te => te.Employee != null && te.Employee.EmplyeeId == employeeId && te.ProjectId.ToString().Equals(projectId)).ToListAsync();
        }
        else if (!string.IsNullOrEmpty(employeeId))
        {
            results = await context.TimeEntries.Include(te => te.Employee).Include(te => te.Project)
                .Where(te => te.Employee != null && te.Employee.EmplyeeId == employeeId).ToListAsync();
        }
        else if (!string.IsNullOrEmpty(projectId))
        {
            results = await context.TimeEntries.Include(te => te.Employee).Include(te => te.Project)
                .Where(te => te.ProjectId.ToString().Equals(projectId)).ToListAsync();
        }
        else
        {
            results = await context.TimeEntries.Include(te => te.Employee).Include(te => te.Project).ToListAsync();
        }

        return Results.Ok(results);
    }

    public static async Task<IResult> HandleTimeentriesIdQuerry(ApplicationDataContext context, [FromRoute] int id)
    {
        var result = await context.TimeEntries
        .Include(te => te.Employee)
        .Include(te => te.Project)
        .FirstOrDefaultAsync(te => te.Id == id);

        if (result == null)
        {
            return Results.NotFound("Timeentry with the given id does not exist.");
        }

        return Results.Ok(result);
    }

    public static async Task<IResult> HandleTimeentriesIdDelete(ApplicationDataContext context, [FromRoute] int id)
    {
        var result = await context.TimeEntries.FindAsync(id);

        if (result == null)
        {
            return Results.NotFound("Timeentry with the given id does not exist.");
        }

        context.TimeEntries.Remove(result);
        await context.SaveChangesAsync();

        return Results.NoContent();
    }

    public static async Task<IResult> HandleTimeentryIdUpdate(ApplicationDataContext context, [FromBody] TimeEntryUpdateReq req, [FromRoute] int id)
    {
        var result = await context.TimeEntries
        .Include(te => te.Employee)
        .Include(te => te.Project)
        .FirstOrDefaultAsync(te => te.Id == id); ;

        if (result == null)
        {
            return Results.NotFound("Timeentry with the given id does not exist.");
        }

        if (string.IsNullOrEmpty(req.Description) || req.Description.Length > 200)
        {
            return Results.BadRequest("Description must exist and be less than 200 Characters long.");
        }

        if (!Is_Number().IsMatch(req.Employee.EmployeeId) || req.Employee.EmployeeId.Length > 5 || req.Employee.EmployeeId.Length == 0)
        {
            return Results.BadRequest("EmployeeId must consist of 1-5 numbers.");
        }

        if (req.Employee.EmployeeName.Length > 100 || req.Employee.EmployeeName.Length == 0)
        {
            return Results.BadRequest("EmployeeName must consist of 1-100 letters.");
        }

        if (result.Employee == null || req.Employee.EmployeeId != result.Employee.EmplyeeId)
        {
            var secondEmp = await context.Employees.FirstOrDefaultAsync(e => e.EmplyeeId == req.Employee.EmployeeId);

            var firstEmp = result.Employee;
            firstEmp?.TimeEntries.Remove(result);
            result.Employee = null;
            if (firstEmp != null)
            {
                context.UpdateRange(result, firstEmp);
            }

            secondEmp ??= new Employee
            {
                EmplyeeId = req.Employee.EmployeeId,
                EmployeeName = req.Employee.EmployeeName
            };

            secondEmp.TimeEntries.Add(result);
            result.Employee = secondEmp;

            context.UpdateRange(result, secondEmp);
        }

        if (result.Project == null || req.Project.ProjectCode != result.Project.ProjectCode)
        {
            var secondProject = await context.Projects.FirstOrDefaultAsync(p => p.ProjectCode == req.Project.ProjectCode);

            var firstProject = result.Project;
            firstProject?.TimeEntries.Remove(result);
            result.Project = null;

            if (firstProject != null)
            {
                context.UpdateRange(result, firstProject);
            }

            secondProject ??= new Project
            {
                ProjectCode = req.Project.ProjectCode
            };

            secondProject.TimeEntries.Add(result);
            result.Project = secondProject;

            context.UpdateRange(result, secondProject);
        }

        result.Date = req.Date;
        result.Description = req.Description;
        result.EndTime = req.EndTime;
        result.StartTime = req.StartTime;

        await context.SaveChangesAsync();

        return Results.Ok();
    }

    private static bool TimeEntrieIsFromEmployee(string employeeId, TimeEntry te)
    {
        return te.Employee != null && te.Employee.EmplyeeId == employeeId;
    }
    private static bool TimeEntrieIsFromProject(string projectId, TimeEntry te)
    {
        return te.ProjectId.ToString().Equals(projectId);
    }
}
