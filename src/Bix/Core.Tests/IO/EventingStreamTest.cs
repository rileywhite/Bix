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

using Autofac.Extras.Moq;
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
    }
}
