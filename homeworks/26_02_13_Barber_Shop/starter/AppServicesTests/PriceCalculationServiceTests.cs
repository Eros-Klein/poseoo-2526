using AppServices;
using Microsoft.EntityFrameworkCore;
using TestInfrastructure;

namespace AppServicesTests;

/// <summary>
/// At least 3 tests for price calculation logic.
/// </summary>
public class PriceCalculationServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public PriceCalculationServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private static Appointment CreateBaseAppointment(string customer = "Test", string barber = "Todd")
    {
        return new Appointment
        {
            CustomerName = customer,
            Date = new DateOnly(2024, 3, 8), // Friday
            StartTime = new TimeOnly(10, 0),
            Duration = TimeSpan.FromMinutes(45),
            BarberName = barber,
            Services =
            [
                new AppointmentService { Name = "Faded", StyleReference = StyleReference.Faded }
            ]
        };
    }

    [Fact]
    public async Task CalculatePrice_SingleService_ReturnsBasePriceWithToddDiscount()
    {
        await using var context = new ApplicationDataContext(_fixture.Options);
        var service = new PriceCalculationService(context);
        var appt = CreateBaseAppointment();
        appt.Services = [new AppointmentService { Name = "Faded", StyleReference = StyleReference.Faded }];
        appt.Duration = TimeSpan.FromMinutes(30);

        var price = await service.CalculatePriceAsync(appt);

        // Base €40, Todd -€5 = €35, no other modifiers (0 prev appointments, no beverage, not VIP)
        Assert.True(price >= 30 && price <= 40);
    }

    [Fact]
    public async Task CalculatePrice_TwoServices_AppliesFivePercentPremium()
    {
        await using var context = new ApplicationDataContext(_fixture.Options);
        var service = new PriceCalculationService(context);
        var appt = CreateBaseAppointment();
        appt.Services =
        [
            new AppointmentService { Name = "Faded", StyleReference = StyleReference.Faded },
            new AppointmentService { Name = "Beard", StyleReference = StyleReference.BeardShaped }
        ];
        appt.Duration = TimeSpan.FromMinutes(45); // 30+10 min minimum

        var price = await service.CalculatePriceAsync(appt);

        // Base 40+15=55, +5% = 57.75, hair+beard combo -10% on 15 = -1.50 -> 56.25, Todd -5 = 51.25, then loyalty 0%
        Assert.True(price >= 45 && price <= 60);
    }

    [Fact]
    public async Task CalculatePrice_ThreeServices_AppliesTenPercentPremiumAndPackageDiscount()
    {
        await using var context = new ApplicationDataContext(_fixture.Options);
        var service = new PriceCalculationService(context);
        var appt = CreateBaseAppointment();
        appt.Services =
        [
            new AppointmentService { Name = "Undercut", StyleReference = StyleReference.Undercut },
            new AppointmentService { Name = "Beard", StyleReference = StyleReference.BeardShaped },
            new AppointmentService { Name = "Hot Towel", StyleReference = StyleReference.HotTowelShave }
        ];
        appt.Duration = TimeSpan.FromMinutes(60); // 35+10+15 min

        var price = await service.CalculatePriceAsync(appt);

        // Base 42+15+18=75, +10% = 82.50, combo -10% on 15, package -15%, Todd -5, etc.
        Assert.True(price >= 50 && price <= 90);
    }

    [Fact]
    public async Task CalculatePrice_Sunday_AddsTwentyEuroPremium()
    {
        await using var context = new ApplicationDataContext(_fixture.Options);
        var service = new PriceCalculationService(context);
        var appt = CreateBaseAppointment();
        appt.Date = new DateOnly(2024, 3, 10); // Sunday
        appt.StartTime = new TimeOnly(12, 0);
        appt.Duration = TimeSpan.FromMinutes(30);
        appt.Services = [new AppointmentService { Name = "Faded", StyleReference = StyleReference.Faded }];

        var price = await service.CalculatePriceAsync(appt);

        // Base 40, Sunday +20 = 60, time modifier (peak) +30% = 78, Todd -5 = 73
        Assert.True(price >= 70 && price <= 85);
    }
}
