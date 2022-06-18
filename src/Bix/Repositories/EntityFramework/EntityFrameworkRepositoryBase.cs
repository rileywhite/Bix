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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace Bix.Repositories.EntityFramework
{
    public abstract class EntityFrameworkRepositoryBase<TIdentity, TModel, TDbContext> : IRepository<TIdentity, TModel>
        where TModel : class, IModel<TIdentity>, IAggregateRoot
        where TDbContext : DbContext
    {
        protected IHttpContextAccessor HttpContextAccessor { get; }
        protected ILogger<EntityFrameworkRepositoryBase<TIdentity, TModel, TDbContext>> Logger { get; }
        protected ICache Cache { get; }
        public IAuditingColumns AuditingColumns { get; }
        protected TDbContext Context { get; }

        /// <summary>
        /// Gets the Items source for the repository.
        /// </summary>
        /// <remarks>
        /// EntityFramework may begin eager loading data from the database if this
        /// property is decorated with calls to
        /// <see cref="EntityFrameworkQueryableExtensions.Include{TEntity, TProperty}(IQueryable{TEntity}, Expression{Func{TEntity, TProperty}})"/>
        /// or similar. It's better to use <see cref="OnAfterRetrieveAsync(IQueryable{TModel}, CancellationToken)"/> to decorate
        /// items after a filter has been applied.
        /// </remarks>
        protected abstract IQueryable<TModel> Items { get; }

        public EntityFrameworkRepositoryBase(
            IHttpContextAccessor httpContextAccessor,
            ILogger<EntityFrameworkRepositoryBase<TIdentity, TModel, TDbContext>> logger,
            ICache cache,
            IAuditingColumns auditingColumns,
            TDbContext context)
        {
            Contract.Requires(httpContextAccessor != null);
            Contract.Requires(logger != null);
            Contract.Requires(cache != null);
            Contract.Requires(context != null);
            Contract.Requires(auditingColumns != null);
            Contract.Ensures(this.Logger != null);
            Contract.Ensures(this.Context != null);

            this.HttpContextAccessor = httpContextAccessor;
            this.Logger = logger;
            this.Cache = cache;
            this.AuditingColumns = auditingColumns;
            this.Context = context;
        }

        protected virtual bool PopulateChildModelsOnGet => true;

        #region Standard Access Methods

        public async Task<IQueryable<TModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var items = await this.OnAfterRetrieveAsync(this.Items, cancellationToken).ConfigureAwait(false);
                if (this.PopulateChildModelsOnGet)
                {
                    await this.Context.EnsureChildModelsArePopulated(items, this.Cache, cancellationToken).ConfigureAwait(false);
                }
                return items;
            }
            catch (Exception ex)
            {
                var ve = new EntityFrameworkRepositoryException("Failure to GetAllAsync", ex);
                this.Logger.LogError("LogInstance {LogInstance}, Exception {Exception}", ve.LogInstance, ex);
                throw ve;
            }
        }

        public async Task<TModel> FindAsync(TIdentity identity, CancellationToken cancellationToken = default)
        {
            try
            {
                // TODO: use linq expression instead of AsEnumerable() for filtering for perf
                var foundItem = (await this.OnAfterRetrieveAsync(
                    this.Items.AsEnumerable().Where(i => identity.Equals(i.Identity)).AsQueryable(), cancellationToken).ConfigureAwait(false))
                    .First();
                if (this.PopulateChildModelsOnGet)
                {
                    await this.Context.EnsureChildModelsArePopulated(foundItem, this.Cache, cancellationToken).ConfigureAwait(false);
                }
                return foundItem;
            }
            catch (Exception ex)
            {
                var ve = new EntityFrameworkRepositoryException("Failure to FindAsync", ex);
                this.Logger.LogError("LogInstance {LogInstance}, Key {Key}, Exception {Exception}", ve.LogInstance, identity, ex);
                throw ve;
            }
        }

        public async Task<TModel> AddAsync(TModel item, CancellationToken cancellationToken = default)
        {
            try
            {
                this.Context.TrackAddWithEntityDetection(item, this.Cache, cancellationToken);

                item = await this.OnBeforeAddAsync(item, cancellationToken).ConfigureAwait(false);
                await this.Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                return await this.OnAfterAddAsync(await this.FindAsync(item.Identity, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var ve = new EntityFrameworkRepositoryException("Failure to AddAsync", ex);
                this.Logger.LogError("LogInstance {LogInstance}, Item {Item}, Exception {Exception}", ve.LogInstance, item.ToJson(), ex);
                throw ve;
            }
        }

        public async Task RemoveAsync(TIdentity identity, CancellationToken cancellationToken = default)
        {
            try
            {
                var item = await this.FindAsync(identity, cancellationToken).ConfigureAwait(false);
                if (item == null) { return; }

                item = await this.OnBeforeRemoveAsync(item, cancellationToken).ConfigureAwait(false);

                await Task.Run(() => this.Context.Remove(item)).ConfigureAwait(false);
                await this.Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                await this.OnAfterRemoveAsync(identity, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var ve = new EntityFrameworkRepositoryException("Failure to RemoveAsync", ex);
                this.Logger.LogError("LogInstance {LogInstance}, Key {Key}, Exception {Exception}", ve.LogInstance, identity, ex);
                throw ve;
            }
        }

        public async Task<TModel> UpdateAsync(TModel item, CancellationToken cancellationToken = default)
        {
            try
            {
                this.Context.TrackUpdateWithEntityDetection(item, this.Cache, cancellationToken);

                item = await this.OnBeforeUpdateAsync(item, cancellationToken).ConfigureAwait(false);
                await this.Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                return await this.OnAfterUpdateAsync(await this.FindAsync(item.Identity, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var ve = new EntityFrameworkRepositoryException("Failure to UpdateAsync", ex);
                this.Logger.LogError("LogInstance {LogInstance}, Item {Item}, Exception {Exception}", ve.LogInstance, item.ToJson(), ex);
                throw ve;
            }
        }

        public async Task<ModelMetadata> GetMetadataAsync(TIdentity identity, CancellationToken cancellationToken = default)
        {
            var item = await this.FindAsync(identity, cancellationToken).ConfigureAwait(false);
            if (item == null) { return null; }

            var entry = this.Context.Entry(item);

            switch (entry.State)
            {
                case EntityState.Deleted:
                case EntityState.Modified:
                case EntityState.Unchanged:
                    return new ModelMetadata
                    {
                        CreatedAt = (DateTime?)entry.Properties.FirstOrDefault(p => p.Metadata.Name == this.AuditingColumns.CreatedAtColumnName)?.CurrentValue,
                        CreatedById = (decimal?)entry.Properties.FirstOrDefault(p => p.Metadata.Name == this.AuditingColumns.CreatedByColumnName)?.CurrentValue,
                        LastUpdatedAt = (DateTime?)entry.Properties.FirstOrDefault(p => p.Metadata.Name == this.AuditingColumns.UpdatedAtColumnName)?.CurrentValue,
                        LastUpdatedById = (decimal?)entry.Properties.FirstOrDefault(p => p.Metadata.Name == this.AuditingColumns.UpdatedByColumnName)?.CurrentValue,
                    };

                default:
                    return null;
            }
        }

        #endregion

        #region Customization Hooks

        /// <summary>
        /// Post-processing and decoration for retrived items.
        /// </summary>
        /// <param name="items">Items retrieved from the repository so far.</param>
        /// <param name="cancellationToken">For cancelling the operation.</param>
        /// <returns>Updated set of items.</returns>
        /// <remarks>
        /// This is a good place to decorate filtered items, for example using
        /// <see cref="EntityFrameworkQueryableExtensions.Include{TEntity, TProperty}(IQueryable{TEntity}, Expression{Func{TEntity, TProperty}})"/>
        /// or similar.
        /// 
        /// EntityFramework may begin eager loading data from the database if the <see cref="Items"/>
        /// property is decorated with such calls, which can kill performance.
        /// 
        /// The default implemenation simply returns <paramref name="items"/>.
        /// </remarks>
        protected virtual Task<IQueryable<TModel>> OnAfterRetrieveAsync(IQueryable<TModel> items, CancellationToken cancellationToken) => Task.FromResult(items);

        protected virtual Task<TModel> OnBeforeAddAsync(TModel item, CancellationToken cancellationToken)
        {
            this.Context.AuditChanges(this.HttpContextAccessor.HttpContext, this.AuditingColumns);
            return Task.FromResult(item);
        }
        protected virtual Task<TModel> OnAfterAddAsync(TModel item, CancellationToken cancellationToken) => Task.FromResult(item);

        protected virtual Task<TModel> OnBeforeUpdateAsync(TModel item, CancellationToken cancellationToken)
        {
            this.Context.AuditChanges(this.HttpContextAccessor.HttpContext, this.AuditingColumns);
            return Task.FromResult(item);
        }

        protected virtual Task<TModel> OnAfterUpdateAsync(TModel item, CancellationToken cancellationToken) => Task.FromResult(item);
        protected virtual Task<TModel> OnBeforeRemoveAsync(TModel item, CancellationToken cancellationToken) => Task.FromResult(item);
        protected virtual Task OnAfterRemoveAsync(TIdentity identity, CancellationToken cancellationToken) => Task.CompletedTask;

        #endregion
    }
}
