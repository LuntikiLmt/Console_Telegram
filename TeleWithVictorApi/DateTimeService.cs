using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleWithVictorApi
{
    static class DateTimeService
    {
        public static DateTime TimeUnixToWindows(double timestampToConvert, bool isLocal)
        {
            var mdt = new DateTime(1970, 1, 1, 0, 0, 0);
            if (isLocal)
            {
                return mdt.AddSeconds(timestampToConvert).ToLocalTime();
            }
            else
            {
                return mdt.AddSeconds(timestampToConvert);
            }
        }
    }
}
