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

using Nito.AsyncEx;
using Nito.AsyncEx.Interop;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Bix.Core.IO
{
    public class ForwardReadingSubstream : MemoryStream, IEventingStream
    {
        #region Construction & Disposal

        public ForwardReadingSubstream(IEventingStream baseStream, long startAt = 0, long? maxLength = default(long?))
        {
            Contract.Requires(baseStream != null);
            Contract.Requires(baseStream.InnerStream.CanRead);
            Contract.Ensures(this.MappedStream != null);
            Contract.Ensures(this.MappedStream.InnerStream.CanRead);
            this.MappedStream = baseStream;
            this.StartAt = startAt;
            this.MaxLength = maxLength;

            baseStream.DataReadCompleted += this.MappedStream_DataReadCompleted;
        }

        protected ForwardReadingSubstream(IEventingStream baseStream, int maxLength) : this(baseStream, 0, maxLength)
        {
            Contract.Requires(baseStream != null);
            Contract.Requires(baseStream.InnerStream.CanRead);
            Contract.Ensures(this.MappedStream != null);
            Contract.Ensures(this.MappedStream.InnerStream.CanRead);
        }

        protected ForwardReadingSubstream(IEventingStream baseStream, long startAt, int maxLength)
            : base(maxLength)
        {
            Contract.Requires(baseStream != null);
            Contract.Requires(baseStream.InnerStream.CanRead);
            Contract.Ensures(this.MappedStream != null);
            Contract.Ensures(this.MappedStream.InnerStream.CanRead);
            this.MappedStream = baseStream;
            this.StartAt = startAt;
            this.MaxLength = maxLength;

            baseStream.DataReadCompleted += this.MappedStream_DataReadCompleted;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.MappedStream.DataReadCompleted -= this.MappedStream_DataReadCompleted;
            }
        }

        #endregion

        Stream IEventingStream<Stream>.InnerStream => (Stream)this.MappedStream;
        public IEventingStream MappedStream { get; }
        public long StartAt { get; }
        public long? MaxLength { get; }

        #region Stream Behavior

        private AsyncManualResetEvent ManualResetEvent { get; } = new AsyncManualResetEvent(false);
        private AsyncMonitor ReadWriteMonitor { get; } = new AsyncMonitor();
        private long WritePosition { get; set; }

        private void MappedStream_DataReadCompleted(object sender, DataReadCompletedEventArgs e)
        {
            int writeOffset, writeCount;
            if (this.MaxLength.HasValue)
            {
                if (e.PositionAfterRead < this.StartAt || e.PositionBeforeRead > this.StartAt + this.MaxLength.Value) { return; }
                writeOffset = Math.Max(0, (int)(this.StartAt - e.PositionBeforeRead));
                writeCount = Math.Min(e.ActualCount, (int)(this.StartAt + this.MaxLength.Value - e.PositionBeforeRead - writeOffset));
            }
            else
            {
                if (e.PositionAfterRead < this.StartAt) { return; }
                writeOffset = Math.Max(0, (int)(this.StartAt - e.PositionBeforeRead));
                writeCount = Math.Min(e.ActualCount, e.ActualCount - writeOffset);
            }

            using (this.ReadWriteMonitor.Enter())
            {
                base.Flush();
                this.MemoryStreamBufferPosition = this.WritePosition;

                this.IsCurrentlyWritable = true;
                try { base.Write(e.Buffer, writeOffset, writeCount); }
                finally { this.IsCurrentlyWritable = false; }

                this.WritePosition = this.MemoryStreamBufferPosition;
            }
            this.ManualResetEvent.Set();
        }

        public override void Close()
        {
            this.ManualResetEvent.Set();
            base.Close();
        }

        #endregion

        #region Reading

        public override bool CanRead => true;
        private bool IsReading { get; set; }
        public event EventHandler<DataReadCompletedEventArgs> DataReadCompleted;
        private long ReadPosition { get; set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.IsReading) { return base.Read(buffer, offset, count); }

            this.ManualResetEvent.Wait();
            this.ManualResetEvent.Reset();

            long originalPosition;
            int actualCount;
            using (this.ReadWriteMonitor.Enter())
            {
                base.Flush();
                this.MemoryStreamBufferPosition = this.ReadPosition;
                originalPosition = this.Position;
                this.IsReading = true;
                try
                {
                    actualCount = base.Read(buffer, offset, count);
                }
                finally { this.IsReading = false; }
                this.ReadPosition = this.MemoryStreamBufferPosition;
                this.CompressMemory();
            }
            if (actualCount > 0)
            {
                this.DataReadCompleted?.Invoke(this, new DataReadCompletedEventArgs(originalPosition, this.Position, buffer, offset, count, actualCount));
            }
            if (base.Length > 0) { this.ManualResetEvent.Set(); }
            return actualCount;
        }

        public async override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (this.IsReading) { return await base.ReadAsync(buffer, offset, count, cancellationToken); }

            await this.ManualResetEvent.WaitAsync(cancellationToken);
            this.ManualResetEvent.Reset();

            long originalPosition;
            int actualCount;
            using (await this.ReadWriteMonitor.EnterAsync(cancellationToken))
            {
                await base.FlushAsync();
                this.MemoryStreamBufferPosition = this.ReadPosition;
                originalPosition = this.Position;
                this.IsReading = true;
                try
                {
                    actualCount = await base.ReadAsync(buffer, offset, count, cancellationToken);
                }
                finally { this.IsReading = false; }
                this.ReadPosition = this.MemoryStreamBufferPosition;
                this.CompressMemory();
            }
            if (actualCount > 0)
            {
                this.DataReadCompleted?.Invoke(this, new DataReadCompletedEventArgs(originalPosition, this.Position, buffer, offset, count, actualCount));
            }
            if (base.Length > 0) { this.ManualResetEvent.Set(); }
            return actualCount;
        }

        private void CompressMemory()
        {
            // causes data to be read twice...

            //if (this.BytesInBuffer == 0)
            //{
            //    this.ReadPosition = 0;
            //    this.WritePosition = 0;
            //}
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            => ApmAsyncFactory.ToBegin(this.ReadAsync(buffer, offset, count), callback, state);

        public override int EndRead(IAsyncResult asyncResult) => ApmAsyncFactory.ToEnd<int>(asyncResult);

        #endregion

        #region Writing

        private bool IsCurrentlyWritable { get; set; }
        public override bool CanWrite => this.IsCurrentlyWritable;

        #endregion

        #region Recalculated Items

        public override long Length
        {
            get
            {
                var potentialLength = this.MappedStream.InnerStream.Length - this.StartAt;
                return this.MaxLength.HasValue ? Math.Min(this.MaxLength.Value, potentialLength) : potentialLength;
            }
        }

        private long MemoryStreamBufferPosition
        {
            get => base.Position;
            set => base.Position = value;
        }

        private long BytesInBuffer => this.WritePosition - this.ReadPosition;

        public override long Position
        {
            get
            {
                var positionOffsetByStart = Math.Max(this.MappedStream.InnerStream.Position - this.StartAt - this.BytesInBuffer, 0L);
                if (!this.MaxLength.HasValue) { return positionOffsetByStart; }

                var overshotOffset = Math.Max(this.MappedStream.InnerStream.Position - this.MaxLength.Value, 0);
                return positionOffsetByStart - overshotOffset;
            }
            set => this.MappedStream.InnerStream.Position = this.StartAt + value;
        }

        #endregion

        #region Unsupported

        public override bool CanSeek => false;
        public event EventHandler<DataWriteCompletedEventArgs> DataWriteCompleted;
        public override int ReadByte() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void WriteByte(byte value) => throw new NotSupportedException();

        #endregion
    }
}
