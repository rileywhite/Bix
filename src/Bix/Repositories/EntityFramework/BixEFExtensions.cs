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
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bix.Repositories.EntityFramework
{
    internal static class BixEFExtensions
    {
        /// <summary>
        /// For backwards compatibility with older data, child model existence constraints
        /// are not enforced in the database. Instead, at the WebApi layer, any missing models
        /// within an aggregate are populated on retrieval using this method.
        /// </summary>
        /// <returns>
        /// <c>true</c> if any missing children were added, else <c>false</c>
        /// </returns>
        /// <remarks>
        /// If children were added, then the associted context will be tracking the model as dirty,
        /// and the caller may want to save the aggregate model.
        /// 
        /// There is also the expectation that potentially missing child model properties and
        /// fields are referenced through concrete types with parameterless constructors.
        /// </remarks>
        public static bool TryPopulateMissingChildModels<TAggregateRoot>(
            this TAggregateRoot source,
            ICache cache,
            CancellationToken cancellationToken)
            where TAggregateRoot : ModelBase, IAggregateRoot
        {
            if (source == null) { return false; }

            var isChanged = false;

            var modelsToProcess = new Stack<ModelBase>();
            modelsToProcess.Push(source);

            var allModels = new HashSet<ModelBase> { source };

            while (modelsToProcess.Any())
            {
                var model = modelsToProcess.Pop();
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var childModelProperty in model.GetType().GetChildModelProperties(cache, cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!(childModelProperty.GetValue(model) is ModelBase childModel))
                    {
                        childModel = (ModelBase)childModelProperty.PropertyType.GetConstructor(new Type[0]).Invoke(new object[0]);
                        childModelProperty.SetValue(model, childModel);
                        isChanged = true;
                    }

                    if (!allModels.Contains(childModel))
                    {
                        modelsToProcess.Push(childModel); // yes, even process new ones, just in case
                        allModels.Add(childModel);
                    }
                }

                foreach (var childEnumPropertyAndType in model.GetType().GetChildModelCollectionProperties(cache, cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!(childEnumPropertyAndType.Item1.GetValue(model) is IEnumerable<ModelBase> childModelList)) { continue; }

                    foreach (var childModel in childModelList.Where(cm => cm != null))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (!allModels.Contains(childModel))
                        {
                            modelsToProcess.Push(childModel);
                            allModels.Add(childModel);
                        }
                    }
                }
            }

            return isChanged;
        }

        internal async static Task EnsureChildModelsArePopulated<TAggregateRoot>(
            this DbContext source,
            TAggregateRoot aggregateRoot,
            ICache cache,
            CancellationToken cancellationToken)
            where TAggregateRoot : ModelBase, IAggregateRoot
        {
            if (aggregateRoot.TryPopulateMissingChildModels(cache, cancellationToken))
            {
                source.MarkAllAggregatedModelsAsModified(aggregateRoot, cache, cancellationToken);
                await source.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public static void TrackAddWithEntityDetection<TAggregateRoot>(
            this DbContext source,
            TAggregateRoot model,
            ICache cache,
            CancellationToken cancellationToken)
            where TAggregateRoot : ModelBase, IAggregateRoot
        {
            source.MarkAllAggregatedModelsAsModified(model, cache, cancellationToken);

            // TODO find a way to enforce this or stop treating add and update separately
            //var entry = source.Entry(model);
            //if (entry.IsKeySet)
            //{
            //    throw new EntityFrameworkRepositoryException($"Item is not valid for add: {model.ToJson()}");
            //}
        }

        public static void TrackUpdateWithEntityDetection<TAggregateRoot>(
            this DbContext source,
            TAggregateRoot model,
            ICache cache,
            CancellationToken cancellationToken)
            where TAggregateRoot : ModelBase, IAggregateRoot
        {
            source.MarkAllAggregatedModelsAsModified(model, cache, cancellationToken);

            // TODO find a way to enforce this or stop treating add and update separately
            //var entry = source.Entry(model);
            //if (!entry.IsKeySet)
            //{
            //    throw new EntityFrameworkRepositoryException($"Item is not valid for update: {model.ToJson()}");
            //}
        }

        private static void MarkAllAggregatedModelsAsModified<TAggregateRoot>(
            this DbContext source,
            TAggregateRoot model,
            ICache cache,
            CancellationToken cancellationToken)
            where TAggregateRoot : ModelBase, IAggregateRoot
        {
            foreach (var containedModel in model.CollectModelsInReverseDependencyOrder(cache, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var entry = source.Entry(containedModel);

                if (entry.State == EntityState.Detached)
                {
                    source.Attach(containedModel);
                }

                if (entry.State == EntityState.Unchanged)
                {
                    entry.State = EntityState.Modified;
                }
            }
        }

        internal async static Task EnsureChildModelsArePopulated<TAggregateRoot>(
            this DbContext source,
            IQueryable<TAggregateRoot> aggregateRoots,
            ICache cache,
            CancellationToken cancellationToken)
            where TAggregateRoot : ModelBase, IAggregateRoot
        {
            var isChanged = false;
            foreach (var aggregateRoot in aggregateRoots)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (aggregateRoot.TryPopulateMissingChildModels(cache, cancellationToken))
                {
                    source.Update(aggregateRoot);
                    isChanged = true;
                }
            }

            if (isChanged) { await source.SaveChangesAsync(cancellationToken).ConfigureAwait(false); }
        }

        internal static decimal GetLoggedInEmployeeId(this HttpContext source)
        {
            try
            {
                return decimal.Parse(source.User.Identity.Name);
            }
            catch (Exception ex)
            {
                throw new EntityFrameworkRepositoryException("Cannot determine logged in user for auditing", ex);
            }
        }

        internal static void AuditChanges(
            this DbContext source,
            HttpContext httpContext,
            IAuditingColumns auditingColumns)
        {
            var employeeId = httpContext.GetLoggedInEmployeeId();

            var now = DateTime.Now;

            foreach (var entry in source.ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                if (entry.State == EntityState.Added)
                {
                    var createdByIdProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == auditingColumns.CreatedByColumnName);
                    if (createdByIdProperty != null) { createdByIdProperty.CurrentValue = employeeId; }

                    var createdAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == auditingColumns.CreatedAtColumnName);
                    if (createdAtProperty != null) { createdAtProperty.CurrentValue = now; }
                }

                var updatedByIdProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == auditingColumns.UpdatedByColumnName);
                if (updatedByIdProperty != null) { updatedByIdProperty.CurrentValue = employeeId; }

                var updatedAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == auditingColumns.UpdatedAtColumnName);
                if (updatedAtProperty != null) { updatedAtProperty.CurrentValue = now; }
            }
        }
    }
}
