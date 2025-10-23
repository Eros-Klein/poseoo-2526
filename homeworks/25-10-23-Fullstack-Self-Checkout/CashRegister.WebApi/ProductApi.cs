using AppServices;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

public static partial class ProductApi
{
    public static IEndpointRouteBuilder MapProductApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/product");

        api.MapGet("/", GetAllProducts)
            .WithName("GetAllProducts");
        
        api.MapPost("/checkout", Checkout)
            .WithName("Checkout");
        
        return api;
    }

    static async Task<IResult> GetAllProducts(ApplicationDataContext context)
    {
        var products = await context.Products.ToListAsync();

        if (!products.Any())
        {
            foreach (var product in Samples.Products)
            {
                await context.Products.AddAsync(product);
            }
            
            await context.SaveChangesAsync();
            
            products = await context.Products.ToListAsync();
        }
        
        return Results.Ok(products);
    }

    static async Task<IResult> Checkout(CheckoutRequestDTO req, ApplicationDataContext context)
    {
        var receipt = new Receipt();
        
        context.Receipts.Add(receipt);
        await context.SaveChangesAsync();

        var totalPrice = 0f;

        foreach (var reqReceiptLine in req.ReceiptLines)
        {
            var product = await context.Products.FindAsync(reqReceiptLine.ProductId);

            if (product == null)
            {
                return Results.NotFound("At least one product does not exist");
            }
            
            totalPrice += product.UnitPrice * reqReceiptLine.Quantity;

            var receiptLine = new ReceiptLine()
            {
                ProductId = reqReceiptLine.ProductId,
                Quantity = reqReceiptLine.Quantity,
                Price = product.UnitPrice * reqReceiptLine.Quantity,
                ReceiptId = receipt.Id
            };
            
            context.ReceiptLines.Add(receiptLine);
        }
        
        receipt.TotalPrice = totalPrice;
        
        context.Receipts.Update(receipt);
        
        await context.SaveChangesAsync();
        
        return Results.Created();
    }
}

public record CheckoutRequestDTO(List<ReceiptLineDTO> ReceiptLines);

public record ReceiptLineDTO(int ProductId, int Quantity);