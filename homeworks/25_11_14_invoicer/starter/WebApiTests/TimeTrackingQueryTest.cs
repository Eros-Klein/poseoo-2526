namespace WebApiTests;

public class TimeTrackingQueryTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    [Fact]
    public async Task GetEmployees_ReturnsOk()
    {
        var response = await fixture.HttpClient.GetAsync("/employees");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProjects_ReturnsOk()
    {
        var response = await fixture.HttpClient.GetAsync("/projects");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}