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


using Foundation;
using Prism.Native;
using Prism.UI.Media;
using UIKit;

namespace Prism.iOS
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeResources"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeResources), IsSingleton = true)]
    public class Resources : INativeResources
    {
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
            var resourceKey = key as ResourceKey;
            if (resourceKey != null)
            {
                switch ((SystemResourceKeyId)resourceKey.Id)
                {
                    case SystemResourceKeyId.ActionMenuMaxDisplayItems:
                        value = 2;
                        return true;
                    case SystemResourceKeyId.ButtonBorderWidth:
                    case SystemResourceKeyId.DateTimePickerBorderWidth:
                    case SystemResourceKeyId.SelectListBorderWidth:
                    case SystemResourceKeyId.TextBoxBorderWidth:
                        value = 0.0;
                        return true;
                    case SystemResourceKeyId.SearchBoxBorderWidth:
                        value = 1.0;
                        return true;
                    case SystemResourceKeyId.ButtonPadding:
                        value = new Thickness(9.5, 3.5);
                        return true;
                    case SystemResourceKeyId.ListBoxItemDetailHeight:
                        value = 52.0;
                        return true;
                    case SystemResourceKeyId.ListBoxItemStandardHeight:
                        value = 44.0;
                        return true;
                    case SystemResourceKeyId.ListBoxItemIndicatorSize:
                        value = new Size(33, 24);
                        return true;
                    case SystemResourceKeyId.ListBoxItemInfoButtonSize:
                        value = new Size(46, 34);
                        return true;
                    case SystemResourceKeyId.ListBoxItemInfoIndicatorSize:
                        value = new Size(67, 34);
                        return true;
                    case SystemResourceKeyId.PopupSize:
                        value = new Size(540, 620);
                        return true;
                    case SystemResourceKeyId.SelectListDisplayItemPadding:
                        value = new Thickness(12, 8, 12, 8);
                        return true;
                    case SystemResourceKeyId.SelectListListItemPadding:
                        value = new Thickness(18, 10, 33, 10);
                        return true;
                    case SystemResourceKeyId.ShouldAutomaticallyIndentSeparators:
                        value = true;
                        return true;
                    case SystemResourceKeyId.HorizontalScrollBarHeight:
                    case SystemResourceKeyId.VerticalScrollBarWidth:
                        value = 6.0;
                        return true;
                    case SystemResourceKeyId.BaseFontFamily:
                        value = new FontFamily(UIFont.PreferredBody.FamilyName);
                        return true;
                    case SystemResourceKeyId.ButtonFontSize:
                        value = (double)UIFont.ButtonFontSize;
                        return true;
                    case SystemResourceKeyId.DateTimePickerFontSize:
                    case SystemResourceKeyId.LabelFontSize:
                    case SystemResourceKeyId.LoadIndicatorFontSize:
                    case SystemResourceKeyId.SelectListFontSize:
                    case SystemResourceKeyId.ValueLabelFontSize:
                        value = (double)UIFont.PreferredBody.PointSize;
                        return true;
                    case SystemResourceKeyId.DetailLabelFontSize:
                        value = (double)UIFont.PreferredCaption1.PointSize;
                        return true;
                    case SystemResourceKeyId.GroupedSectionHeaderFontSize:
                        value = (double)UIFont.PreferredFootnote.PointSize;
                        return true;
                    case SystemResourceKeyId.SearchBoxFontSize:
                        value = (double)UIFont.SystemFontSize;
                        return true;
                    case SystemResourceKeyId.SectionHeaderFontSize:
                        value = (double)UIFont.PreferredSubheadline.PointSize;
                        return true;
                    case SystemResourceKeyId.ViewHeaderFontSize:
                        value = (double)UIFont.PreferredHeadline.PointSize;
                        return true;
                    case SystemResourceKeyId.TabItemFontSize:
                        value = 10.0;
                        return true;
                    case SystemResourceKeyId.TextBoxFontSize:
                        value = (double)UIFont.LabelFontSize;
                        return true;
                    case SystemResourceKeyId.ButtonFontStyle:
                        value = UIFont.SystemFontOfSize(UIFont.ButtonFontSize).GetFontStyle();
                        return true;
                    case SystemResourceKeyId.DateTimePickerFontStyle:
                    case SystemResourceKeyId.LabelFontStyle:
                    case SystemResourceKeyId.LoadIndicatorFontStyle:
                    case SystemResourceKeyId.SelectListFontStyle:
                    case SystemResourceKeyId.ValueLabelFontStyle:
                        value = UIFont.PreferredBody.GetFontStyle();
                        return true;
                    case SystemResourceKeyId.DetailLabelFontStyle:
                        value = UIFont.PreferredCaption1.GetFontStyle();
                        return true;
                    case SystemResourceKeyId.GroupedSectionHeaderFontStyle:
                        value = UIFont.PreferredFootnote.GetFontStyle();
                        return true;
                    case SystemResourceKeyId.SearchBoxFontStyle:
                        value = UIFont.SystemFontOfSize(UIFont.SystemFontSize).GetFontStyle();
                        return true;
                    case SystemResourceKeyId.SectionHeaderFontStyle:
                    case SystemResourceKeyId.ViewHeaderFontStyle:
                        value = UIFont.PreferredHeadline.GetFontStyle();
                        return true;
                    case SystemResourceKeyId.TabItemFontStyle:
                        value = UIFont.SystemFontOfSize(10.0f).GetFontStyle();
                        return true;
                    case SystemResourceKeyId.TextBoxFontStyle:
                        value = UIFont.SystemFontOfSize(UIFont.LabelFontSize).GetFontStyle();
                        return true;
                    case SystemResourceKeyId.AccentBrush:
                    case SystemResourceKeyId.ActionMenuForegroundBrush:
                    case SystemResourceKeyId.ButtonForegroundBrush:
                    case SystemResourceKeyId.ProgressBarForegroundBrush:
                    case SystemResourceKeyId.SliderForegroundBrush:
                    case SystemResourceKeyId.TabViewForegroundBrush:
                        value = new SolidColorBrush(new Prism.UI.Color(0, 128, 255));
                        return true;
                    case SystemResourceKeyId.ActionMenuBackgroundBrush:
                    case SystemResourceKeyId.ButtonBackgroundBrush:
                    case SystemResourceKeyId.DateTimePickerBackgroundBrush:
                    case SystemResourceKeyId.ListBoxBackgroundBrush:
                    case SystemResourceKeyId.ListBoxItemBackgroundBrush:
                    case SystemResourceKeyId.ListBoxItemSelectedBackgroundBrush:
                    case SystemResourceKeyId.SelectListBackgroundBrush:
                    case SystemResourceKeyId.SliderBackgroundBrush: // a bug prevents this from being anything but null
                    case SystemResourceKeyId.SliderThumbBrush:
                    case SystemResourceKeyId.TabViewBackgroundBrush:
                    case SystemResourceKeyId.TextBoxBackgroundBrush:
                    case SystemResourceKeyId.ToggleSwitchBackgroundBrush:
                    case SystemResourceKeyId.ToggleSwitchThumbOnBrush:
                    case SystemResourceKeyId.ToggleSwitchThumbOffBrush:
                    case SystemResourceKeyId.ViewHeaderBackgroundBrush:
                        value = null;
                        return true;
                    case SystemResourceKeyId.ActivityIndicatorForegroundBrush:
                    case SystemResourceKeyId.ButtonBorderBrush:
                    case SystemResourceKeyId.DateTimePickerBorderBrush:
                    case SystemResourceKeyId.SectionHeaderForegroundBrush:
                    case SystemResourceKeyId.SelectListBorderBrush:
                    case SystemResourceKeyId.TextBoxBorderBrush:
                    case SystemResourceKeyId.ToggleSwitchBorderBrush:
                        value = new SolidColorBrush(new Prism.UI.Color(0, 0, 0));
                        return true;
                    case SystemResourceKeyId.DateTimePickerForegroundBrush:
                    case SystemResourceKeyId.LabelForegroundBrush:
                    case SystemResourceKeyId.LoadIndicatorForegroundBrush:
                    case SystemResourceKeyId.SearchBoxForegroundBrush:
                    case SystemResourceKeyId.SelectListForegroundBrush:
                    case SystemResourceKeyId.TextBoxForegroundBrush:
                    case SystemResourceKeyId.ViewHeaderForegroundBrush:
                        value = new SolidColorBrush(UIColor.DarkTextColor.CGColor.GetColor());
                        return true;
                    case SystemResourceKeyId.DetailLabelForegroundBrush:
                    case SystemResourceKeyId.GroupedSectionHeaderForegroundBrush:
                    case SystemResourceKeyId.TabItemForegroundBrush:
                    case SystemResourceKeyId.ValueLabelForegroundBrush:
                        value = new SolidColorBrush(new Prism.UI.Color(128, 128, 128));
                        return true;
                    case SystemResourceKeyId.GroupedListBoxItemBackgroundBrush:
                    case SystemResourceKeyId.LoadIndicatorBackgroundBrush:
                    case SystemResourceKeyId.SearchBoxBackgroundBrush:
                    case SystemResourceKeyId.SelectListListBackgroundBrush:
                    case SystemResourceKeyId.ViewBackgroundBrush:
                        value = new SolidColorBrush(new Prism.UI.Color(255, 255, 255));
                        return true;
                    case SystemResourceKeyId.GroupedSectionHeaderBackgroundBrush:
                        value = new SolidColorBrush(UIColor.GroupTableViewBackgroundColor.CGColor.GetColor());
                        return true;
                    case SystemResourceKeyId.ListBoxSeparatorBrush:
                    case SystemResourceKeyId.SelectListListSeparatorBrush:
                        value = new SolidColorBrush(new Prism.UI.Color(199, 199, 204));
                        return true;
                    case SystemResourceKeyId.ProgressBarBackgroundBrush:
                        value = new SolidColorBrush(new Prism.UI.Color(183, 183, 183));
                        return true;
                    case SystemResourceKeyId.SearchBoxBorderBrush:
                        value = new SolidColorBrush(new Prism.UI.Color(201, 201, 206));
                        return true;
                    case SystemResourceKeyId.SectionHeaderBackgroundBrush:
                        value = new SolidColorBrush(new Prism.UI.Color(247, 247, 247));
                        return true;
                    case SystemResourceKeyId.ToggleSwitchForegroundBrush:
                        value = new SolidColorBrush(new Prism.UI.Color(75, 216, 99));
                        return true;
                }
            }

            value = null;
            return false;
        }
    }
}