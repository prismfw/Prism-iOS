/*
Copyright (C) 2016  Prism Framework Team

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
using System.Linq;
using UIKit;
using Prism.Native;

namespace Prism.iOS.UI.Controls
{
    internal class TabItemCollection : IList
    {
        public int Count
        {
            get { return parent.ViewControllers == null ? 0 : parent.ViewControllers.Count(vc => vc.TabBarItem is INativeTabItem); }
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return null; }
        }

        public object this[int index]
        {
            get
            {
                if (parent.ViewControllers == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return parent.ViewControllers.Where(vc => vc.TabBarItem is INativeTabItem).ElementAt(index).TabBarItem;
            }
            set
            {
                if (parent.ViewControllers == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                var item = value as UITabBarItem;
                if (item == null)
                {
                    throw new ArgumentException("Value must be an object of type UITabBarItem.", nameof(value));
                }

                var controllers = new UIViewController[parent.ViewControllers.Length];
                parent.ViewControllers.CopyTo(controllers, 0);

                var controller = parent.ViewControllers.Where(vc => vc.TabBarItem is INativeTabItem).ElementAt(index);
                for (int i = 0; i < controllers.Length; i++)
                {
                    if (controllers[i] == controller)
                    {
                        var tabItem = (INativeTabItem)value;
                        controller = tabItem.Content as UIViewController ?? new UIViewController();
                        controller.TabBarItem = item;
                        controllers[i] = controller;

                        parent.SetViewControllers(controllers, ShouldAnimateChanges);
                        return;
                    }
                }
            }
        }

        private bool ShouldAnimateChanges
        {
            get
            {
                var tabView = (parent as INativeTabView) ?? parent.GetNextResponder<INativeTabView>();
                return tabView == null ? false : tabView.AreAnimationsEnabled;
            }
        }
        private readonly UITabBarController parent;
        
        public TabItemCollection(UITabBarController parent)
        {
            this.parent = parent;
        }

        public int Add(object value)
        {
            var item = value as UITabBarItem;
            if (item == null)
            {
                throw new ArgumentException("Value must be an object of type UITabBarItem.", nameof(value));
            }

            int count = parent.ViewControllers == null ? 0 : parent.ViewControllers.Length;

            var controllers = new UIViewController[count + 1];
            if (count > 0)
            {
                parent.ViewControllers.CopyTo(controllers, 0);
            }

            var tabItem = (INativeTabItem)value;
            var controller = tabItem.Content as UIViewController ?? new UIViewController();
            controller.TabBarItem = item;

            controllers[count] = controller;
            parent.SetViewControllers(controllers, ShouldAnimateChanges);

            return parent.ViewControllers.Length - count;
        }

        public void Clear()
        {
            parent.SetViewControllers(new UIViewController[0], ShouldAnimateChanges);
        }

        public bool Contains(object value)
        {
            return parent.TabBar.Items.Any(i => i == value);
        }

        public int IndexOf(object value)
        {
            int index = 0;
            foreach (var tabItem in parent.ViewControllers.Select(vc => vc.TabBarItem).OfType<INativeTabItem>())
            {
                if (tabItem == value)
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        public void Insert(int index, object value)
        {
            var item = value as UITabBarItem;
            if (item == null)
            {
                throw new ArgumentException("Value must be an object of type UITabBarItem.", nameof(value));
            }

            if (index == Count)
            {
                Add(value);
            }
            else
            {
                int currentIndex = 0;
                for (int i = 0; i < parent.ViewControllers.Length; i++)
                {
                    if (parent.ViewControllers[i].TabBarItem is INativeTabItem && currentIndex++ == index)
                    {
                        var tabItem = item as INativeTabItem;
                        var controller = tabItem == null ? new UIViewController() : tabItem.Content as UIViewController ?? new UIViewController();
                        controller.TabBarItem = (UITabBarItem)tabItem;

                        var controllers = new List<UIViewController>(parent.ViewControllers);
                        controllers.Insert(i, controller);
                        parent.SetViewControllers(controllers.ToArray(), ShouldAnimateChanges);

                        return;
                    }
                }
            }
        }

        public void Remove(object value)
        {
            var item = value as UITabBarItem;
            if (item != null)
            {
                var controllers = new List<UIViewController>(parent.ViewControllers);
                if (controllers.RemoveAll(vc => vc.TabBarItem == item) > 0)
                {
                    parent.SetViewControllers(controllers.ToArray(), ShouldAnimateChanges);
                }
            }
        }

        public void RemoveAt(int index)
        {
            int currentIndex = 0;
            var controllers = new List<UIViewController>(parent.ViewControllers);
            for (int i = 0; i < controllers.Count; i++)
            {
                if (controllers[i].TabBarItem is INativeTabItem && currentIndex++ == index)
                {
                    controllers.RemoveAt(i);
                    parent.SetViewControllers(controllers.ToArray(), ShouldAnimateChanges);
                    return;
                }
            }
        }

        public void CopyTo(Array array, int index)
        {
            parent.ViewControllers.Select(vc => vc.TabBarItem).OfType<INativeTabItem>().ToArray().CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return new TabItemEnumerator(parent.ViewControllers.Select(vc => vc.TabBarItem).GetEnumerator());
        }

        private class TabItemEnumerator : IEnumerator<INativeTabItem>, IEnumerator
        {
            public INativeTabItem Current
            {
                get { return tabItemEnumerator.Current as INativeTabItem; }
            }

            object IEnumerator.Current
            {
                get { return tabItemEnumerator.Current; }
            }

            private readonly IEnumerator tabItemEnumerator;

            public TabItemEnumerator(IEnumerator tabItemEnumerator)
            {
                this.tabItemEnumerator = tabItemEnumerator;
            }

            public void Dispose()
            {
                var disposable = tabItemEnumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }

            public bool MoveNext()
            {
                do
                {
                    if (!tabItemEnumerator.MoveNext())
                    {
                        return false;
                    }
                }
                while (!(tabItemEnumerator.Current is INativeTabItem));

                return true;
            }

            public void Reset()
            {
                tabItemEnumerator.Reset();
            }
        }
    }
}

