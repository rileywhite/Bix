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
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Bix.IO
{
    public static class Extensions
    {
        public static EventingStream ToEventingStream(this Stream stream)
        {
            switch (stream)
            {
                case EventingStream es: return es;
                default: return new EventingStream(stream);
            }
        }

        public static async Task<byte[][]> GetNestedHashes(this Stream stream, byte partCount = 5, long start = 0, long? byteCount = null)
            => await stream.GetNestedHashes(HashAlgorithmName.MD5, partCount, start, byteCount);

        public static async Task<byte[][]> GetNestedHashes(this Stream stream, HashAlgorithmName hashAlgorithmName, byte partCount = 5, long start = 0, long? byteCount = null)
        {
            Contract.Requires(stream != null);
            Contract.Requires(hashAlgorithmName != null);
            Contract.Requires(partCount > 1);
            Contract.Requires(start >= 0);

            var actualByteCount = byteCount.HasValue && byteCount.Value > 0 ? byteCount.Value : stream.Length;

            var eventingStream = stream.ToEventingStream();
            eventingStream.Position = start;

            var streams = new IEventingStream[partCount + 1];
            var hashTasks = new Task<byte[]>[partCount + 1];

            try
            {
                streams[0] = new ForwardReadingSubstream(eventingStream, start, actualByteCount);

                var partSize = actualByteCount / partCount;
                for (int i = 1; i < partCount; i++)
                {
                    streams[i] = new ForwardReadingSubstream(streams[0], (i - 1) * partSize, partSize);
                }
                streams[partCount] = new ForwardReadingSubstream(streams[0], (partCount - 1) * partSize);

                for (int i = 0; i <= partCount; i++)
                {
                    var current = i;
                    hashTasks[current] = Task.Run(() =>
                    {
                        using (var hasher = CryptoConfig.CreateFromName(hashAlgorithmName.Name) as HashAlgorithm)
                        {
                            return hasher.ComputeHash(streams[current].AsStream);
                        }
                    });
                }

                int readCount;
                var buffer = new byte[1024 * 8];
                long totalCount = 0;
                do
                {
                    readCount = await eventingStream.ReadAsync(buffer, 0, buffer.Length);
                    totalCount += readCount;
                } while (readCount > 0 && totalCount < start + actualByteCount);
                eventingStream.SignalEnd();

                return await Task.WhenAll(hashTasks);
            }
            finally
            {
                foreach (var s in streams) { s?.AsStream?.Dispose(); }
            }
        }

        public static async Task<long> GetInconsistentIndex(this IMultipartHashChecker source, IMultipartHashChecker target)
        {
            Contract.Requires(source != null);
            Contract.Requires(target != null);
            Contract.Requires(source.CanGetLength);

            var sourceLength = source.GetLength();

            if (target.CanGetLength && sourceLength == target.GetLength())
            {
                // source and target are intended to be the same length, so indicate inconsistency at the beginning
                return 0;
            }

            var start = 0L;
            var currentLength = sourceLength;

            while (true)
            {
                var segments = (byte)Math.Min(5, currentLength);

                if (segments == 1) { return start; }

                var sourceSubstreamDetailsTask = source.GetSubstreamDetails(start, currentLength, segments);
                var targetSubstreamDetailsTask = target.GetSubstreamDetails(start, currentLength, segments);

                var sourceSubstreamDetails = await sourceSubstreamDetailsTask;
                var targetSubstreamDetails = await targetSubstreamDetailsTask;

                if (sourceSubstreamDetails.Hash == targetSubstreamDetails.Hash) { return start - 1; }

                for (int i = 0; i < segments; i++)
                {
                    if (sourceSubstreamDetails.SegmentHashes[i] != targetSubstreamDetails.SegmentHashes[i])
                    {
                        var currentSegmentSize = currentLength / segments;
                        start += i * currentSegmentSize;
                        currentLength = i < segments - 1 ? currentSegmentSize : currentSegmentSize + (currentLength % segments);
                        break;
                    }
                }
            }
        }
    }
}
