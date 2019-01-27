/***************************************************************************/
// Copyright 2013-2019 Riley White
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

namespace Bix.Core
{
    /// <summary>
    /// A simple cache interface
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Sets a value cached value
        /// </summary>
        /// <typeparam name="T">Type of the value to cache</typeparam>
        /// <param name="key">Cache key for later retrieval</param>
        /// <param name="value">Value to save in the cache</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c></exception>
        void Set<T>(string key, T value);

        /// <summary>
        /// Gets a value from the cache
        /// </summary>
        /// <typeparam name="T">Type of the cached value</typeparam>
        /// <param name="key">Key of the value to retrieve</param>
        /// <returns>Retreived value</returns>
        /// <remarks>
        /// Any exception caused by a failure to cast the existing value will be passed up the stack.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c></exception>
        T Get<T>(string key);

        /// <summary>
        /// Tries to get a value from the cache
        /// </summary>
        /// <typeparam name="T">Type of the cached value</typeparam>
        /// <param name="key">Key of the value to retrieve</param>
        /// <param name="value">Will be populated with retrieved value</param>
        /// <returns><c>true</c> if the retrieval was successful, else <c>false</c></returns>
        /// <remarks>A failure to cast will cause a return value of <c>false</c></remarks>
        bool TryGet<T>(string key, out T value);

        /// <summary>
        /// Gets a value from the cache
        /// </summary>
        /// <param name="key">Key of the value to retrieve</param>
        /// <returns>Retreived value</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c></exception>
        object Get(string key);

        /// <summary>
        /// Tries to get a value from the cache
        /// </summary>
        /// <param name="key">Key of the value to retrieve</param>
        /// <param name="value">Will be populated with retrieved value</param>
        /// <returns><c>true</c> if the retrieval was successful, else <c>false</c></returns>
        bool TryGet(string key, out object value);
    }
}
