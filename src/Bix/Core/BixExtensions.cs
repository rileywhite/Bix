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
