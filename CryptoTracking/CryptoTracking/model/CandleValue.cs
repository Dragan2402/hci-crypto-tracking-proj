using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTracking.model
{
    internal enum CandleValue
    {
        Open = 1,
        High = 2,
        Low = 3,
        Close = 4
    }

    public static class ValueTypeExtensions
    {
        public static string GetKey(this ValueType value, string market, bool intraDay)
        {
            string key = ((int)(CandleValue)value).ToString();
            if (!intraDay)
                key += "a";
            key += ". " + value.ToString().ToLower();
            if (!intraDay)
                key += " (" + market + ")";
            return key;
        }
    }
}
