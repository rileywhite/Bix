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

using AutoFixture;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Bix.IO.Tests
{
    public class StreamMultipartHashCheckerTest
    {
        [Fact]
        public async Task IndexInMiddleOfTargetIsFound()
        {
            // arrange
            var fixture = new Fixture();
            var sourceBuffer = fixture.CreateMany<byte>(1024 * 10).ToArray();
            var targetBuffer = new byte[sourceBuffer.Length];

            // copy all bytes, then ensure the last one is different
            sourceBuffer.CopyTo(targetBuffer, 0);
            ++targetBuffer[8497];

            using (var sourceStream = new MemoryStream(sourceBuffer))
            using (var sourceHashChecker = new StreamMultipartHashChecker(sourceStream))
            using (var targetStream = new MemoryStream(targetBuffer))
            using (var targetHashChecker = new StreamMultipartHashChecker(targetStream))
            {
                // act
                var inconsistentIndex = await sourceHashChecker.GetInconsistentIndex(targetHashChecker);


                // assert
                Assert.Equal(8497, inconsistentIndex);
            }
        }

        [Fact]
        public async Task IndexAtBeginningOfTargetIsFound()
        {
            // arrange
            var fixture = new Fixture();
            var sourceBuffer = fixture.CreateMany<byte>(1024 * 10).ToArray();
            var targetBuffer = new byte[sourceBuffer.Length];

            // copy all bytes, then change the first
            sourceBuffer.CopyTo(targetBuffer, 0);
            ++targetBuffer[0];

            using (var sourceStream = new MemoryStream(sourceBuffer))
            using (var sourceHashChecker = new StreamMultipartHashChecker(sourceStream))
            using (var targetStream = new MemoryStream(targetBuffer))
            using (var targetHashChecker = new StreamMultipartHashChecker(targetStream))
            {
                // act
                var inconsistentIndex = await sourceHashChecker.GetInconsistentIndex(targetHashChecker);


                // assert
                Assert.Equal(0, inconsistentIndex);
            }
        }

        [Fact]
        public async Task IndexAtEndOfTargetIsFound()
        {
            // arrange
            var fixture = new Fixture();
            var sourceBuffer = fixture.CreateMany<byte>(1024 * 10).ToArray();
            var targetBuffer = new byte[sourceBuffer.Length];

            // copy all bytes, then change the last
            sourceBuffer.CopyTo(targetBuffer, 0);
            ++targetBuffer[targetBuffer.Length - 1];

            using (var sourceStream = new MemoryStream(sourceBuffer))
            using (var sourceHashChecker = new StreamMultipartHashChecker(sourceStream))
            using (var targetStream = new MemoryStream(targetBuffer))
            using (var targetHashChecker = new StreamMultipartHashChecker(targetStream))
            {
                // act
                var inconsistentIndex = await sourceHashChecker.GetInconsistentIndex(targetHashChecker);


                // assert
                Assert.Equal(sourceBuffer.Length - 1, inconsistentIndex);
            }
        }

        [Fact]
        public async Task IdenticalTargetIsFound()
        {
            // arrange
            var fixture = new Fixture();
            var sourceBuffer = fixture.CreateMany<byte>(1024 * 10).ToArray();
            var targetBuffer = new byte[sourceBuffer.Length];

            // copy all bytes
            sourceBuffer.CopyTo(targetBuffer, 0);

            using (var sourceStream = new MemoryStream(sourceBuffer))
            using (var sourceHashChecker = new StreamMultipartHashChecker(sourceStream))
            using (var targetStream = new MemoryStream(targetBuffer))
            using (var targetHashChecker = new StreamMultipartHashChecker(targetStream))
            {
                // act
                var inconsistentIndex = await sourceHashChecker.GetInconsistentIndex(targetHashChecker);


                // assert
                Assert.Equal(-1, inconsistentIndex);
            }
        }
    }
}
