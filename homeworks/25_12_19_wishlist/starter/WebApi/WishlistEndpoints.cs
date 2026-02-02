using System.Collections.ObjectModel;
using AppServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using OneOf.Types;

namespace WebApi;

public static class WishlistEndpoints
{
    public static IEndpointRouteBuilder MapWishListEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/verify-pin/{name}", HandlePinCheck)
            .WithName("Verify Pin")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<string>();

        app.MapPost("/wishlist/{name}/items", HandleWishlistRetrieval)
            .WithName("")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<WishlistItem[]>();

        app.MapPatch("/wishlist/{name}/items/{itemId}/mark-as-bought", HandleItemBoughtRemark)
            .WithName("Mark As Bought")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status200OK);

        app.MapDelete("/wishlist/{name}/items/{itemId}", HandleItemDelete)
            .WithName("Delete Item")
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status204NoContent);
        
        app.MapPost("/wishlist/{name}/items/add", HandleItemAdd)
            .WithName("Add Item")
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<WishlistItem>(StatusCodes.Status201Created);

        app.MapPost("/gift-categories", HandleGiftCategoryRetrieval)
            .WithName("Retrieve Gift Categories")
            .Produces<string[]>();

        return app;
    }

    public static async Task<IResult> HandlePinCheck([FromRoute] string name, [FromBody] PinCheckReq req, ApplicationDataContext context)
    {
        var wishlist = await context.Wishlists.FirstAsync(w => w.Name.Equals(name));

        if(wishlist == null)
        {
            return Results.NotFound();
        }

        if (wishlist.ChildPin.Equals(req.Pin))
        {
            return Results.Ok("child");
        }

        if (wishlist.ParentPin.Equals(req.Pin))
        {
            return Results.Ok("parent");
        }

        return Results.BadRequest("Pin is not valid at all");
    }

    public static async Task<IResult> HandleWishlistRetrieval([FromRoute] string name, [FromBody] AuthReq req, ApplicationDataContext context)
    {
        var authRes = await AuthLayer(context, name, req.Pin, PinResponsibility.parent);

        if (authRes != null)
        {
            return authRes;
        }

        var wishlist = await context.Wishlists.Include(w => w.Items).FirstAsync(w => w.Name.Equals(name));

        return Results.Ok(wishlist.Items);
    }

    public static async Task<IResult> HandleItemBoughtRemark([FromRoute] string name, [FromRoute] int itemId, [FromBody] AuthReq req, ApplicationDataContext context)
    {
        var authRes = await AuthLayer(context, name, req.Pin, PinResponsibility.parent);

        if (authRes != null)
        {
            return authRes;
        }

        var wishlist = await context.Wishlists.Include(w => w.Items).FirstAsync(w => w.Name.Equals(name));

        var item = wishlist.Items.Find(i => i.Id.Equals(itemId));

        if(item == null)
        {
            return Results.NotFound();
        }

        if (item.Bought)
        {
            return Results.BadRequest("Item is already marked as bought");
        }

        item.Bought = true;

        context.WishlistItems.Update(item);
        await context.SaveChangesAsync();

        return Results.Ok();
    }

    public static async Task<IResult> HandleItemDelete([FromRoute] string name, [FromRoute] int itemId, [FromBody] AuthReq req, ApplicationDataContext context)
    {
        var authRes = await AuthLayer(context, name, req.Pin, PinResponsibility.parent);

        if (authRes != null)
        {
            return authRes;
        }

        var wishlist = await context.Wishlists.Include(w => w.Items).FirstAsync(w => w.Name.Equals(name));

        var item = wishlist.Items.Find(i => i.Id.Equals(itemId));

        if (item == null)
        {
            return Results.NotFound();
        }

        context.Remove(item);

        await context.SaveChangesAsync();

        return Results.NoContent();
    }

    public static async Task<IResult> HandleItemAdd([FromRoute] string name, [FromBody] ItemPushReq req, ApplicationDataContext context)
    {
        var authRes = await AuthLayer(context, name, req.Auth.Pin, PinResponsibility.child);

        if (authRes != null)
        {
            return authRes;
        }

        var wishlist = await context.Wishlists.Include(w => w.Items).FirstAsync(w => w.Name.Equals(name));

        var category = await context.GiftCategories.FirstAsync(gc => gc.Name.Equals(req.Category));
        
        if (category == null)
        {
            category = new ()
            {
                Name = req.Category
            };

            await context.GiftCategories.AddAsync(category);
        }

        var item = new WishlistItem()
        {
            Wishlist=wishlist,
            Category=category,
            ItemName=req.Name,
            Bought=false
        };

        await context.WishlistItems.AddAsync(item);

        await context.SaveChangesAsync();

        return Results.Created($"{item.Id}", item);
    }

    public static async Task<IResult> HandleGiftCategoryRetrieval(ApplicationDataContext context)
    {
        var categories = await context.GiftCategories.Select(gc => gc.Name).ToListAsync();

        return Results.Ok(categories);
    }

    public static async Task<IResult?> AuthLayer(ApplicationDataContext context, string name, string pin, PinResponsibility level)
    {
        var wishlist = await context.Wishlists.FirstAsync(w => w.Name.Equals(name));

        if(wishlist == null)
        {
            return Results.NotFound();
        }

        if (level == PinResponsibility.parent && !wishlist.ParentPin.Equals(pin))
        {
            return Results.Unauthorized();
        }

        if (level == PinResponsibility.child && !wishlist.ChildPin.Equals(pin))
        {
            return Results.Unauthorized();
        }

        return null;
    }
}

public record PinCheckReq(string Pin);

public record AuthReq(string Pin);

public record ItemPushReq(string Category, string Name, AuthReq Auth);

public enum PinResponsibility
{
    parent=0,
    child=1
}