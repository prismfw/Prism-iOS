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
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using MediaPlayer;
using Photos;
using Prism.IO;
using Prism.Native;

namespace Prism.iOS.IO
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeDirectoryInfo"/> that specifically handles system libraries.
    /// </summary>
    public class LibraryDirectoryInfo : INativeDirectoryInfo
    {
        /// <summary>
        /// Gets or sets the attributes of the directory.
        /// </summary>
        public FileAttributes Attributes
        {
            get { return FileAttributes.Directory; }
            set { }
        }

        /// <summary>
        /// Gets the date and time that the directory was created.
        /// </summary>
        public DateTime CreationTime
        {
            get { return CreationTimeUtc.ToLocalTime(); }
        }

        /// <summary>
        /// Gets the date and time, in coordinated universal time (UTC), that the directory was created.
        /// </summary>
        public DateTime CreationTimeUtc
        {
            get { return new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc); }
        }

        /// <summary>
        /// Gets a value indicating whether the directory exists.
        /// </summary>
        public bool Exists
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the date and time that the directory was last accessed.
        /// </summary>
        public DateTime LastAccessTime
        {
            get { return CreationTimeUtc.ToLocalTime(); }
        }

        /// <summary>
        /// Gets the date and time, in coordinated universal time (UTC), that the directory was last accessed.
        /// </summary>
        public DateTime LastAccessTimeUtc
        {
            get { return CreationTimeUtc; }
        }

        /// <summary>
        /// Gets the date and time that the directory was last modified.
        /// </summary>
        public DateTime LastWriteTime
        {
            get { return CreationTimeUtc.ToLocalTime(); }
        }

        /// <summary>
        /// Gets the date and time, in coordinated universal time (UTC), that the directory was last modified.
        /// </summary>
        public DateTime LastWriteTimeUtc
        {
            get { return CreationTimeUtc; }
        }

        /// <summary>
        /// Gets the name of the directory.
        /// </summary>
        public string Name
        {
            get { return Enum.GetName(typeof(LibraryType), library); }
        }

        /// <summary>
        /// Gets the full path to the directory.
        /// </summary>
        public string Path
        {
            get { return string.Empty; }
        }
        
        private readonly LibraryType library;

        internal LibraryDirectoryInfo(LibraryType library)
        {
            this.library = library;
        }

        /// <summary>
        /// Gets information about the subdirectories within the current directory,
        /// optionally getting information about directories in any subdirectories as well.
        /// </summary>
        /// <param name="searchOption">A value indicating whether to search subdirectories or just the top directory.</param>
        /// <returns>An <see cref="Array"/> containing the directory information.</returns>
        public Task<INativeDirectoryInfo[]> GetDirectoriesAsync(SearchOption searchOption)
        {
            return Task.FromResult(new INativeDirectoryInfo[0]);
        }

        /// <summary>
        /// Gets information about the files in the current directory,
        /// optionally getting information about files in any subdirectories as well.
        /// </summary>
        /// <param name="searchOption">A value indicating whether to search subdirectories or just the top directory.</param>
        /// <returns>An <see cref="Array"/> containing the file information.</returns>
        public Task<INativeFileInfo[]> GetFilesAsync(SearchOption searchOption)
        {
            return Task.Run(async () =>
            {
                if (library == LibraryType.Photos)
                {
                    if ((await PHPhotoLibrary.RequestAuthorizationAsync()) != PHAuthorizationStatus.Authorized)
                    {
                        return new INativeFileInfo[0];
                    }

                    var assets = PHAsset.FetchAssets(PHAssetMediaType.Image, new PHFetchOptions()
                    {
                        IncludeAssetSourceTypes = PHAssetSourceType.UserLibrary | PHAssetSourceType.iTunesSynced
                    });

                    return assets.OfType<PHAsset>().Select(a => new LibraryFileInfo(this,
                        NSUrl.FromString(a.LocalIdentifier), a.CreationDate, null, a.ModificationDate)).ToArray();
                }
                else
                {
                    return new MPMediaQuery((NSSet)null).Items.Where(i => (library == LibraryType.Videos) ?
                        MPMediaType.TypeAnyVideo.HasFlag(i.MediaType) : MPMediaType.AnyAudio.HasFlag(i.MediaType))
                        .Select(i => new LibraryFileInfo(this, i.AssetURL, i.DateAdded, i.LastPlayedDate, null)).ToArray();
                }
            });
        }

        /// <summary>
        /// Gets information about the parent directory in which the current directory exists.
        /// </summary>
        /// <returns>The directory information.</returns>
        public Task<INativeDirectoryInfo> GetParentAsync()
        {
            return Task.FromResult<INativeDirectoryInfo>(null);
        }
    }

    internal enum LibraryType
    {
        Music,
        Photos,
        Videos
    }
}
