using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic.CompilerServices;

namespace _25_09_12_Avalonia_Calculator.ViewModels;

public partial class CalculatorViewModel : ViewModelBase
{
    [ObservableProperty] private string _displayValue = string.Empty;

    private double _storedNumber = 0.0;

    [RelayCommand]
    public void StoreNumber()
    {
        _storedNumber = Calculate(uiRefresh: false);
    }

    [RelayCommand]
    public void RetrieveNumber()
    {
        AddNumber(_storedNumber);
    }

    [RelayCommand]
    public void ClearNumber()
    {
        _storedNumber = 0.0;
    }
    
    [RelayCommand]
    public void AddNumber(double number)
    {
        if(!(!string.IsNullOrEmpty(DisplayValue) && DisplayValue.Last() == ' ' && DisplayValue.Split(' ').Last() == "/" && number == 0))
        {
            DisplayValue += number;
        }
    }

    [RelayCommand]
    public void AddOperator(char op)
    {
        if ((string.IsNullOrEmpty(DisplayValue) || DisplayValue.Last() == ' ') && op == '-')
        {
            DisplayValue += op;
        }
        else if (DisplayValue.Last() == ' ')
        {
            DisplayValue = DisplayValue.Substring(0, DisplayValue.Length - 2);
            DisplayValue += op + " ";
        }
        else DisplayValue += $" {op} ";
    }

    [RelayCommand]
    public void Clear()
    {
        DisplayValue = string.Empty;
    }

    public double Calculate(bool uiRefresh)
    {
        if (string.IsNullOrEmpty(DisplayValue))
        {
            return 0.0;
        }
        
        var calculationSteps = DisplayValue.Split(' ');
        
        var startVal = Convert.ToDouble(calculationSteps[0]);
        
        char storedOp = ' ';
        
        List<CalculationStep> steps = calculationSteps.Skip(1)
                                                .Select((s, i) =>
                                                {
                                                    if (i % 2 == 0)
                                                    {
                                                        storedOp = s[0];
                                                        return null;
                                                    }
                                                    return new CalculationStep(storedOp, Convert.ToDouble(s));
                                                })
                                                .Where(calc => calc != null)
                                                .ToList()!;

        var result = steps.Aggregate(startVal, (res, step) =>
        {
            return step.Operation switch
            {
                '+' => res + step.Value,
                '-' => res - step.Value,
                '*' => res * step.Value,
                '/' => res / step.Value,
                _ => throw new ArgumentException()
            };
        });

        if (uiRefresh)
        {
            DisplayValue = result.ToString(CultureInfo.CurrentCulture);
        }
        
        return result;
    }
    
    private record CalculationStep(char Operation, double Value);
}