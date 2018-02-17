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

using Nito.AsyncEx.Interop;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Bix.Core.IO
{
    public class EventingStream : Stream, IEventingStream
    {
        #region Construction & Disposal

        public EventingStream(Stream innerStream) => this.InnerStream = innerStream;

        protected override void Dispose(bool disposing)
        {
            if (disposing) { this.InnerStream?.Dispose(); }
        }

        #endregion

        Stream IEventingStream<Stream>.AsStream => this.InnerStream;
        private Stream InnerStream { get; }

        #region Reading

        public event EventHandler<DataReadCompletedEventArgs> DataReadCompleted;

        public override int Read(byte[] buffer, int offset, int count)
        {
            var initialPosition = this.Position;
            var actualCount = this.InnerStream.Read(buffer, offset, count);
            if (actualCount > 0)
            {
                this.DataReadCompleted?.Invoke(this, new DataReadCompletedEventArgs(initialPosition, this.Position, buffer, offset, count, actualCount));
            }
            return actualCount;
        }

        public async override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var initialPosition = this.Position;
            var actualCount = await this.InnerStream.ReadAsync(buffer, offset, count, cancellationToken);
            if (actualCount > 0)
            {
                this.DataReadCompleted?.Invoke(this, new DataReadCompletedEventArgs(initialPosition, this.Position, buffer, offset, count, actualCount));
            }
            return actualCount;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            => ApmAsyncFactory.ToBegin(this.ReadAsync(buffer, offset, count), callback, state);

        public override int EndRead(IAsyncResult asyncResult) => ApmAsyncFactory.ToEnd<int>(asyncResult);

        public override int ReadByte()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Writing

        public event EventHandler<DataWriteCompletedEventArgs> DataWriteCompleted;

        public override void Write(byte[] buffer, int offset, int count)
        {
            var initialPosition = this.Position;
            this.InnerStream.Write(buffer, offset, count);
            this.DataWriteCompleted?.Invoke(this, new DataWriteCompletedEventArgs(initialPosition, this.Position, buffer, offset, count));
        }

        public async override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var initialPosition = this.Position;
            await this.InnerStream.WriteAsync(buffer, offset, count, cancellationToken);
            this.DataWriteCompleted?.Invoke(this, new DataWriteCompletedEventArgs(initialPosition, this.Position, buffer, offset, count));
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            => ApmAsyncFactory.ToBegin(this.WriteAsync(buffer, offset, count), callback, state);

        public override void EndWrite(IAsyncResult asyncResult) => ApmAsyncFactory.ToEnd(asyncResult);

        public override void WriteByte(byte value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Pass-through

        public override bool CanRead => this.InnerStream.CanRead;
        public override bool CanSeek => this.InnerStream.CanSeek;
        public override bool CanTimeout => this.InnerStream.CanTimeout;
        public override bool CanWrite => this.InnerStream.CanWrite;
        public override void Close() => this.InnerStream.Close();
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            => this.InnerStream.CopyToAsync(destination, bufferSize, cancellationToken);
        public override void Flush() => this.InnerStream.Flush();
        public override Task FlushAsync(CancellationToken cancellationToken) => this.InnerStream.FlushAsync(cancellationToken);
        public override long Length => this.InnerStream.Length;
        public override long Position { get => this.InnerStream.Position; set => this.InnerStream.Position = value; }
        public override int ReadTimeout { get => this.InnerStream.ReadTimeout; set => this.InnerStream.ReadTimeout = value; }
        public override int WriteTimeout { get => this.InnerStream.WriteTimeout; set => this.InnerStream.WriteTimeout = value; }
        public override long Seek(long offset, SeekOrigin origin) => this.InnerStream.Seek(offset, origin);
        public override void SetLength(long value) => this.InnerStream.SetLength(value);

        #endregion
    }
}
