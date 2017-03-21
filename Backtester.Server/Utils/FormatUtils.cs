using Capital.GSG.FX.Data.Core.ContractData;

namespace Backtester.Server.Utils
{
    public class FormatUtils
    {
        public static string FormatRate(Cross cross, double? rate)
        {
            if (!rate.HasValue)
                return null;
            else
            {
                if (cross.IsJpyCross())
                    return rate.Value.ToString("N3");
                else
                    return rate.Value.ToString("N5");
            }
        }
    }
}
