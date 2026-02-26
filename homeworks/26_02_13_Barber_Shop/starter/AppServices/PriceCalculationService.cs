using Microsoft.EntityFrameworkCore;

namespace AppServices;

public class PriceCalculationException : Exception
{
    public int StatusCode { get; }
    public PriceCalculationException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class PriceCalculationService
{
    private readonly ApplicationDataContext _db;

    public PriceCalculationService(ApplicationDataContext db)
    {
        _db = db;
    }

    private static readonly StyleReference[] LengthCategories =
    [StyleReference.Short, StyleReference.Medium, StyleReference.Long];

    public void ValidateAppointment(Appointment appointment)
    {
        var day = appointment.Date.DayOfWeek;
        if (day is DayOfWeek.Monday or DayOfWeek.Tuesday or DayOfWeek.Wednesday or DayOfWeek.Thursday)
            throw new PriceCalculationException(
                "Gerrit's Cuts is closed Monday-Thursday. We value our leisure time.");

        if (appointment.Services.Count == 0)
            throw new PriceCalculationException("No services selected for appointment.");

        var serviceRefs = appointment.Services.Select(s => s.StyleReference).ToList();

        if (serviceRefs.Contains(StyleReference.BeardShaped) && serviceRefs.Contains(StyleReference.CleanShaven))
            throw new PriceCalculationException(
                "Service combination conflict detected: BeardShaped and CleanShaven cannot be booked together.");

        var lengthCount = serviceRefs.Intersect(LengthCategories).Count();
        if (lengthCount > 1)
            throw new PriceCalculationException(
                "Service combination conflict detected: multiple hair length services (Short/Medium/Long) cannot be booked together.");

        var minRequired = appointment.Services.Sum(s => ServiceMetadata.GetMinimumDuration(s.StyleReference));
        if (appointment.Duration.TotalMinutes < minRequired)
            throw new PriceCalculationException(
                $"Appointment duration ({(int)appointment.Duration.TotalMinutes} min) is insufficient for selected services (requires minimum {minRequired} min).");

        var barber = (appointment.BarberName ?? "").Trim();
        var start = appointment.StartTime;
        var endTime = start.Add(appointment.Duration);

        if (string.Equals(barber, "Gerrit", StringComparison.OrdinalIgnoreCase))
        {
            var (peakStart, peakEnd) = day == DayOfWeek.Friday
                ? (new TimeOnly(16, 0), new TimeOnly(20, 0))
                : (new TimeOnly(10, 0), new TimeOnly(18, 0));
            var valid = day is DayOfWeek.Friday or DayOfWeek.Saturday or DayOfWeek.Sunday
                && start >= peakStart
                && endTime >= start && endTime <= peakEnd;
            if (!valid)
                throw new PriceCalculationException(
                    "Gerrit only works during peak hours (Fri 16:00-20:00, Sat-Sun 10:00-18:00).");
        }

        var existingSameBarber = _db.Appointments
            .Where(a => a.BarberName == barber && a.Id != appointment.Id && a.Date == appointment.Date)
            .ToList();

        foreach (var a in existingSameBarber)
        {
            var aEnd = a.StartTime.Add(a.Duration);
            var apptEnd = start.Add(appointment.Duration);
            if (start < aEnd && apptEnd > a.StartTime)
                throw new PriceCalculationException(
                    $"Time slot unavailable. {barber} already has an appointment at this time.", 409);
        }
    }

    public async Task<decimal> CalculatePriceAsync(Appointment appointment)
    {
        ValidateAppointment(appointment);

        var services = appointment.Services;
        var serviceRefs = services.Select(s => s.StyleReference).ToList();

        // Step 1: Base price
        var total = services.Sum(s => ServiceMetadata.GetBasePrice(s.StyleReference));

        // Step 2: Service count premium
        if (services.Count == 2) total *= 1.05m;
        else if (services.Count >= 3) total *= 1.10m;

        // Step 3: Combo discounts (after service count premium)
        var hasHair = serviceRefs.Any(ServiceMetadata.IsHaircutService);
        var hasBeard = serviceRefs.Any(ServiceMetadata.IsBeardService);
        if (hasHair && hasBeard)
        {
            var beardServices = services.Where(s => ServiceMetadata.IsBeardService(s.StyleReference)).ToList();
            var cheapestBeard = beardServices.Min(s => ServiceMetadata.GetBasePrice(s.StyleReference));
            total -= cheapestBeard * 0.10m;
        }
        if (services.Count >= 3)
            total *= 0.85m;

        // Step 4: Payday surcharge
        if (appointment.Date.Day == 15)
            total *= 1.25m;

        // Step 5: Sunday premium
        if (appointment.Date.DayOfWeek == DayOfWeek.Sunday)
            total += 20m;

        // Step 6: Time-based modifier (Peak → Happy → Off-Peak)
        var (hour, day) = (appointment.StartTime.Hour, appointment.Date.DayOfWeek);
        if (day == DayOfWeek.Friday && hour >= 16 && hour < 20 ||
            (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) && hour >= 10 && hour < 18)
            total *= 1.30m;
        else if (day == DayOfWeek.Friday && hour >= 14 && hour < 16)
            total *= 0.85m;
        else if (day == DayOfWeek.Friday && hour >= 8 && hour < 10)
            total *= 0.80m;

        // Step 7: Barber markup
        var barberName = (appointment.BarberName ?? "").Trim();
        if (string.Equals(barberName, "Gerrit", StringComparison.OrdinalIgnoreCase))
            total *= 1.20m;
        else if (string.Equals(barberName, "Todd", StringComparison.OrdinalIgnoreCase))
            total -= 5m;

        // Step 8: Duration fee
        var requiredMin = services.Sum(s => ServiceMetadata.GetMinimumDuration(s.StyleReference));
        var actualMin = (int)appointment.Duration.TotalMinutes;
        if (actualMin > requiredMin)
        {
            var extra = actualMin - requiredMin;
            var increments = (int)Math.Ceiling(extra / 15.0);
            total += increments * 2.50m;
        }

        // Step 9: Beverage surcharge
        if (!string.IsNullOrWhiteSpace(appointment.BeverageChoice))
            total += 8m;

        // Step 10: Loyalty tier (DB query)
        var previousCount = await _db.Appointments
            .Where(a => a.CustomerName == appointment.CustomerName && a.Id != appointment.Id)
            .CountAsync();
        var loyaltyDiscount = previousCount switch
        {
            >= 11 => 0.85m,
            >= 6 => 0.90m,
            >= 3 => 0.95m,
            _ => 1.00m
        };
        total *= loyaltyDiscount;

        // Step 11: Group booking (DB query) - same customer, same date, start within ±30 min
        var sameDaySameCustomer = await _db.Appointments
            .Where(a => a.CustomerName == appointment.CustomerName && a.Date == appointment.Date && a.Id != appointment.Id)
            .Select(a => a.StartTime)
            .ToListAsync();
        var groupCount = 1 + sameDaySameCustomer.Count(t =>
            Math.Abs((double)(t.Ticks - appointment.StartTime.Ticks) / TimeSpan.TicksPerMinute) <= 30);
        var groupDiscount = groupCount >= 4 ? 0.80m : groupCount >= 2 ? 0.90m : 1.00m;
        total *= groupDiscount;

        // Step 12: VIP multiplier
        if (appointment.IsVip)
            total *= 1.5m;

        return Math.Max(Math.Round(total, 2, MidpointRounding.AwayFromZero), 0m);
    }
}
