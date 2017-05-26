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
using AVFoundation;
using Foundation;
using Prism.Native;

namespace Prism.iOS.Media
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeAudioPlayer"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeAudioPlayer))]
    public class AudioPlayer : INativeAudioPlayer
    {
        /// <summary>
        /// Occurs when there is an error during loading or playing of the audio track.
        /// </summary>
        public event EventHandler<ErrorEventArgs> AudioFailed;

        /// <summary>
        /// Occurs when buffering of the audio track has finished.
        /// </summary>
        public event EventHandler BufferingEnded;

        /// <summary>
        /// Occurs when buffering of the audio track has begun.
        /// </summary>
        public event EventHandler BufferingStarted;

        /// <summary>
        /// Occurs when playback of the audio track has finished.
        /// </summary>
        public event EventHandler PlaybackEnded;

        /// <summary>
        /// Occurs when playback of the audio track has begun.
        /// </summary>
        public event EventHandler PlaybackStarted;

        /// <summary>
        /// Gets or sets a value indicating whether playback of the audio track should automatically begin once buffering is finished.
        /// </summary>
        public bool AutoPlay { get; set; }

        /// <summary>
        /// Gets the duration of the audio track.
        /// </summary>
        public TimeSpan Duration
        {
            get { return audioPlayer == null ? TimeSpan.MinValue : TimeSpan.FromSeconds(audioPlayer.Duration); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the audio track will automatically begin playing again once it has finished.
        /// </summary>
        public bool IsLooping
        {
            get { return isLooping; }
            set
            {
                isLooping = value;
                if (audioPlayer != null)
                {
                    audioPlayer.NumberOfLoops = isLooping ? -1 : 0;
                }
            }
        }
        private bool isLooping;

        /// <summary>
        /// Gets a value indicating whether the audio track is currently playing.
        /// </summary>
        public bool IsPlaying
        {
            get { return audioPlayer != null && audioPlayer.Playing; }
        }

        /// <summary>
        /// Gets or sets a coefficient of the rate at which the audio track is played back.
        /// </summary>
        public double PlaybackRate
        {
            get { return playbackRate; }
            set
            {
                playbackRate = value;
                if (audioPlayer != null)
                {
                    audioPlayer.Rate = (float)playbackRate;
                }
            }
        }
        private double playbackRate;

        /// <summary>
        /// Gets or sets the position of the audio track.
        /// </summary>
        public TimeSpan Position
        {
            get { return audioPlayer == null ? position : TimeSpan.FromSeconds(audioPlayer.CurrentTime); }
            set
            {
                position = value;
                if (audioPlayer != null)
                {
                    audioPlayer.CurrentTime = position.TotalSeconds;
                }
            }
        }
        private TimeSpan position;

        /// <summary>
        /// Gets or sets the volume of the audio track.
        /// </summary>
        public double Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                if (audioPlayer != null)
                {
                    audioPlayer.Volume = (float)volume;
                }
            }
        }
        private double volume;

        private AVAudioPlayer audioPlayer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioPlayer"/> class.
        /// </summary>
        public AudioPlayer()
        {
        }

        /// <summary>
        /// Loads the audio track from the file at the specified location.
        /// </summary>
        /// <param name="source">The URI of the source file for the audio track.</param>
        public void Open(Uri source)
        {
            if (audioPlayer != null)
            {
                audioPlayer.Delegate = null;
                audioPlayer.Stop();
            }

            try
            {
                NSError error;
                audioPlayer = AVAudioPlayer.FromUrl(!source.IsAbsoluteUri || source.IsFile ?
                    NSUrl.FromFilename(source.OriginalString) : (NSUrl)source, out error);

                if (error != null)
                {
                    OnAudioFailed(new Exception(error.LocalizedDescription));
                    return;
                }

                audioPlayer.Delegate = new AudioPlayerDelegate(this);
                audioPlayer.EnableRate = true;
                audioPlayer.CurrentTime = position.TotalSeconds;
                audioPlayer.NumberOfLoops = isLooping ? -1 : 0;
                audioPlayer.Rate = (float)playbackRate;
                audioPlayer.Volume = (float)volume;
                position = TimeSpan.Zero;

                BufferingStarted(this, EventArgs.Empty);
                if (audioPlayer.PrepareToPlay())
                {
                    BufferingEnded(this, EventArgs.Empty);
                }
            }
            catch (Exception e)
            {
                OnAudioFailed(e);
                return;
            }

            if (AutoPlay)
            {
                Play();
            }
        }

        /// <summary>
        /// Pauses playback of the audio track.
        /// </summary>
        public void Pause()
        {
            audioPlayer?.Pause();
        }

        /// <summary>
        /// Starts or resumes playback of the audio track.
        /// </summary>
        public void Play()
        {
            if ((audioPlayer?.Play()).GetValueOrDefault())
            {
                PlaybackStarted(this, EventArgs.Empty);
            }
        }

        private void OnAudioFailed(Exception e)
        {
            AudioFailed(this, new ErrorEventArgs(e));
        }

        private void OnPlaybackCompleted()
        {
            PlaybackEnded(this, EventArgs.Empty);
            audioPlayer?.PrepareToPlay();
        }

        private class AudioPlayerDelegate : AVAudioPlayerDelegate
        {
            private readonly WeakReference audioPlayer;

            public AudioPlayerDelegate(AudioPlayer player)
            {
                audioPlayer = new WeakReference(player);
            }

            public override void DecoderError(AVAudioPlayer player, NSError error)
            {
                (audioPlayer.Target as AudioPlayer)?.OnAudioFailed(new NSErrorException(error));
            }

            public override void FinishedPlaying(AVAudioPlayer player, bool flag)
            {
                if (flag)
                {
                    (audioPlayer.Target as AudioPlayer)?.OnPlaybackCompleted();
                }
            }
        }
    }
}

