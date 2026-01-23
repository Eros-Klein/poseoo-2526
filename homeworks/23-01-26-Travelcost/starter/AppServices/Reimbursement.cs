namespace AppServices;

public interface IReimbursementCalculator
{
    ReimbursementResult CalculateReimbursement(Travel travel);
}

public record ReimbursementResult(
    decimal Mileage,
    decimal PerDiem,
    decimal Expenses
);

public class ReimbursementCalculator : IReimbursementCalculator
{
    public ReimbursementResult CalculateReimbursement(Travel travel)
    {
        double mileage = 0;
        double perDiem = 0;
        double expenses = 0;

        foreach (var reimbursement in travel.Reimbursements)
        {
            if (reimbursement is DriveWithPrivateCarReimbursement dw)
            {
                mileage += dw.KM * 0.5;
            }
            else if (reimbursement is ExpenseReimbursement ex)
            {
                expenses += ex.Amount;
            }
        }

        var timeDiff = (travel.End - travel.Start).TotalHours;

        do
        {
            timeDiff -= 24;
            perDiem += 30;
        } while (timeDiff > 12);

        perDiem += Math.Ceiling(timeDiff)/12 * 30;

        return new ReimbursementResult((decimal) mileage,(decimal) perDiem,(decimal) expenses);
    }
}