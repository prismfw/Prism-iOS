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
using System.Collections.Generic;
using Prism.Native;
using UIKit;

namespace Prism.iOS
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeResources"/>.
    /// </summary>
    [Register(typeof(INativeResources), IsSingleton = true)]
    public class Resources : INativeResources
    {
        private readonly Dictionary<object, object> resourceValues = new Dictionary<object, object>()
        {
            { SystemResources.HorizontalScrollBarHeightKey, 6.0 },
            { SystemResources.ListBoxItemDetailHeightKey, 52.0 },
            { SystemResources.ListBoxItemIndicatorSizeKey, new Size(33, 24) },
            { SystemResources.ListBoxItemInfoButtonSizeKey, new Size(46, 34) },
            { SystemResources.ListBoxItemInfoIndicatorSizeKey, new Size(67, 34) },
            { SystemResources.ListBoxItemStandardHeightKey, 44.0 },
            { SystemResources.PopupSizeKey, new Size(540, 620) },
            { SystemResources.SearchBoxBorderWidthKey, 1.0 },
            { SystemResources.SelectListDisplayItemPaddingKey, new Thickness(12, 8, 12, 8) },
            { SystemResources.SelectListListItemPaddingKey, new Thickness(18, 10, 0, 10) },
            { SystemResources.TabItemFontSizeKey, 10.0 },
            { SystemResources.VerticalScrollBarWidthKey, 6.0 },
        };

        /// <summary>
        /// Gets the names of all available fonts.
        /// </summary>
        public string[] GetAvailableFontNames()
        {
            return UIFont.FamilyNames;
        }

        /// <summary>
        /// Gets the system resource associated with the specified key.
        /// </summary>
        /// <param name="owner">The object that owns the resource, or <c>null</c> if the resource is not owned by a specified object.</param>
        /// <param name="key">The key associated with the resource to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, <c>null</c>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the system resources contain a resource with the specified key; otherwise, <c>false</c>.</returns>
        public bool TryGetResource(object owner, object key, out object value)
        {
            if (resourceValues.TryGetValue(key, out value))
            {
                return true;
            }
            
            if (!(key is ResourceKey))
            {
                return false;
            }
            
            // The following resources are not stored since their values can change during runtime.
            if (key == SystemResources.BaseFontFamilyKey)
            {
                value = new Prism.UI.Media.FontFamily(UIFont.PreferredBody.FamilyName);
            }
            else if (key == SystemResources.BaseFontSizeKey)
            {
                value = (double)UIFont.PreferredBody.PointSize;
            }
            else if (key == SystemResources.BaseFontStyleKey)
            {
                value = UIFont.PreferredBody.GetFontStyle();
            }
            else if (key == SystemResources.DetailLabelFontSizeKey)
            {
                value = (double)UIFont.PreferredCaption1.PointSize;
            }
            else if (key == SystemResources.DetailLabelFontStyleKey)
            {
                value = UIFont.PreferredCaption1.GetFontStyle();
            }
            else if (key == SystemResources.SectionHeaderFontSizeKey)
            {
                value = (double)UIFont.PreferredHeadline.PointSize;
            }
            else if (key == SystemResources.SectionHeaderFontStyleKey)
            {
                value = UIFont.PreferredHeadline.GetFontStyle();
            }
            else if (key == SystemResources.GroupedSectionHeaderFontSizeKey)
            {
                value = (double)UIFont.PreferredFootnote.PointSize;
            }
            else if (key == SystemResources.GroupedSectionHeaderFontStyleKey)
            {
                value = UIFont.PreferredFootnote.GetFontStyle();
            }
            else if (key == SystemResources.TextBoxFontSizeKey)
            {
                value = (double)UIFont.LabelFontSize;
            }
            else if (key == SystemResources.TextBoxFontStyleKey)
            {
                value = UIFont.SystemFontOfSize(UIFont.LabelFontSize).GetFontStyle();
            }
            else if (key == SystemResources.ButtonFontSizeKey)
            {
                value = (double)UIFont.ButtonFontSize;
            }
            else if (key == SystemResources.ButtonFontStyleKey)
            {
                value = UIFont.SystemFontOfSize(UIFont.ButtonFontSize).GetFontStyle();
            }
            else if (key == SystemResources.ViewHeaderFontSizeKey)
            {
                value = (double)UIFont.PreferredHeadline.PointSize;
            }
            else if (key == SystemResources.ViewHeaderFontStyleKey)
            {
                value = UIFont.PreferredHeadline.GetFontStyle();
            }
            else if (key == SystemResources.SearchBoxFontSizeKey)
            {
                value = (double)UIFont.SystemFontSize;
            }
            else if (key == SystemResources.SearchBoxFontStyleKey)
            {
                value = UIFont.SystemFontOfSize(UIFont.SystemFontSize).GetFontStyle();
            }
            else if (key == SystemResources.TabItemFontStyleKey)
            {
                value = UIFont.SystemFontOfSize(10.0f).GetFontStyle();
            }

            return value != null;
        }
    }
}
