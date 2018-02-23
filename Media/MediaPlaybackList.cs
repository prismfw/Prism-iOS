/*
Copyright (C) 2018  Prism Framework Team

This file is part of the Prism Framework.

The Prism Framework is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

The Prism Framework is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
*/


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using AVFoundation;
using CoreMedia;
using Foundation;
using Prism.Native;

namespace Prism.iOS.Media
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeMediaPlaybackList"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeMediaPlaybackList))]
    public class MediaPlaybackList : NSObject, INativeMediaPlaybackList
    {
        /// <summary>
        /// Occurs when the currently playing item has changed.
        /// </summary>
        public event EventHandler<NativeItemChangedEventArgs> CurrentItemChanged;

        /// <summary>
        /// Occurs when a playback item has failed to open.
        /// </summary>
        public event EventHandler<NativeErrorEventArgs> ItemFailed;

        /// <summary>
        /// Occurs when a playback item has been successfully opened.
        /// </summary>
        public event EventHandler<NativeItemEventArgs> ItemOpened;

        /// <summary>
        /// Gets the zero-based index of the current item in the <see cref="Items"/> collection.
        /// </summary>
        public int CurrentItemIndex { get; private set; } = -1;

        /// <summary>
        /// Gets or sets a value indicating whether the playlist should automatically restart after the last item has finished playing.
        /// </summary>
        public bool IsRepeatEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the items in the playlist should be played in random order.
        /// </summary>
        public bool IsShuffleEnabled
        {
            get { return isShuffleEnabled; }
            set
            {
                if (value == isShuffleEnabled)
                {
                    return;
                }

                isShuffleEnabled = value;
                if (isShuffleEnabled)
                {
                    shuffledItems = new List<object>(Items as IEnumerable<object>);

                    var random = new Random();
                    for (int i = shuffledItems.Count - 1; i > 1; i--)
                    {
                        int index = random.Next(i + 1);
                        var item = shuffledItems[index];
                        shuffledItems[index] = shuffledItems[i];
                        shuffledItems[i] = item;
                    }
                }

                var items = isShuffleEnabled ? shuffledItems : Items;
                var player = playerRef?.Target as AVQueuePlayer;
                if (player != null)
                {
                    for (int i = player.Items.Length - 1; i > 0; i--)
                    {
                        player.RemoveItem(player.Items[i]);
                    }

                    AddItemsToQueue(player, items.IndexOf(player.CurrentItem) + 1, false);
                    SetPlayerAction(player);
                }
            }
        }
        private bool isShuffleEnabled;

        /// <summary>
        /// Gets a collection of playback items that make up the playlist.
        /// </summary>
        public IList Items { get; }

        private WeakReference playerRef;
        private IList shuffledItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPlaybackList"/> class.
        /// </summary>
        public MediaPlaybackList()
        {
            Items = new ObservableCollection<object>();
            ((ObservableCollection<object>)Items).CollectionChanged += OnItemsCollectionChanged;

            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onItemDidFinish:"), AVPlayerItem.DidPlayToEndTimeNotification, null);
        }

        /// <summary>
        /// Attachs the specified AVPlayer to the playlist.
        /// </summary>
        /// <param name="player">The player to be attached.</param>
        public void AttachPlayer(AVQueuePlayer player)
        {
            if (player != null && playerRef?.Target == null)
            {
                playerRef = new WeakReference(player);
                player.AddObserver(this, "currentItem", NSKeyValueObservingOptions.OldNew, IntPtr.Zero);

                player.RemoveAllItems();

                var items = isShuffleEnabled ? shuffledItems : Items;
                for (int i = 0; i < items.Count; i++)
                {
                    player.InsertItem(items[i] as AVPlayerItem, null);
                    (items[i] as AVPlayerItem).Asset.LoadValuesAsynchronously(new[] { "playable", "tracks", "duration" }, () => { });
                }

                SetPlayerAction(player);
            }
        }

        /// <summary>
        /// Detachs the specified AVPlayer from the playlist.
        /// </summary>
        /// <param name="player">The player to be detached.</param>
        public void DetachPlayer(AVQueuePlayer player)
        {
            var oldPlayer = playerRef?.Target as AVQueuePlayer;
            if (oldPlayer == player)
            {
                oldPlayer.RemoveObserver(this, "currentItem");
                playerRef = null;
            }
        }

        /// <summary>
        /// Moves to the next item in the playlist.
        /// </summary>
        public void MoveNext()
        {
            var player = playerRef?.Target as AVQueuePlayer;
            if (player != null)
            {
                if (player.Items.Length > 1)
                {
                    player.AdvanceToNextItem();
                }
                else if (player.Items.Length == 1)
                {
                    if (Items.Count == 1)
                    {
                        player.Seek(CMTime.Zero);
                        CurrentItemChanged(this, new NativeItemChangedEventArgs(player.CurrentItem, player.CurrentItem));
                    }
                    else if (Items.Count > 1)
                    {
                        AddItemsToQueue(player, 0, true);
                    }
                }

                SetPlayerAction(player);
            }
        }

        /// <summary>
        /// Moves to the previous item in the playlist.
        /// </summary>
        public void MovePrevious()
        {
            var player = playerRef?.Target as AVQueuePlayer;
            if (player != null)
            {
                if (player.Items.Length == 1 && Items.Count == 1)
                {
                    player.Seek(CMTime.Zero);
                    CurrentItemChanged(this, new NativeItemChangedEventArgs(player.CurrentItem, player.CurrentItem));
                }
                else
                {
                    var items = isShuffleEnabled ? shuffledItems : Items;
                    if (items.Count > 0 && items[0] == player.CurrentItem)
                    {
                        // If we're on the first item in the playlist, jump to the last item.
                        for (int i = player.Items.Length - 2; i >= 0; i--)
                        {
                            player.RemoveItem(player.Items[i]);
                        }
                    }
                    else
                    {
                        ReplaceItemsInQueue(player, items.Count - player.Items.Length - 1);
                    }
                }

                SetPlayerAction(player);
            }
        }

        /// <summary>
        /// Moves to the item in the playlist that is located at the specified index.
        /// </summary>
        /// <param name="itemIndex">The zero-based index of the item to move to.</param>
        public void MoveTo(int itemIndex)
        {
            var player = (playerRef?.Target as AVQueuePlayer);
            if (player == null)
            {
                return;
            }

            var oldItem = Items[CurrentItemIndex];
            if (CurrentItemIndex == itemIndex)
            {
                player.Seek(CMTime.Zero);
                CurrentItemChanged(this, new NativeItemChangedEventArgs(oldItem, oldItem));
                return;
            }

            int oldIndex = -1;
            int newIndex = -1;
            if (isShuffleEnabled)
            {
                var newItem = Items[itemIndex];
                for (int i = 0; i < shuffledItems.Count; i++)
                {
                    var item = shuffledItems[i];
                    if (item == oldItem)
                    {
                        oldIndex = i;
                        if (newIndex >= 0)
                        {
                            break;
                        }
                    }
                    if (item == newItem)
                    {
                        newIndex = i;
                        if (oldIndex >= 0)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                oldIndex = CurrentItemIndex;
                newIndex = itemIndex;
            }

            CurrentItemIndex = itemIndex;

            int indexDiff = newIndex - oldIndex;
            if (indexDiff > 0)
            {
                for (indexDiff -= 1; indexDiff >= 0; indexDiff--)
                {
                    player.RemoveItem(player.Items[indexDiff]);
                }
            }
            else
            {
                ReplaceItemsInQueue(player, newIndex);
            }

            SetPlayerAction(player);
        }

        /// <summary></summary>
        /// <param name="keyPath"></param>
        /// <param name="ofObject"></param>
        /// <param name="change"></param>
        /// <param name="context"></param>
        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            if (keyPath == "currentItem")
            {
                var oldItem = change.ObjectForKey(ChangeOldKey) as AVPlayerItem;
                var newItem = change.ObjectForKey(ChangeNewKey) as AVPlayerItem;

                CurrentItemIndex = Items.IndexOf(newItem);
                CurrentItemChanged(this, new NativeItemChangedEventArgs(oldItem as MediaPlaybackItem, newItem as MediaPlaybackItem));
            }
            else
            {
                var item = ofObject as AVPlayerItem;
                if (item != null)
                {
                    item.RemoveObserver(this, "status");
                    if (item.Status == AVPlayerItemStatus.ReadyToPlay)
                    {
                        ItemOpened(this, new NativeItemEventArgs(item));
                    }
                    else if (item.Status == AVPlayerItemStatus.Failed)
                    {
                        ItemFailed(this, new NativeErrorEventArgs(item, new Exception(item.Error.LocalizedDescription)));
                    }
                }
            }
        }

        /// <summary></summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(this);
            base.Dispose(disposing);
        }

        private void OnItemsCollectionChanged(object o, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                // Not supporting Move actions since they shouldn't ever occur.
                return;
            }

            var player = playerRef?.Target as AVQueuePlayer;
            if (e.OldItems != null)
            {
                for (int i = 0; i < e.OldItems.Count; i++)
                {
                    var oldItem = e.OldItems[i] as AVPlayerItem;
                    if (oldItem != null)
                    {
                        if (oldItem.Status == AVPlayerItemStatus.Unknown)
                        {
                            oldItem.RemoveObserver(this, "status");
                        }
                    }
                }
            }

            if (e.NewItems != null)
            {
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    var newItem = e.NewItems[i] as AVPlayerItem;
                    if (newItem != null && newItem.Status == AVPlayerItemStatus.Unknown)
                    {
                        newItem.AddObserver(this, "status", NSKeyValueObservingOptions.New, IntPtr.Zero);
                    }
                }
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (isShuffleEnabled)
                    {
                        var random = new Random();
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            shuffledItems.Insert(random.Next(0, shuffledItems.Count), e.NewItems[i]);
                        }
                    }

                    if (player != null)
                    {
                        CurrentItemIndex = Items.IndexOf(player.CurrentItem);

                        var items = Items;
                        int currentIndex = CurrentItemIndex;
                        if (isShuffleEnabled)
                        {
                            items = shuffledItems;
                            currentIndex = shuffledItems.IndexOf(player.CurrentItem);
                        }
                        else if (e.NewStartingIndex + e.NewItems.Count <= currentIndex)
                        {
                            // No need to do anything with the queue since all of the new items are before the current one.
                            return;
                        }

                        for (int i = 1; i + currentIndex < items.Count; i++)
                        {
                            var item = (AVPlayerItem)items[currentIndex + i];
                            if (i >= player.Items.Length)
                            {
                                player.InsertItem(item, null);
                            }
                            else if (player.Items[i] != item)
                            {
                                player.InsertItem(item, player.Items[i - 1]);
                            }
                        }

                        SetPlayerAction(player);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (isShuffleEnabled)
                    {
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            shuffledItems.Remove(e.OldItems[i]);
                        }
                    }

                    if (player != null)
                    {
                        CurrentItemIndex = Items.IndexOf(player.CurrentItem);
                        if (!isShuffleEnabled && e.OldStartingIndex + e.OldItems.Count <= CurrentItemIndex)
                        {
                            // No need to do anything with the queue since all of the removed items were before the current one.
                            return;
                        }

                        for (int i = e.OldItems.Count - 1; i >= 0; i--)
                        {
                            var item = (AVPlayerItem)e.OldItems[i];
                            player.RemoveItem(item);
                            if (player.Items.Length == 0)
                            {
                                break;
                            }
                        }

                        SetPlayerAction(player);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (isShuffleEnabled)
                    {
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            int index = shuffledItems.IndexOf(e.OldItems[i]);
                            if (index >= 0)
                            {
                                shuffledItems[index] = e.NewItems[i];
                            }
                        }
                    }

                    if (player != null)
                    {
                        if (isShuffleEnabled)
                        {
                            // Handling a shuffled list isn't as quick since we don't know which of the replaced items are still in the queue.
                            for (int i = 0; i < e.OldItems.Count; i++)
                            {
                                var oldItem = (AVPlayerItem)e.OldItems[i];
                                for (int j = 0; j < player.Items.Length; j++)
                                {
                                    if (player.Items[j] == oldItem)
                                    {
                                        player.InsertItem((AVPlayerItem)e.NewItems[i], oldItem);
                                        player.RemoveItem(oldItem);
                                    }
                                }
                            }
                        }
                        else if (e.OldStartingIndex + e.OldItems.Count > CurrentItemIndex)
                        {
                            // We can make some assumptions when handling an unshuffled list to help make the queue update more efficient.
                            int j = -1;
                            for (int i = 0; i < e.OldItems.Count; i++)
                            {
                                var oldItem = (AVPlayerItem)e.OldItems[i];
                                if (j < 0)
                                {
                                    j = Array.IndexOf(player.Items, oldItem);
                                    if (j < 0)
                                    {
                                        continue;
                                    }
                                }

                                for (; j < player.Items.Length; j++)
                                {
                                    if (player.Items[j] == oldItem)
                                    {
                                        player.InsertItem((AVPlayerItem)e.NewItems[i], oldItem);
                                        player.RemoveItem(oldItem);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    var currentItem = player?.CurrentItem;
                    player?.RemoveAllItems();
                    shuffledItems?.Clear();

                    CurrentItemIndex = -1;
                    if (currentItem != null)
                    {
                        CurrentItemChanged(this, new NativeItemChangedEventArgs(currentItem, null));
                    }
                    break;
            }
        }

        [Export("onItemDidFinish:")]
        private void OnItemDidFinish(NSNotification notification)
        {
            InvokeOnMainThread(() =>
            {
                if (IsRepeatEnabled)
                {
                    var player = playerRef?.Target as AVQueuePlayer;
                    if (player != null && notification.Object == player.CurrentItem &&
                        player.Items.Length == 1 && player.ActionAtItemEnd != AVPlayerActionAtItemEnd.None)
                    {
                        AddItemsToQueue(player, 0, true);
                        SetPlayerAction(player);
                        player.Play();
                    }
                }
            });
        }

        private void AddItemsToQueue(AVQueuePlayer player, int startIndex, bool advanceTo)
        {
            var items = isShuffleEnabled ? shuffledItems : Items;
            if (advanceTo)
            {
                player.InsertItem((AVPlayerItem)items[startIndex], null);
                player.AdvanceToNextItem();

                if (player.Items.Length > 1)
                {
                    for (int i = player.Items.Length - 2; i >= 0; i--)
                    {
                        player.RemoveItem((AVPlayerItem)items[i]);
                    }
                }

                startIndex++;
            }

            for (int i = startIndex; i < items.Count; i++)
            {
                player.InsertItem((AVPlayerItem)items[i], null);
            }
        }

        private void ReplaceItemsInQueue(AVQueuePlayer player, int index)
        {
            var items = isShuffleEnabled ? shuffledItems : Items;
            player.InsertItem((AVPlayerItem)items[index], null);

            for (int i = player.Items.Length - 2; i >= 0; i--)
            {
                player.RemoveItem(player.Items[i]);
            }

            for (int i = index + 1; i < items.Count; i++)
            {
                player.InsertItem((AVPlayerItem)items[i], null);
            }
        }

        private void SetPlayerAction(AVQueuePlayer player)
        {
            // An action of None is assumed to mean that the player is looping the current item.
            if (player.ActionAtItemEnd != AVPlayerActionAtItemEnd.None)
            {
                player.ActionAtItemEnd = player.Items.Length > 1 ? AVPlayerActionAtItemEnd.Advance : AVPlayerActionAtItemEnd.Pause;
            }
        }
    }
}

