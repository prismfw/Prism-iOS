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
using Foundation;
using Prism.Native;
using Prism.UI;
using UIKit;

namespace Prism.iOS
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeSystemParameters"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeSystemParameters), IsSingleton = true)]
    public class SystemParameters : INativeSystemParameters
    {
        /// <summary>
        /// Gets the default maximum number of items that can be displayed in an action menu before they are placed into an overflow menu.
        /// </summary>
        public int ActionMenuMaxDisplayItems
        {
            get { return 2; }
        }
    
        /// <summary>
        /// Gets the preferred amount of space between the bottom of a UI element and the bottom of its parent.
        /// </summary>
        public double BottomMargin
        {
            get { return 5; }
        }

        /// <summary>
        /// Gets the preferred width of the border around a button.
        /// </summary>
        public double ButtonBorderWidth
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets the preferred amount of padding between a button's content and its edges.
        /// </summary>
        public Thickness ButtonPadding
        {
            get { return new Thickness(); }
        }

        /// <summary>
        /// Gets the preferred width of the border around a date picker.
        /// </summary>
        public double DatePickerBorderWidth
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets the height of a horizontal scroll bar.
        /// </summary>
        public double HorizontalScrollBarHeight
        {
            get { return 6; }
        }

        /// <summary>
        /// Gets the preferred amount of space between the left edge of a UI element and the left edge of its parent.
        /// </summary>
        public double LeftMargin
        {
            get { return 15; }
        }

        /// <summary>
        /// Gets the preferred height of a list box item with a detail text label.
        /// </summary>
        public double ListBoxItemDetailHeight
        {
            get { return 52; }
        }

        /// <summary>
        /// Gets the size of the indicator accessory in a list box item.
        /// </summary>
        public Size ListBoxItemIndicatorSize
        {
            get { return new Size(33, 24); }
        }

        /// <summary>
        /// Gets the size of the info button accessory in a list box item.
        /// </summary>
        public Size ListBoxItemInfoButtonSize
        {
            get { return new Size(46, 34); }
        }

        /// <summary>
        /// Gets the size of the info indicator accessory in a list box item.
        /// </summary>
        public Size ListBoxItemInfoIndicatorSize
        {
            get { return new Size(67, 34); }
        }

        /// <summary>
        /// Gets the preferred height of a standard list box item.
        /// </summary>
        public double ListBoxItemStandardHeight
        {
            get { return 44; }
        }

        /// <summary>
        /// Gets the preferred width of the border around a password box.
        /// </summary>
        public double PasswordBoxBorderWidth
        {
            get { return 0; }
        }
        
        /// <summary>
        /// Gets the size of a popup when presented with the default style.
        /// </summary>
        public Size PopupSize
        {
            get { return new Size(540, 620); }
        }

        /// <summary>
        /// Gets the preferred amount of space between the right edge of a UI element and the right edge of its parent.
        /// </summary>
        public double RightMargin
        {
            get { return 15; }
        }

        /// <summary>
        /// Gets the preferred width of the border around a search box.
        /// </summary>
        public double SearchBoxBorderWidth
        {
            get { return 1; }
        }

        /// <summary>
        /// Gets the preferred width of the border around a select list.
        /// </summary>
        public double SelectListBorderWidth
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets the default amount of padding between a select list's display item and its edges.
        /// </summary>
        public Thickness SelectListDisplayItemPadding
        {
            get { return new Thickness(12, 8, 12, 8); }
        }

        /// <summary>
        /// Gets the default amount of padding between a select list's list items and its edges.
        /// </summary>
        public Thickness SelectListListItemPadding
        {
            get { return new Thickness(18, 10, 0, 10); }
        }

        /// <summary>
        /// Gets a value indicating whether the separator of a list box item should be automatically indented
        /// in order to be flush with the text labels of the item.
        /// </summary>
        public bool ShouldAutomaticallyIndentSeparators
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the preferred width of the border around a text area.
        /// </summary>
        public double TextAreaBorderWidth
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets the preferred width of the border around a text box.
        /// </summary>
        public double TextBoxBorderWidth
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets the preferred width of the border around a time picker.
        /// </summary>
        public double TimePickerBorderWidth
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets the preferred amount of space between the top of a UI element and the top of its parent.
        /// </summary>
        public double TopMargin
        {
            get { return 5; }
        }

        /// <summary>
        /// Gets the width of a vertical scroll bar.
        /// </summary>
        public double VerticalScrollBarWidth
        {
            get { return 6; }
        }

        /// <summary>
        /// Gets the amount that a header is inset on top of the current view of a view stack while in landscape orientation.
        /// </summary>
        public Thickness ViewStackHeaderInsetLandscape
        {
            get { return new Thickness(0, UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone ? 32 : 64, 0, 0); }
        }

        /// <summary>
        /// Gets the amount that a header is inset on top of the current view of a view stack while in portrait orientation.
        /// </summary>
        public Thickness ViewStackHeaderInsetPortrait
        {
            get { return new Thickness(0, 64, 0, 0); }
        }

        /// <summary>
        /// Gets the amount that a header offsets the current view of a view stack while in landscape orientation.
        /// </summary>
        public Thickness ViewStackHeaderOffsetLandscape
        {
            get { return new Thickness(); }
        }

        /// <summary>
        /// Gets the amount that a header offsets the current view of a view stack while in portrait orientation.
        /// </summary>
        public Thickness ViewStackHeaderOffsetPortrait
        {
            get { return new Thickness(); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemParameters"/> class.
        /// </summary>
        public SystemParameters()
        {
        }
    }
}

