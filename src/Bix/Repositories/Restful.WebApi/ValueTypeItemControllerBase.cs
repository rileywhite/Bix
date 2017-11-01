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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Bix.Repositories.Restful.WebApi
{
    public abstract class ValueTypeItemControllerBase<TIdentity, TNaturalKey, TItem, TRepository>
        : ItemControllerBase<TIdentity, TItem, TRepository>
        where TItem : class, IHasIdentity<TIdentity>, IHasNaturalKey<TNaturalKey>, IAggregateRoot
        where TRepository : IValueTypeRepository<TIdentity, TNaturalKey, TItem>
    {
        public ValueTypeItemControllerBase(ILogger logger, TRepository repository)
            : base(logger, repository)
        {
            Contract.Requires(logger != null);
            Contract.Requires(repository != null);
        }

        [HttpPatch("{naturalKey}")]
        public async Task<IActionResult> CreateOrUpdate(string naturalKey, [FromBody] TItem item)
        {
            if (naturalKey == null || item == null || !naturalKey.Equals(item.NaturalKey))
            {
                return this.BadRequest();
            }
            return this.Ok(await this.Repository.FindOrAddAsync(item));
        }

        [HttpPatch]
        public async Task<IActionResult> CreateOrUpdate([FromBody] TItem item)
        {
            if (item == null || item.NaturalKey == null)
            {
                return this.BadRequest();
            }
            return this.Ok(await this.Repository.FindOrAddAsync(item));
        }
    }

    public abstract class ValueTypeItemControllerBase<TNaturalKey, TItem, TRepository>
        : ValueTypeItemControllerBase<TNaturalKey, TNaturalKey, TItem, TRepository>
        where TItem : class, IHasIdentity<TNaturalKey>, IHasNaturalKey<TNaturalKey>, IAggregateRoot
        where TRepository : IValueTypeRepository<TNaturalKey, TItem>
    {
        public ValueTypeItemControllerBase(ILogger logger, TRepository repository)
            : base(logger, repository)
        {
            Contract.Requires(logger != null);
            Contract.Requires(repository != null);
        }
    }
}
