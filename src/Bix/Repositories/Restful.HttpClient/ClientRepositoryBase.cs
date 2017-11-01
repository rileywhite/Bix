/***************************************************************************/
// Copyright 2013-2017 Riley White
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
using Bix.Http;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using snh = System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bix.Repositories.Restful.HttpClient
{
    public abstract class ClientRepositoryBase<TIdentity, TItem> : IRepository<TIdentity, TItem>
        where TItem : class, IAggregateRoot, IHasIdentity<TIdentity>
    {
        protected IClientConfiguration Config { get; }
        protected IAuthenticationHeaderGenerator AuthenticationHeaderGenerator { get; }
        protected ILogger Logger { get; }

        protected abstract string ItemsControllerPath { get; }
        protected abstract string ItemControllerPath { get; }

        public ClientRepositoryBase(
            IClientConfiguration config,
            IAuthenticationHeaderGenerator authenticationHeaderGenerator,
            ILogger logger)
        {
            this.Config = config;
            this.AuthenticationHeaderGenerator = authenticationHeaderGenerator;
            this.Logger = logger;
        }

        public async Task<IQueryable<TItem>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                using (var client = new snh.HttpClient())
                using (var streamReader = new StreamReader(
                    await client.GetStreamWithAuthenticationAsync(
                        $"{this.Config.BaseControllerPath}{this.ItemsControllerPath}",
                        this.AuthenticationHeaderGenerator,
                        cancellationToken)))
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return new JsonSerializer().Deserialize<List<TItem>>(jsonTextReader)?.AsQueryable();
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "Failure to GetAllAsync");
                throw;
            }
        }

        public async Task<TItem> FindAsync(TIdentity identity, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                using (var client = new snh.HttpClient())
                using (var streamReader = new StreamReader(
                    await client.GetStreamWithAuthenticationAsync(
                        $"{this.Config.BaseControllerPath}{this.ItemControllerPath}/{identity.ToUrlSegment()}",
                        this.AuthenticationHeaderGenerator,
                        cancellationToken)))
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return new JsonSerializer().Deserialize<TItem>(jsonTextReader);
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "Failure to FindAsync. Identity {Identity}", identity);
                throw;
            }
        }

        public async Task<TItem> AddAsync(TItem item, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                using (var client = new snh.HttpClient())
                {
                    var response = await client.PostWithAuthenticationAsync(
                        $"{this.Config.BaseControllerPath}{this.ItemControllerPath}",
                        item.ToJson(),
                        this.AuthenticationHeaderGenerator,
                        cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new RestfulRepositoryHttpClientException("Unsuccessful service call response") { ErrorResponse = response };
                    }
                    using (var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return new JsonSerializer().Deserialize<TItem>(jsonTextReader);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "Failure to AddAsync. Item {Item}", item.ToJson());
                throw;
            }
        }

        public async Task RemoveAsync(TIdentity identity, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                using (var client = new snh.HttpClient())
                {
                    var response = await client.DeleteWithAuthenticationAsync(
                        $"{this.Config.BaseControllerPath}{this.ItemControllerPath}/{identity.ToUrlSegment()}",
                        this.AuthenticationHeaderGenerator,
                        cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new RestfulRepositoryHttpClientException("Unsuccessful service call response") { ErrorResponse = response };
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "Failure to RemoveAsync. Identity {Identity}", identity);
                throw;
            }
        }

        public async Task<TItem> UpdateAsync(TItem updated, CancellationToken cancellationToken = default(CancellationToken))
        {
            var identity = updated.Identity;

            try
            {
                using (var client = new snh.HttpClient())
                {
                    var response = await client.PutWithAuthenticationAsync(
                        $"{this.Config.BaseControllerPath}{this.ItemControllerPath}/{identity.ToUrlSegment()}",
                        updated.ToJson(),
                        this.AuthenticationHeaderGenerator,
                        cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new RestfulRepositoryHttpClientException("Unsuccessful service call response") { ErrorResponse = response };
                    }
                    using (var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return new JsonSerializer().Deserialize<TItem>(jsonTextReader);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "Failure to UpdateAsync. Item {Identity}", updated.ToJson());
                throw;
            }
        }

        public async Task<ModelMetadata> GetMetadataAsync(TIdentity identity, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                using (var client = new snh.HttpClient())
                using (var streamReader = new StreamReader(
                    await client.GetStreamWithAuthenticationAsync(
                        $"{this.Config.BaseControllerPath}{this.ItemControllerPath}/metadata/{identity.ToUrlSegment()}",
                        this.AuthenticationHeaderGenerator,
                        cancellationToken)))
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return new JsonSerializer().Deserialize<ModelMetadata>(jsonTextReader);
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "Failure to GetMetadata. Identity {Identity}", identity);
                throw;
            }
        }
    }
}
