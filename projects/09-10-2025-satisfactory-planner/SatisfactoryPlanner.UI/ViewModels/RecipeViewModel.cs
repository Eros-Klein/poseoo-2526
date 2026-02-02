using Microsoft.EntityFrameworkCore;
using SatisfactoryPlanner.Data;

namespace SatisfactoryPlanner.UI.ViewModels;

public class RecipeViewModel : ViewModelBase
{
    private readonly IDbContextFactory<ApplicationDataContext> _dbContextFactory;

    public RecipeViewModel(IDbContextFactory<ApplicationDataContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }
}