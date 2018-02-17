/***************************************************************************/
// Copyright 2013-2018 Riley White
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
using System.Collections.Generic;
using System.Collections;
using System.Linq.Expressions;

namespace Bix.Repositories.EntityFramework
{
    public abstract class EntityFrameworkRepositoryBase<TIdentity, TModel, TDbContext> : IRepository<TIdentity, TModel>
        where TModel : ModelBase<TModel, TIdentity>, IAggregateRoot, IHasIdentity<TIdentity>, new()
        where TDbContext : DbContext
    {
        protected IHttpContextAccessor HttpContextAccessor { get; }
        protected ILogger<EntityFrameworkRepositoryBase<TIdentity, TModel, TDbContext>> Logger { get; }
        protected ICache Cache { get; }
        public IAuditingColumns AuditingColumns { get; }
        protected TDbContext Context { get; }

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

        public async Task<IQueryable<TModel>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                if (this.PopulateChildModelsOnGet)
                {
                    await this.Context.EnsureChildModelsArePopulated(this.Items, this.Cache, cancellationToken);
                }
                await this.OnAfterRetrieveAsync(this.Items, cancellationToken);
                return await Task.Run(() => this.Items, cancellationToken);
            }
            catch (Exception ex)
            {
                var ve = new EntityFrameworkRepositoryException("Failure to GetAllAsync", ex);
                this.Logger.LogError("LogInstance {LogInstance}, Exception {Exception}", ve.LogInstance, ex);
                throw ve;
            }
        }

        public async Task<TModel> FindAsync(TIdentity identity, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var foundItem = await this.Items.FirstAsync(i => identity.Equals(i.Identity), cancellationToken);
                await this.Context.EnsureChildModelsArePopulated(foundItem, this.Cache, cancellationToken);
                await this.OnAfterRetrieveAsync(foundItem, cancellationToken);
                return foundItem;
            }
            catch (Exception ex)
            {
                var ve = new EntityFrameworkRepositoryException("Failure to FindAsync", ex);
                this.Logger.LogError("LogInstance {LogInstance}, Key {Key}, Exception {Exception}", ve.LogInstance, identity, ex);
                throw ve;
            }
        }

        public async Task<TModel> AddAsync(TModel item, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                this.Context.TrackAddWithEntityDetection(item, this.Cache, cancellationToken);

                item = await this.OnBeforeAddAsync(item, cancellationToken);
                await this.Context.SaveChangesAsync(cancellationToken);

                return await this.OnAfterAddAsync(await this.FindAsync(item.Identity, cancellationToken), cancellationToken);
            }
            catch (Exception ex)
            {
                var ve = new EntityFrameworkRepositoryException("Failure to AddAsync", ex);
                this.Logger.LogError("LogInstance {LogInstance}, Item {Item}, Exception {Exception}", ve.LogInstance, item.ToJson(), ex);
                throw ve;
            }
        }

        public async Task RemoveAsync(TIdentity identity, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var item = await this.FindAsync(identity, cancellationToken);
                if (item == null) { return; }

                item = await this.OnBeforeRemoveAsync(item, cancellationToken);

                await Task.Run(() => this.Context.Remove(item));
                await this.Context.SaveChangesAsync(cancellationToken);

                await this.OnAfterRemoveAsync(identity, cancellationToken);
            }
            catch (Exception ex)
            {
                var ve = new EntityFrameworkRepositoryException("Failure to RemoveAsync", ex);
                this.Logger.LogError("LogInstance {LogInstance}, Key {Key}, Exception {Exception}", ve.LogInstance, identity, ex);
                throw ve;
            }
        }

        public async Task<TModel> UpdateAsync(TModel item, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                this.Context.TrackUpdateWithEntityDetection(item, this.Cache, cancellationToken);

                item = await this.OnBeforeUpdateAsync(item, cancellationToken);
                await this.Context.SaveChangesAsync(cancellationToken);

                return await this.OnAfterUpdateAsync(await this.FindAsync(item.Identity, cancellationToken), cancellationToken);
            }
            catch (Exception ex)
            {
                var ve = new EntityFrameworkRepositoryException("Failure to UpdateAsync", ex);
                this.Logger.LogError("LogInstance {LogInstance}, Item {Item}, Exception {Exception}", ve.LogInstance, item.ToJson(), ex);
                throw ve;
            }
        }

        public async Task<ModelMetadata> GetMetadataAsync(TIdentity identity, CancellationToken cancellationToken = default(CancellationToken))
        {
            var item = await this.FindAsync(identity, cancellationToken);
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

        #region Class to provide async enumerable wrapper of a single item (remove when yield return become async compatible)

        private class SingleItemAsyncEnumerableWrapper<T> : IAsyncEnumerable<T>, IEnumerable<T>
        {
            public SingleItemAsyncEnumerableWrapper(T item)
            {
                this.Item = item;
            }

            public T Item { get; }

            public Type ElementType => typeof(T);

            public Expression Expression => throw new NotImplementedException();

            public IQueryProvider Provider => throw new NotImplementedException();

            public IAsyncEnumerator<T> GetEnumerator()
            {
                return new SingleItemAsyncEnumerator(this.Item);
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return new SingleItemAsyncEnumerator(this.Item);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new SingleItemAsyncEnumerator(this.Item);
            }

            private class SingleItemAsyncEnumerator : IAsyncEnumerator<T>, IEnumerator<T>, IEnumerator
            {
                public SingleItemAsyncEnumerator(T item)
                {
                    this.Current = item;
                }

                private bool HasReturnedItem { get; set; }
                public T Current { get; }

                object IEnumerator.Current => throw new NotImplementedException();

                public void Dispose() { }

                Task<bool> IAsyncEnumerator<T>.MoveNext(CancellationToken cancellationToken)
                {
                    return Task.FromResult(this.MoveNext());
                }

                public bool MoveNext()
                {
                    if (this.HasReturnedItem)
                    {
                        return false;
                    }

                    this.HasReturnedItem = true;
                    return true;
                }

                public void Reset()
                {
                    this.HasReturnedItem = false;
                }
            }
        }

        #endregion

        #region Customization Hooks

        protected virtual Task OnAfterRetrieveAsync(TModel item, CancellationToken cancellationToken)
        {
            return this.OnAfterRetrieveAsync(new SingleItemAsyncEnumerableWrapper<TModel>(item).AsQueryable(), cancellationToken);
        }

        protected virtual Task OnAfterRetrieveAsync(IQueryable<TModel> items, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task<TModel> OnBeforeAddAsync(TModel item, CancellationToken cancellationToken)
        {
            this.Context.AuditChanges(this.HttpContextAccessor.HttpContext, this.AuditingColumns);
            return Task.FromResult(item);
        }
        protected virtual Task<TModel> OnAfterAddAsync(TModel item, CancellationToken cancellationToken) { return Task.FromResult(item); }

        protected virtual Task<TModel> OnBeforeUpdateAsync(TModel item, CancellationToken cancellationToken)
        {
            this.Context.AuditChanges(this.HttpContextAccessor.HttpContext, this.AuditingColumns);
            return Task.FromResult(item);
        }

        protected virtual Task<TModel> OnAfterUpdateAsync(TModel item, CancellationToken cancellationToken) { return Task.FromResult(item); }
        protected virtual Task<TModel> OnBeforeRemoveAsync(TModel item, CancellationToken cancellationToken) { return Task.FromResult(item); }
        protected virtual Task OnAfterRemoveAsync(TIdentity identity, CancellationToken cancellationToken) { return Task.CompletedTask; }

        #endregion
    }
}
