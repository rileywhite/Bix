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

using System;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Bix.IO.HttpClient
{
    /// <summary>
    /// Thrown when an error occurs in data transfer using <see cref="HttpClientDataSinkBase"/>.
    /// </summary>
    [Serializable]
    public class DataSinkHttpClientException : Exception
    {
        /// <summary>
        /// Gets a unique identifier of this error that can be found in log files
        /// </summary>
        public Guid LogInstance { get; private set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the error response message received from the server.
        /// </summary>
        public HttpResponseMessage ErrorResponse { get; set; }

        /// <summary>
        /// Creates a new <see cref="DataSinkHttpClientException"/>
        /// </summary>
        public DataSinkHttpClientException() { }

        /// <summary>
        /// Creates a new <see cref="DataSinkHttpClientException"/>
        /// </summary>
        /// <param name="message">Error message</param>
        public DataSinkHttpClientException(string message) : base(message) { }

        /// <summary>
        /// Creates a new <see cref="DataSinkHttpClientException"/>
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="innerException">Exception that was handled in the generation of this exception</param>
        public DataSinkHttpClientException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Creates a new <see cref="DataSinkHttpClientException"/> through deserialization
        /// </summary>
        /// <param name="info">Info for deserialization</param>
        /// <param name="context">Context for deserialization</param>
        private DataSinkHttpClientException(SerializationInfo info, StreamingContext context)
        {
            this.LogInstance = Guid.Parse(info.GetString("LogInstance"));
        }

        /// <summary>
        /// Serializes an exception
        /// </summary>
        /// <param name="info">Info for serialization</param>
        /// <param name="context">Context for serialization</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LogInstance", this.LogInstance.ToString());
        }
    }
}
