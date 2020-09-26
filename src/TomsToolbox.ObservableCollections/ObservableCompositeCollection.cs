﻿namespace TomsToolbox.ObservableCollections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;

    using JetBrains.Annotations;

    using TomsToolbox.Essentials;

    /// <summary>
    /// Factory methods for the <see cref="ObservableCompositeCollection{T}"/>
    /// </summary>
    public static class ObservableCompositeCollection
    {
        /// <summary>
        /// Create a collection initially containing one single item
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="singleItem">The first single item in the collection</param>
        /// <returns>A new <see cref="ObservableCompositeCollection{T}"/> containing one fixed list with one single item.</returns>
        [NotNull, ItemCanBeNull]
        public static ObservableCompositeCollection<T> FromSingleItem<T>([CanBeNull] T singleItem)
        {
            return new ObservableCompositeCollection<T>(new[] { singleItem });
        }

        /// <summary>
        /// Create a collection initially containing one single item plus one list.
        /// </summary>
        /// <typeparam name="T">The type of the single item.</typeparam>
        /// <typeparam name="TItem">The type of elements in the list.</typeparam>
        /// <param name="singleItem">The first single item in the collection</param>
        /// <param name="list">The list to add after the single item.</param>
        /// <returns>A new <see cref="ObservableCompositeCollection{T}"/> containing one fixed list with the single item plus all items from the list.</returns>
        [NotNull, ItemCanBeNull]
        public static ObservableCompositeCollection<T> FromSingleItemAndList<T, TItem>([CanBeNull] T singleItem, [NotNull, ItemCanBeNull] IList<TItem> list)
            where TItem : T
        {
            return typeof(T) == typeof(TItem)
                ? new ObservableCompositeCollection<T>(new[] { singleItem }, (IList<T>)list)
                : new ObservableCompositeCollection<T>(new[] { singleItem }, list.ObservableCast<T>());
        }

        /// <summary>
        /// Create a collection initially containing one list plus one single item.
        /// </summary>
        /// <typeparam name="T">The type of the single item.</typeparam>
        /// <typeparam name="TItem">The type of elements in the list.</typeparam>
        /// <param name="list">The list to add before the single item.</param>
        /// <param name="singleItem">The last single item in the collection</param>
        /// <returns>A new <see cref="ObservableCompositeCollection{T}"/> containing all items from the list plus one fixed list with the single item at the end.</returns>
        [NotNull, ItemCanBeNull]
        public static ObservableCompositeCollection<T> FromListAndSingleItem<TItem, T>([NotNull, ItemCanBeNull] IList<TItem> list, [CanBeNull] T singleItem)
            where TItem : T
        {
            return typeof(T) == typeof(TItem)
            ? new ObservableCompositeCollection<T>((IList<T>)list, new[] { singleItem })
            : new ObservableCompositeCollection<T>(list.ObservableCast<T>(), new[] { singleItem });
        }
    }

    /// <summary>
    /// View a set of collections as one continuous list.
    /// <para />
    /// Similar to the System.Windows.Data.CompositeCollection, plus:
    /// <list type="bullet"><item>Generic type</item><item>Transparent separation of the real content and the resulting list</item><item>Nestable, i.e. composite collections of composite collections</item></list></summary>
    /// <typeparam name="T">Type of the items in the collection</typeparam>
    /// <inheritdoc cref="ReadOnlyObservableCollection{T}" />
    public sealed class ObservableCompositeCollection<T> : ReadOnlyObservableCollection<T>, IObservableCollection<T>
    {
        [NotNull, ItemNotNull]
        private readonly ContentManager _content;

        /// <summary>
        /// Taking care of the physical content
        /// </summary>
        private class ContentManager : IList<IList<T>>
        {
            // The parts that make up the composite collection
            [NotNull, ItemNotNull]
            private readonly List<IList<T>> _parts = new List<IList<T>>();
            // The composite collection that we manage
            [NotNull, ItemCanBeNull]
            private readonly ObservableCompositeCollection<T> _owner;

            public ContentManager([NotNull, ItemCanBeNull] ObservableCompositeCollection<T> owner)
            {
                _owner = owner;
            }
            private void Parts_CollectionChanged([CanBeNull] object sender, [NotNull] NotifyCollectionChangedEventArgs e)
            {
                // Monitor changes of parts and forward events properly
                // Offset to apply is the sum of all counts of all parts preceding this part
                var offset = _parts.TakeWhile(part => !Equals(part, sender)).Select(part => part?.Count ?? 0).Sum();

                _owner.ContentCollectionChanged(TranslateEventArgs(e, offset));
            }

            [NotNull]
            private static NotifyCollectionChangedEventArgs TranslateEventArgs([NotNull] NotifyCollectionChangedEventArgs e, int offset)
            {
                // Translate given event args by adding the given offset to the starting index

                if (offset == 0)
                    return e; // no offset - nothing to translate

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        return new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, e.NewStartingIndex + offset);

                    case NotifyCollectionChangedAction.Reset:
                        return e;

                    case NotifyCollectionChangedAction.Remove:
                        return new NotifyCollectionChangedEventArgs(e.Action, e.OldItems, e.OldStartingIndex + offset);

                    case NotifyCollectionChangedAction.Move:
                        return new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, e.NewStartingIndex + offset, e.OldStartingIndex + offset);

                    case NotifyCollectionChangedAction.Replace:
                        return new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, e.OldItems, e.OldStartingIndex + offset);

                    default:
                        throw new NotImplementedException();
                }
            }
            #region IList<IList<T>> Members

            public int IndexOf([CanBeNull] IList<T> item)
            {
                return _parts.IndexOf(item);
            }

            public void Insert(int index, [CanBeNull] IList<T> item)
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));
                if (index > _parts.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                // now add this list to the list of all
                _parts.Insert(index, item);

                // If this list implements INotifyCollectionChanged monitor changes so they can be forwarded properly.
                // If it does not implement INotifyCollectionChanged, no notification should be necessary.
                if (item is INotifyCollectionChanged dynamicPart)
                    dynamicPart.CollectionChanged += Parts_CollectionChanged;

                if (item.Count == 0)
                    return;

                // calculate the absolute offset of the first item (= sum of all preceding items in all lists before the new item)
                var offset = _parts.GetRange(0, index).Select(p => p?.Count ?? 0).Sum();
                var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)item, offset);
                _owner.ContentCollectionChanged(args);
            }

            public void RemoveAt(int index)
            {
                var part = _parts[index];
                _parts.RemoveAt(index);

                // If this list implements INotifyCollectionChanged events must be unregistered!
                if (part is INotifyCollectionChanged dynamicPart)
                    dynamicPart.CollectionChanged -= Parts_CollectionChanged;

                if (part.Count == 0)
                    return;

                // calculate the absolute offset of the first item (sum of all items in all lists before the removed item)
                var offset = _parts.GetRange(0, index).Select(p => p?.Count ?? 0).Sum();

                _owner.ContentCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)part, offset));
            }

            [CanBeNull]
            public IList<T> this[int index]
            {
                get => _parts[index];
                set
                {
                    RemoveAt(index);
                    Insert(index, value);
                }
            }

            #endregion

            #region ICollection<IList<T>> Members

            public void Add([CanBeNull] IList<T> item)
            {
                if (item == null)
                    throw new ArgumentException(@"None of the parts may be null!");

                Insert(_parts.Count, item);
            }

            public void Clear()
            {
                while (Count > 0)
                {
                    RemoveAt(0);
                }
            }

            public bool Contains([CanBeNull] IList<T> item)
            {
                return IndexOf(item) != -1;
            }

            public void CopyTo([NotNull] IList<T>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public int Count => _parts.Count;

            public bool IsReadOnly => false;

            public bool Remove([CanBeNull] IList<T> item)
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));

                var index = IndexOf(item);
                if (index == -1)
                    return false;
                RemoveAt(index);
                return true;
            }

            #endregion

            #region IEnumerable<IList> Members

            public IEnumerator<IList<T>> GetEnumerator()
            {
                return _parts.GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _parts.GetEnumerator();
            }

            #endregion
        }

        /// <summary>
        /// Create an empty collection
        /// </summary>
        public ObservableCompositeCollection()
            : base(new ObservableCollection<T>())
        {
            _content = new ContentManager(this);
        }

        /// <summary>
        /// Create a collection initially wrapping a set of lists
        /// </summary>
        /// <param name="parts">The lists to wrap</param>
        /// <exception cref="System.ArgumentException">None of the parts may be null!</exception>
        /// <exception cref="T:System.ArgumentException">None of the parts may be null!</exception>
        public ObservableCompositeCollection([NotNull, ItemNotNull] params IList<T>[] parts)
            : this()
        {
            foreach (var part in parts)
            {
                _content.Add(part);
            }
        }

        /// <summary>
        /// Access to the physical layer of the content
        /// </summary>
        [NotNull, ItemNotNull]
        public IList<IList<T>> Content
        {
            get
            {
                return _content;
            }
        }

        private void ContentCollectionChanged([NotNull] NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var insertionIndex = e.NewStartingIndex;
                    foreach (var item in e.NewItems.Cast<T>())
                    {
                        Items.Insert(insertionIndex++, item);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    var removeIndex = e.OldStartingIndex;
                    for (var k = 0; k < e.OldItems.Count; k++)
                    {
                        Items.RemoveAt(removeIndex);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    var replaceIndex = e.OldStartingIndex;
                    foreach (var item in e.NewItems.Cast<T>())
                    {
                        Items[replaceIndex++] = item;
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    ((ObservableCollection<T>)Items).Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    Items.Clear();
                    Items.AddRange(_content.SelectMany(list => list));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(e));
            }
        }

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public new event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => base.CollectionChanged += value;
            remove => base.CollectionChanged -= value;
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public new event PropertyChangedEventHandler PropertyChanged
        {
            add => base.PropertyChanged += value;
            remove => base.PropertyChanged -= value;
        }
    }
}