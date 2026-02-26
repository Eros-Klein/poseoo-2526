using AppServices;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

public static class AppointmentEndpoints
{
    public static IEndpointRouteBuilder MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/appointments")
            .WithTags("Appointments")
            .WithDescription("Manage barber shop appointments.");

        group.MapGet("/", HandleGetAllAppointments)
            .WithName("GetAllAppointments")
            .WithDescription("Retrieve all appointments");

        group.MapGet("/{id}", HandleGetAppointmentById)
            .WithName("GetAppointmentById")
            .WithDescription("Retrieve a specific appointment by ID");

        group.MapPost("/", HandleCreateAppointment)
            .WithName("CreateAppointment")
            .WithDescription("Create a new appointment");

        group.MapPost("/estimate", HandleEstimatePrice)
            .WithName("EstimatePrice")
            .WithDescription("Get price estimate for an appointment without saving");

        group.MapDelete("/{id}", HandleDeleteAppointment)
            .WithName("DeleteAppointment")
            .WithDescription("Delete an appointment by ID");

        return app;
    }

    public static async Task<IResult> HandleGetAllAppointments(ApplicationDataContext db, PriceCalculationService priceService)
    {
        var appointments = await db.Appointments
            .Include(a => a.Services)
            .ToListAsync();

        var result = new List<object>();
        foreach (var a in appointments)
        {
            try
            {
                var price = await priceService.CalculatePriceAsync(a);
                result.Add(new
                {
                    a.Id,
                    a.Date,
                    a.StartTime,
                    a.Duration,
                    a.CustomerName,
                    a.Services,
                    a.BarberName,
                    a.BeverageChoice,
                    a.IsVip,
                    CalculatedPrice = price
                });
            }
            catch (PriceCalculationException)
            {
                result.Add(new { a.Id, a.Date, a.StartTime, a.Duration, a.CustomerName, a.Services, a.BarberName, a.BeverageChoice, a.IsVip, CalculatedPrice = (decimal?)null });
            }
        }

        return Results.Ok(result);
    }

    public static async Task<IResult> HandleGetAppointmentById(int id, ApplicationDataContext db, PriceCalculationService priceService)
    {
        var appointment = await db.Appointments
            .Include(a => a.Services)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment is null)
            return Results.NotFound();

        try
        {
            var price = await priceService.CalculatePriceAsync(appointment);
            return Results.Ok(new
            {
                appointment.Id,
                appointment.Date,
                appointment.StartTime,
                appointment.Duration,
                appointment.CustomerName,
                appointment.Services,
                appointment.BarberName,
                appointment.BeverageChoice,
                appointment.IsVip,
                CalculatedPrice = price
            });
        }
        catch (PriceCalculationException)
        {
            return Results.Ok(new
            {
                appointment.Id,
                appointment.Date,
                appointment.StartTime,
                appointment.Duration,
                appointment.CustomerName,
                appointment.Services,
                appointment.BarberName,
                appointment.BeverageChoice,
                appointment.IsVip,
                CalculatedPrice = (decimal?)null
            });
        }
    }

    public static async Task<IResult> HandleCreateAppointment(Appointment appointment, ApplicationDataContext db, PriceCalculationService priceService)
    {
        try
        {
            priceService.ValidateAppointment(appointment);
        }
        catch (PriceCalculationException ex)
        {
            return Results.Json(new { message = ex.Message }, statusCode: ex.StatusCode);
        }

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync();

        var price = await priceService.CalculatePriceAsync(appointment);

        return Results.Created($"/appointments/{appointment.Id}", new
        {
            appointment.Id,
            appointment.Date,
            appointment.StartTime,
            appointment.Duration,
            appointment.CustomerName,
            appointment.Services,
            appointment.BarberName,
            appointment.BeverageChoice,
            appointment.IsVip,
            CalculatedPrice = price
        });
    }

    public static async Task<IResult> HandleEstimatePrice(Appointment appointment, PriceCalculationService priceService)
    {
        try
        {
            var price = await priceService.CalculatePriceAsync(appointment);
            return Results.Ok(new { calculatedPrice = price });
        }
        catch (PriceCalculationException ex)
        {
            return Results.Json(new { message = ex.Message }, statusCode: ex.StatusCode);
        }
    }

    public static async Task<IResult> HandleDeleteAppointment(int id, ApplicationDataContext db)
    {
        var appointment = await db.Appointments.FindAsync(id);

        if (appointment is null)
            return Results.NotFound();

        db.Appointments.Remove(appointment);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
