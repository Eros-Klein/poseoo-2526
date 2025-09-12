using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace _2025_09_12_Avalonia_Mvvm.ViewModels;

public partial class CalculatorViewModel : ViewModelBase
{
    public double FirstNumber { get; set; } = 99; 
    public double SecondNumber { get; set; } = 99;

    [ObservableProperty] private double _result = 0;
    
    public string SelectedOperator { get; set; } = "+";

    public ObservableCollection<string> Operators { get; } = ["+", "-", "*", "/"];

    [RelayCommand]
    private void Calculate()
    {
        System.Data.DataTable table = new System.Data.DataTable();
        if (SecondNumber != 0)
            Result = Convert.ToDouble(table.Compute($"{FirstNumber}{SelectedOperator}{SecondNumber}", String.Empty));
        else Result = 0;
    }
}