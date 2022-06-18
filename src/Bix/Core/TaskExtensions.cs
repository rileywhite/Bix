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
using System.Threading.Tasks;

namespace Bix.Core
{
    /// <summary>
    /// Expanded functionality for <see cref="Task"/> objects
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Warps a task in a retry loop that runs with delays per to the rules of an exponential backoff algorithm.
        /// </summary>
        /// <param name="source">Task to wrap</param>
        /// <param name="beforeRetryCallback">Runs each before each retry after first failure. Will be passed the failure exception and the number of ms before the next retry. Delay occurs after callback completes.</param>
        /// <param name="ceilingPowerOf2Delay"></param>
        /// <param name="maxTries">Max number of retries. 0 for infinite.</param>
        /// <param name="continueOnCapturedContext"><c>true</c> if async calls should keep the original calling context</param>
        /// <returns>A task that wraps the given task in an expenential backoff retry loop</returns>
        /// <remarks>A <see cref="TaskCanceledException"/> will halt the retries. All other exceptions will result in continuing with the requested retries.</remarks>
        public static async Task WrapWithExponentialBackoff(
            this Func<Task> source,
            Action<Exception, int> beforeRetryCallback = null,
            uint maxTries = 16,
            uint ceilingPowerOf2Delay = 10,
            bool continueOnCapturedContext = false)
        {
            uint tryCount = 0;

            while (true)
            {
                var exponentialBackoffTime = 0;
                var exponentialBackoffCounter = 0;
                var rand = new Random();
                try
                {
                    ++tryCount;
                    if (exponentialBackoffTime > 0) { await Task.Delay(exponentialBackoffTime).ConfigureAwait(continueOnCapturedContext); }
                    await source().ConfigureAwait(continueOnCapturedContext);
                    return;
                }
                catch (Exception ex)
                {
                    if (maxTries > 0 && tryCount >= maxTries) { throw; }

                    // starts with fast retries
                    // will work up to waiting random time up to 8 seconds between attempts
                    if (exponentialBackoffCounter < ceilingPowerOf2Delay) { ++exponentialBackoffCounter; }
                    exponentialBackoffTime = rand.Next((int)Math.Pow(2, exponentialBackoffCounter));
                    beforeRetryCallback?.Invoke(ex, exponentialBackoffTime);
                }
            }
        }

        /// <summary>
        /// Warps a task in a retry loop that runs with delays per to the rules of an exponential backoff algorithm.
        /// </summary>
        /// <param name="source">Task to wrap</param>
        /// <param name="beforeRetryCallback">Runs each before each retry after first failure. Will be passed the failure exception and the number of ms before the next retry. Delay occurs after callback completes.</param>
        /// <param name="ceilingPowerOf2Delay"></param>
        /// <param name="maxTries">Max number of retries. 0 for infinite.</param>
        /// <param name="continueOnCapturedContext"><c>true</c> if async calls should keep the original calling context</param>
        /// <returns>A task that wraps the given task in an expenential backoff retry loop</returns>
        /// <remarks>A <see cref="TaskCanceledException"/> will halt the retries. All other exceptions will result in continuing with the requested retries.</remarks>
        /// <returns>A task that wraps the given task in an expenential backoff retry loop</returns>
        public static async Task<T> WrapWithExponentialBackoff<T>(
            this Func<Task<T>> source,
            Action<Exception, int> beforeRetryCallback = null,
            uint maxTries = 16,
            uint ceilingPowerOf2Delay = 10,
            bool continueOnCapturedContext = false)
        {
            uint tryCount = 0;

            var exponentialBackoffTime = 0;
            var exponentialBackoffCounter = 0;
            var rand = new Random();

            while (true)
            {
                try
                {
                    if (maxTries > 0) { ++tryCount; }
                    if (exponentialBackoffTime > 0) { await Task.Delay(exponentialBackoffTime).ConfigureAwait(continueOnCapturedContext); }
                    return await source().ConfigureAwait(continueOnCapturedContext);
                }
                catch (Exception ex)
                {
                    if (maxTries > 0 && tryCount >= maxTries) { throw; }

                    // starts with fast retries
                    // will work up to waiting random time up to 8 seconds between attempts
                    if (exponentialBackoffCounter < ceilingPowerOf2Delay) { ++exponentialBackoffCounter; }
                    exponentialBackoffTime = rand.Next((int)Math.Pow(2, exponentialBackoffCounter));
                    beforeRetryCallback?.Invoke(ex, exponentialBackoffTime);
                }
            }
        }
    }
}
