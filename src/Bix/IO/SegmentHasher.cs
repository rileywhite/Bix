/***************************************************************************/
// Copyright 2013-2022 Riley White
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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Security.Cryptography;

namespace Bix.IO
{
    /// <summary>
    /// Encapsulates efficient logic for hashing segments and subsegments of a data file.
    /// </summary>
    public class SegmentHasher
    {
        /// <summary>
        /// Hashes a requested number of segments within a file.
        /// </summary>
        /// <param name="filePath">Path of the file to hash segments of</param>
        /// <param name="segmentCount">Number of segments to hash</param>
        /// <param name="hashAlgorithmName">Name of the hash algorithm to use</param>
        /// <returns>Colletion of <paramref name="segmentCount"/> hash segments</returns>
        public SegmentHash[] GetSegmentHashes(
            string filePath,
            byte segmentCount,
            HashAlgorithmName hashAlgorithmName)
        {
            Contract.Requires(filePath != null);
            Contract.Requires(File.Exists(filePath));
            Contract.Requires(segmentCount >= 1);
            Contract.Ensures(Contract.Result<SegmentHash[]>() != null);
            Contract.Ensures(Contract.Result<SegmentHash[]>().Length == segmentCount);
            Contract.Ensures(Contract.Result<SegmentHash[]>().All(h => !string.IsNullOrEmpty(h.Hash)));

            return this.GetSegmentHashes(new FileInfo(filePath), segmentCount, hashAlgorithmName);
        }

        /// <summary>
        /// Hashes a requested number of segments within a file.
        /// </summary>
        /// <param name="file">File to hash segments of</param>
        /// <param name="segmentCount">Number of segments to hash</param>
        /// <param name="hashAlgorithmName">Name of the hash algorithm to use</param>
        /// <returns>Colletion of <paramref name="segmentCount"/> hash segments</returns>
        public SegmentHash[] GetSegmentHashes(
            FileInfo file,
            byte segmentCount,
            HashAlgorithmName hashAlgorithmName)
        {
            Contract.Requires(file != null);
            Contract.Requires(file.Exists);
            Contract.Requires(segmentCount >= 1);
            Contract.Ensures(Contract.Result<SegmentHash[]>() != null);
            Contract.Ensures(Contract.Result<SegmentHash[]>().Length == segmentCount);
            Contract.Ensures(Contract.Result<SegmentHash[]>().All(h => !string.IsNullOrEmpty(h.Hash)));

            return this.GetSubsegmentHashes(file, 0, file.Length, segmentCount, hashAlgorithmName);
        }

        /// <summary>
        /// Hashes a requested number of subsegments within a file segment.
        /// </summary>
        /// <param name="file">File to hash subsegments of</param>
        /// <param name="segmentStart">Position in file of the segment for which you want to hash subsegments</param>
        /// <param name="segmentLength">Length in file of the segment for which you want to hash subsegments</param>
        /// <param name="subsegmentCount">Number of subsegments to hash</param>
        /// <param name="hashAlgorithmName">Name of the hash algorithm to use</param>
        /// <returns>Colletion of <paramref name="subsegmentCount"/> hash segments</returns>
        public SegmentHash[] GetSubsegmentHashes(
            FileInfo file,
            long segmentStart,
            long segmentLength,
            byte subsegmentCount,
            HashAlgorithmName hashAlgorithmName)
        {
            Contract.Requires(file != null);
            Contract.Requires(file.Exists);
            Contract.Requires(subsegmentCount >= 1);
            Contract.Ensures(Contract.Result<SegmentHash[]>() != null);
            Contract.Ensures(Contract.Result<SegmentHash[]>().Length == subsegmentCount);
            Contract.Ensures(Contract.Result<SegmentHash[]>().All(h => !string.IsNullOrEmpty(h.Hash)));

            using (var memoryMappedFile = MemoryMappedFile.CreateFromFile(file.FullName))
            {
                return this.GetSubsegmentHashes(memoryMappedFile, segmentStart, segmentLength, subsegmentCount, hashAlgorithmName);
            }
        }

        /// <summary>
        /// Hashes a requested number of subsegments within a file segment.
        /// </summary>
        /// <param name="memoryMappedFile">File, mapped to memory for very efficient access, to hash subsegments of</param>
        /// <param name="segmentStart">Position in file of the segment for which you want to hash subsegments</param>
        /// <param name="segmentLength">Length in file of the segment for which you want to hash subsegments</param>
        /// <param name="subsegmentCount">Number of subsegments to hash</param>
        /// <param name="hashAlgorithmName">Name of the hash algorithm to use</param>
        /// <returns>Colletion of <paramref name="subsegmentCount"/> hash segments</returns>
        public SegmentHash[] GetSubsegmentHashes(
            MemoryMappedFile memoryMappedFile,
            long segmentStart,
            long segmentLength,
            byte subsegmentCount,
            HashAlgorithmName hashAlgorithmName)
        {
            Contract.Requires(memoryMappedFile != null);
            Contract.Requires(segmentStart >= 0);
            Contract.Requires(segmentLength >= 0);
            Contract.Requires(subsegmentCount >= 1);
            Contract.Ensures(Contract.Result<SegmentHash[]>() != null);
            Contract.Ensures(Contract.Result<SegmentHash[]>().Length == subsegmentCount);
            Contract.Ensures(Contract.Result<SegmentHash[]>().All(h => h != null && !string.IsNullOrEmpty(h.Hash)));

            GetSegmentationParameters(segmentLength, subsegmentCount, out var standardSubsegmentLength, out var subsegmentLengths);
            using (var hasher = CryptoConfig.CreateFromName(hashAlgorithmName.Name) as HashAlgorithm)
            {
                var subsegmentHashes = new SegmentHash[subsegmentCount];
                for (var i = 0; i < subsegmentLengths.Count(); ++i)
                {
                    var start = segmentStart + i * standardSubsegmentLength;
                    var length = subsegmentLengths[i];

                    subsegmentHashes[i] = GetSegmentHash(memoryMappedFile, hasher, start, length);
                }

                return subsegmentHashes;
            }
        }

        /// <summary>
        /// Finds the first segment in a file where it differs from a given reference set of segment hashes, and then
        /// drills down into that segment, pulling subsegment hashes for further comparison.
        /// </summary>
        /// <param name="referenceSegmentHashes">Segment hashes to compare the given file segments to</param>
        /// <param name="filePath">Path of the file to check against the <paramref name="referenceSegmentHashes"/></param>
        /// <param name="segmentStart">Position in file of the segment that is described by <paramref name="referenceSegmentHashes"/></param>
        /// <param name="segmentLength">Length in file of the segment that is described by <paramref name="referenceSegmentHashes"/></param>
        /// <param name="subsegmentCount">Number of subsegment hashes to break the first differing segment into</param>
        /// <param name="hashAlgorithmName">Hash algorithm name used for comparing data</param>
        /// <param name="diffSubsegmentHashes">Will be populated with the collection of <paramref name="subsegmentCount"/> hashes from the first differing segment or <c>null</c> if no difference is found.</param>
        /// <returns><c>true</c> if a difference is found, else <c>false</c></returns>
        public bool TryGetSubsegmentHashesOfFirstDiff(
            SegmentHash[] referenceSegmentHashes,
            string filePath,
            long segmentStart,
            long segmentLength,
            byte subsegmentCount,
            HashAlgorithmName hashAlgorithmName,
            out SegmentHash[] diffSubsegmentHashes)
        {
            Contract.Requires(referenceSegmentHashes != null && referenceSegmentHashes.Length > 0);
            Contract.Ensures(referenceSegmentHashes.All(h => h != null && !string.IsNullOrEmpty(h.Hash)));
            Contract.Requires(filePath != null);
            Contract.Requires(File.Exists(filePath));
            Contract.Requires(segmentStart >= 0);
            Contract.Requires(segmentLength >= 0);
            Contract.Requires(subsegmentCount >= 1);
            Contract.Ensures(Contract.ValueAtReturn(out diffSubsegmentHashes) != null);
            Contract.Ensures(Contract.ValueAtReturn(out diffSubsegmentHashes).Length == subsegmentCount);
            Contract.Ensures(Contract.ValueAtReturn(out diffSubsegmentHashes).All(h => h != null && !string.IsNullOrEmpty(h.Hash)));

            return this.TryGetSubsegmentHashesOfFirstDiff(
                referenceSegmentHashes,
                new FileInfo(filePath),
                segmentStart,
                segmentLength,
                subsegmentCount,
                hashAlgorithmName,
                out diffSubsegmentHashes);
        }

        /// <summary>
        /// Finds the first segment in a file where it differs from a given reference set of segment hashes, and then
        /// drills down into that segment, pulling subsegment hashes for further comparison.
        /// </summary>
        /// <param name="referenceSegmentHashes">Segment hashes to compare the given file segments to</param>
        /// <param name="file">File to check against the <paramref name="referenceSegmentHashes"/></param>
        /// <param name="segmentStart">Position in file of the segment that is described by <paramref name="referenceSegmentHashes"/></param>
        /// <param name="segmentLength">Length in file of the segment that is described by <paramref name="referenceSegmentHashes"/></param>
        /// <param name="subsegmentCount">Number of subsegment hashes to break the first differing segment into</param>
        /// <param name="hashAlgorithmName">Hash algorithm name used for comparing data</param>
        /// <param name="diffSubsegmentHashes">Will be populated with the collection of <paramref name="subsegmentCount"/> hashes from the first differing segment or <c>null</c> if no difference is found.</param>
        /// <returns><c>true</c> if a difference is found, else <c>false</c></returns>
        public bool TryGetSubsegmentHashesOfFirstDiff(
            SegmentHash[] referenceSegmentHashes,
            FileInfo file,
            long segmentStart,
            long segmentLength,
            byte subsegmentCount,
            HashAlgorithmName hashAlgorithmName,
            out SegmentHash[] diffSubsegmentHashes)
        {
            Contract.Requires(referenceSegmentHashes != null && referenceSegmentHashes.Length > 0);
            Contract.Ensures(referenceSegmentHashes.All(h => h != null && !string.IsNullOrEmpty(h.Hash)));
            Contract.Requires(file != null);
            Contract.Requires(file.Exists);
            Contract.Requires(segmentStart >= 0);
            Contract.Requires(segmentLength >= 0);
            Contract.Requires(subsegmentCount >= 1);
            Contract.Ensures(Contract.ValueAtReturn(out diffSubsegmentHashes) != null);
            Contract.Ensures(Contract.ValueAtReturn(out diffSubsegmentHashes).Length == subsegmentCount);
            Contract.Ensures(Contract.ValueAtReturn(out diffSubsegmentHashes).All(h => h != null && !string.IsNullOrEmpty(h.Hash)));

            GetSegmentationParameters(segmentLength, subsegmentCount, out var standardSubsegmentLength, out var subsegmentLengths);
            using (var hasher = CryptoConfig.CreateFromName(hashAlgorithmName.Name) as HashAlgorithm)
            using (var memoryMappedFile = MemoryMappedFile.CreateFromFile(file.FullName))
            {
                for (var i = 0; i < subsegmentLengths.Count(); ++i)
                {
                    var start = segmentStart + i * standardSubsegmentLength;
                    var length = subsegmentLengths[i];

                    SegmentHash subsegmentHash = GetSegmentHash(memoryMappedFile, hasher, start, length);

                    if (subsegmentHash.Hash != referenceSegmentHashes[i].Hash)
                    {
                        diffSubsegmentHashes = this.GetSubsegmentHashes(
                            memoryMappedFile,
                            subsegmentHash.Start,
                            subsegmentHash.Length,
                            subsegmentCount,
                            hashAlgorithmName);
                        return true;
                    }
                }
            }

            diffSubsegmentHashes = null;
            return false;
        }

        private static SegmentHash GetSegmentHash(MemoryMappedFile memoryMappedFile, HashAlgorithm hasher, long start, long length)
        {
            Contract.Requires(memoryMappedFile != null);
            Contract.Requires(hasher != null);
            Contract.Requires(start >= 0);
            Contract.Requires(length >= 0);

            var segmentHash = new SegmentHash { Start = start, Length = length };
            using (var segmentStream = memoryMappedFile.CreateViewStream(start, length, MemoryMappedFileAccess.Read))
            {
                segmentHash.Hash = Convert.ToBase64String(hasher.ComputeHash(segmentStream));
            }

            return segmentHash;
        }

        private static void GetSegmentationParameters(long fileLength, byte segmentCount, out long standardSegmentLength, out List<long> segmentLengths)
        {
            Contract.Requires(fileLength >= 0);
            Contract.Requires(segmentCount >= 1);
            Contract.Ensures(Contract.ValueAtReturn(out standardSegmentLength) >= 0);
            Contract.Ensures(Contract.ValueAtReturn(out segmentLengths) != null && Contract.ValueAtReturn(out segmentLengths).Count == segmentCount);

            {
                // creating "len" because out parameters cannot be used in a lambda
                var len = standardSegmentLength = fileLength / segmentCount;
                segmentLengths = (from s in Enumerable.Range(0, segmentCount - 1) select len).ToList();
            }
            segmentLengths.Add(standardSegmentLength + (int)(fileLength % standardSegmentLength));
        }
    }
}
