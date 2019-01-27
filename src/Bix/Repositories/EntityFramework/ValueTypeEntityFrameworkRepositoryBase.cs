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
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Bix.Repositories.EntityFramework
{
    public abstract class ValueTypeEntityFrameworkRepositoryBase<TIdentity, TNaturalKey, TModel, TDbContext>
        : EntityFrameworkRepositoryBase<TIdentity, TModel, TDbContext>, IValueTypeRepository<TIdentity, TNaturalKey, TModel>
        where TModel : ValueTypeModelBase<TModel, TIdentity, TNaturalKey>, IAggregateRoot, IHasIdentity<TIdentity>, IHasNaturalKey<TNaturalKey>, new()
        where TDbContext : DbContext
    {
        public ValueTypeEntityFrameworkRepositoryBase(
            IHttpContextAccessor httpContextAccessor,
            ILogger<EntityFrameworkRepositoryBase<TIdentity, TModel, TDbContext>> logger,
            ICache cache,
            IAuditingColumns auditingColumns,
            TDbContext context)
            : base(httpContextAccessor, logger, cache, auditingColumns, context)
        {
            Contract.Requires(httpContextAccessor != null);
            Contract.Requires(logger != null);
            Contract.Requires(cache != null);
            Contract.Requires(auditingColumns != null);
            Contract.Requires(context != null);
            Contract.Ensures(this.Logger != null);
            Contract.Ensures(this.Context != null);
        }

        public async Task<TModel> FindOrAddAsync(TModel item, CancellationToken cancellationToken)
        {
            try
            {
                var existingItem = await this.Items.FirstOrDefaultAsync(i => i.NaturalKey.Equals(item.NaturalKey));
                if (existingItem != null)
                {
                    await this.Context.EnsureChildModelsArePopulated(existingItem, this.Cache, cancellationToken);
                    await this.OnAfterRetrieveAsync(existingItem, cancellationToken);
                    return existingItem;
                }

                return await this.AddAsync(item, cancellationToken);
            }
            catch (Exception ex)
            {
                var ve = new EntityFrameworkRepositoryException("Failure to FindOrAddAsync", ex);
                this.Logger.LogError("LogInstance {LogInstance}, Item {Item}, Exception {Exception}", ve.LogInstance, item.ToJson(), ex);
                throw ve;
            }
        }
    }

    public abstract class ValueTypeEntityFrameworkRepositoryBase<TNaturalKey, TModel, TDbContext>
        : ValueTypeEntityFrameworkRepositoryBase<TNaturalKey, TNaturalKey, TModel, TDbContext>, IValueTypeRepository<TNaturalKey, TModel>
        where TModel : ValueTypeModelBase<TModel, TNaturalKey>, IAggregateRoot, IHasIdentity<TNaturalKey>, IHasNaturalKey<TNaturalKey>, new()
        where TDbContext : DbContext
    {
        public ValueTypeEntityFrameworkRepositoryBase(
            IHttpContextAccessor httpContextAccessor,
            ILogger<EntityFrameworkRepositoryBase<TNaturalKey, TModel, TDbContext>> logger,
            ICache cache,
            IAuditingColumns auditingColumns,
            TDbContext context)
            : base(httpContextAccessor, logger, cache, auditingColumns, context)
        {
            Contract.Requires(httpContextAccessor != null);
            Contract.Requires(logger != null);
            Contract.Requires(cache != null);
            Contract.Requires(auditingColumns != null);
            Contract.Requires(context != null);
            Contract.Ensures(this.Logger != null);
            Contract.Ensures(this.Context != null);
        }
    }
}
