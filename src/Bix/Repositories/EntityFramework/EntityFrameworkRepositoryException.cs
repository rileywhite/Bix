/***************************************************************************/
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

namespace Bix.Repositories.EntityFramework
{
    //[Serializable]
    public class EntityFrameworkRepositoryException : Exception
    {
        public Guid LogInstance { get; private set; } = Guid.NewGuid();

        public EntityFrameworkRepositoryException() { }
        public EntityFrameworkRepositoryException(string message) : base(message) { }
        public EntityFrameworkRepositoryException(string message, Exception innerException) : base(message, innerException) { }

        // see https://github.com/dotnet/coreclr/issues/2715 for discussion on this
        //private EntityFrameworkRepositoryException(SerializationInfo info, StreamingContext context)
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
