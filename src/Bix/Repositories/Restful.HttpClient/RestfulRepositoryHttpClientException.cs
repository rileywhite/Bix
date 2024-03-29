﻿/***************************************************************************/
// Copyright 2013-2022 Riley White
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

namespace Bix.Repositories.Restful.HttpClient
{
    [Serializable]
    public class RestfulRepositoryHttpClientException : Exception
    {
        public Guid LogInstance { get; private set; } = Guid.NewGuid();

        public HttpResponseMessage ErrorResponse { get; set; }

        public RestfulRepositoryHttpClientException() { }
        public RestfulRepositoryHttpClientException(string message) : base(message) { }
        public RestfulRepositoryHttpClientException(string message, Exception innerException) : base(message, innerException) { }

        private RestfulRepositoryHttpClientException(SerializationInfo info, StreamingContext context)
        {
            this.LogInstance = Guid.Parse(info.GetString("LogInstance"));
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LogInstance", this.LogInstance.ToString());
        }
    }
}
