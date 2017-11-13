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
using CoreAnimation;
using Foundation;
using Prism.Native;
using UIKit;

namespace Prism.iOS.UI.Media.Animation
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeAnimationClock"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeAnimationClock))]
    public class AnimationClock : INativeAnimationClock
    {
        /// <summary>
        /// Occurs when a new animation frame has begun to signal that animation values should be updated.
        /// </summary>
        public event EventHandler Tick;

        /// <summary>
        /// Gets the amount of time, in milliseconds, that has passed since the last clock tick.
        /// </summary>
        public double DeltaTime { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the clock is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets the total amount of time, in milliseconds, that has passed since the clock was started.
        /// </summary>
        public double TotalTime { get; private set; }

        /// <summary>
        /// Gets the display link that connects the clock to the main run loop.
        /// </summary>
        protected CADisplayLink Link { get; private set; }

        private double deltaTimestamp;
        private double totalTimestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationClock"/> class.
        /// </summary>
        public AnimationClock()
        {
        }

        /// <summary>
        /// Pauses the clock.
        /// </summary>
        public void PauseClock()
        {
            if (Link != null)
            {
                Link.Paused = true;
            }

            IsRunning = false;
        }

        /// <summary>
        /// Resets the clock so that the <see cref="TotalTime"/> is 0.0.
        /// </summary>
        public void ResetClock()
        {
            DeltaTime = (CAAnimation.CurrentMediaTime() - deltaTimestamp) * 1000;
            deltaTimestamp = totalTimestamp = CAAnimation.CurrentMediaTime();
            TotalTime = 0;
            Tick(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sets the preferred number of frames per second for the animation clock.
        /// </summary>
        /// <param name="frameRate">The preferred number of frames per second.</param>
        public void SetPreferredFrameRate(int frameRate)
        {
            if (Link == null)
            {
                CreateDisplayLink();
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                Link.PreferredFramesPerSecond = frameRate == 0 ? 60 : frameRate;
            }
            else
            {
                Link.FrameInterval = frameRate == 0 ? 1 : (60 / frameRate);
            }
        }

        /// <summary>
        /// Starts or resumes the clock.
        /// </summary>
        public void StartClock()
        {
            if (Link == null)
            {
                CreateDisplayLink();
            }

            if (!IsRunning)
            {
                totalTimestamp = CAAnimation.CurrentMediaTime();
                IsRunning = true;

                if (Link.Paused)
                {
                    Link.Paused = false;
                }
                else
                {
                    deltaTimestamp = totalTimestamp;
                    Link.AddToRunLoop(NSRunLoop.Main, NSRunLoop.NSDefaultRunLoopMode);
                }
            }
        }

        /// <summary>
        /// Stops the clock and resets it to the beginning.
        /// </summary>
        public void StopClock()
        {
            Link?.Invalidate();
            Link = null;

            IsRunning = false;
            DeltaTime = (CAAnimation.CurrentMediaTime() - deltaTimestamp) * 1000;
            deltaTimestamp = totalTimestamp = CAAnimation.CurrentMediaTime();
            TotalTime = 0;
            Tick(this, EventArgs.Empty);
        }

        /// <summary>
        /// Generates a new <see cref="CADisplayLink"/> object for the clock.
        /// </summary>
        protected void CreateDisplayLink()
        {
            Link = CADisplayLink.Create(() =>
            {
                DeltaTime = Math.Max(Link.Timestamp - deltaTimestamp, 0) * 1000;
                TotalTime += Math.Max(Link.Timestamp - totalTimestamp, 0) * 1000;
                deltaTimestamp = totalTimestamp = Link.Timestamp;
                Tick(this, EventArgs.Empty);
            });
        }
    }
}