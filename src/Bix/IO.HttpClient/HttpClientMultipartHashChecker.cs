///***************************************************************************/
//// Copyright 2013-2018 Riley White
//// 
//// Licensed under the Apache License, Version 2.0 (the "License");
//// you may not use this file except in compliance with the License.
//// You may obtain a copy of the License at
//// 
////     http://www.apache.org/licenses/LICENSE-2.0
//// 
//// Unless required by applicable law or agreed to in writing, software
//// distributed under the License is distributed on an "AS IS" BASIS,
//// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//// See the License for the specific language governing permissions and
//// limitations under the License.
///***************************************************************************/

//using System;
//using System.Diagnostics.Contracts;
//using System.IO;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Bix.IO.HttpClient
//{
//    public class HttpClientMultipartHashChecker : ISourceMultipartHashChecker
//    {
//        public HttpClientMultipartHashChecker(
//            HttpClientDataSinkBase dataSinkClient,
//            Stream localStream)
//        {
//            Contract.Requires(dataSinkClient != null);
//            Contract.Requires(localStreamDescriptor != null);
//            Contract.Requires(localSubstreamDetails != null);
//            Contract.Ensures(this.DataSinkClient != null);
//            Contract.Ensures(this.LocalStreamDescriptor != null);
//            Contract.Requires(this.LocalSubstreamDetails != null);

//            this.DataSinkClient = dataSinkClient;
//            this.LocalStreamDescriptor = localStreamDescriptor;
//            this.LocalSubstreamDetails = localSubstreamDetails;
//        }

//        public HttpClientDataSinkBase DataSinkClient { get; }
//        public StreamDescriptor LocalStreamDescriptor { get; }
//        public SubstreamDetails LocalSubstreamDetails { get; }

//        public bool CanGetLength => false;

//        public long GetLength() => throw new NotSupportedException();

//        public async Task<SubstreamDetails> GetSubstreamDetailsAsync(
//            long startAt,
//            long byteCount,
//            byte partCount,
//            string hashName = "MD5",
//            CancellationToken cancellationToken = default(CancellationToken))
//        {
//            throw new NotImplementedException("TODO need to get a new substream details for the requested substream. Probably need local stream passed into constructor.");
//            var streamStatus = new StreamStatus
//            {
//                Descriptor = this.LocalStreamDescriptor,
//                SourceSubstreamDetails = this.LocalSubstreamDetails,
//            };

//            var filledStatus = await this.DataSinkClient.BumpAsync(streamStatus, cancellationToken);

//            return filledStatus.TargetSubstreamDetails;
//        }
//    }
//}
