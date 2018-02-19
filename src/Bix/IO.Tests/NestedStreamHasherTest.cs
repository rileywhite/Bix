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
using System.Security.Cryptography;
using System.Threading.Tasks;
using Xunit;

namespace Bix.IO.Tests
{
    public class NestedStreamHasherTest
    {
        [Fact]
        public async Task NestedHashesAreComputedCorrectly()
        {
            // arrange
            var fixture = new Fixture();
            var parts = new byte[6][];
            parts[1] = fixture.CreateMany<byte>(2000).ToArray();
            parts[2] = fixture.CreateMany<byte>(2000).ToArray();
            parts[3] = fixture.CreateMany<byte>(2000).ToArray();
            parts[4] = fixture.CreateMany<byte>(2000).ToArray();
            parts[5] = fixture.CreateMany<byte>(2004).ToArray();

            parts[0] = new byte[parts[1].Length + parts[2].Length + parts[3].Length + parts[4].Length + parts[5].Length];
            parts[1].CopyTo(parts[0], 0);
            parts[2].CopyTo(parts[0], 2000);
            parts[3].CopyTo(parts[0], 4000);
            parts[4].CopyTo(parts[0], 6000);
            parts[5].CopyTo(parts[0], 8000);

            var expectedHashes = new byte[6][];
            using (var hasher = CryptoConfig.CreateFromName(HashAlgorithmName.MD5.Name) as HashAlgorithm)
            {
                for (int i = 0; i < expectedHashes.Length; i++)
                {
                    expectedHashes[i] = hasher.ComputeHash(parts[i]);
                }
            }


            // act
            byte[][] hashes;
            using (var stream = new MemoryStream(parts[0]))
            {
                hashes = await stream.GetNestedHashes(5, 0);
            }


            // assert
            for (int i = 0; i < expectedHashes.Length; i++)
            {
                Assert.Equal(expectedHashes[i], hashes[i]);
            }
        }
    }
}
