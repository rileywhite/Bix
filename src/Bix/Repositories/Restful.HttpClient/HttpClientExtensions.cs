using System;
using System.Net;

namespace Bix.Repositories.Restful.HttpClient
{
    public static class HttpClientExtensions
    {
        public static string ToUrlSegment(this object source)
        {
            return WebUtility.UrlEncode(source.ToString()).Replace("+", "%20");
        }
    }
}
