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
using Prism.UI.Media;
using UIKit;

namespace Prism.iOS.UI.Media
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeFonts"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeFonts), IsSingleton = true)]
    public class Fonts : INativeFonts
    {
        /// <summary>
        /// Gets the preferred font size for a button.
        /// </summary>
        public double ButtonFontSize => UIFont.ButtonFontSize;

        /// <summary>
        /// Gets the preferred font style for a button.
        /// </summary>
        public FontStyle ButtonFontStyle => UIFont.SystemFontOfSize(UIFont.ButtonFontSize).GetFontStyle();

        /// <summary>
        /// Gets the preferred font size for a date picker.
        /// </summary>
        public double DatePickerFontSize => UIFont.PreferredBody.PointSize;

        /// <summary>
        /// Gets the preferred font style for a date picker.
        /// </summary>
        public FontStyle DatePickerFontStyle => UIFont.PreferredBody.GetFontStyle();

        /// <summary>
        /// Gets the default font family for UI elements that do not have a font family preference.
        /// </summary>
        public Prism.UI.Media.FontFamily DefaultFontFamily { get; } = new Prism.UI.Media.FontFamily(UIFont.PreferredBody.FamilyName);

        /// <summary>
        /// Gets the preferred font size for the detail label of a list box item.
        /// </summary>
        public double DetailLabelFontSize => UIFont.PreferredCaption1.PointSize;

        /// <summary>
        /// Gets the preferred font style for the detail label of a list box item.
        /// </summary>
        public FontStyle DetailLabelFontStyle => UIFont.PreferredCaption1.GetFontStyle();

        /// <summary>
        /// Gets the preferred font size for a section footer in a list box that uses a grouped style.
        /// </summary>
        public double GroupedSectionFooterFontSize => UIFont.PreferredFootnote.PointSize;

        /// <summary>
        /// Gets the preferred font style for a section footer in a list box that uses a grouped style.
        /// </summary>
        public FontStyle GroupedSectionFooterFontStyle => UIFont.PreferredFootnote.GetFontStyle();

        /// <summary>
        /// Gets the preferred font size for a section header in a list box that uses a grouped style.
        /// </summary>
        public double GroupedSectionHeaderFontSize => UIFont.PreferredFootnote.PointSize;

        /// <summary>
        /// Gets the preferred font style for a section header in a list box that uses a grouped style.
        /// </summary>
        public FontStyle GroupedSectionHeaderFontStyle => UIFont.PreferredFootnote.GetFontStyle();

        /// <summary>
        /// Gets the preferred font size for the header of a view.
        /// </summary>
        public double HeaderFontSize => UIFont.PreferredHeadline.PointSize;

        /// <summary>
        /// Gets the preferred font style for the header of a view.
        /// </summary>
        public FontStyle HeaderFontStyle => UIFont.PreferredHeadline.GetFontStyle();

        /// <summary>
        /// Gets the preferred font size for the title text of a load indicator.
        /// </summary>
        public double LoadIndicatorFontSize => UIFont.PreferredBody.PointSize;

        /// <summary>
        /// Gets the preferred font style for the title text of a load indicator.
        /// </summary>
        public FontStyle LoadIndicatorFontStyle => UIFont.PreferredBody.GetFontStyle();

        /// <summary>
        /// Gets the preferred font size for a search box.
        /// </summary>
        public double SearchBoxFontSize => UIFont.SystemFontSize;

        /// <summary>
        /// Gets the preferred font style for a search box.
        /// </summary>
        public FontStyle SearchBoxFontStyle => UIFont.SystemFontOfSize(UIFont.SystemFontSize).GetFontStyle();

        /// <summary>
        /// Gets the preferred font size for a section footer in a list box that uses the default style.
        /// </summary>
        public double SectionFooterFontSize => UIFont.PreferredHeadline.PointSize;

        /// <summary>
        /// Gets the preferred font style for a section footer in a list box that uses the default style.
        /// </summary>
        public FontStyle SectionFooterFontStyle => UIFont.PreferredHeadline.GetFontStyle();

        /// <summary>
        /// Gets the preferred font size for a section header in a list box that uses the default style.
        /// </summary>
        public double SectionHeaderFontSize => UIFont.PreferredHeadline.PointSize;

        /// <summary>
        /// Gets the preferred font style for a section header in a list box that uses the default style.
        /// </summary>
        public FontStyle SectionHeaderFontStyle => UIFont.PreferredHeadline.GetFontStyle();

        /// <summary>
        /// Gets the preferred font size for the display item of a select list.
        /// </summary>
        public double SelectListFontSize => UIFont.PreferredBody.PointSize;

        /// <summary>
        /// Gets the preferred font style for the display item of a select list.
        /// </summary>
        public FontStyle SelectListFontStyle => UIFont.PreferredBody.GetFontStyle();

        /// <summary>
        /// Gets the preferred font size for a standard text label.
        /// </summary>
        public double StandardLabelFontSize => UIFont.PreferredBody.PointSize;

        /// <summary>
        /// Gets the preferred font style for a standard text label.
        /// </summary>
        public FontStyle StandardLabelFontStyle => UIFont.PreferredBody.GetFontStyle();

        /// <summary>
        /// Gets the preferred font size for a tab item.
        /// </summary>
        public double TabItemFontSize => 10;

        /// <summary>
        /// Gets the preferred font style for a tab item.
        /// </summary>
        public FontStyle TabItemFontStyle => UIFont.SystemFontOfSize(10).GetFontStyle();

        /// <summary>
        /// Gets the preferred font size for a text area.
        /// </summary>
        public double TextAreaFontSize => UIFont.LabelFontSize;

        /// <summary>
        /// Gets the preferred font style for a text area.
        /// </summary>
        public FontStyle TextAreaFontStyle => UIFont.SystemFontOfSize(UIFont.LabelFontSize).GetFontStyle();

        /// <summary>
        /// Gets the preferred font size for a text box.
        /// </summary>
        public double TextBoxFontSize => UIFont.LabelFontSize;

        /// <summary>
        /// Gets the preferred font style for a text box.
        /// </summary>
        public FontStyle TextBoxFontStyle => UIFont.SystemFontOfSize(UIFont.LabelFontSize).GetFontStyle();

        /// <summary>
        /// Gets the preferred font size for a time picker.
        /// </summary>
        public double TimePickerFontSize => UIFont.PreferredBody.PointSize;

        /// <summary>
        /// Gets the preferred font style for a time picker.
        /// </summary>
        public FontStyle TimePickerFontStyle => UIFont.PreferredBody.GetFontStyle();

        /// <summary>
        /// Gets the preferred font size for the value label of a list box item.
        /// </summary>
        public double ValueLabelFontSize => UIFont.PreferredBody.PointSize;

        /// <summary>
        /// Gets the preferred font style for the value label of a list box item.
        /// </summary>
        public FontStyle ValueLabelFontStyle => UIFont.PreferredBody.GetFontStyle();

        /// <summary>
        /// Gets the names of all available fonts.
        /// </summary>
        public string[] GetAvailableFontNames()
        {
            return UIFont.FamilyNames;
        }
    }
}

