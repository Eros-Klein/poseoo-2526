using AppServices;
using Microsoft.EntityFrameworkCore;
using TestInfrastructure;

namespace AppServicesTests;

public class DatabaseTestsWithClassFixture(DatabaseFixture fixture)
    : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task CanAddAndRetrieveAppointment()
    {
        // Arrange
        int appointmentId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var appt = new Appointment
            {
                CustomerName = "Test Customer",
                Date = new DateOnly(2023, 10, 1),
                StartTime = new TimeOnly(14, 0),
                Duration = TimeSpan.FromMinutes(30),
                BarberName = "Gerrit"
            };
            context.Appointments.Add(appt);
            await context.SaveChangesAsync();
            appointmentId = appt.Id;
        }

        // Act & Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var match = await context.Appointments.FindAsync(appointmentId);
            Assert.NotNull(match);
            Assert.Equal("Test Customer", match.CustomerName);
            Assert.Equal("Gerrit", match.BarberName);
            Assert.Equal(new DateOnly(2023, 10, 1), match.Date);
        }
    }

    [Fact]
    public async Task CanAddAppointmentWithServices()
    {
        // Arrange
        int appointmentId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var appt = new Appointment
            {
                CustomerName = "Fancy Customer",
                Date = new DateOnly(2024, 1, 1),
                StartTime = new TimeOnly(10, 0),
                Duration = TimeSpan.FromHours(1)
            };

            appt.Services.Add(new AppointmentService { Name = "Haircut", StyleReference = StyleReference.Faded });
            appt.Services.Add(new AppointmentService { Name = "Shave", StyleReference = StyleReference.HotTowelShave });

            context.Appointments.Add(appt);
            await context.SaveChangesAsync();
            appointmentId = appt.Id;
        }

        // Act & Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var match = await context.Appointments
                .Include(a => a.Services)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            Assert.NotNull(match);
            Assert.Equal(2, match.Services.Count);
            Assert.Contains(match.Services, s => s.Name == "Haircut");
        }
    }

    [Fact]
    public async Task CanDeleteAppointment()
    {
        // Arrange
        int appointmentId;
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var appt = new Appointment
            {
                CustomerName = "To Be Deleted",
                Date = new DateOnly(2024, 3, 1),
                StartTime = new TimeOnly(9, 0),
                Duration = TimeSpan.FromMinutes(15)
            };
            context.Appointments.Add(appt);
            await context.SaveChangesAsync();
            appointmentId = appt.Id;
        }

        // Act
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var appt = await context.Appointments.FindAsync(appointmentId);
            if (appt != null)
            {
                context.Appointments.Remove(appt);
            }
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = new ApplicationDataContext(fixture.Options))
        {
            var appt = await context.Appointments.FindAsync(appointmentId);
            Assert.Null(appt);
        }
    }
}
