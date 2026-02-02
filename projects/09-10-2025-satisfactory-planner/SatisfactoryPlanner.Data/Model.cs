namespace SatisfactoryPlanner.Data;

public class Element
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
}

public class ElementLine
{
    public Guid Id { get; set; }
    
    public Guid ElementId { get; set; }
    public Element?  Element { get; set; }
    
    public int Quantity { get; set; }
    
    public Recipe[]? Recipes { get; set; }
}

public class Recipe
{
    public Guid Id { get; set; }
    
    public ElementLine[]? ElementLines { get; set; }
    
    public Guid ResultingElementId { get; set; }
    public Element? ResultingElement { get; set; }
    
    public RecipeType RecipeType { get; set; }
}

public class Machine
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    public int RecipeLevel { get; set; }
    public RecipeType RecipeType { get; set; }
    
    public Recipe? Recipe { get; set; }
    public Guid? RecipeId { get; set; }
}

public enum RecipeType
{
    Normal,
    Smelter,
    Power
}