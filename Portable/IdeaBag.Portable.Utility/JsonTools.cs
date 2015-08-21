using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IdeaBag.Portable.Utilty
{
    public static class JsonTools
    {
        #region Deserialize

        public static T Deserialize<T>(string value)
        {

            T unpackedResult = JsonConvert.DeserializeObject<T>(value); 

            return unpackedResult;
        }

        #endregion


        #region Serialize

        public static string Serialize<T>(T source)
        {
            string result = JsonConvert.SerializeObject(source);

            return result;
        }

        #endregion

    }
}
