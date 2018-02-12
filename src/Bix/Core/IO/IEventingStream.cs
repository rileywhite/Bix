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

using System;
using System.IO;

namespace Bix.Core.IO
{
    public interface IEventingStream<out TStream> where TStream : Stream
    {
        event EventHandler<DataReadCompletedEventArgs> DataReadCompleted;
        event EventHandler<DataWriteCompletedEventArgs> DataWriteCompleted;
        TStream InnerStream { get; }
    }

    public interface IEventingStream : IEventingStream<Stream> { }

    public class DataReadCompletedEventArgs : EventArgs
    {
        public DataReadCompletedEventArgs(long positionBeforeRead, long positionAfterRead, byte[] buffer, int offset, int maxCount, int actualCount)
        {
            this.PositionBeforeRead = positionBeforeRead;
            this.PositionAfterRead = positionAfterRead;
            this.Buffer = buffer;
            this.Offset = offset;
            this.MaxCount = maxCount;
            this.ActualCount = actualCount;
        }

        public long PositionBeforeRead { get; }
        public long PositionAfterRead { get; }
        public byte[] Buffer { get; }
        public int Offset { get; }
        public int MaxCount { get; }
        public int ActualCount { get; }
    }

    public class DataWriteCompletedEventArgs : EventArgs
    {
        public DataWriteCompletedEventArgs(long positionBeforeRead, long positionAfterRead, byte[] buffer, int offset, int count)
        {
            this.PositionBeforeRead = positionBeforeRead;
            this.PositionAfterRead = positionAfterRead;
            this.Buffer = buffer;
            this.Offset = offset;
            this.Count = count;
        }

        public long PositionBeforeRead { get; }
        public long PositionAfterRead { get; }
        public byte[] Buffer { get; }
        public int Offset { get; }
        public int Count { get; }
    }
}
