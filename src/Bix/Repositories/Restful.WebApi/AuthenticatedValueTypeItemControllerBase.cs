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

using Bix.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Bix.Repositories.Restful.WebApi
{
    public abstract class AuthenticatedValueTypeItemControllerBase<TIdentity, TNaturalKey, TItem, TRepository>
        : AuthenticatedItemControllerBase<TIdentity, TItem, TRepository>
        where TItem : class, IValueTypeModel<TIdentity, TNaturalKey>, IAggregateRoot
        where TRepository : IValueTypeRepository<TIdentity, TNaturalKey, TItem>
    {
        public AuthenticatedValueTypeItemControllerBase(ILogger logger, TRepository repository)
            : base(logger, repository)
        {
            Contract.Requires(logger != null);
            Contract.Requires(repository != null);
        }

        [HttpPatch("{naturalKey}")]
        public async Task<IActionResult> CreateOrUpdate(TNaturalKey naturalKey, [FromBody] TItem item)
        {
            this.Logger?.LogDebug("Processing {Verb} request for {Item} with naturalKey {NaturalKey} at {Uri}", this.Request.Method, item?.ToJson() ?? "{}", naturalKey?.ToJson() ?? "{}", this.Request.Path);
            if (naturalKey == null || item == null || !naturalKey.Equals(item.NaturalKey))
            {
                return this.BadRequest();
            }
            return this.Ok(await this.Repository.FindOrAddAsync(item).ConfigureAwait(false));
        }

        [HttpPatch]
        public async Task<IActionResult> CreateOrUpdate([FromBody] TItem item)
        {
            this.Logger?.LogDebug("Processing {Verb} request for {Item} at {Uri}", this.Request.Method, item?.ToJson() ?? "{}", this.Request.Path);
            if (item == null || item.NaturalKey == null)
            {
                return this.BadRequest();
            }
            return this.Ok(await this.Repository.FindOrAddAsync(item).ConfigureAwait(false));
        }
    }

    public abstract class AuthenticatedValueTypeItemControllerBase<TNaturalKey, TItem, TRepository>
        : AuthenticatedValueTypeItemControllerBase<TNaturalKey, TNaturalKey, TItem, TRepository>
        where TItem : class, IValueTypeModel<TNaturalKey>, IAggregateRoot
        where TRepository : IValueTypeRepository<TNaturalKey, TItem>
    {
        public AuthenticatedValueTypeItemControllerBase(ILogger logger, TRepository repository)
            : base(logger, repository)
        {
            Contract.Requires(logger != null);
            Contract.Requires(repository != null);
        }
    }
}
