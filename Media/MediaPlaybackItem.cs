/*
Copyright (C) 2017  Prism Framework Team

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
using System.Collections.ObjectModel;
using AVFoundation;
using Foundation;
using Prism.Media;
using Prism.Native;

namespace Prism.iOS.Media
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeMediaPlaybackItem"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeMediaPlaybackItem))]
    public class MediaPlaybackItem : AVPlayerItem, INativeMediaPlaybackItem
    {
        /// <summary>
        /// Gets the duration of the playback item.
        /// </summary>
        public new TimeSpan Duration
        {
            get { return base.Duration.IsIndefinite ? TimeSpan.Zero : TimeSpan.FromSeconds(base.Duration.Seconds); }
        }

        /// <summary>
        /// Gets a collection of the individual media tracks that contain the playback data.
        /// </summary>
        public new ReadOnlyCollection<MediaTrack> Tracks
        {
            get
            {
                if (tracks == null || tracks.Count != Asset.Tracks.Length)
                {
                    var trackArray = new MediaTrack[Asset.Tracks.Length];
                    for (int i = 0; i < trackArray.Length; i++)
                    {
                        var avTrack = Asset.Tracks[i];
                        MediaTrackType type;
                        if (avTrack.MediaType == AVMediaType.Audio)
                        {
                            type = MediaTrackType.Audio;
                        }
                        else if (avTrack.MediaType == AVMediaType.Video)
                        {
                            type = MediaTrackType.Video;
                        }
                        else if (avTrack.MediaType == AVMediaType.TimedMetadata)
                        {
                            type = MediaTrackType.TimedMetadata;
                        }
                        else if (Enum.IsDefined(typeof(AVMediaTypes), avTrack.MediaType))
                        {
                            type = MediaTrackType.Other;
                        }
                        else
                        {
                            type = MediaTrackType.Unknown;
                        }

                        trackArray[i] = new MediaTrack(avTrack.TrackID.ToString(), type, avTrack.LanguageCode);
                    }

                    tracks = new ReadOnlyCollection<MediaTrack>(trackArray);
                }

                return tracks;
            }
        }
        private ReadOnlyCollection<MediaTrack> tracks;

        /// <summary>
        /// Gets the URI of the playback item.
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPlaybackItem"/> class.
        /// </summary>
        /// <param name="uri">The URI of the playback item..</param>
        public MediaPlaybackItem(Uri uri)
            : base(!uri.IsAbsoluteUri || uri.IsFile ? NSUrl.FromFilename(uri.OriginalString) : (NSUrl)uri)
        {
            Uri = uri;
        }
    }
}

