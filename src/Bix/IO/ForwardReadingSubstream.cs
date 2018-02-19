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

using Nito.AsyncEx;
using Nito.AsyncEx.Interop;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Bix.IO
{
    public class ForwardReadingSubstream : MemoryStream, IEventingStream
    {
        #region Construction & Disposal

        public ForwardReadingSubstream(IEventingStream baseStream, long startAt = 0, long? maxLength = default(long?))
        {
            Contract.Requires(baseStream != null);
            Contract.Requires(baseStream.AsStream.CanRead);
            Contract.Ensures(this.MappedStream != null);
            Contract.Ensures(this.MappedStream.AsStream.CanRead);
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

        Stream IEventingStream<Stream>.AsStream => this;
        public IEventingStream MappedStream { get; }
        public long StartAt { get; }
        public long? MaxLength { get; }

        #region Stream Behavior

        private AsyncManualResetEvent ManualResetEvent { get; } = new AsyncManualResetEvent(false);
        private AsyncMonitor ReadWriteMonitor { get; } = new AsyncMonitor();
        protected virtual long WritePosition { get; set; }

        private bool HasMappedStreamSignaledEmptyRead { get; set; }

        private void MappedStream_DataReadCompleted(object sender, DataReadCompletedEventArgs e)
        {
            if (e.ActualCount == 0)
            {
                this.HasMappedStreamSignaledEmptyRead = true;
                this.ManualResetEvent.Set();
                return;
            }

            if (!e.TryGetWriteOffsetAndCountForFowardReadingSubstream(this.StartAt, this.MaxLength, out var writeOffset, out var writeCount)) { return; }

            using (this.ReadWriteMonitor.Enter())
            {
                base.Flush();
                this.MemoryStreamBufferPosition = this.WritePosition;

                this.IsCurrentlyWritable = true;
                try { base.Write(e.Buffer, e.Offset + writeOffset, writeCount); }
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
        protected virtual long ReadPosition { get; set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.IsReading) { return base.Read(buffer, offset, count); }

            if (!this.HasMappedStreamSignaledEmptyRead)
            {
                this.ManualResetEvent.Wait();
                this.ManualResetEvent.Reset();
            }

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
            var position = this.Position;
            this.DataReadCompleted?.Invoke(this, new DataReadCompletedEventArgs(originalPosition, position, buffer, offset, count, actualCount));
            if (this.BytesInBuffer > 0 || this.HasMappedStreamSignaledEmptyRead || position > this.Length) { this.ManualResetEvent.Set(); }
            return actualCount;
        }

        public async override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (this.IsReading) { return await base.ReadAsync(buffer, offset, count, cancellationToken); }

            if (!this.HasMappedStreamSignaledEmptyRead)
            {
                await this.ManualResetEvent.WaitAsync(cancellationToken);
                this.ManualResetEvent.Reset();
            }

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
            var position = this.Position;
            this.DataReadCompleted?.Invoke(this, new DataReadCompletedEventArgs(originalPosition, position, buffer, offset, count, actualCount));
            if (this.BytesInBuffer > 0 || this.HasMappedStreamSignaledEmptyRead || position > this.Length) { this.ManualResetEvent.Set(); }
            return actualCount;
        }

        private void CompressMemory()
        {
            // this has been problematic...makes full integration test deadlock

            //if (this.BytesInBuffer == 0)
            //{
            //    base.Flush();
            //    var wasReading = this.IsReading;
            //    this.IsReading = true;
            //    try
            //    {
            //        this.MemoryStreamBufferPosition = 0;
            //        base.SetLength(0);
            //    }
            //    finally { this.IsReading = wasReading; }
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
                if (this.IsCurrentlyWritable) { return base.Length; }
                var potentialLength = this.MappedStream.AsStream.Length - this.StartAt;
                return Math.Max(0, this.MaxLength.HasValue ? Math.Min(this.MaxLength.Value, potentialLength) : potentialLength);
            }
        }

        private long MemoryStreamBufferPosition
        {
            get => base.Position;
            set => base.Position = value;
        }

        public virtual long BytesInBuffer => Math.Max(0, this.WritePosition - this.ReadPosition);

        public override long Position
        {
            get
            {
                if (this.IsCurrentlyWritable) { return base.Position; }
                var unmappedPosition = this.MappedStream.AsStream.Position;
                var bytesInBuffer = this.BytesInBuffer;
                var positionOffsetByStartAndBuffer = Math.Max(unmappedPosition - this.StartAt - bytesInBuffer, 0L);
                if (!this.MaxLength.HasValue) { return positionOffsetByStartAndBuffer; }

                var overrun = Math.Max(0, positionOffsetByStartAndBuffer - this.MaxLength.Value + bytesInBuffer);

                return Math.Min(positionOffsetByStartAndBuffer - overrun, this.MaxLength.Value);
            }
            set => throw new NotSupportedException();
        }

        #endregion

        #region Unsupported

        public override bool CanSeek => false;
        public event EventHandler<DataWriteCompletedEventArgs> DataWriteCompleted
        {
            add { throw new NotSupportedException(); }
            remove { throw new NotSupportedException(); }
        }
        public override int ReadByte() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void WriteByte(byte value) => throw new NotSupportedException();

        #endregion
    }

    public static class ForwardReadingSubstreamExtensions
    {
        public static bool TryGetWriteOffsetAndCountForFowardReadingSubstream(
            this DataReadCompletedEventArgs source,
            long startAt,
            long? maxLength,
            out int writeOffset,
            out int writeCount)
        {
            writeOffset = Math.Max(0, (int)(startAt - source.PositionBeforeRead));

            switch (maxLength)
            {
                // data is fully left or right of the target part of the stream
                case null when source.PositionAfterRead < startAt:
                case long ml when source.PositionAfterRead < startAt || source.PositionBeforeRead >= startAt + ml:
                    writeCount = -1;
                    break;

                // data overlaps left border of target area only
                case long ml when source.PositionBeforeRead < startAt && source.PositionAfterRead < (startAt + ml):
                case null when source.PositionBeforeRead < startAt:
                    writeCount = source.ActualCount - writeOffset;
                    break;

                // data overlaps right border of target area only
                case long ml when source.PositionBeforeRead >= startAt && source.PositionAfterRead >= (startAt + ml):
                    writeCount = (int)(startAt + ml - source.PositionBeforeRead);
                    break;

                // data is completely contained within target area
                case null when source.PositionBeforeRead >= startAt:
                case long ml when source.PositionBeforeRead >= startAt && source.PositionAfterRead < (startAt + ml):
                    writeCount = source.ActualCount;
                    break;

                // data completely contains target area
                default:
                    Contract.Assert(
                        maxLength.HasValue &&
                        source.PositionBeforeRead < startAt &&
                        source.PositionAfterRead >= (startAt + maxLength.Value));
                    writeCount = (int)maxLength.Value;
                    break;
            }

            if (writeCount > 0 && writeOffset >= 0) { return true; }

            writeCount = -1;
            writeOffset = -1;
            return false;
        }
    }
}
