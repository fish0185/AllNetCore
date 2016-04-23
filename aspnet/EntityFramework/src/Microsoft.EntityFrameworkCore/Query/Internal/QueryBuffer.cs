// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class QueryBuffer : IQueryBuffer
    {
        private readonly IStateManager _stateManager;
        private readonly IChangeDetector _changeDetector;

        private IWeakReferenceIdentityMap _identityMap0;
        private IWeakReferenceIdentityMap _identityMap1;
        private Dictionary<IKey, IWeakReferenceIdentityMap> _identityMaps;

        private readonly ConditionalWeakTable<object, object> _valueBuffers
            = new ConditionalWeakTable<object, object>();

        public QueryBuffer(
            [NotNull] IStateManager stateManager,
            [NotNull] IChangeDetector changeDetector)
        {
            _stateManager = stateManager;
            _changeDetector = changeDetector;
        }

        public virtual object GetEntity(
            IKey key, EntityLoadInfo entityLoadInfo, bool queryStateManager, bool throwOnNullKey)
        {
            if (queryStateManager)
            {
                var entry = _stateManager.TryGetEntry(key, entityLoadInfo.ValueBuffer, throwOnNullKey);

                if (entry != null)
                {
                    return entry.Entity;
                }
            }

            var identityMap = GetOrCreateIdentityMap(key);

            bool hasNullKey;
            var weakReference = identityMap.TryGetEntity(entityLoadInfo.ValueBuffer, out hasNullKey);
            if (hasNullKey)
            {
                if (throwOnNullKey)
                {
                    throw new InvalidOperationException(CoreStrings.InvalidKeyValue(key.DeclaringEntityType.DisplayName()));
                }
                return null;
            }

            object entity;
            if (weakReference == null
                || !weakReference.TryGetTarget(out entity))
            {
                entity = entityLoadInfo.Materialize();

                if (weakReference != null)
                {
                    weakReference.SetTarget(entity);
                }
                else
                {
                    identityMap.CollectGarbage();

                    identityMap.Add(entityLoadInfo.ValueBuffer, entity);
                }

                _valueBuffers.Add(entity, entityLoadInfo.ValueBuffer);
            }

            return entity;
        }

        public virtual object GetPropertyValue(object entity, IProperty property)
        {
            var entry = _stateManager.TryGetEntry(entity);

            if (entry != null)
            {
                return entry[property];
            }

            object boxedValueBuffer;
            var found = _valueBuffers.TryGetValue(entity, out boxedValueBuffer);

            Debug.Assert(found);

            var valueBuffer = (ValueBuffer)boxedValueBuffer;

            return valueBuffer[property.GetIndex()];
        }

        public virtual void StartTracking(object entity, EntityTrackingInfo entityTrackingInfo)
        {
            object boxedValueBuffer;
            if (!_valueBuffers.TryGetValue(entity, out boxedValueBuffer))
            {
                boxedValueBuffer = ValueBuffer.Empty;
            }

            entityTrackingInfo
                .StartTracking(_stateManager, entity, (ValueBuffer)boxedValueBuffer);

            foreach (var includedEntity 
                in entityTrackingInfo.GetIncludedEntities(entity)
                    .Where(includedEntity
                        => _valueBuffers.TryGetValue(includedEntity.Entity, out boxedValueBuffer)))
            {
                includedEntity.StartTracking(_stateManager, (ValueBuffer)boxedValueBuffer);
            }
        }

        public virtual void Include(
            QueryContext queryContext,
            object entity,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<IRelatedEntitiesLoader> relatedEntitiesLoaders,
            bool queryStateManager)
            => Include(
                queryContext,
                entity,
                navigationPath,
                relatedEntitiesLoaders,
                currentNavigationIndex: 0,
                queryStateManager: queryStateManager);

        private void Include(
            QueryContext queryContext,
            object entity,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<IRelatedEntitiesLoader> relatedEntitiesLoaders,
            int currentNavigationIndex,
            bool queryStateManager)
        {
            if (entity == null
                || currentNavigationIndex == navigationPath.Count)
            {
                return;
            }

            var navigation = navigationPath[currentNavigationIndex];
            var keyComparer = IncludeCore(entity, navigation);
            var key = navigation.GetTargetType().FindPrimaryKey();

            LoadNavigationProperties(
                entity,
                navigationPath,
                currentNavigationIndex,
                relatedEntitiesLoaders[currentNavigationIndex]
                    .Load(queryContext, keyComparer)
                    .Select(eli =>
                        {
                            var targetEntity = GetEntity(key, eli, queryStateManager, throwOnNullKey: false);

                            Include(
                                queryContext,
                                targetEntity,
                                navigationPath,
                                relatedEntitiesLoaders,
                                currentNavigationIndex + 1,
                                queryStateManager);

                            return targetEntity;
                        })
                    .Where(e => e != null)
                    .ToList(),
                queryStateManager);
        }

        public virtual Task IncludeAsync(
            QueryContext queryContext,
            object entity,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<IAsyncRelatedEntitiesLoader> relatedEntitiesLoaders,
            bool queryStateManager,
            CancellationToken cancellationToken)
            => IncludeAsync(
                queryContext,
                entity,
                navigationPath,
                relatedEntitiesLoaders,
                currentNavigationIndex: 0,
                queryStateManager: queryStateManager,
                cancellationToken: cancellationToken);

        private async Task IncludeAsync(
            QueryContext queryContext,
            object entity,
            IReadOnlyList<INavigation> navigationPath,
            IReadOnlyList<IAsyncRelatedEntitiesLoader> relatedEntitiesLoaders,
            int currentNavigationIndex,
            bool queryStateManager,
            CancellationToken cancellationToken)
        {
            if (entity == null
                || currentNavigationIndex == navigationPath.Count)
            {
                return;
            }

            var navigation = navigationPath[currentNavigationIndex];
            var keyComparer = IncludeCore(entity, navigation);
            var key = navigation.GetTargetType().FindPrimaryKey();

            LoadNavigationProperties(
                entity,
                navigationPath,
                currentNavigationIndex,
                await relatedEntitiesLoaders[currentNavigationIndex]
                    .Load(queryContext, keyComparer)
                    .Select(async (eli, ct) =>
                        {
                            var targetEntity = GetEntity(key, eli, queryStateManager, throwOnNullKey: false);

                            await IncludeAsync(
                                queryContext,
                                targetEntity,
                                navigationPath,
                                relatedEntitiesLoaders,
                                currentNavigationIndex + 1,
                                queryStateManager,
                                ct);

                            return targetEntity;
                        })
                    .Where(e => e != null)
                    .ToList(cancellationToken),
                queryStateManager);
        }

        private IIncludeKeyComparer IncludeCore(
            object entity,
            INavigation navigation)
        {
            var identityMap = GetOrCreateIdentityMap(navigation.ForeignKey.PrincipalKey);

            object boxedValueBuffer;
            if (!_valueBuffers.TryGetValue(entity, out boxedValueBuffer))
            {
                var entry = _stateManager.TryGetEntry(entity);

                Debug.Assert(entry != null);

                return identityMap.CreateIncludeKeyComparer(navigation, entry);
            }

            return identityMap.CreateIncludeKeyComparer(navigation, (ValueBuffer)boxedValueBuffer);
        }

        private void LoadNavigationProperties(
            object entity,
            IReadOnlyList<INavigation> navigationPath,
            int currentNavigationIndex,
            IReadOnlyList<object> relatedEntities,
            bool tracking)
        {
            _changeDetector.Suspend();
            try
            {
                var navigation = navigationPath[currentNavigationIndex];
                var inverseNavigation = navigation.FindInverse();

                if (navigation.IsDependentToPrincipal()
                    && relatedEntities.Any())
                {
                    var relatedEntity = relatedEntities[0];

                    SetNavigation(entity, navigation, relatedEntity, tracking);

                    if (inverseNavigation != null)
                    {
                        if (inverseNavigation.IsCollection())
                        {
                            AddToCollection(relatedEntity, inverseNavigation, entity, tracking);
                        }
                        else
                        {
                            SetNavigation(relatedEntity, inverseNavigation, entity, tracking);
                        }
                    }
                }
                else
                {
                    if (navigation.IsCollection())
                    {
                        AddRangeToCollection(entity, navigation, relatedEntities, tracking);

                        if (inverseNavigation != null)
                        {
                            var setter = inverseNavigation.GetSetter();

                            foreach (var relatedEntity in relatedEntities)
                            {
                                SetNavigation(relatedEntity, inverseNavigation, setter, entity, tracking);
                            }
                        }
                    }
                    else if (relatedEntities.Any())
                    {
                        var relatedEntity = relatedEntities[0];

                        SetNavigation(entity, navigation, relatedEntity, tracking);

                        if (inverseNavigation != null)
                        {
                            SetNavigation(relatedEntity, inverseNavigation, entity, tracking);
                        }
                    }
                }
            }
            finally
            {
                _changeDetector.Resume();
            }
        }

        private void SetNavigation(object entity, INavigation navigation, object value, bool tracking)
            => SetNavigation(entity, navigation, navigation.GetSetter(), value, tracking);

        private void SetNavigation(object entity, INavigation navigation, IClrPropertySetter setter, object value, bool tracking)
        {
            setter.SetClrValue(entity, value);

            if (tracking)
            {
                _stateManager.TryGetEntry(entity)?.SetRelationshipSnapshotValue(navigation, value);
            }
        }

        private void AddToCollection(object entity, INavigation navigation, object value, bool tracking)
        {
            navigation.GetCollectionAccessor().Add(entity, value);

            if (tracking)
            {
                _stateManager.TryGetEntry(entity)?.AddToCollectionSnapshot(navigation, value);
            }
        }

        private void AddRangeToCollection(object entity, INavigation navigation, IEnumerable<object> values, bool tracking)
        {
            navigation.GetCollectionAccessor().AddRange(entity, values);

            if (tracking)
            {
                _stateManager.TryGetEntry(entity)?.AddRangeToCollectionSnapshot(navigation, values);
            }
        }

        private IWeakReferenceIdentityMap GetOrCreateIdentityMap(IKey key)
        {
            if (_identityMap0 == null)
            {
                _identityMap0 = key.GetWeakReferenceIdentityMapFactory()();
                return _identityMap0;
            }

            if (_identityMap0.Key == key)
            {
                return _identityMap0;
            }

            if (_identityMap1 == null)
            {
                _identityMap1 = key.GetWeakReferenceIdentityMapFactory()();
                return _identityMap1;
            }

            if (_identityMap1.Key == key)
            {
                return _identityMap1;
            }

            if (_identityMaps == null)
            {
                _identityMaps = new Dictionary<IKey, IWeakReferenceIdentityMap>();
            }

            IWeakReferenceIdentityMap identityMap;
            if (!_identityMaps.TryGetValue(key, out identityMap))
            {
                identityMap = key.GetWeakReferenceIdentityMapFactory()();
                _identityMaps[key] = identityMap;
            }
            return identityMap;
        }
    }
}
