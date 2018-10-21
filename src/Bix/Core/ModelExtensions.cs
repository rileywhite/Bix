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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Bix.Core
{
    /// <summary>
    /// Behaviors available to enhance usability of <see cref="ModelBase"/> types
    /// </summary>
    public static class ModelExtensions
    {
        /// <summary>
        /// Determines whether a type is an aggregate root model.
        /// </summary>
        /// <param name="source">Type to examing</param>
        /// <returns><c>true</c> if the type is a model and an aggregate root, else <c>false</c>.</returns>
        public static bool IsAggregateRootModelType(this Type source)
        {
            return
                source != null &&
                typeof(ModelBase).GetTypeInfo().IsAssignableFrom(source) &&
                typeof(IAggregateRoot).GetTypeInfo().IsAssignableFrom(source) &&
                !source.GetTypeInfo().IsAbstract;
        }

        /// <summary>
        /// Determines whether a type is a model that is not an aggregate root.
        /// </summary>
        /// <param name="source">Type to examine</param>
        /// <returns><c>true</c> if a type is a model but not an aggregate root, else <c>false</c>.</returns>
        public static bool IsNonAggregateRootModelType(this Type source)
        {
            var typeInfo = typeof(ModelBase).GetTypeInfo();
            return
                source != null &&
                typeof(ModelBase).GetTypeInfo().IsAssignableFrom(source) &&
                !typeof(IAggregateRoot).GetTypeInfo().IsAssignableFrom(source) &&
                !source.GetTypeInfo().IsAbstract;
        }

        /// <summary>
        /// Finds all child properties that are of non-aggregateroot model types, menaing
        /// that they are Contained rather than Referenced, and they should be updated in tandem with their parent.
        /// </summary>
        /// <param name="source">Type to examine</param>
        /// <param name="cache">Cache to use for avoiding multiple expensive reflection operations</param>
        /// <param name="cancellationToken">Cancellation token for cancelling the operation.</param>
        /// <returns>Collection of all child model properties</returns>
        public static IEnumerable<PropertyInfo> GetChildModelProperties(
            this Type source,
            ICache cache,
            CancellationToken cancellationToken = default)
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
                from p in source.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                where IsChildModelProperty(p)
                select p;
        }

        [Obsolete("Replaced by GetChildModelCollectionProperties()")]
        public static IEnumerable<ValueTuple<PropertyInfo, Type>> GetChildModelListProperties(
            this Type source,
            ICache cache,
            CancellationToken cancellationToken = default)
            => GetChildModelCollectionProperties(source, cache, cancellationToken);

        /// <summary>
        /// Gets the properties of a type the represent collections of child model types.
        /// </summary>
        /// <param name="source">Type to examine</param>
        /// <param name="cache">Cache to use for avoiding multiple expensive reflection operations</param>
        /// <param name="cancellationToken">Cancellation token for cancelling the operation.</param>
        /// <returns>Collection of properties that are collections of child models</returns>
        public static IEnumerable<ValueTuple<PropertyInfo, Type>> GetChildModelCollectionProperties(
            this Type source,
            ICache cache,
            CancellationToken cancellationToken = default)
        {
            if (source == null) { throw new ArgumentNullException(nameof(source)); }

            var cacheKey = $"GetChildModelCollectionProperties_{source.FullName}";

            if (!cache.TryGet(
                cacheKey,
                out IEnumerable<ValueTuple<PropertyInfo, Type>> cachedValue))
            {
                cancellationToken.ThrowIfCancellationRequested();
                cachedValue = source.DoGetChildModelCollectionProperties().ToList();
                cache.Set(cacheKey, cachedValue);
            }

            return cachedValue;
        }

        private static IEnumerable<ValueTuple<PropertyInfo, Type>> DoGetChildModelCollectionProperties(this Type source)
        {
            if (source == null) { yield break; }

            foreach (var property in
                source.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                if (property.IsChildModelCollectionProperty(out Type childModelType))
                {
                    yield return ValueTuple.Create(property, childModelType);
                }
            }
        }

        /// <summary>
        /// Examines a property to determine if it is a child model.
        /// </summary>
        /// <param name="source">Property to examine</param>
        /// <returns><c>true</c> if the property holds a child model, else <c>false</c></returns>
        public static bool IsChildModelProperty(this PropertyInfo source)
        {
            return
                source != null &&
                source.CanRead &&
                source.CanWrite &&
                source.PropertyType.IsNonAggregateRootModelType();
        }

        [Obsolete("Replaced by IsChildModelCollectionProperty")]
        public static bool IsChildModelListProperty(this PropertyInfo source, out Type childModelType) => IsChildModelCollectionProperty(source, out childModelType);

        /// <summary>
        /// Examines a property to determine if it is a collection of child models
        /// </summary>
        /// <param name="source">Property to examine</param>
        /// <param name="childModelType">Populated with the child model type if the property is a collection, else <c>null</c>.</param>
        /// <returns><c>true</c> if the property holds a collection child models, else <c>false</c></returns>
        public static bool IsChildModelCollectionProperty(this PropertyInfo source, out Type childModelType)
        {
            if(source == null || !source.CanRead)
            {
                childModelType = null;
                return false;
            }

            if(source.PropertyType.IsArray)
            {
                if (source.PropertyType.GetArrayRank() != 1)
                {
                    childModelType = null;
                    return false;
                }

                childModelType = source.PropertyType.GetElementType();
            }
            else
            {
                var collectionInterface = source.PropertyType.GetInterfaces().FirstOrDefault(
                    it => it.IsGenericType && it.GetGenericTypeDefinition() == typeof(ICollection<>));

                if (collectionInterface == null &&
                    source.PropertyType.IsGenericType &&
                    source.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                {
                    collectionInterface = source.PropertyType;
                }

                if (collectionInterface == null)
                {
                    childModelType = null;
                    return false;
                }

                var genericArguments = collectionInterface.GetGenericArguments();

                if (genericArguments.Length != 1)
                {
                    childModelType = null;
                    return false;
                }

                childModelType = genericArguments[0];
            }

            Contract.Assert(childModelType != null);

            if (!childModelType.IsNonAggregateRootModelType())
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
        /// <typeparam name="TAggregateRoot">Type of the aggregate root to look at</typeparam>
        /// <param name="source">Root item to grab sub-items from</param>
        /// <param name="cache">Cache containing previously determined dependencies between model types</param>
        /// <param name="cancellationToken">Tracks cancellation for the operation</param>
        /// <returns>Models contained in the aggregate ordered in reverse dependency order so that children come before parents</returns>
        public static IEnumerable<ModelBase> CollectModelsInReverseDependencyOrder<TAggregateRoot>(
            this TAggregateRoot source,
            ICache cache,
            CancellationToken cancellationToken = default)
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
            foreach (var childEnumPropertyAndType in model.GetType().GetChildModelCollectionProperties(cache, cancellationToken))
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
