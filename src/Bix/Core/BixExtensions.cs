/***************************************************************************/
// Copyright 2013-2018 Riley White
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

namespace Bix.Core
{
    /// <summary>
    /// Extensions methods that are useful through all of Bix
    /// </summary>
    public static class BixExtensions
    {
        /// <summary>
        /// Attempts to serialize an object to JSON
        /// </summary>
        /// <param name="source">Object to serialized</param>
        /// <param name="jsonSerializerSettings">Optional serializer settings</param>
        /// <returns>Serialized object</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <c>null</c></exception>
        public static string ToJson(this object source, JsonSerializerSettings jsonSerializerSettings = null)
        {
            if (source == null) { throw new ArgumentNullException(nameof(source)); }

            return JsonConvert.SerializeObject(source, jsonSerializerSettings);
        }

        /// <summary>
        /// Deserialize an object from JSON
        /// </summary>
        /// <typeparam name="T">Type of serialized object</typeparam>
        /// <param name="source">JSON to deserialize</param>
        /// <param name="jsonSerializerSettings">Optional serializer settings</param>
        /// <returns>Serialized object or <c>null</c> if the serialized object is invalid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is <c>null</c></exception>
        public static T ConvertFromJson<T>(this string source, JsonSerializerSettings jsonSerializerSettings = null)
        {
            if (source == null) { throw new ArgumentNullException(nameof(source)); }

            return JsonConvert.DeserializeObject<T>(source, jsonSerializerSettings);
        }

        /// <summary>
        /// Attempt to deserialize an object from JSON
        /// </summary>
        /// <typeparam name="T">Type of serialized object</typeparam>
        /// <param name="source">JSON to deserialize</param>
        /// <param name="object">To be populated with desieralized object</param>
        /// <param name="jsonSerializerSettings">Optional serializer settings</param>
        /// <returns><c>true</c> if deserialization is successful, else <c>false</c></returns>
        public static bool TryConvertFromJson<T>(this string source, out T @object, JsonSerializerSettings jsonSerializerSettings = null)
        {
            try { @object = source.ConvertFromJson<T>(jsonSerializerSettings); }
            catch
            {
                @object = default(T);
                return false;
            }

            return @object != null;
        }
    }
}
