using DataAccess;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddSqliteDbContext<ApplicationDataContext>("sqlite-db");

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/", async (ApplicationDataContext context) =>
{
    var customers = await context.Customers.ToListAsync();
    return Results.Json(customers);
});

app.Run();