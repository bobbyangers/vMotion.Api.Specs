using System;

namespace vMotion.Api.Specs
{
    public static class DateTimeExtensions
    {
        public static DateTimeOffset FirstDayOfNextMonth(this DateTimeOffset dt)
        {
            var ss = new DateTimeOffset(dt.Year, dt.Month, 1, 0, 0, 0, dt.Offset);
            var result = ss.AddMonths(1);
            return result;
        }

        public static DateTimeOffset FirstMondayOfNextMonth(this DateTimeOffset dt)
        {
            var ss = new DateTimeOffset(dt.Year, dt.Month, 1, 0, 0, 0, dt.Offset);
            var result = ss.AddMonths(1);
            while (result.DayOfWeek != DayOfWeek.Monday)
            {
                result = result.AddDays(1);
            }

            return result;
        }
    }
}