using AppServices;
using TestInfrastructure;

namespace AppServicesTests;

/// <summary>
/// At least 2 tests covering validation rules.
/// </summary>
public class AppointmentValidationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public AppointmentValidationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ValidateAppointment_WeekdayMonday_Throws400()
    {
        using var context = new ApplicationDataContext(_fixture.Options);
        var service = new PriceCalculationService(context);
        var appt = new Appointment
        {
            CustomerName = "Test",
            Date = new DateOnly(2024, 3, 4), // Monday
            StartTime = new TimeOnly(14, 0),
            Duration = TimeSpan.FromMinutes(30),
            BarberName = "Todd",
            Services = [new AppointmentService { Name = "Faded", StyleReference = StyleReference.Faded }]
        };

        var ex = Assert.Throws<PriceCalculationException>(() => service.ValidateAppointment(appt));
        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("Monday-Thursday", ex.Message);
    }

    [Fact]
    public void ValidateAppointment_CleanShavenAndBeardShaped_Throws400()
    {
        using var context = new ApplicationDataContext(_fixture.Options);
        var service = new PriceCalculationService(context);
        var appt = new Appointment
        {
            CustomerName = "Test",
            Date = new DateOnly(2024, 3, 8), // Friday
            StartTime = new TimeOnly(14, 0),
            Duration = TimeSpan.FromMinutes(30),
            BarberName = "Todd",
            Services =
            [
                new AppointmentService { Name = "Shave", StyleReference = StyleReference.CleanShaven },
                new AppointmentService { Name = "Beard", StyleReference = StyleReference.BeardShaped }
            ]
        };

        var ex = Assert.Throws<PriceCalculationException>(() => service.ValidateAppointment(appt));
        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("BeardShaped", ex.Message);
        Assert.Contains("CleanShaven", ex.Message);
    }

    [Fact]
    public void ValidateAppointment_InsufficientDuration_Throws400()
    {
        using var context = new ApplicationDataContext(_fixture.Options);
        var service = new PriceCalculationService(context);
        var appt = new Appointment
        {
            CustomerName = "Test",
            Date = new DateOnly(2024, 3, 8),
            StartTime = new TimeOnly(14, 0),
            Duration = TimeSpan.FromMinutes(20), // Faded needs 30 min
            BarberName = "Todd",
            Services = [new AppointmentService { Name = "Faded", StyleReference = StyleReference.Faded }]
        };

        var ex = Assert.Throws<PriceCalculationException>(() => service.ValidateAppointment(appt));
        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("insufficient", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateAppointment_GerritOutsidePeakHours_Throws400()
    {
        using var context = new ApplicationDataContext(_fixture.Options);
        var service = new PriceCalculationService(context);
        var appt = new Appointment
        {
            CustomerName = "Test",
            Date = new DateOnly(2024, 3, 8), // Friday
            StartTime = new TimeOnly(10, 0), // Before 16:00 peak
            Duration = TimeSpan.FromMinutes(30),
            BarberName = "Gerrit",
            Services = [new AppointmentService { Name = "Faded", StyleReference = StyleReference.Faded }]
        };

        var ex = Assert.Throws<PriceCalculationException>(() => service.ValidateAppointment(appt));
        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("Gerrit", ex.Message);
        Assert.Contains("peak", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
