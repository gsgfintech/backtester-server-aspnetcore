using Capital.GSG.FX.Data.Core.ContractData;
using Capital.GSG.FX.Data.Core.OrderData;

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

        public static string ShortenOrigin(OrderOrigin origin)
        {
            switch (origin)
            {
                case OrderOrigin.PositionOpen:
                    return "PO";
                case OrderOrigin.PositionClose:
                    return "PC";
                case OrderOrigin.PositionClose_ContStop:
                    return "PCS";
                case OrderOrigin.PositionClose_ContLimit:
                    return "PCL";
                case OrderOrigin.PositionClose_TE:
                    return "PCT";
                case OrderOrigin.PositionClose_CircuitBreaker:
                    return "PCB";
                case OrderOrigin.PositionReverse_Close:
                    return "PRC";
                case OrderOrigin.PositionReverse_Open:
                    return "PRO";
                case OrderOrigin.PositionDouble:
                    return "PD";
                default:
                    return null;
            }
        }
    }
}
