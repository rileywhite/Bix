/***************************************************************************/
// Copyright 2013-2017 Riley White
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
/***************************************************************************/

using Newtonsoft.Json;
using System;
using System.Text;

namespace Bix.Core
{
    public static class BixExtensions
    {
        public static string ToJson(this object source, JsonSerializerSettings jsonSerializerSettings = null)
        {
            if (source == null) { throw new ArgumentNullException(nameof(source)); }

            return JsonConvert.SerializeObject(source, jsonSerializerSettings);
        }

        public static T ConvertFromJson<T>(this string source, JsonSerializerSettings jsonSerializerSettings = null)
        {
            if (source == null) { throw new ArgumentNullException(nameof(source)); }

            return JsonConvert.DeserializeObject<T>(source, jsonSerializerSettings);
        }

        public static bool TryConvertFromJson<T>(this string source, out T @object, JsonSerializerSettings jsonSerializerSettings = null)
            where T : class
        {
            try { @object = source.ConvertFromJson<T>(jsonSerializerSettings); }
            catch { @object = null; }

            return @object != null;
        }

        public static string Base64Encode(this object source, JsonSerializerSettings jsonSerializerSettings = null)
        {
            return source.ToJson(jsonSerializerSettings).Base64Encode();
        }

        public static string Base64Encode(this string source)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(source));
        }

        public static T Base64Decode<T>(this string source, JsonSerializerSettings jsonSerializerSettings = null)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(source)).ConvertFromJson<T>(jsonSerializerSettings);
        }

        public static string Base64Decode(this string source)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(source));
        }
    }
}
