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
using Foundation;
using Prism.IO;
using Prism.Native;

namespace Prism.iOS.IO
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeFileInfo"/> that specifically handles library files.
    /// </summary>
    public class LibraryFileInfo : INativeFileInfo
    {
        /// <summary>
        /// Gets or sets the attributes of the file.
        /// </summary>
        public FileAttributes Attributes
        {
            get { return FileAttributes.ReadOnly; }
            set { }
        }

        /// <summary>
        /// Gets the date and time that the file was created.
        /// </summary>
        public DateTime CreationTime
        {
            get { return CreationTimeUtc.ToLocalTime(); }
        }

        /// <summary>
        /// Gets the date and time, in coordinated universal time (UTC), that the file was created.
        /// </summary>
        public DateTime CreationTimeUtc { get; }

        /// <summary>
        /// Gets the directory in which the file exists.
        /// </summary>
        public INativeDirectoryInfo Directory { get; }

        /// <summary>
        /// Gets a value indicating whether the file exists.
        /// </summary>
        public bool Exists
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the extension of the file.
        /// </summary>
        public string Extension { get; }

        /// <summary>
        /// Gets a value indicating whether the file is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the date and time that the file was last accessed.
        /// </summary>
        public DateTime LastAccessTime
        {
            get { return LastAccessTimeUtc.ToLocalTime(); }
        }

        /// <summary>
        /// Gets the date and time, in coordinated universal time (UTC), that the file was last accessed.
        /// </summary>
        public DateTime LastAccessTimeUtc { get; }

        /// <summary>
        /// Gets the date and time that the file was last modified.
        /// </summary>
        public DateTime LastWriteTime
        {
            get { return LastWriteTimeUtc.ToLocalTime(); }
        }

        /// <summary>
        /// Gets the date and time, in coordinated universal time (UTC), that the file was last modified.
        /// </summary>
        public DateTime LastWriteTimeUtc { get; }

        /// <summary>
        /// Gets the size of the file, in bytes.
        /// </summary>
        public long Length { get; }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the full path to the file.
        /// </summary>
        public string Path { get; }

        internal LibraryFileInfo(INativeDirectoryInfo library, NSUrl url, NSDate addDate, NSDate accessDate, NSDate writeDate)
        {
            CreationTimeUtc = (DateTime)addDate;
            LastAccessTimeUtc = (DateTime)(accessDate ?? writeDate ?? addDate);
            LastWriteTimeUtc = (DateTime)(writeDate ?? addDate);
            Directory = library;
            Extension = System.IO.Path.GetExtension(url.Path);
            Name = string.IsNullOrEmpty(url.PathExtension) ? url.AbsoluteString : System.IO.Path.GetFileName(url.AbsoluteString);
            Path = url.AbsoluteString;
        }
    }
}
