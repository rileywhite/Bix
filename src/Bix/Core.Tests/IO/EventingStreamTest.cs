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

using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bix.Core.IO
{
    public class EventingStreamTest
    {
        [Fact]
        public void CannotReadOrWriteByte()
        {
            // arrange
            var stream = Mock.Of<Stream>();
            var m = Mock.Get(stream);
            m.Setup(s => s.CanRead).Returns(true);
            m.Setup(s => s.CanWrite).Returns(true);
            var eventingStream = new EventingStream(stream);

            // act & assert
            Assert.Throws<NotSupportedException>(() => eventingStream.ReadByte());
            Assert.Throws<NotSupportedException>(() => eventingStream.WriteByte(0));
        }

        [Fact]
        public void PassThroughFunctionalityWorks()
        {
            // arrange
            var copyTargetStream = new MemoryStream();
            var stream = Mock.Of<Stream>();
            var m = Mock.Get(stream);
            m.Setup(s => s.CanRead).Returns(true).Verifiable();
            m.Setup(s => s.CanSeek).Returns(true).Verifiable();
            m.Setup(s => s.CanTimeout).Returns(true).Verifiable();
            m.Setup(s => s.CanWrite).Returns(true).Verifiable();
            m.Setup(s => s.Close()).Verifiable();
            m.Setup(s => s.CopyToAsync(copyTargetStream, 15, default(CancellationToken))).Returns(Task.CompletedTask).Verifiable();
            m.Setup(s => s.Flush()).Verifiable();
            m.Setup(s => s.FlushAsync(default(CancellationToken))).Returns(Task.CompletedTask).Verifiable();
            m.Setup(s => s.Length).Returns(93874).Verifiable();
            m.Setup(s => s.Position).Returns(3232).Verifiable();
            m.SetupSet(s => s.Position = 2334).Verifiable();
            m.Setup(s => s.ReadTimeout).Returns(827).Verifiable();
            m.SetupSet(s => s.ReadTimeout = 345).Verifiable();
            m.Setup(s => s.WriteTimeout).Returns(923).Verifiable();
            m.SetupSet(s => s.WriteTimeout = 223).Verifiable();
            m.Setup(s => s.Seek(3897, SeekOrigin.End)).Returns(8927489).Verifiable();
            m.Setup(s => s.SetLength(974897)).Verifiable();
            var eventingStream = new EventingStream(stream);

            // act
            var canRead = eventingStream.CanRead;
            var canSeek = eventingStream.CanSeek;
            var canTimeout = eventingStream.CanTimeout;
            var canWrite = eventingStream.CanWrite;
            eventingStream.Close();
            var copyToAsyncTask = eventingStream.CopyToAsync(copyTargetStream, 15, default(CancellationToken));
            eventingStream.Flush();
            var flushAsyncTask = eventingStream.FlushAsync(default(CancellationToken));
            var length = eventingStream.Length;
            var position = eventingStream.Position;
            eventingStream.Position = 2334;
            var readTimeout = eventingStream.ReadTimeout;
            eventingStream.ReadTimeout = 345;
            var writeTimeout = eventingStream.WriteTimeout;
            eventingStream.WriteTimeout = 223;
            eventingStream.Seek(3897, SeekOrigin.End);
            eventingStream.SetLength(974897);

            // assert
            m.Verify();
            Assert.True(canRead);
            Assert.True(canSeek);
            Assert.True(canTimeout);
            Assert.True(canWrite);
            Assert.True(copyToAsyncTask.IsCompletedSuccessfully);
            Assert.True(flushAsyncTask.IsCompletedSuccessfully);
            Assert.Equal(93874, length);
            Assert.Equal(3232, position);
            Assert.Equal(827, readTimeout);
            Assert.Equal(923, writeTimeout);
        }

        [Fact]
        public void WriteRaisesDataWriteCompleted()
        {
            // arrange
            var buffer = new byte[7];
            var m = new Mock<Stream>();
            m.SetupSequence(s => s.Position).Returns(3374893).Returns(7237278);
            m.Setup(s => s.Write(buffer, 938, 3884)).Verifiable();
            DataWriteCompletedEventArgs args = null;
            var eventingStream = new EventingStream(m.Object);
            eventingStream.DataWriteCompleted += (object sender, DataWriteCompletedEventArgs dwce) => args = dwce;

            // act
            eventingStream.Write(buffer, 938, 3884);

            // assert
            m.Verify();
            Assert.NotNull(args);
            Assert.Same(buffer, args.Buffer);
            Assert.Equal(938, args.Offset);
            Assert.Equal(3884, args.Count);
            Assert.Equal(3374893, args.PositionBeforeWrite);
            Assert.Equal(7237278, args.PositionAfterWrite);
        }

        [Fact]
        public async Task WriteAsyncRaisesDataWriteCompleted()
        {
            // arrange
            var buffer = new byte[0];
            var m = new Mock<Stream>();
            m.SetupSequence(s => s.Position).Returns(6821).Returns(684651);
            m.Setup(s => s.WriteAsync(buffer, 831, 4833, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
            DataWriteCompletedEventArgs args = null;
            var eventingStream = new EventingStream(m.Object);
            eventingStream.DataWriteCompleted += (object sender, DataWriteCompletedEventArgs dwce) => args = dwce;

            // act
            await eventingStream.WriteAsync(buffer, 831, 4833);

            // assert
            m.Verify();
            Assert.NotNull(args);
            Assert.Same(buffer, args.Buffer);
            Assert.Equal(831, args.Offset);
            Assert.Equal(4833, args.Count);
            Assert.Equal(6821, args.PositionBeforeWrite);
            Assert.Equal(684651, args.PositionAfterWrite);
        }

        [Fact]
        public void BeginWriteCallsThroughToWriteAsync()
        {
            // arrange
            var buffer = new byte[3];
            var asyncCallback = Mock.Of<AsyncCallback>();
            var m = new Mock<EventingStream>(Stream.Null) { CallBase = true };
            m.Setup(es => es.WriteAsync(buffer, 234, 112, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
            var eventingStream = m.Object;

            // act
            var result = eventingStream.BeginWrite(buffer, 234, 112, asyncCallback, null);
            eventingStream.EndWrite(result);

            // assert
            m.Verify();
            Assert.True(result.IsCompleted);
        }

        [Fact]
        public void ReadRaisesDataReadCompleted()
        {
            // arrange
            var buffer = new byte[0];
            var m = new Mock<Stream>();
            m.SetupSequence(s => s.Position).Returns(846).Returns(7);
            m.Setup(s => s.Read(buffer, 88, 88483)).Returns(48).Verifiable();
            DataReadCompletedEventArgs args = null;
            var eventingStream = new EventingStream(m.Object);
            eventingStream.DataReadCompleted += (object sender, DataReadCompletedEventArgs drce) => args = drce;

            // act
            var count = eventingStream.Read(buffer, 88, 88483);

            // assert
            m.Verify();
            Assert.NotNull(args);
            Assert.Equal(48, count);
            Assert.Same(buffer, args.Buffer);
            Assert.Equal(88, args.Offset);
            Assert.Equal(88483, args.MaxCount);
            Assert.Equal(48, args.ActualCount);
            Assert.Equal(846, args.PositionBeforeRead);
            Assert.Equal(7, args.PositionAfterRead);
        }

        [Fact]
        public void EmptyReadDoesNotRaiseDataReadCompleted()
        {
            // arrange
            var buffer = new byte[0];
            var m = new Mock<Stream>();
            m.SetupSequence(s => s.Position).Returns(846).Returns(7);
            m.Setup(s => s.Read(buffer, 88, 88483)).Returns(0).Verifiable();
            DataReadCompletedEventArgs args = null;
            var eventingStream = new EventingStream(m.Object);
            eventingStream.DataReadCompleted += (object sender, DataReadCompletedEventArgs drce) => args = drce;

            // act
            var count = eventingStream.Read(buffer, 88, 88483);

            // assert
            m.Verify();
            Assert.Null(args);
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task ReadAsyncRaisesDataReadCompleted()
        {
            // arrange
            var buffer = new byte[0];
            var m = new Mock<Stream>();
            m.SetupSequence(s => s.Position).Returns(1684).Returns(684315);
            m.Setup(s => s.ReadAsync(buffer, 8453, 3515, It.IsAny<CancellationToken>())).Returns(Task.FromResult(463)).Verifiable();
            DataReadCompletedEventArgs args = null;
            var eventingStream = new EventingStream(m.Object);
            eventingStream.DataReadCompleted += (object sender, DataReadCompletedEventArgs drce) => args = drce;

            // act
            var count = await eventingStream.ReadAsync(buffer, 8453, 3515);

            // assert
            m.Verify();
            Assert.NotNull(args);
            Assert.Equal(463, count);
            Assert.Same(buffer, args.Buffer);
            Assert.Equal(8453, args.Offset);
            Assert.Equal(3515, args.MaxCount);
            Assert.Equal(463, args.ActualCount);
            Assert.Equal(1684, args.PositionBeforeRead);
            Assert.Equal(684315, args.PositionAfterRead);
        }

        [Fact]
        public async Task EmptyReadAsyncDoesNotRaiseDataReadCompleted()
        {
            // arrange
            var buffer = new byte[0];
            var m = new Mock<Stream>();
            m.SetupSequence(s => s.Position).Returns(1684).Returns(684315);
            m.Setup(s => s.ReadAsync(buffer, 8453, 3515, It.IsAny<CancellationToken>())).Returns(Task.FromResult(0)).Verifiable();
            DataReadCompletedEventArgs args = null;
            var eventingStream = new EventingStream(m.Object);
            eventingStream.DataReadCompleted += (object sender, DataReadCompletedEventArgs drce) => args = drce;

            // act
            var count = await eventingStream.ReadAsync(buffer, 8453, 3515);

            // assert
            m.Verify();
            Assert.Null(args);
            Assert.Equal(0, count);
        }

        [Fact]
        public void BeginReadCallsThroughToReadAsync()
        {
            // arrange
            var buffer = new byte[3];
            var asyncCallback = Mock.Of<AsyncCallback>();
            var m = new Mock<EventingStream>(Stream.Null) { CallBase = true };
            m.Setup(es => es.ReadAsync(buffer, 4848, 554, It.IsAny<CancellationToken>())).Returns(Task.FromResult(2622)).Verifiable();
            var eventingStream = m.Object;

            // act
            var result = eventingStream.BeginRead(buffer, 4848, 554, asyncCallback, null);
            var count = eventingStream.EndRead(result);

            // assert
            m.Verify();
            Assert.True(result.IsCompleted);
            Assert.Equal(2622, count);
        }
    }
}
