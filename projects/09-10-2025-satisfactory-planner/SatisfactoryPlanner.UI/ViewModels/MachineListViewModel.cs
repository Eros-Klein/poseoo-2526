using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SatisfactoryPlanner.Data;

namespace SatisfactoryPlanner.UI.ViewModels;

public class MachineListViewModel: ViewModelBase
{
    public ObservableCollection<Machine> Machines { get; set; }
    
    private readonly IDbContextFactory<ApplicationDataContext> _dbContextFactory;

    public MachineListViewModel(IDbContextFactory<ApplicationDataContext> factory)
    {
        _dbContextFactory = factory;
        Machines = new ObservableCollection<Machine>();
        
        SeedDbAsync();
        InitializeAsync();
    }

    private async Task SeedDbAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        if(!context.Machines.Any())
        {
            await Sample.SeedData(context);
        }
    }
    private async Task InitializeAsync()
    {
        var machines = await GetAllMachines();

        foreach (var machine in machines)
        {
            Machines.Add(machine);
        }
    }

    private async Task<Machine[]> GetAllMachines()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Machines
            .Include(m => m.Recipe)
                .ThenInclude(r => r.ElementLines)
                .ThenInclude(el => el.Element)
            .Include(m => m.Recipe)
                .ThenInclude(r => r.ResultingElement)
            .ToArrayAsync();
    }
}