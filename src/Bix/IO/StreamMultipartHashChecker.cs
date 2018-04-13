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
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bix.IO
{
    public class StreamMultipartHashChecker : IMultipartHashChecker, IDisposable
    {
        public static StreamMultipartHashChecker CreateFrom(string filePath)
        {
            return new StreamMultipartHashChecker(File.OpenRead(filePath));
        }

        public StreamMultipartHashChecker(Stream stream, bool leaveStreamOpenOnDispose = false)
        {
            Contract.Requires(stream != null);
            Contract.Requires(stream.CanRead);
            Contract.Requires(stream.CanSeek || stream.Length > 0);
            Contract.Ensures(this.IsDisposed ||  this.Stream != null);
            Contract.Ensures(this.IsDisposed || this.Stream.CanRead);
            Contract.Ensures(this.IsDisposed || this.Stream.CanSeek || stream.Length > 0);

            this.Stream = stream;
            this.LeaveStreamOpenOnDispose = leaveStreamOpenOnDispose;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private bool IsDisposed { get; set; } = false;

        protected virtual void Dispose(bool isDisposing)
        {
            if (!this.IsDisposed)
            {
                if (isDisposing)
                {
                    if (!this.LeaveStreamOpenOnDispose)
                    {
                        this.Stream?.Dispose();
                    }
                }

                this.IsDisposed = true;
            }
        }

        private Stream Stream { get; }

        public bool LeaveStreamOpenOnDispose { get; }

        public async Task<SubstreamDetails> GetSubstreamDetails(long startAt, long byteCount, byte partCount, string hashName = "MD5")
        {
            var hashes = await this.Stream.GetNestedHashes(partCount, startAt, byteCount);
            return new SubstreamDetails
            {
                Start = startAt,
                Length = byteCount,
                SegmentLength = partCount,
                HashName = hashName,
                Hash = Convert.ToBase64String(hashes[0]),
                SegmentHashes = hashes.Skip(1).Select(hash => Convert.ToBase64String(hash)).ToArray(),
            };
        }

        public bool CanGetLength => this.Stream.CanSeek;
        public long GetLength() => this.Stream.Length;
    }
}
