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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Bix.Core
{
    public static class ModelExtensions
    {
        public static bool IsAggregateRootModelType(this Type source)
        {
            return
                source != null &&
                typeof(ModelBase).IsAssignableFrom(source) &&
                typeof(IAggregateRoot).IsAssignableFrom(source) &&
                !source.IsAbstract;
        }

        public static bool IsNonAggregateRootModelType(this Type source)
        {
            return
                source != null &&
                typeof(ModelBase).IsAssignableFrom(source) &&
                !typeof(IAggregateRoot).IsAssignableFrom(source) &&
                !source.IsAbstract;
        }

        public static IEnumerable<PropertyInfo> GetChildModelProperties(
            this Type source,
            ICache cache,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = $"GetChildModelProperties_{source.FullName}";

            if (!cache.TryGet(
                cacheKey,
                out IEnumerable<PropertyInfo> cachedValue))
            {
                cancellationToken.ThrowIfCancellationRequested();
                cachedValue = source.DoGetChildModelProperties().ToList();
                cache.Set(cacheKey, cachedValue);
            }

            return cachedValue;
        }

        private static IEnumerable<PropertyInfo> DoGetChildModelProperties(this Type source)
        {
            if (source == null) { return new PropertyInfo[0]; }

            return
                from p in source.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                where IsChildModelProperty(p)
                select p;
        }

        public static IEnumerable<ValueTuple<PropertyInfo, Type>> GetChildModelListProperties(
            this Type source,
            ICache cache,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (source == null) { throw new ArgumentNullException(nameof(source)); }

            var cacheKey = $"GetChildModelListProperties_{source.FullName}";

            if (!cache.TryGet(
                cacheKey,
                out IEnumerable<ValueTuple<PropertyInfo, Type>> cachedValue))
            {
                cancellationToken.ThrowIfCancellationRequested();
                cachedValue = source.DoGetChildModelListProperties().ToList();
                cache.Set(cacheKey, cachedValue);
            }

            return cachedValue;
        }

        private static IEnumerable<ValueTuple<PropertyInfo, Type>> DoGetChildModelListProperties(this Type source)
        {
            if (source == null) { yield break; }

            foreach (var property in
                source.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                if (property.IsChildModelListProperty(out Type childModelType))
                {
                    yield return ValueTuple.Create(property, childModelType);
                }
            }
        }

        public static bool IsChildModelProperty(this PropertyInfo source)
        {
            return
                source != null &&
                source.CanRead &&
                source.CanWrite &&
                source.PropertyType.IsNonAggregateRootModelType();
        }

        public static bool IsChildModelListProperty(this PropertyInfo source, out Type childModelType)
        {
            var isChildModelEnumerationProperty =
                source != null &&
                source.CanRead &&
                source.CanWrite &&
                source.PropertyType.IsConstructedGenericType &&
                typeof(List<>).Equals(source.PropertyType.GetGenericTypeDefinition());

            if (!isChildModelEnumerationProperty)
            {
                childModelType = null;
                return false;
            }

            var genericArguments = source.PropertyType.GetGenericArguments();

            if (genericArguments.Length != 1)
            {
                childModelType = null;
                return false;
            }

            childModelType = genericArguments[0];

            if (!typeof(ModelBase).IsAssignableFrom(childModelType))
            {
                childModelType = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Collects the models in an aggregate ordered such that they are safe to attach to an EF context grapsh
        /// in a manner that will ensure that attaching to each, in turn, will cause EF to detected updated vs added
        /// entities.
        /// </summary>
        /// <remarks>
        /// Currently, the ordering logic depends on each entity and sub-entity having no back-references to parent items.
        /// This could be updated by adding an additional topological sort, but until/unless that's needed, I'll leave it out.
        /// </remarks>
        /// <typeparam name="TAggregateRoot">Type of the aggregate root to look at./typeparam>
        /// <param name="source">Root item to grab sub-items from</param>
        /// <param name="cache">Cache containing previously determined dependencies between model types</param>
        /// <param name="cancellationToken"/>Tracks cancellation for the operation</param>>
        /// <returns>Models contained in the aggregate ordered in reverse dependency order so that children come before parents</returns>
        public static IEnumerable<ModelBase> CollectModelsInReverseDependencyOrder<TAggregateRoot>(
            this TAggregateRoot source,
            ICache cache,
            CancellationToken cancellationToken = default(CancellationToken))
            where TAggregateRoot : ModelBase, IAggregateRoot
        {
            if (source == null) { yield break; }

            var modelsToProcess = new Queue<ModelBase>(); // use a queue for breadth-first traversal (dfs probably wouldn't make a difference, though)
            modelsToProcess.Enqueue(source);

            var modelDupeChecker = new HashSet<ModelBase> { source };

            var allModelsStackedByDependency = new Stack<ModelBase>();
            allModelsStackedByDependency.Push(source);

            while (modelsToProcess.Any())
            {
                var model = modelsToProcess.Dequeue();
                CollectDirectChildModels(allModelsStackedByDependency, modelDupeChecker, modelsToProcess, model, cache, cancellationToken);
                CollectEnumeratedChildModels(allModelsStackedByDependency, modelDupeChecker, modelsToProcess, model, cache, cancellationToken);
            }

            // reverse dependency order so that EF will detect existing
            while (allModelsStackedByDependency.Any()) { yield return allModelsStackedByDependency.Pop(); }
        }

        private static void CollectEnumeratedChildModels(
            Stack<ModelBase> allModelsStackedByDependency,
            HashSet<ModelBase> modelDupeChecker,
            Queue<ModelBase> modelsToProcess,
            ModelBase model,
            ICache cache,
            CancellationToken cancellationToken)
        {
            foreach (var childEnumPropertyAndType in model.GetType().GetChildModelListProperties(cache, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var childModelList = childEnumPropertyAndType.Item1.GetValue(model) as IEnumerable<ModelBase>;
                if (childModelList == null) { continue; }

                foreach (var childModel in childModelList.Where(cm => cm != null))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!modelDupeChecker.Contains(childModel))
                    {
                        allModelsStackedByDependency.Push(childModel);
                        modelDupeChecker.Add(childModel);
                        modelsToProcess.Enqueue(childModel);
                    }
                }
            }
        }

        private static void CollectDirectChildModels(
            Stack<ModelBase> allModelsStackedByDependency,
            HashSet<ModelBase> modelDupeChecker,
            Queue<ModelBase> modelsToProcess,
            ModelBase model,
            ICache cache,
            CancellationToken cancellationToken)
        {
            foreach (var childModelProperty in model.GetType().GetChildModelProperties(cache, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var childModel = childModelProperty.GetValue(model) as ModelBase;
                if (childModel == null) { continue; }

                if (!modelDupeChecker.Contains(childModel))
                {
                    allModelsStackedByDependency.Push(childModel);
                    modelDupeChecker.Add(childModel);
                    modelsToProcess.Enqueue(childModel);
                }
            }
        }
    }
}
