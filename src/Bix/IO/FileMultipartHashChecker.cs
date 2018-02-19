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

using System;
using System.IO;
using System.Threading.Tasks;

namespace Bix.IO
{
    public class FileMultipartHashChecker : IMultipartHashChecker
    {
        public FileMultipartHashChecker(string filePath)
        {
            this.FilePath = filePath;
        }

        public string FilePath { get; }

        public async Task<byte[][]> GetHashes(long startAt, long byteCount, byte partCount, string hashName = "MD5")
        {
            using (var fileStream = File.OpenRead(this.FilePath))
            {
                return await fileStream.GetNestedHashes(partCount, startAt, byteCount);
            }
        }

        public long GetLength()
        {
            return new FileInfo(this.FilePath).Length;
        }
    }
}
