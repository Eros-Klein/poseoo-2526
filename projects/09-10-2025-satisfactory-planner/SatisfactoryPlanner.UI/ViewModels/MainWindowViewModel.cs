using SatisfactoryPlanner.Data;

namespace SatisfactoryPlanner.UI.ViewModels;

public partial class MainWindowViewModel(MachineListViewModel machineListViewModel, ApplicationDataContext applicationDataContext) : ViewModelBase
{
    public ApplicationDataContext ApplicationDataContext { get; } = applicationDataContext;
    public MachineListViewModel MachineListViewModel { get; } = machineListViewModel;
}
