namespace SatisfactoryPlanner.Data;

public class Sample
{
    public static async Task SeedData(ApplicationDataContext context)
    {
        // Clear existing data
        context.Machines.RemoveRange(context.Machines);
        context.Recipes.RemoveRange(context.Recipes);
        context.ElementLines.RemoveRange(context.ElementLines);
        context.Elements.RemoveRange(context.Elements);
        await context.SaveChangesAsync();

        // Create Elements (raw materials and products)
        var ironOre = new Element
        {
            Id = Guid.NewGuid(),
            Name = "Iron Ore"
        };

        var ironIngot = new Element
        {
            Id = Guid.NewGuid(),
            Name = "Iron Ingot"
        };

        var copperOre = new Element
        {
            Id = Guid.NewGuid(),
            Name = "Copper Ore"
        };

        var copperIngot = new Element
        {
            Id = Guid.NewGuid(),
            Name = "Copper Ingot"
        };

        context.Elements.AddRange(ironOre, ironIngot, copperOre, copperIngot);
        await context.SaveChangesAsync();
        
        var ironOreInputLine = new ElementLine
        {
            Id = Guid.NewGuid(),
            ElementId = ironOre.Id,
            Quantity = 30
        };
        // Create Recipe 1: Iron Ore -> Iron Ingot (Smelter)
        var ironSmeltingRecipe = new Recipe
        {
            Id = Guid.NewGuid(),
            ResultingElementId = ironIngot.Id,
            RecipeType = RecipeType.Smelter,
            ElementLines = []
        };

        context.ElementLines.Add(ironOreInputLine);
        context.Recipes.Add(ironSmeltingRecipe);
        
        await context.SaveChangesAsync();

        // Link ElementLine to Recipe
        ironOreInputLine.Recipes = new[] { ironSmeltingRecipe };
        ironSmeltingRecipe.ElementLines = new[] { ironOreInputLine };
        await context.SaveChangesAsync();

        // Create Recipe 2: Copper Ore -> Copper Ingot (Smelter)
        var copperSmeltingRecipe = new Recipe
        {
            Id = Guid.NewGuid(),
            ResultingElementId = copperIngot.Id,
            RecipeType = RecipeType.Smelter
        };

        var copperOreInputLine = new ElementLine
        {
            Id = Guid.NewGuid(),
            ElementId = copperOre.Id,
            Quantity = 30
        };

        context.Recipes.Add(copperSmeltingRecipe);
        context.ElementLines.Add(copperOreInputLine);
        context.SaveChanges();

        // Link ElementLine to Recipe
        copperOreInputLine.Recipes = new[] { copperSmeltingRecipe };
        copperSmeltingRecipe.ElementLines = new[] { copperOreInputLine };
        context.SaveChanges();

        // Create Machine 1: Iron Smelter
        var ironSmelter = new Machine
        {
            Id = Guid.NewGuid(),
            Name = "Iron Smelter Mk.1",
            RecipeLevel = 1,
            RecipeType = RecipeType.Smelter,
            Recipe = ironSmeltingRecipe
        };

        // Create Machine 2: Copper Smelter
        var copperSmelter = new Machine
        {
            Id = Guid.NewGuid(),
            Name = "Copper Smelter Mk.1",
            RecipeLevel = 1,
            RecipeType = RecipeType.Smelter,
            Recipe = copperSmeltingRecipe
        };

        context.Machines.AddRange(ironSmelter, copperSmelter);
        await context.SaveChangesAsync();
    }
}