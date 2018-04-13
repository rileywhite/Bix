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

namespace Bix.IO
{
    public class TempFileTargetStreamFactory : ITargetStreamFactory
    {
        public Stream CreateStream(string partition, string id)
        {
            return new FileStream(
                ConvertStreamIdToTargetFileName(partition, id),
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.ReadWrite);
        }

        public void DestroyUnmanagedData(string partition, string id)
        {
            try { File.Delete(ConvertStreamIdToTargetFileName(partition, id)); } catch { /* ignore */ }
        }

        private static string ConvertStreamIdToTargetFileName(string partition, string id)
        {
            if (string.IsNullOrWhiteSpace(partition)) { partition = "DefaultPartition"; };

            // replace any invalid partition characters with underscores
            // (potential for collision here, but I'm accepting that risk considering expected source of the sent argument)
            partition = string.Join("_", partition.Split(Path.GetInvalidFileNameChars()));

            return Path.Combine(
                Path.GetTempPath(),
                "BixDataUpload",
                partition,
                id);
        }
    }
}
