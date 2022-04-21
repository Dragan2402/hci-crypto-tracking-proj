using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTracking.model
{
    public enum Interval
    {
        OneMin,
        FiveMin,
        FifteenMin,
        ThirtyMin,
        SixtyMin,
        Daily,
        Weekly,
        Monthly
    }

    public static class IntervalExtensions
    {
        public const string INTRADAY_FUNCTION = "CRYPTO_INTRADAY";
        public const string DAILY_FUNCTION = "DIGITAL_CURRENCY_DAILY";
        public const string WEEKLY_FUNCTION = "DIGITAL_CURRENCY_WEEKLY";
        public const string MONTHLY_FUNCTION = "DIGITAL_CURRENCY_MONTHLY";

        public static string GetKey(this Interval value)
        {
            if (value.IsIntraday())
                return "Time Series Crypto (" + value.GetInterval() + ")";
            else
                return "Time Series (Digital Currency " + value.GetInterval() + ")";
        }

        public static bool IsIntraday(this Interval value)
        {
            return value.Equals(Interval.OneMin) || 
                value.Equals(Interval.FiveMin) ||
                value.Equals(Interval.FifteenMin) ||
                value.Equals(Interval.ThirtyMin) ||
                value.Equals(Interval.SixtyMin);
        }

        public static string GetFunction(this Interval value)
        {
            switch (value)
            {
                case Interval.Daily: return DAILY_FUNCTION;
                case Interval.Weekly: return WEEKLY_FUNCTION;
                case Interval.Monthly: return MONTHLY_FUNCTION;
                default: return INTRADAY_FUNCTION;
            }
        }

        public static string GetInterval(this Interval value)
        {
            switch (value)
            {
                case Interval.OneMin: return "1min";
                case Interval.FiveMin: return "5min";
                case Interval.FifteenMin: return "15min";
                case Interval.ThirtyMin: return "30min";
                case Interval.SixtyMin: return "60min";
                default: return value.ToString();
            }
        }
    }

}
