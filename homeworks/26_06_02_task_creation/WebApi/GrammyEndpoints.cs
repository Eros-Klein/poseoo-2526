using AppServices;

namespace WebApi;

public static class GrammyEndpoints
{
    public static IEndpointRouteBuilder MapGrammyEndpoints(this IEndpointRouteBuilder app)
    {
        var categoryGroup = app.MapGroup("/categories")
            .WithTags("Categories");
        
        var performanceGroup = app.MapGroup("/performances")
            .WithTags("Performances");

        var artistGroup = app.MapGroup("/artists")
            .WithTags("Artists");

        var stageGroup = app.MapGroup("/stages")
            .WithTags("Stages");
        
        var statisticsGroup = app.MapGroup("/statistics")
            .WithTags("Statistics");

        // TODO: Append endpoints to according groups.
        
        return app;
    }
}

public record GrammyStatisticsDto(string ArtistName, decimal Budget, int WinningCategories, int NominatedCategoriesPoints, decimal PerformanceScore);