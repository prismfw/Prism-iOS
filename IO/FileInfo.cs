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
using Prism.IO;
using Prism.Native;

namespace Prism.iOS.IO
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeFileInfo"/>.
    /// </summary>
    [Register(typeof(INativeFileInfo))]
    public class FileInfo : INativeFileInfo
    {
        /// <summary>
        /// Gets or sets the attributes of the file.
        /// </summary>
        public FileAttributes Attributes
        {
            get { return (FileAttributes)info.Attributes; }
            set { info.Attributes = (System.IO.FileAttributes)value; }
        }

        /// <summary>
        /// Gets the date and time that the file was created.
        /// </summary>
        public DateTime CreationTime
        {
            get { return info.CreationTime; }
        }

        /// <summary>
        /// Gets the date and time, in coordinated universal time (UTC), that the file was created.
        /// </summary>
        public DateTime CreationTimeUtc
        {
            get { return info.CreationTimeUtc; }
        }

        /// <summary>
        /// Gets the directory in which the file exists.
        /// </summary>
        public INativeDirectoryInfo Directory
        {
            get { return new DirectoryInfo(info.Directory); }
        }

        /// <summary>
        /// Gets a value indicating whether the file exists.
        /// </summary>
        public bool Exists
        {
            get { return info.Exists; }
        }

        /// <summary>
        /// Gets the extension of the file.
        /// </summary>
        public string Extension
        {
            get { return info.Extension; }
        }

        /// <summary>
        /// Gets a value indicating whether the file is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return info.IsReadOnly; }
        }

        /// <summary>
        /// Gets the date and time that the file was last accessed.
        /// </summary>
        public DateTime LastAccessTime
        {
            get { return info.LastAccessTime; }
        }

        /// <summary>
        /// Gets the date and time, in coordinated universal time (UTC), that the file was last accessed.
        /// </summary>
        public DateTime LastAccessTimeUtc
        {
            get { return info.LastAccessTimeUtc; }
        }

        /// <summary>
        /// Gets the date and time that the file was last modified.
        /// </summary>
        public DateTime LastWriteTime
        {
            get { return info.LastWriteTime; }
        }

        /// <summary>
        /// Gets the date and time, in coordinated universal time (UTC), that the file was last modified.
        /// </summary>
        public DateTime LastWriteTimeUtc
        {
            get { return info.LastWriteTimeUtc; }
        }

        /// <summary>
        /// Gets the size of the file, in bytes.
        /// </summary>
        public long Length
        {
            get { return info.Length; }
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        public string Name
        {
            get { return info.Name; }
        }

        /// <summary>
        /// Gets the full path to the file.
        /// </summary>
        public string Path
        {
            get { return info.FullName; }
        }

        private readonly System.IO.FileInfo info;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileInfo"/> class.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        public FileInfo(string filePath)
        {
            info = new System.IO.FileInfo(filePath);
        }

        internal FileInfo(System.IO.FileInfo info)
        {
            this.info = info;
        }
    }
}
