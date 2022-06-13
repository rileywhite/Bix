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

using Bix.Core;
using Bix.WebApi.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Bix.Repositories.Restful.WebApi
{
    public abstract class UnauthenticatedItemControllerBase<TIdentity, TItem, TRepository> : UnauthenticatedBixControllerBase
        where TItem : class, IModel<TIdentity>, IAggregateRoot
        where TRepository : IRepository<TIdentity, TItem>
    {
        protected ILogger Logger { get; }
        protected TRepository Repository { get; }

        public UnauthenticatedItemControllerBase(ILogger logger, TRepository repository)
        {
            Contract.Requires(logger != null);
            Contract.Requires(repository != null);
            Contract.Ensures(this.Logger != null);
            Contract.Ensures(this.Repository != null);

            this.Logger = logger;
            this.Repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TItem item, CancellationToken cancellationToken)
        {
            this.Logger?.LogDebug("Processing {Verb} request for {Item} at {Uri}", this.Request.Method, item?.ToJson() ?? "{}", this.Request.Path);
            if (item == null)
            {
                return this.BadRequest();
            }
            var createdItem = await this.Repository.AddAsync(item, cancellationToken).ConfigureAwait(false);
            return this.CreatedAtRoute(new { id = createdItem.Identity }, createdItem);
        }

        [HttpGet("{identity}")]
        public async Task<IActionResult> GetById(TIdentity identity, CancellationToken cancellationToken)
        {
            this.Logger?.LogDebug("Processing {Verb} request for {Identity} at {Uri}", this.Request.Method, identity?.ToJson() ?? "{}", this.Request.Path);
            var item = await this.Repository.FindAsync(identity, cancellationToken).ConfigureAwait(false);
            if (item == null)
            {
                return this.NotFound();
            }
            return this.Ok(item);
        }

        [HttpPut("{identity}")]
        public async Task<IActionResult> Update(TIdentity identity, [FromBody] TItem item, CancellationToken cancellationToken)
        {
            this.Logger?.LogDebug("Processing {Verb} request for {Identity} at {Uri}", this.Request.Method, identity?.ToJson() ?? "{}", this.Request.Path);
            if (identity == null || item == null || !identity.Equals(item.Identity))
            {
                return this.BadRequest();
            }
            return this.Ok(await this.Repository.UpdateAsync(item, cancellationToken).ConfigureAwait(false));
        }

        [HttpDelete("{identity}")]
        public async Task<IActionResult> Delete(TIdentity identity, CancellationToken cancellationToken)
        {
            this.Logger?.LogDebug("Processing {Verb} request for {Identity} at {Uri}", this.Request.Method, identity?.ToJson() ?? "{}", this.Request.Path);
            var foundItem = await this.Repository.FindAsync(identity, cancellationToken).ConfigureAwait(false);
            if (foundItem == null)
            {
                return this.NotFound();
            }
            try
            {
                await this.Repository.RemoveAsync(identity, cancellationToken).ConfigureAwait(false);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex);
            }
        }

        [HttpGet("metadata/{identity}")]
        public async Task<IActionResult> GetMetadata(TIdentity identity, CancellationToken cancellationToken)
        {
            this.Logger?.LogDebug("Processing {Verb} request for {Identity} at {Uri}", this.Request.Method, identity?.ToJson() ?? "{}", this.Request.Path);
            if (identity == null) { return this.BadRequest(); }

            var metadata = await this.Repository.GetMetadataAsync(identity, cancellationToken).ConfigureAwait(false);
            if (metadata == null) { return this.NotFound(); }

            return this.Ok(metadata);
        }
    }
}
