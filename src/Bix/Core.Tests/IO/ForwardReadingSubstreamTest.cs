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

using Moq;
using Moq.Protected;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Bix.Core.IO
{
    public class ForwardReadingSubstreamTest
    {
        [Fact]
        public void ConstructorsSetsExpectedProperties()
        {
            // arrange
            var stream = new Mock<Stream>().As<IEventingStream>().Object;


            // act
            var substreamWithMaxLength = new ForwardReadingSubstream(stream, 789327, 243);
            var substreamWithoutMaxLength = new ForwardReadingSubstream(stream, 558);


            // assert
            Assert.Same(stream, substreamWithMaxLength.MappedStream);
            Assert.Same(stream, ((IEventingStream)substreamWithMaxLength).AsStream);
            Assert.Equal(789327, substreamWithMaxLength.StartAt);
            Assert.True(substreamWithMaxLength.MaxLength.HasValue);
            Assert.Equal(243, substreamWithMaxLength.MaxLength.Value);
            Assert.True(substreamWithMaxLength.CanRead);

            Assert.Same(stream, substreamWithoutMaxLength.MappedStream);
            Assert.Same(stream, ((IEventingStream)substreamWithoutMaxLength).AsStream);
            Assert.Equal(558, substreamWithoutMaxLength.StartAt);
            Assert.False(substreamWithoutMaxLength.MaxLength.HasValue);
            Assert.True(substreamWithoutMaxLength.CanRead);
        }

        [Fact]
        public async Task UnsupportedFunctionalityFails()
        {
            // arrange
            var stream = Mock.Of<IEventingStream>();
            var m = Mock.Get(stream);
            var substream = new ForwardReadingSubstream(stream);

            // act
            var canSeek = substream.CanSeek;
            var canWrite = substream.CanWrite;

            // assert
            m.Verify();
            Assert.False(canSeek);
            Assert.False(canWrite);
            Assert.Throws<NotSupportedException>(() => substream.ReadByte());
            Assert.Throws<NotSupportedException>(() => substream.Seek(0, SeekOrigin.Begin));
            Assert.Throws<NotSupportedException>(() => substream.Position = 23);
            Assert.Throws<NotSupportedException>(() => substream.SetLength(3423));
            Assert.Throws<NotSupportedException>(() => substream.WriteByte(3));
            Assert.Throws<NotSupportedException>(() => substream.Write(new byte[] { 0 }, 0, 0));
            Assert.Throws<NotSupportedException>(() => substream.BeginWrite(new byte[] { 0 }, 0, 0, null, null));
            await Assert.ThrowsAsync<NotSupportedException>(async () => await substream.WriteAsync(new byte[] { 0 }, 0, 0));
            Assert.Throws<NotSupportedException>(() => substream.DataWriteCompleted += (sender, e) => { });
            Assert.Throws<NotSupportedException>(() => substream.DataWriteCompleted -= (sender, e) => { });
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(15, 15, 0)]
        [InlineData(0, 100, 100)]
        [InlineData(35, 60, 25)]
        [InlineData(100, 0, 0)]
        public void BytesInBufferIsCalculatedCorrectly(long innerReadPosition, long innerWritePosition, long expectedBytesInBuffer)
        {
            // arrange
            var substreamMock = new Mock<ForwardReadingSubstream>(new object[] { Mock.Of<IEventingStream>(), 0, null }) { CallBase = true };
            substreamMock.Protected().Setup<long>("ReadPosition").Returns(innerReadPosition);
            substreamMock.Protected().Setup<long>("WritePosition").Returns(innerWritePosition);
            var substream = substreamMock.Object;


            // act
            var bytesInBuffer = substream.BytesInBuffer;

            // assert
            Assert.Equal(expectedBytesInBuffer, bytesInBuffer);
        }

        [Theory]
        [InlineData(0, null, 0, 0)]
        [InlineData(0, null, 10, 10)]
        [InlineData(1, null, 10, 9)]
        [InlineData(9, null, 10, 1)]
        [InlineData(10, null, 10, 0)]
        [InlineData(11, null, 10, 0)]
        [InlineData(0, 2, 10, 2)]
        [InlineData(9, 2, 10, 1)]
        [InlineData(10, 2, 10, 0)]
        [InlineData(11, 2, 10, 0)]
        [InlineData(11, 0, 10, 0)]
        public void LengthIsCalculatedCorrectly(long startAt, long? maxLength, long innerLength, long expectedLength)
        {
            // arrange
            var m = new Mock<Stream>();
            m.Setup(s => s.Length).Returns(innerLength);
            var interfaceMock = m.As<IEventingStream>();
            interfaceMock.Setup(s => s.AsStream).Returns(m.Object);
            var substream = new ForwardReadingSubstream(interfaceMock.Object, startAt, maxLength);


            // act
            var length = substream.Length;

            // assert
            Assert.Equal(expectedLength, length);
        }

        [Theory]
        [InlineData(0, null, 0, 0, 0)]
        [InlineData(0, null, 0, 10, 10)]
        [InlineData(0, null, 0, 8468165486415, 8468165486415)]

        [InlineData(5, null, 0, 0, 0)]
        [InlineData(5, null, 0, 4, 0)]
        [InlineData(5, null, 0, 10, 5)]
        [InlineData(5, null, 0, 8468165486415, 8468165486410)]

        [InlineData(0, 4, 0, 0, 0)]
        [InlineData(0, 4, 0, 2, 2)]
        [InlineData(0, 4, 0, 10, 4)]
        [InlineData(0, 4, 0, 8468165486415, 4)]

        [InlineData(5, 8, 0, 0, 0)]
        [InlineData(5, 8, 0, 4, 0)]
        [InlineData(5, 8, 0, 10, 5)]
        [InlineData(5, 8, 0, 13, 8)]
        [InlineData(5, 8, 0, 14, 8)]
        [InlineData(5, 8, 0, 8468165486415, 8)]

        [InlineData(0, null, 4, 10, 6)]
        [InlineData(0, null, 2, 10, 8)]
        [InlineData(0, null, 4, 8468165486415, 8468165486411)]

        [InlineData(5, null, 4, 0, 0)]
        [InlineData(5, null, 4, 10, 1)]
        [InlineData(5, null, 2, 10, 3)]
        [InlineData(5, null, 4, 8468165486415, 8468165486406)]

        [InlineData(0, 8, 4, 4, 0)]
        [InlineData(0, 8, 4, 10, 4)]
        [InlineData(0, 8, 2, 10, 6)]
        [InlineData(0, 8, 4, 8468165486415, 4)]
        [InlineData(0, 8, 1, 8468165486415, 7)]

        [InlineData(5, 8, 4, 9, 0)]
        [InlineData(5, 8, 2, 9, 2)]
        [InlineData(5, 8, 4, 10, 1)]
        [InlineData(5, 8, 2, 10, 3)]
        [InlineData(5, 8, 8, 13, 0)]
        [InlineData(5, 8, 8, 8468165486415, 0)]
        [InlineData(5, 8, 3, 8468165486415, 5)]
        [InlineData(5, 8, 1, 8468165486415, 7)]
        public void PositionIsCalculatedCorrectly(long startAt, long? maxLength, long bytesInBuffer, long innerPosition, long expectedPosition)
        {
            // arrange
            var m = new Mock<Stream>();
            m.Setup(s => s.Position).Returns(innerPosition);
            var interfaceMock = m.As<IEventingStream>();
            interfaceMock.Setup(s => s.AsStream).Returns(m.Object);
            var substreamMock = new Mock<ForwardReadingSubstream>(new object[] { interfaceMock.Object, startAt, maxLength }) { CallBase = true };
            substreamMock.Setup(s => s.BytesInBuffer).Returns(bytesInBuffer);
            var substream = substreamMock.Object;

            // act
            var position = substream.Position;


            // assert
            Assert.Equal(expectedPosition, position);
        }

        [Theory]
        // empty read
        [InlineData(0, null, 0, 0, 0, false, -1, -1)]
        [InlineData(0, null, 100, 100, 0, false, -1, -1)]
        [InlineData(100, null, 0, 0, 0, false, -1, -1)]
        [InlineData(100, 75, 0, 0, 0, false, -1, -1)]

        // data is left of target area without max length
        [InlineData(100, null, 0, 50, 50, false, -1, -1)]
        [InlineData(100, null, 0, 99, 99, false, -1, -1)]
        [InlineData(100, null, 0, 100, 100, false, -1, -1)]

        // data is left of target area with max length
        [InlineData(100, 75, 0, 50, 50, false, -1, -1)]
        [InlineData(100, 75, 0, 99, 99, false, -1, -1)]
        [InlineData(100, 75, 0, 100, 100, false, -1, -1)]

        // data is right of target area
        [InlineData(100, 75, 175, 250, 75, false, -1, -1)]
        [InlineData(100, 75, 176, 177, 1, false, -1, -1)]
        [InlineData(0, 75, 100, 150, 50, false, -1, -1)]

        // data overlaps left border of target area without max length
        [InlineData(100, null, 0, 101, 101, true, 100, 1)]
        [InlineData(100, null, 10, 101, 91, true, 90, 1)]
        [InlineData(100, null, 75, 127, 52, true, 25, 27)]
        [InlineData(100, null, 99, 127, 28, true, 1, 27)]

        // data overlaps left border of target area with max length
        [InlineData(100, 50, 0, 101, 101, true, 100, 1)]
        [InlineData(100, 50, 10, 101, 91, true, 90, 1)]
        [InlineData(100, 50, 75, 127, 52, true, 25, 27)]
        [InlineData(100, 50, 99, 127, 28, true, 1, 27)]

        // data overlaps right border of target area
        [InlineData(100, 75, 174, 176, 2, true, 0, 1)]
        [InlineData(100, 75, 124, 352, 228, true, 0, 51)]
        [InlineData(100, 75, 100, 352, 252, true, 0, 75)]

        // data fully contains target area
        [InlineData(100, 75, 100, 175, 75, true, 0, 75)]
        [InlineData(100, 75, 50, 352, 302, true, 50, 75)]

        // data is fully contained by target area without max length
        [InlineData(0, null, 0, 100, 100, true, 0, 100)]
        [InlineData(100, null, 50, 150, 100, true, 50, 50)]
        [InlineData(100, null, 100, 150, 50, true, 0, 50)]
        [InlineData(100, null, 110, 160, 50, true, 0, 50)]

        // data is fully contained by target area with max length
        [InlineData(100, 75, 100, 150, 50, true, 0, 50)]
        [InlineData(100, 75, 110, 160, 50, true, 0, 50)]
        public void WriteOffsetAndCountAreCorrectlyCalculated(
            long startAt,
            long? maxLength,
            long positionBeforeRead,
            long positionAfterRead,
            int actualReadCount,
            bool expectedSuccess,
            int expectedOffset,
            int expectedCount)
        {
            // arrange
            var e = new DataReadCompletedEventArgs(positionBeforeRead, positionAfterRead, new byte[0], 8734897, 892378493, actualReadCount);


            // act
            var success = e.TryGetWriteOffsetAndCountForFowardReadingSubstream(startAt, maxLength, out var offset, out var count);


            // assert
            Assert.Equal(expectedSuccess, success);
            Assert.Equal(expectedOffset, offset);
            Assert.Equal(expectedCount, count);
        }

        //[Fact]
        //public void ReadRaisesDataReadCompleted()
        //{
        //    // arrange
        //    var buffer = new byte[0];
        //    var m = new Mock<Stream>();
        //    m.SetupSequence(s => s.Position).Returns(846).Returns(7);
        //    m.Setup(s => s.Read(buffer, 88, 88483)).Returns(48).Verifiable();
        //    DataReadCompletedEventArgs args = null;
        //    var eventingStream = new EventingStream(m.Object);
        //    eventingStream.DataReadCompleted += (object sender, DataReadCompletedEventArgs drce) => args = drce;

        //    // act
        //    var count = eventingStream.Read(buffer, 88, 88483);

        //    // assert
        //    m.Verify();
        //    Assert.NotNull(args);
        //    Assert.Equal(48, count);
        //    Assert.Same(buffer, args.Buffer);
        //    Assert.Equal(88, args.Offset);
        //    Assert.Equal(88483, args.MaxCount);
        //    Assert.Equal(48, args.ActualCount);
        //    Assert.Equal(846, args.PositionBeforeRead);
        //    Assert.Equal(7, args.PositionAfterRead);
        //}

        //[Fact]
        //public void EmptyReadDoesNotRaiseDataReadCompleted()
        //{
        //    // arrange
        //    var buffer = new byte[0];
        //    var m = new Mock<Stream>();
        //    m.SetupSequence(s => s.Position).Returns(846).Returns(7);
        //    m.Setup(s => s.Read(buffer, 88, 88483)).Returns(0).Verifiable();
        //    DataReadCompletedEventArgs args = null;
        //    var eventingStream = new EventingStream(m.Object);
        //    eventingStream.DataReadCompleted += (object sender, DataReadCompletedEventArgs drce) => args = drce;

        //    // act
        //    var count = eventingStream.Read(buffer, 88, 88483);

        //    // assert
        //    m.Verify();
        //    Assert.Null(args);
        //    Assert.Equal(0, count);
        //}

        //[Fact]
        //public async Task ReadAsyncRaisesDataReadCompleted()
        //{
        //    // arrange
        //    var buffer = new byte[0];
        //    var m = new Mock<Stream>();
        //    m.SetupSequence(s => s.Position).Returns(1684).Returns(684315);
        //    m.Setup(s => s.ReadAsync(buffer, 8453, 3515, It.IsAny<CancellationToken>())).Returns(Task.FromResult(463)).Verifiable();
        //    DataReadCompletedEventArgs args = null;
        //    var eventingStream = new EventingStream(m.Object);
        //    eventingStream.DataReadCompleted += (object sender, DataReadCompletedEventArgs drce) => args = drce;

        //    // act
        //    var count = await eventingStream.ReadAsync(buffer, 8453, 3515);

        //    // assert
        //    m.Verify();
        //    Assert.NotNull(args);
        //    Assert.Equal(463, count);
        //    Assert.Same(buffer, args.Buffer);
        //    Assert.Equal(8453, args.Offset);
        //    Assert.Equal(3515, args.MaxCount);
        //    Assert.Equal(463, args.ActualCount);
        //    Assert.Equal(1684, args.PositionBeforeRead);
        //    Assert.Equal(684315, args.PositionAfterRead);
        //}

        //[Fact]
        //public async Task EmptyReadAsyncDoesNotRaiseDataReadCompleted()
        //{
        //    // arrange
        //    var buffer = new byte[0];
        //    var m = new Mock<Stream>();
        //    m.SetupSequence(s => s.Position).Returns(1684).Returns(684315);
        //    m.Setup(s => s.ReadAsync(buffer, 8453, 3515, It.IsAny<CancellationToken>())).Returns(Task.FromResult(0)).Verifiable();
        //    DataReadCompletedEventArgs args = null;
        //    var eventingStream = new EventingStream(m.Object);
        //    eventingStream.DataReadCompleted += (object sender, DataReadCompletedEventArgs drce) => args = drce;

        //    // act
        //    var count = await eventingStream.ReadAsync(buffer, 8453, 3515);

        //    // assert
        //    m.Verify();
        //    Assert.Null(args);
        //    Assert.Equal(0, count);
        //}

        //[Fact]
        //public void BeginReadCallsThroughToReadAsync()
        //{
        //    // arrange
        //    var buffer = new byte[3];
        //    var asyncCallback = Mock.Of<AsyncCallback>();
        //    var m = new Mock<EventingStream>(Stream.Null) { CallBase = true };
        //    m.Setup(es => es.ReadAsync(buffer, 4848, 554, It.IsAny<CancellationToken>())).Returns(Task.FromResult(2622)).Verifiable();
        //    var eventingStream = m.Object;

        //    // act
        //    var result = eventingStream.BeginRead(buffer, 4848, 554, asyncCallback, null);
        //    var count = eventingStream.EndRead(result);

        //    // assert
        //    m.Verify();
        //    Assert.True(result.IsCompleted);
        //    Assert.Equal(2622, count);
        //}
    }
}
