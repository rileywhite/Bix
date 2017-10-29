using System;
using System.Net.Http;

namespace Bix.Repositories.Restful.HttpClient
{
    //[Serializable]
    public class RestfulRepositoryHttpClientException : Exception
    {
        public Guid LogInstance { get; private set; } = Guid.NewGuid();

        public HttpResponseMessage ErrorResponse { get; set; }

        public RestfulRepositoryHttpClientException() { }
        public RestfulRepositoryHttpClientException(string message) : base(message) { }
        public RestfulRepositoryHttpClientException(string message, Exception innerException) : base(message, innerException) { }

        // see https://github.com/dotnet/coreclr/issues/2715 for discussion on this
        //private VelocityApiClientException(SerializationInfo info, StreamingContext context)
        //{
        //    this.LogInstance = Guid.Parse(info.GetString("LogInstance"));
        //}

        //[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    base.GetObjectData(info, context);
        //    info.AddValue("LogInstance", this.LogInstance.ToString());
        //}
    }
}
