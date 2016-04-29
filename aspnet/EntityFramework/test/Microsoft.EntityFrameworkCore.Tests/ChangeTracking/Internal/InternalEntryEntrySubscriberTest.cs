// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.ChangeTracking.Internal
{
    public class InternalEntryEntrySubscriberTest
    {
        [Theory]
        [InlineData(ChangeTrackingStrategy.Snapshot)]
        [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
        public void Original_and_relationship_values_recorded_when_no_changing_notifications(
            ChangeTrackingStrategy changeTrackingStrategy)
        {
            var entry = TestHelpers.Instance.CreateInternalEntry<FullNotificationEntity>(
                BuildModel(changeTrackingStrategy));

            entry.SetEntityState(EntityState.Unchanged);

            Assert.True(entry.HasOriginalValuesSnapshot);
            Assert.True(entry.HasRelationshipSnapshot);
        }

        [Theory]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
        public void Original_and_relationship_values_not_recorded_when_full_notifications(
            ChangeTrackingStrategy changeTrackingStrategy)
        {
            var entry = TestHelpers.Instance.CreateInternalEntry<FullNotificationEntity>(
                BuildModel(changeTrackingStrategy));

            entry.SetEntityState(EntityState.Unchanged);

            Assert.False(entry.HasOriginalValuesSnapshot);
            Assert.False(entry.HasRelationshipSnapshot);
        }

        [Fact]
        public void Notifying_collections_are_not_created_when_snapshot_tracking()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry<FullNotificationEntity>(
                BuildModel(ChangeTrackingStrategy.Snapshot));

            entry.SetEntityState(EntityState.Unchanged);

            Assert.Null(((FullNotificationEntity)entry.Entity).RelatedCollection);
        }

        [Theory]
        [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
        public void Notifying_collections_are_created_when_notification_tracking(
            ChangeTrackingStrategy changeTrackingStrategy)
        {
            var entry = TestHelpers.Instance.CreateInternalEntry<FullNotificationEntity>(
                BuildModel(changeTrackingStrategy));

            entry.SetEntityState(EntityState.Unchanged);

            Assert.IsType<ObservableCollectionWithClear<ChangedOnlyNotificationEntity>>(
                ((FullNotificationEntity)entry.Entity).RelatedCollection);
        }

        [Fact]
        public void Non_notifying_collection_acceptable_when_snapshot_tracking()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry<FullNotificationEntity>(
                BuildModel(ChangeTrackingStrategy.Snapshot));

            var collection = new List<ChangedOnlyNotificationEntity>();
            ((FullNotificationEntity)entry.Entity).RelatedCollection = collection;

            entry.SetEntityState(EntityState.Unchanged);

            Assert.Same(collection, ((FullNotificationEntity)entry.Entity).RelatedCollection);
        }

        [Theory]
        [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
        [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
        public void Non_notifying_collections_not_acceotable_when_noitification_tracking(
            ChangeTrackingStrategy changeTrackingStrategy)
        {
            var entry = TestHelpers.Instance.CreateInternalEntry<FullNotificationEntity>(
                BuildModel(changeTrackingStrategy));

            ((FullNotificationEntity)entry.Entity).RelatedCollection = new List<ChangedOnlyNotificationEntity>();

            Assert.Equal(
                CoreStrings.NonNotifyingCollection("RelatedCollection", "FullNotificationEntity", changeTrackingStrategy),
                Assert.Throws<InvalidOperationException>(
                    () => entry.SetEntityState(EntityState.Unchanged)).Message);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Entry_subscribes_to_INotifyCollectionChanged_for_Add(bool ourCollection)
        {
            var collection = CreateCollection(ourCollection);
            var testListener = SetupTestCollectionListener(collection);

            var item = new ChangedOnlyNotificationEntity();
            collection.Add(item);

            Assert.Equal("RelatedCollection", testListener.CollectionChanged.Single().Item1.Name);
            Assert.Same(item, testListener.CollectionChanged.Single().Item2.Single());
            Assert.Empty(testListener.CollectionChanged.Single().Item3);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Entry_subscribes_to_INotifyCollectionChanged_for_Remove(bool ourCollection)
        {
            var item = new ChangedOnlyNotificationEntity();
            var collection = CreateCollection(ourCollection, item);
            var testListener = SetupTestCollectionListener(collection);

            collection.Remove(item);

            Assert.Equal("RelatedCollection", testListener.CollectionChanged.Single().Item1.Name);
            Assert.Empty(testListener.CollectionChanged.Single().Item2);
            Assert.Same(item, testListener.CollectionChanged.Single().Item3.Single());
        }


        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Entry_subscribes_to_INotifyCollectionChanged_for_Replace(bool ourCollection)
        {
            var item1 = new ChangedOnlyNotificationEntity();
            var collection = CreateCollection(ourCollection, item1);
            var testListener = SetupTestCollectionListener(collection);

            var item2 = new ChangedOnlyNotificationEntity();
            if (ourCollection)
            {
                ((ObservableCollectionWithClear<ChangedOnlyNotificationEntity>)collection)[0] = item2;
            }
            else
            {
                ((ObservableCollection<ChangedOnlyNotificationEntity>)collection)[0] = item2;
            }

            Assert.Equal("RelatedCollection", testListener.CollectionChanged.Single().Item1.Name);
            Assert.Same(item2, testListener.CollectionChanged.Single().Item2.Single());
            Assert.Same(item1, testListener.CollectionChanged.Single().Item3.Single());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Entry_ignores_INotifyCollectionChanged_for_Move(bool ourCollection)
        {
            var item1 = new ChangedOnlyNotificationEntity();
            var item2 = new ChangedOnlyNotificationEntity();
            var collection = CreateCollection(ourCollection, item1, item2);
            var testListener = SetupTestCollectionListener(collection);

            if (ourCollection)
            {
                ((ObservableCollectionWithClear<ChangedOnlyNotificationEntity>)collection).Move(0, 1);
            }
            else
            {
                ((ObservableCollection<ChangedOnlyNotificationEntity>)collection).Move(0, 1);
            }

            Assert.Empty(testListener.CollectionChanged);
        }

        [Fact]
        public void Entry_throws_for_INotifyCollectionChanged_Reset()
        {
            var item1 = new ChangedOnlyNotificationEntity();
            var item2 = new ChangedOnlyNotificationEntity();
            var collection = new ObservableCollection<ChangedOnlyNotificationEntity> { item1, item2 };
            var testListener = SetupTestCollectionListener(collection);

            Assert.Equal(
                CoreStrings.ResetNotSupported,
                Assert.Throws<InvalidOperationException>(() => collection.Clear()).Message);

            Assert.Empty(testListener.CollectionChanged);
        }

        [Fact]
        public void Entry_handles_clear_as_replace_with_ObservableCollectionWithClear()
        {
            var item1 = new ChangedOnlyNotificationEntity();
            var item2 = new ChangedOnlyNotificationEntity();
            var collection = new ObservableCollectionWithClear<ChangedOnlyNotificationEntity> { item1, item2 };
            var testListener = SetupTestCollectionListener(collection);

            collection.Clear();

            Assert.Empty(collection);

            Assert.Equal("RelatedCollection", testListener.CollectionChanged.Single().Item1.Name);
            Assert.Empty(testListener.CollectionChanged.Single().Item2);
            Assert.Same(item1, testListener.CollectionChanged.Single().Item3.First());
            Assert.Same(item2, testListener.CollectionChanged.Single().Item3.Skip(1).Single());
        }

        private static ICollection<ChangedOnlyNotificationEntity> CreateCollection(
            bool ourCollection, params ChangedOnlyNotificationEntity[] items)
            => ourCollection
            ? (ICollection<ChangedOnlyNotificationEntity>)new ObservableCollectionWithClear<ChangedOnlyNotificationEntity>(items)
            : new ObservableCollection<ChangedOnlyNotificationEntity>(items);

        private static TestNavigationListener SetupTestCollectionListener(
            ICollection<ChangedOnlyNotificationEntity> collection)
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<INavigationListener, TestNavigationListener>(),
                BuildModel());

            var testListener = contextServices
                .GetRequiredService<IEnumerable<INavigationListener>>()
                .OfType<TestNavigationListener>()
                .Single();

            var entity = new FullNotificationEntity { RelatedCollection = collection };
            var entry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(entity);
            entry.SetEntityState(EntityState.Unchanged);

            return testListener;
        }

        [Fact]
        public void Entry_subscribes_to_INotifyPropertyChanging_and_INotifyPropertyChanged_for_properties()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<IPropertyListener, TestPropertyListener>(),
                BuildModel());

            var testListener = contextServices.GetRequiredService<IEnumerable<IPropertyListener>>().OfType<TestPropertyListener>().Single();

            var entity = new FullNotificationEntity();
            var entry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(entity);
            entry.SetEntityState(EntityState.Unchanged);

            Assert.Empty(testListener.Changing);
            Assert.Empty(testListener.Changed);

            entity.Name = "Palmer";

            var property = entry.EntityType.FindProperty("Name");
            Assert.Same(property, testListener.Changing.Single());
            Assert.Same(property, testListener.Changed.Single());
        }

        [Fact]
        public void Entry_handles_null_or_empty_string_in_INotifyPropertyChanging_and_INotifyPropertyChanged()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<IPropertyListener, TestPropertyListener>(),
                BuildModel());

            var testListener = contextServices.GetRequiredService<IEnumerable<IPropertyListener>>().OfType<TestPropertyListener>().Single();

            var entity = new FullNotificationEntity();
            var entry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(entity);
            entry.SetEntityState(EntityState.Unchanged);

            Assert.Empty(testListener.Changing);
            Assert.Empty(testListener.Changed);

            entity.NotifyChanging(null);

            Assert.Equal(
                new[] { "Name", "RelatedCollection" },
                testListener.Changing.Select(e => e.Name).OrderBy(e => e).ToArray());

            Assert.Empty(testListener.Changed);

            entity.NotifyChanged("");

            Assert.Equal(
                new[] { "Name", "RelatedCollection" },
                testListener.Changed.Select(e => e.Name).OrderBy(e => e).ToArray());
        }

        [Fact]
        public void Entry_subscribes_to_INotifyPropertyChanging_and_INotifyPropertyChanged_for_navigations()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<IPropertyListener, TestPropertyListener>(),
                BuildModel());

            var testListener = contextServices.GetRequiredService<IEnumerable<IPropertyListener>>().OfType<TestPropertyListener>().Single();

            var entity = new FullNotificationEntity();
            var entry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(entity);
            entry.SetEntityState(EntityState.Unchanged);

            Assert.Empty(testListener.Changing);
            Assert.Empty(testListener.Changed);

            entity.RelatedCollection = new List<ChangedOnlyNotificationEntity>();

            var property = entry.EntityType.FindNavigation("RelatedCollection");
            Assert.Same(property, testListener.Changing.Single());
            Assert.Same(property, testListener.Changed.Single());
        }

        [Fact]
        public void Subscriptions_to_INotifyPropertyChanging_and_INotifyPropertyChanged_ignore_unmapped_properties()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<IPropertyListener, TestPropertyListener>(),
                BuildModel());

            var testListener = contextServices.GetRequiredService<IEnumerable<IPropertyListener>>().OfType<TestPropertyListener>().Single();

            var entity = new FullNotificationEntity();
            contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(entity);

            Assert.Empty(testListener.Changing);
            Assert.Empty(testListener.Changed);

            entity.NotMapped = "Luckey";

            Assert.Empty(testListener.Changing);
            Assert.Empty(testListener.Changed);
        }

        private class TestPropertyListener : IPropertyListener
        {
            public List<IPropertyBase> Changing { get; } = new List<IPropertyBase>();
            public List<IPropertyBase> Changed { get; } = new List<IPropertyBase>();

            public void PropertyChanged(InternalEntityEntry entry, IPropertyBase property, bool setModified)
                => Changed.Add(property);

            public void PropertyChanging(InternalEntityEntry entry, IPropertyBase property)
                => Changing.Add(property);
        }

        private class TestNavigationListener : INavigationListener
        {
            public List<Tuple<INavigation, IEnumerable<object>, IEnumerable<object>>> CollectionChanged { get; }
                = new List<Tuple<INavigation, IEnumerable<object>, IEnumerable<object>>>();

            public void NavigationReferenceChanged(
                InternalEntityEntry entry, INavigation navigation, object oldValue, object newValue)
            {
            }

            public void NavigationCollectionChanged(
                InternalEntityEntry entry, INavigation navigation, IEnumerable<object> added, IEnumerable<object> removed) 
                => CollectionChanged.Add(Tuple.Create(navigation, added, removed));
        }

        private static IModel BuildModel(
            ChangeTrackingStrategy changeTrackingStrategy = ChangeTrackingStrategy.ChangingAndChangedNotifications)
        {
            var builder = TestHelpers.Instance.CreateConventionBuilder();

            builder.Entity<FullNotificationEntity>(b =>
                {
                    b.Ignore(e => e.NotMapped);
                    b.HasMany(e => e.RelatedCollection).WithOne(e => e.Related).HasForeignKey(e => e.Fk);
                    b.HasChangeTrackingStrategy(changeTrackingStrategy);
                });

            return builder.Model;
        }

        private class FullNotificationEntity : INotifyPropertyChanging, INotifyPropertyChanged
        {
            private int _id;
            private string _name;
            private string _notMapped;
            private ICollection<ChangedOnlyNotificationEntity> _relatedCollection;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public string Name
            {
                get { return _name; }
                set { SetWithNotify(value, ref _name); }
            }

            public string NotMapped
            {
                get { return _notMapped; }
                set { SetWithNotify(value, ref _notMapped); }
            }

            public ICollection<ChangedOnlyNotificationEntity> RelatedCollection
            {
                get { return _relatedCollection; }
                set { SetWithNotify(value, ref _relatedCollection); }
            }

            private void SetWithNotify<T>(T value, ref T field, [CallerMemberName] string propertyName = "")
            {
                if (!StructuralComparisons.StructuralEqualityComparer.Equals(field, value))
                {
                    NotifyChanging(propertyName);
                    field = value;
                    NotifyChanged(propertyName);
                }
            }

            public event PropertyChangingEventHandler PropertyChanging;
            public event PropertyChangedEventHandler PropertyChanged;

            public void NotifyChanged(string propertyName)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            public void NotifyChanging(string propertyName)
                => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        private class ChangedOnlyNotificationEntity : INotifyPropertyChanged
        {
            private int _id;
            private string _name;
            private int _fk;
            private FullNotificationEntity _related;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public string Name
            {
                get { return _name; }
                set { SetWithNotify(value, ref _name); }
            }

            public int Fk
            {
                get { return _fk; }
                set { SetWithNotify(value, ref _fk); }
            }

            public FullNotificationEntity Related
            {
                get { return _related; }
                set { SetWithNotify(value, ref _related); }
            }

            private void SetWithNotify<T>(T value, ref T field, [CallerMemberName] string propertyName = "")
            {
                if (!StructuralComparisons.StructuralEqualityComparer.Equals(field, value))
                {
                    field = value;
                    NotifyChanged(propertyName);
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyChanged(string propertyName)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

}
}
