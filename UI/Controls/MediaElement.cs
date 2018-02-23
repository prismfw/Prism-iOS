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
using System.Timers;
using AVFoundation;
using AVKit;
using CoreGraphics;
using CoreMedia;
using Foundation;
using Prism.Input;
using Prism.Native;
using Prism.UI;
using UIKit;

namespace Prism.iOS.UI.Controls
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeMediaElement"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeMediaElement))]
    public class MediaElement : UIView, INativeMediaElement
    {
        /// <summary>
        /// Occurs when this instance has been attached to the visual tree and is ready to be rendered.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Occurs when playback of a media source has finished.
        /// </summary>
        public event EventHandler MediaEnded;

        /// <summary>
        /// Occurs when a media source has failed to open.
        /// </summary>
        public event EventHandler<ErrorEventArgs> MediaFailed;

        /// <summary>
        /// Occurs when a media source has been successfully opened.
        /// </summary>
        public event EventHandler MediaOpened;

        /// <summary>
        /// Occurs when the system loses track of the pointer for some reason.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerCanceled;

        /// <summary>
        /// Occurs when the pointer has moved while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerMoved;

        /// <summary>
        /// Occurs when the pointer has been pressed while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerPressed;

        /// <summary>
        /// Occurs when the pointer has been released while over the element.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerReleased;

        /// <summary>
        /// Occurs when the value of a property is changed.
        /// </summary>
        public event EventHandler<FrameworkPropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Occurs when a seek operation has been completed.
        /// </summary>
        public event EventHandler SeekCompleted;

        /// <summary>
        /// Occurs when this instance has been detached from the visual tree.
        /// </summary>
        public event EventHandler Unloaded;

        /// <summary>
        /// Gets or sets a value indicating whether animations are enabled for this instance.
        /// </summary>
        public bool AreAnimationsEnabled
        {
            get { return areAnimationsEnabled; }
            set
            {
                if (value != areAnimationsEnabled)
                {
                    areAnimationsEnabled = value;
                    OnPropertyChanged(Visual.AreAnimationsEnabledProperty);
                }
            }
        }
        private bool areAnimationsEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether to show the default playback controls (play, pause, etc).
        /// </summary>
        public bool ArePlaybackControlsEnabled
        {
            get { return Controller.ShowsPlaybackControls; }
            set
            {
                if (value != Controller.ShowsPlaybackControls)
                {
                    Controller.ShowsPlaybackControls = value;
                    OnPropertyChanged(Prism.UI.Controls.MediaElement.ArePlaybackControlsEnabledProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests an arrangement of its children.
        /// </summary>
        public ArrangeRequestHandler ArrangeRequest { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether playback of a media source should automatically begin once buffering is finished.
        /// </summary>
        public bool AutoPlay
        {
            get { return autoPlay; }
            set
            {
                if (value != autoPlay)
                {
                    autoPlay = value;
                    OnPropertyChanged(Prism.UI.Controls.MediaElement.AutoPlayProperty);
                }
            }
        }
        private bool autoPlay;

        /// <summary>
        /// Gets the amount that the current playback item has buffered as a value between 0.0 and 1.0.
        /// </summary>
        public double BufferingProgress
        {
            get { return bufferingProgress; }
            private set
            {
                value = double.IsNaN(value) || double.IsInfinity(value) ? 0 : value;
                if (value != bufferingProgress)
                {
                    bufferingProgress = value;
                    OnPropertyChanged(Prism.UI.Controls.MediaElement.BufferingProgressProperty);
                }
            }
        }
        private double bufferingProgress;

        /// <summary>
        /// Gets the duration of the current playback item.
        /// </summary>
        public TimeSpan Duration
        {
            get { return duration; }
            private set
            {
                if (value != duration)
                {
                    duration = value;
                    OnPropertyChanged(Prism.UI.Controls.MediaElement.DurationProperty);
                }
            }
        }
        private TimeSpan duration;

        /// <summary>
        /// Gets or sets a <see cref="Rectangle"/> that represents the size and position of the element relative to its parent container.
        /// </summary>
        public new Rectangle Frame
        {
            get { return new Rectangle(Center.X - (Bounds.Width / 2), Center.Y - (Bounds.Height / 2), Bounds.Width, Bounds.Height); }
            set
            {
                Bounds = new CGRect(Bounds.Location, value.Size.GetCGSize());
                Center = new CGPoint(value.X + (value.Width / 2), value.Y + (value.Height / 2));

                Controller.View.Frame = base.Frame;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can be considered a valid result for hit testing.
        /// </summary>
        public bool IsHitTestVisible
        {
            get { return UserInteractionEnabled; }
            set
            {
                if (value != UserInteractionEnabled)
                {
                    UserInteractionEnabled = value;
                    OnPropertyChanged(Visual.IsHitTestVisibleProperty);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has been loaded and is ready for rendering.
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current media source will automatically begin playback again once it has finished.
        /// </summary>
        public bool IsLooping
        {
            get { return player.ActionAtItemEnd == AVPlayerActionAtItemEnd.None; }
            set
            {
                if (value != IsLooping)
                {
                    if (value)
                    {
                        player.ActionAtItemEnd = AVPlayerActionAtItemEnd.None;
                    }
                    else
                    {
                        if (player.CurrentItem != null && player.Items.Length > 1)
                        {
                            player.ActionAtItemEnd = AVPlayerActionAtItemEnd.Advance;
                        }
                        else
                        {
                            player.ActionAtItemEnd = AVPlayerActionAtItemEnd.Pause;
                        }
                    }

                    OnPropertyChanged(Prism.UI.Controls.MediaElement.IsLoopingProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the media content is muted.
        /// </summary>
        public bool IsMuted
        {
            get { return player.Muted; }
            set
            {
                if (value != player.Muted)
                {
                    player.Muted = value;
                    OnPropertyChanged(Prism.UI.Controls.MediaElement.IsMutedProperty);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether a playback item is currently playing.
        /// </summary>
        public bool IsPlaying
        {
            get { return isPlaying; }
            private set
            {
                if (value != isPlaying)
                {
                    isPlaying = value;
                    OnPropertyChanged(Prism.UI.Controls.MediaElement.IsPlayingProperty);
                }
            }
        }
        private bool isPlaying;

        /// <summary>
        /// Gets or sets the method to invoke when this instance requests a measurement of itself and its children.
        /// </summary>
        public MeasureRequestHandler MeasureRequest { get; set; }

        /// <summary>
        /// Gets or sets the level of opacity for the element.
        /// </summary>
        public double Opacity
        {
            get { return Alpha; }
            set
            {
                if (value != Alpha)
                {
                    Alpha = (nfloat)value;
                    OnPropertyChanged(Element.OpacityProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a coefficient of the rate at which media content is played back.  A value of 1.0 is a normal playback rate.
        /// </summary>
        public double PlaybackRate
        {
            get { return player.Rate; }
            set
            {
                if (value != player.Rate)
                {
                    player.Rate = (float)value;
                    OnPropertyChanged(Prism.UI.Controls.MediaElement.PlaybackRateProperty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the position of the playback item.
        /// </summary>
        public TimeSpan Position
        {
            get { return player.CurrentTime.IsInvalid ? TimeSpan.Zero : TimeSpan.FromSeconds(player.CurrentTime.Seconds); }
            set
            {
                player.Seek(CMTime.FromSeconds(value.TotalSeconds, 1), (finished) =>
                {
                    if (finished)
                    {
                        SeekCompleted(this, EventArgs.Empty);
                    }
                });
            }
        }

        /// <summary>
        /// Gets or sets transformation information that affects the rendering position of this instance.
        /// </summary>
        public INativeTransform RenderTransform
        {
            get { return renderTransform; }
            set
            {
                if (value != renderTransform)
                {
                    (renderTransform as Media.Transform)?.RemoveView(this);
                    renderTransform = value;
                    (renderTransform as Media.Transform)?.AddView(this);

                    OnPropertyChanged(Visual.RenderTransformProperty);
                }
            }
        }
        private INativeTransform renderTransform;

        /// <summary>
        /// Gets or sets the visual theme that should be used by this instance.
        /// </summary>
        public Theme RequestedTheme { get; set; }

        /// <summary>
        /// Gets or sets the source of the media content to be played.
        /// </summary>
        public object Source
        {
            get { return source; }
            set
            {
                if (value != source)
                {
                    (source as iOS.Media.MediaPlaybackList)?.DetachPlayer(player);

                    source = value;
                    isMediaOpened = false;
                    OnPropertyChanged(Prism.UI.Controls.MediaElement.SourceProperty);

                    player.RemoveAllItems();

                    var item = source as AVPlayerItem;
                    if (item != null)
                    {
                        player.InsertItem(item, null);
                    }
                    else
                    {
                        (source as iOS.Media.MediaPlaybackList)?.AttachPlayer(player);
                    }

                    if (autoPlay && IsLoaded)
                    {
                        Controller.Player.Play();
                    }
                }
            }
        }
        private object source;

        /// <summary>
        /// Gets or sets the manner in which video content is stretched within its allocated space.
        /// </summary>
        public Stretch Stretch
        {
            get { return stretch; }
            set
            {
                if (value != stretch)
                {
                    stretch = value;
                    Controller.VideoGravity = stretch.GetLayerVideoGravity();
                    OnPropertyChanged(Prism.UI.Controls.MediaElement.StretchProperty);
                }
            }
        }
        private Stretch stretch;

        /// <summary>
        /// Gets the size of the video content, or Size.Empty if there is no video content.
        /// </summary>
        public Size VideoSize
        {
            get { return videoSize; }
            private set
            {
                if (value.Width != videoSize.Width || value.Height != videoSize.Height)
                {
                    videoSize = value;
                    OnPropertyChanged(Prism.UI.Controls.MediaElement.VideoSizeProperty);
                }
            }
        }
        private Size videoSize;

        /// <summary>
        /// Gets or sets the display state of the element.
        /// </summary>
        public Visibility Visibility
        {
            get { return visibility; }
            set
            {
                if (value != visibility)
                {
                    visibility = value;
                    Hidden = value != Visibility.Visible;
                    OnPropertyChanged(Element.VisibilityProperty);
                }
            }
        }
        private Visibility visibility;

        /// <summary>
        /// Gets or sets the volume of the media content as a range between 0.0 (silent) and 1.0 (full).
        /// </summary>
        public double Volume
        {
            get { return player.Volume; }
            set
            {
                if (value != player.Volume)
                {
                    player.Volume = (float)value;
                    OnPropertyChanged(Prism.UI.Controls.MediaElement.VolumeProperty);
                }
            }
        }

        /// <summary>
        /// Gets the controller that contains the media player and its playback controls.
        /// </summary>
        protected AVPlayerViewController Controller { get; }

        private readonly Timer bufferingTimer = new Timer(100) { AutoReset = true };
        private readonly AVQueuePlayer player; // needs a strong ref in the managed environment
        private bool isMediaOpened;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaElement"/> class.
        /// </summary>
        public MediaElement()
        {
            MultipleTouchEnabled = true;

            Controller = new AVPlayerViewController();
            Controller.Player = player = new AVQueuePlayer() { ActionAtItemEnd = AVPlayerActionAtItemEnd.Advance };
            Controller.Player.AddObserver(this, "currentItem", NSKeyValueObservingOptions.OldNew, IntPtr.Zero);
            Controller.Player.AddObserver(this, "timeControlStatus", NSKeyValueObservingOptions.New, IntPtr.Zero);

            AddSubview(Controller.View);

            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onItemDidFinish:"), AVPlayerItem.DidPlayToEndTimeNotification, null);

            bufferingTimer.Elapsed += (o, e) =>
            {
                InvokeOnMainThread(() =>
                {
                    if (player.CurrentItem == null)
                    {
                        bufferingTimer.Stop();
                        return;
                    }

                    if (player.CurrentItem.LoadedTimeRanges.Length > 0)
                    {
                        var timeRange = player.CurrentItem.LoadedTimeRanges[player.CurrentItem.LoadedTimeRanges.Length - 1].CMTimeRangeValue;
                        BufferingProgress = (timeRange.Start.Seconds + timeRange.Duration.Seconds) / player.CurrentItem.Duration.Seconds;
                        if (BufferingProgress >= 1)
                        {
                            bufferingTimer.Stop();
                        }
                    }
                });
            };
        }

        /// <summary>
        /// Invalidates the arrangement of this instance's children.
        /// </summary>
        public void InvalidateArrange()
        {
            SetNeedsLayout();
        }

        /// <summary>
        /// Invalidates the measurement of this instance and its children.
        /// </summary>
        public void InvalidateMeasure()
        {
            SetNeedsLayout();
        }

        /// <summary>
        /// Measures the element and returns its desired size.
        /// </summary>
        /// <param name="constraints">The width and height that the element is not allowed to exceed.</param>
        public Size Measure(Size constraints)
        {
            constraints.Width = Math.Min(constraints.Width, nfloat.MaxValue);
            constraints.Height = Math.Min(constraints.Height, nfloat.MaxValue);

            var frame = base.Frame;
            base.Frame = new CGRect(CGPoint.Empty, new CGSize((nfloat)constraints.Width, (nfloat)constraints.Height));
            SizeToFit();

            var size = new Size(Bounds.Width, Bounds.Height);
            base.Frame = frame;
            return new Size(Math.Min(constraints.Width, size.Width), Math.Min(constraints.Height, size.Height));
        }

        /// <summary></summary>
        public override void LayoutSubviews()
        {
            MeasureRequest(false, null);
            ArrangeRequest(false, null);

            base.LayoutSubviews();
        }

        /// <summary></summary>
        public override void MovedToSuperview()
        {
            base.MovedToSuperview();

            if (Superview == null && IsLoaded)
            {
                OnUnloaded();
            }
            else if (Superview != null)
            {
                var parent = this.GetNextResponder<INativeVisual>();
                if (parent == null || parent.IsLoaded)
                {
                    OnLoaded();
                }

                if (autoPlay && player.CurrentItem != null && player.TimeControlStatus == AVPlayerTimeControlStatus.Paused)
                {
                    player.Play();
                }
            }
        }

        /// <summary></summary>
        /// <param name="keyPath"></param>
        /// <param name="ofObject"></param>
        /// <param name="change"></param>
        /// <param name="context"></param>
        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            if (keyPath == Visual.IsLoadedProperty.Name)
            {
                var isloaded = (NSNumber)change.ObjectForKey(ChangeNewKey);
                if (isloaded.BoolValue)
                {
                    OnLoaded();
                }
                else
                {
                    OnUnloaded();
                }
            }
            else if (keyPath == "currentItem")
            {
                var oldItem = change.ObjectForKey(ChangeOldKey) as AVPlayerItem;
                oldItem?.SeekAsync(CMTime.Zero);
                if (oldItem?.Status == AVPlayerItemStatus.Unknown)
                {
                    oldItem.RemoveObserver(this, "status");
                }

                var newItem = change.ObjectForKey(ChangeNewKey) as AVPlayerItem;

                if (player.ActionAtItemEnd != AVPlayerActionAtItemEnd.None)
                {
                    if (newItem != null && player.Items.Length > 1)
                    {
                        player.ActionAtItemEnd = AVPlayerActionAtItemEnd.Advance;
                    }
                    else
                    {
                        player.ActionAtItemEnd = AVPlayerActionAtItemEnd.Pause;
                    }
                }

                if (newItem == null)
                {
                    bufferingTimer.Stop();
                    VideoSize = Size.Empty;
                }
                else
                {
                    if (newItem.LoadedTimeRanges.Length > 0)
                    {
                        var timeRange = newItem.LoadedTimeRanges[newItem.LoadedTimeRanges.Length - 1].CMTimeRangeValue;
                        BufferingProgress = (timeRange.Start.Seconds + timeRange.Duration.Seconds) / newItem.Duration.Seconds;
                        if (BufferingProgress < 1)
                        {
                            bufferingTimer.Start();
                        }
                    }
                    else
                    {
                        BufferingProgress = 0;
                        bufferingTimer.Start();
                    }

                    if (newItem.Status == AVPlayerItemStatus.ReadyToPlay)
                    {
                        Duration = TimeSpan.FromSeconds(newItem.Duration.Seconds);
                        VideoSize = newItem.PresentationSize.GetSize();

                        if (!isMediaOpened)
                        {
                            isMediaOpened = true;
                            MediaOpened(this, EventArgs.Empty);
                        }
                    }
                    else if (newItem.Status == AVPlayerItemStatus.Unknown)
                    {
                        VideoSize = Size.Empty;
                        newItem.AddObserver(this, "status", NSKeyValueObservingOptions.New, IntPtr.Zero);
                    }
                }
            }
            else if (keyPath == "timeControlStatus")
            {
                IsPlaying = AVPlayerTimeControlStatus.Playing == (AVPlayerTimeControlStatus)((NSNumber)change.ObjectForKey(ChangeNewKey)).Int32Value;
            }
            else
            {
                var item = ofObject as AVPlayerItem;
                if (item != null)
                {
                    item.RemoveObserver(this, "status");
                    if (item.Status == AVPlayerItemStatus.ReadyToPlay)
                    {
                        Duration = TimeSpan.FromSeconds(item.Duration.Seconds);
                        VideoSize = item.PresentationSize.GetSize();

                        if (!isMediaOpened)
                        {
                            isMediaOpened = true;
                            MediaOpened(this, EventArgs.Empty);
                        }
                    }
                    else if (item.Status == AVPlayerItemStatus.Failed)
                    {
                        MediaFailed(this, new ErrorEventArgs(new Exception(item.Error.LocalizedDescription)));
                    }
                }
            }
        }

        /// <summary>
        /// Pauses playback of the current media source.
        /// </summary>
        public void PausePlayback()
        {
            player.Pause();
        }

        /// <summary>
        /// Starts or resumes playback of the current media source.
        /// </summary>
        public void StartPlayback()
        {
            player.Play();
        }

        /// <summary>
        /// Stops playback of the current media source.
        /// </summary>
        public void StopPlayback()
        {
            player.Pause();
            player.Seek(CMTime.FromSeconds(0, 1));
        }

        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            foreach (UITouch touch in touches)
            {
                if (touch.View == this)
                {
                    PointerPressed(this, evt.GetPointerEventArgs(touch, this));
                }
            }

            base.TouchesBegan(touches, evt);
        }

        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            foreach (UITouch touch in touches)
            {
                if (touch.View == this)
                {
                    PointerCanceled(this, evt.GetPointerEventArgs(touch, this));
                }
            }

            base.TouchesCancelled(touches, evt);
        }

        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            foreach (UITouch touch in touches)
            {
                if (touch.View == this)
                {
                    PointerReleased(this, evt.GetPointerEventArgs(touch, this));
                }
            }

            base.TouchesEnded(touches, evt);
        }

        /// <summary></summary>
        /// <param name="touches"></param>
        /// <param name="evt"></param>
        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            foreach (UITouch touch in touches)
            {
                if (touch.View == this)
                {
                    PointerMoved(this, evt.GetPointerEventArgs(touch, this));
                }
            }

            base.TouchesMoved(touches, evt);
        }

        /// <summary></summary>
        /// <param name="disposing">.</param>
        protected override void Dispose(bool disposing)
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(this);
            base.Dispose(disposing);
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="pd">A property descriptor describing the property whose value has been changed.</param>
        protected virtual void OnPropertyChanged(PropertyDescriptor pd)
        {
            PropertyChanged(this, new FrameworkPropertyChangedEventArgs(pd));
        }

        [Export("onItemDidFinish:")]
        private void OnItemDidFinish(NSNotification notification)
        {
            InvokeOnMainThread(() =>
            {
                if (notification.Object == player.CurrentItem)
                {
                    if (player.ActionAtItemEnd == AVPlayerActionAtItemEnd.None)
                    {
                        player.Seek(CMTime.Zero);
                        player.Play();
                    }
                    else if (player.Items.Length == 1)
                    {
                        var playlist = source as INativeMediaPlaybackList;
                        if (playlist == null || !playlist.IsRepeatEnabled)
                        {
                            MediaEnded(this, EventArgs.Empty);
                        }
                    }
                }
            });
        }

        private void OnLoaded()
        {
            if (!IsLoaded)
            {
                IsLoaded = true;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Loaded(this, EventArgs.Empty);
            }
        }

        private void OnUnloaded()
        {
            if (IsLoaded)
            {
                IsLoaded = false;
                OnPropertyChanged(Visual.IsLoadedProperty);
                Unloaded(this, EventArgs.Empty);
            }
        }
    }
}

