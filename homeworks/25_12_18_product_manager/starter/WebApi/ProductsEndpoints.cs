using System.IO.Pipes;
using AppServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneOf.Types;

namespace WebApi;

public static class ProductsEndpoints
{
    public static IEndpointRouteBuilder MapProductsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/categories", HandleCategoryQuery)
            .WithName("GetCategories")
            .Produces<CategoryQueryRes>();

        app.MapGet("/products", HandleProductQuery)
            .WithName("GetProducts")
            .Produces<Product[]>();

        app.MapGet("/products/{id}", HandleSpecificProductQuery)
            .WithName("GetSpecificProduct")
            .Produces(StatusCodes.Status404NotFound)
            .Produces<Product>();

        app.MapPut("/products/{id}", HandleSpecificProductUpdate)
            .WithName("UpdateSpecificProduct")
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status200OK);

        app.MapDelete("/products/{id}", HandleSpecificProductDelete)
            .WithName("DeleteSpecificProduct")
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }

    private async static Task<IResult> HandleCategoryQuery(ApplicationDataContext context)
    {
        var categories = await context.Products.Select(p => p.Category).Distinct().Where(c => c != null).ToArrayAsync();

        return Results.Ok(new CategoryQueryRes(categories));
    }

    private async static Task<IResult> HandleProductQuery([FromQuery] string? category, [FromQuery] decimal? maxPrice, ApplicationDataContext context)
    {
        var query = context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category == category);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.PricePerUnit <= maxPrice.Value);
        }

        var products = await query.ToListAsync();
        return Results.Ok(products);
    }

    private async static Task<IResult> HandleSpecificProductQuery(ApplicationDataContext context, [FromRoute] int id)
    {
        var product = await context.Products.FindAsync(id);

        if(product == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(product);
    }

    private async static Task<IResult> HandleSpecificProductUpdate(ApplicationDataContext context, [FromRoute] int id, [FromBody] ProductUpdateReq req)
    {
        if(req.ProductCode.Length > 10)
        {
            return Results.BadRequest("ProductCode may be a maximum of 10 Letters");
        }

        if (req.ProductName.Length > 100)
        {
            return Results.BadRequest("ProductName may be a maximum of 100 Letters");
        }

        if (req.ProductDescription != null && req.ProductDescription.Length > 255)
        {
            return Results.BadRequest("ProductDescription may be a maximum of 255 Letters");
        }

        if (req.Category != null && req.Category.Length > 50)
        {
            return Results.BadRequest("Category may be a maximum of 50 Letters");
        }

        var product = await context.Products.FindAsync(id);

        if(product == null)
        {
            return Results.NotFound();
        }

        product.Category = req.Category;
        product.PricePerUnit = req.PricePerUnit;
        product.ProductName = req.ProductName;
        product.ProductDescription = req.ProductDescription;

        context.Products.Update(product);
        await context.SaveChangesAsync();

        return Results.Ok();
    }

    private static async Task<IResult> HandleSpecificProductDelete(ApplicationDataContext context, [FromRoute] int id)
    {
        var product = await context.Products.FindAsync(id);

        if(product == null)
        {
            return Results.NotFound();
        }

        context.Products.Remove(product);

        await context.SaveChangesAsync();

        return Results.NoContent();
    }
}

public record CategoryQueryRes(string[] Categories);


public record ProductUpdateReq(string ProductCode, string ProductName, string? ProductDescription, string? Category, decimal PricePerUnit);