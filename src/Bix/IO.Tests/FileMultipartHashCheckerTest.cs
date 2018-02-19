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
    public class FileMultipartHashCheckerTest
    {
        [Fact]
        public async Task IndexInMiddleOfTargetFileIsFound()
        {
            // arrange
            var sourceFilePath = Path.GetTempFileName();
            var targetFilePath = Path.GetTempFileName();
            var fixture = new Fixture();
            var contents = fixture.CreateMany<byte>(1024 * 10).ToArray();

            using (var fileStream = File.OpenWrite(sourceFilePath))
            using (var writer = new BinaryWriter(fileStream))
            {
                writer.Write(contents);
            }

            using (var fileStream = File.OpenWrite(targetFilePath))
            {
                fileStream.Position = contents.Length - 1;
                fileStream.WriteByte(0);
                fileStream.Position = 0;
                using (var writer = new BinaryWriter(fileStream))
                {
                    writer.Write(contents, 0, 8497);
                }
            }

            var sourceHashChecker = new FileMultipartHashChecker(sourceFilePath);
            var targetHashChecker = new FileMultipartHashChecker(targetFilePath);

            try
            {
                // act
                var inconsistentIndex = await sourceHashChecker.GetInconsistentIndex(targetHashChecker);


                // assert
                Assert.Equal(8497, inconsistentIndex);
            }
            finally
            {
                if (File.Exists(sourceFilePath)) { File.Delete(sourceFilePath); }
                if (File.Exists(targetFilePath)) { File.Delete(targetFilePath); }
            }
        }

        [Fact]
        public async Task IndexAtBeginningOfTargetFileIsFound()
        {
            // arrange
            var sourceFilePath = Path.GetTempFileName();
            var targetFilePath = Path.GetTempFileName();
            var fixture = new Fixture();
            var contents = fixture.CreateMany<byte>(1024 * 10).ToArray();

            using (var fileStream = File.OpenWrite(sourceFilePath))
            using (var writer = new BinaryWriter(fileStream))
            {
                writer.Write(contents);
            }

            using (var fileStream = File.OpenWrite(targetFilePath))
            {
                fileStream.Position = contents.Length - 1;
                fileStream.WriteByte(0);
                fileStream.Position = 0;
                using (var writer = new BinaryWriter(fileStream))
                {
                    writer.Write(contents, 0, 1);
                }
            }

            var sourceHashChecker = new FileMultipartHashChecker(sourceFilePath);
            var targetHashChecker = new FileMultipartHashChecker(targetFilePath);

            try
            {
                // act
                var inconsistentIndex = await sourceHashChecker.GetInconsistentIndex(targetHashChecker);


                // assert
                Assert.Equal(1, inconsistentIndex);
            }
            finally
            {
                if (File.Exists(sourceFilePath)) { File.Delete(sourceFilePath); }
                if (File.Exists(targetFilePath)) { File.Delete(targetFilePath); }
            }
        }

        [Fact]
        public async Task IndexAtEndOfTargetFileIsFound()
        {
            // arrange
            var sourceFilePath = Path.GetTempFileName();
            var targetFilePath = Path.GetTempFileName();
            var fixture = new Fixture();
            var contents = fixture.CreateMany<byte>(1024 * 10).ToArray();

            using (var fileStream = File.OpenWrite(sourceFilePath))
            using (var writer = new BinaryWriter(fileStream))
            {
                writer.Write(contents);
            }

            ++contents[contents.Length - 1];
            using (var fileStream = File.OpenWrite(targetFilePath))
            {
                using (var writer = new BinaryWriter(fileStream))
                {
                    // write the full contents except the last byte
                    writer.Write(contents);
                }
            }

            var sourceHashChecker = new FileMultipartHashChecker(sourceFilePath);
            var targetHashChecker = new FileMultipartHashChecker(targetFilePath);

            try
            {
                // act
                var inconsistentIndex = await sourceHashChecker.GetInconsistentIndex(targetHashChecker);


                // assert
                Assert.Equal(contents.Length - 1, inconsistentIndex);
            }
            finally
            {
                if (File.Exists(sourceFilePath)) { File.Delete(sourceFilePath); }
                if (File.Exists(targetFilePath)) { File.Delete(targetFilePath); }
            }
        }

        [Fact]
        public async Task IdenticalTargetFileIsFound()
        {
            // arrange
            var sourceFilePath = Path.GetTempFileName();
            var targetFilePath = Path.GetTempFileName();
            var fixture = new Fixture();
            var contents = fixture.CreateMany<byte>(1024 * 10).ToArray();

            using (var fileStream = File.OpenWrite(sourceFilePath))
            using (var writer = new BinaryWriter(fileStream))
            {
                writer.Write(contents);
            }

            using (var fileStream = File.OpenWrite(targetFilePath))
            using (var writer = new BinaryWriter(fileStream))
            {
                writer.Write(contents);
            }

            var sourceHashChecker = new FileMultipartHashChecker(sourceFilePath);
            var targetHashChecker = new FileMultipartHashChecker(targetFilePath);

            try
            {
                // act
                var inconsistentIndex = await sourceHashChecker.GetInconsistentIndex(targetHashChecker);


                // assert
                Assert.Equal(-1, inconsistentIndex);
            }
            finally
            {
                if (File.Exists(sourceFilePath)) { File.Delete(sourceFilePath); }
                if (File.Exists(targetFilePath)) { File.Delete(targetFilePath); }
            }
        }
    }
}
