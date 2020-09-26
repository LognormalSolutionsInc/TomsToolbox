﻿namespace TomsToolbox.ObservableCollections
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;

    using JetBrains.Annotations;

    using TomsToolbox.Essentials;

    using WeakEventHandler;

    /// <summary>
    /// A read only observable collection of type TTarget where TTarget is a wrapper for type TSource.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the source collection.</typeparam>
    /// <typeparam name="TTarget">The type of elements in the wrapped collection.</typeparam>
    /// <remarks>
    /// This collection does <c>not</c> hold a reference to the source collection.
    /// To keep the source collection alive, the object generating the <see cref="T:TomsToolbox.ObservableCollections.ObservableWrappedCollection`2" /> must hold a reference to the source collection.
    /// </remarks>
    /// <inheritdoc />
    public class ObservableWrappedCollection<TSource, TTarget> : ReadOnlyObservableCollectionAdapter<TTarget, ObservableCollection<TTarget>>
    {
        [NotNull]
        private readonly Func<TSource, TTarget> _itemGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:TomsToolbox.ObservableCollections.ObservableWrappedCollection`2" /> class.
        /// </summary>
        /// <param name="sourceCollection">The source collection to wrap. This instance will not hold a reference to the source collection.</param>
        /// <param name="itemGenerator">The item generator to generate the wrapper for each item.</param>
        public ObservableWrappedCollection([NotNull, ItemNotNull] IEnumerable sourceCollection, [NotNull] Func<TSource, TTarget> itemGenerator)
            : base(new ObservableCollection<TTarget>(sourceCollection.Cast<TSource>().Select(itemGenerator)))
        {
            _itemGenerator = itemGenerator;

            AttachCollectionEvents(sourceCollection as INotifyCollectionChanged);
        }

        /// <summary>
        /// Gets the item generator used to generate the wrapper for each item.
        /// </summary>
        [NotNull]
        protected Func<TSource, TTarget> ItemGenerator => _itemGenerator;

        private void AttachCollectionEvents([CanBeNull] INotifyCollectionChanged? sender)
        {
            if (sender == null)
                return;

            sender.CollectionChanged += SourceCollection_CollectionChanged;
        }

        [MakeWeak]
        private void SourceCollection_CollectionChanged([NotNull] object sender, [NotNull] NotifyCollectionChangedEventArgs e)
        {
            OnSourceCollectionChanged((IEnumerable)sender, e);
        }

        /// <summary>
        /// Called when the source collection has changed.
        /// </summary>
        /// <param name="sourceCollection">The source collection.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.ArgumentException">Event source must provide index!</exception>
        /// <exception cref="System.NotImplementedException">Moving more than one item is not supported</exception>
        protected virtual void OnSourceCollectionChanged([NotNull, ItemCanBeNull] IEnumerable sourceCollection, [NotNull] NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var insertionIndex = e.NewStartingIndex;
                    foreach (var item in e.NewItems.Cast<TSource>())
                    {
                        Items.Insert(insertionIndex++, ItemGenerator(item));
                    }
                    break;


                case NotifyCollectionChangedAction.Move:
                    // Exactly one item....
                    Items.Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;


                case NotifyCollectionChangedAction.Remove:
                    var removeIndex = e.OldStartingIndex;
                    for (var k = 0; k < e.OldItems.Count; k++)
                    {
                        Items.RemoveAt(removeIndex);
                    }
                    break;


                case NotifyCollectionChangedAction.Replace:
                    var replaceIndex = e.NewStartingIndex;
                    foreach (var item in e.NewItems)
                    {
                        Items[replaceIndex++] = ItemGenerator((TSource)item);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    Items.Clear();
                    Items.AddRange(sourceCollection.Cast<TSource>().Select(ItemGenerator));
                    break;
            }
        }

        /// <summary>
        /// Occurs when the collection has changed.
        /// </summary>
        public new event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => base.CollectionChanged += value;
            remove => base.CollectionChanged -= value;
        }

        /// <summary>
        /// Occurs when a property has changed.
        /// </summary>
        public new event PropertyChangedEventHandler PropertyChanged
        {
            add => base.PropertyChanged += value;
            remove => base.PropertyChanged -= value;
        }

/*
        // ReSharper disable once EmptyDestructor
        /// <inheritdoc />
        ~ObservableWrappedCollection()
        {

        }
*/
    }
}