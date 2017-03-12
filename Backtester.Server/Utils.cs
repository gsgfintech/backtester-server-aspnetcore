﻿using Capital.GSG.FX.Data.Core.ContractData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backtester.Server
{
    public static class Utils
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
