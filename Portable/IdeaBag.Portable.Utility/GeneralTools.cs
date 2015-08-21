using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IdeaBag.Portable.Utilty
{
    public static class GeneralTools
    {
        #region Calculation Helpers

        /// <summary>
        /// Converts a local date value to UTC time
        /// </summary>
        /// <param name="currentlocaldate">Local date at this moment</param>
        /// <param name="localdatetoconvert">Local date to be converted to UTC time</param>
        /// <returns>A Local Date converted to its UTC equivalent</returns>
        public static DateTime ConvertLocalToUTC(DateTime currentlocaldate, DateTime localdatetoconvert)
        {
            DateTime utcdate = new DateTime();

            TimeSpan utcoffset = currentlocaldate - DateTime.UtcNow;

            utcdate = localdatetoconvert - utcoffset;

            return utcdate;
        }

        #endregion

    }
}
