﻿/***************************************************************************/
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

using Bix.Core;
using Bix.Http.Client;
using Bix.Http.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using snh = System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bix.Repositories.Restful.HttpClient
{
    public abstract class ValueTypeHttpClientRepositoryBase<TIdentity, TNaturalKey, TItem>
        : HttpClientRepositoryBase<TIdentity, TItem>, IValueTypeRepository<TIdentity, TNaturalKey, TItem>
        where TItem : class, IAggregateRoot, IValueTypeModel<TIdentity, TNaturalKey>
    {
        public ValueTypeHttpClientRepositoryBase(
            IHttpClientConfiguration config,
            IAuthenticationHeaderGenerator authenticationHeaderGenerator,
            ILogger<ValueTypeHttpClientRepositoryBase<TIdentity, TNaturalKey, TItem>> logger = null)
            : base(config, authenticationHeaderGenerator, logger) { }

        public async Task<TItem> FindOrAddAsync(TItem item, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var client = new snh.HttpClient())
                {
                    var response = await client.PatchWithAuthenticationAsync(
                        $"{this.Config.BaseControllerPath}{this.ItemControllerPath}",
                        item.ToJson(),
                        this.AuthenticationHeaderGenerator,
                        this.Logger,
                        cancellationToken).ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new RestfulRepositoryHttpClientException("Unsuccessful service call response") { ErrorResponse = response };
                    }
                    using (var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync().ConfigureAwait(false)))
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return new JsonSerializer().Deserialize<TItem>(jsonTextReader);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, "Failure to FindOrAddAsync. Item {Item}", item.ToJson());
                throw;
            }
        }
    }

    public abstract class ValueTypeHttpClientRepositoryBase<TNaturalKey, TItem>
        : ValueTypeHttpClientRepositoryBase<TNaturalKey, TNaturalKey, TItem>, IValueTypeRepository<TNaturalKey, TItem>
        where TItem : class, IAggregateRoot, IValueTypeModel<TNaturalKey>
    {
        public ValueTypeHttpClientRepositoryBase(
            IHttpClientConfiguration config,
            IAuthenticationHeaderGenerator authenticationHeaderGenerator,
            ILogger<ValueTypeHttpClientRepositoryBase<TNaturalKey, TItem>> logger = null)
            : base(config, authenticationHeaderGenerator, logger) { }
    }
}
