using System.Net.Http.Json;

namespace WebApiTests;

public record EmployeeDto(string EmployeeId, string EmployeeName);
public record ProjectDto(string ProjectCode);
public record TimeEntryUpdateReq(DateOnly Date, TimeOnly StartTime, TimeOnly EndTime, string Description, EmployeeDto Employee, ProjectDto Project);


public class TimeTrackingPutTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    [Fact]
    public async Task PutTimeTrackingInvalidDesc_ReturnsBad400()
    {
        var employee = new EmployeeDto("0123", "tilo der ersteller");
        var project = new ProjectDto("Jailtime");

        var body = new TimeEntryUpdateReq(DateOnly.MinValue, TimeOnly.MinValue, TimeOnly.MaxValue, "", employee, project);

        var response = await fixture.HttpClient.PutAsJsonAsync("/timeentries/3", body);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutTimeTrackingNonExistentId_ReturnsNotFound404()
    {
        var employee = new EmployeeDto("0123", "tilo der ersteller");
        var project = new ProjectDto("Jailtime");

        var body = new TimeEntryUpdateReq(DateOnly.MinValue, TimeOnly.MinValue, TimeOnly.MaxValue, "Hallo Bro", employee, project);

        var response = await fixture.HttpClient.PutAsJsonAsync("/timeentries/3124124124", body);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}