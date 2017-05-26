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
using Foundation;
using UIKit;
using Prism.Native;
using Prism.Systems;

namespace Prism.iOS.Systems
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeDevice"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeDevice), IsSingleton = true)]
    public class Device : NSObject, INativeDevice
    {
        /// <summary>
        /// Occurs when the battery level of the device has changed by at least 1 percent.
        /// </summary>
        public event EventHandler BatteryLevelChanged;

        /// <summary>
        /// Occurs when the orientation of the device has changed.
        /// </summary>
        public event EventHandler OrientationChanged;

        /// <summary>
        /// Occurs when the power source of the device has changed.
        /// </summary>
        public event EventHandler PowerSourceChanged;

        /// <summary>
        /// Gets the battery level of the device as a percentage value between 0 (empty) and 100 (full).
        /// </summary>
        public int BatteryLevel
        {
            get { return (int)(UIDevice.CurrentDevice.BatteryLevel * 100); }
        }

        /// <summary>
        /// Gets the scaling factor of the display monitor.
        /// </summary>
        public double DisplayScale
        {
            get { return UIScreen.MainScreen.Scale; }
        }

        /// <summary>
        /// Gets the form factor of the device on which the application is running.
        /// </summary>
        public FormFactor FormFactor
        {
            get
            {
                switch (UIDevice.CurrentDevice.UserInterfaceIdiom)
                {
                    case UIUserInterfaceIdiom.Pad:
                        return FormFactor.Tablet;
                    case UIUserInterfaceIdiom.Phone:
                        return FormFactor.Phone;
                    default:
                        return FormFactor.Unknown;
                }
            }
        }

        /// <summary>
        /// Gets a unique identifier for the device.
        /// </summary>
        public string Id
        {
            get { return UIDevice.CurrentDevice.IdentifierForVendor.AsString(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the orientation of the device should be monitored.
        /// This affects the ability to read the orientation of the device.
        /// </summary>
        public bool IsOrientationMonitoringEnabled
        {
            get { return UIDevice.CurrentDevice.GeneratesDeviceOrientationNotifications; }
            set
            {
                if (value)
                {
                    UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
                }
                else
                {
                    UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the power state of the device should be monitored.
        /// This affects the ability to read the power source and battery level of the device.
        /// </summary>
        public bool IsPowerMonitoringEnabled
        {
            get { return UIDevice.CurrentDevice.BatteryMonitoringEnabled; }
            set { UIDevice.CurrentDevice.BatteryMonitoringEnabled = value; }
        }

        /// <summary>
        /// Gets the model of the device.
        /// </summary>
        public string Model
        {
            get { return UIDevice.CurrentDevice.Model; }
        }

        /// <summary>
        /// Gets the name of the device.
        /// </summary>
        public string Name
        {
            get { return UIDevice.CurrentDevice.Name; }
        }

        /// <summary>
        /// Gets the operating system that is running on the device.
        /// </summary>
        public Prism.Systems.OperatingSystem OperatingSystem
        {
            get { return Prism.Systems.OperatingSystem.iOS; }
        }

        /// <summary>
        /// Gets the physical orientation of the device.
        /// </summary>
        public DeviceOrientation Orientation
        {
            get
            {
                switch (UIDevice.CurrentDevice.Orientation)
                {
                    case UIDeviceOrientation.Portrait:
                        return DeviceOrientation.PortraitUp;
                    case UIDeviceOrientation.PortraitUpsideDown:
                        return DeviceOrientation.PortraitDown;
                    case UIDeviceOrientation.LandscapeLeft:
                        return DeviceOrientation.LandscapeLeft;
                    case UIDeviceOrientation.LandscapeRight:
                        return DeviceOrientation.LandscapeRight;
                    case UIDeviceOrientation.FaceUp:
                        return DeviceOrientation.FaceUp;
                    case UIDeviceOrientation.FaceDown:
                        return DeviceOrientation.FaceDown;
                    default:
                        return DeviceOrientation.Unknown;
                }
            }
        }

        /// <summary>
        /// Gets the version of the operating system that is running on the device.
        /// </summary>
        public Version OSVersion
        {
            get { return new Version(UIDevice.CurrentDevice.SystemVersion); }
        }

        /// <summary>
        /// Gets the source from which the device is receiving its power.
        /// </summary>
        public PowerSource PowerSource
        {
            get { return UIDevice.CurrentDevice.BatteryState.GetPowerSource(); }
        }

        /// <summary>
        /// Gets the amount of time, in milliseconds, that the system has been awake since it was last restarted.
        /// </summary>
        public long SystemUptime
        {
            get { return (long)(NSProcessInfo.ProcessInfo.SystemUptime * 1000); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class.
        /// </summary>
        public Device()
        {
            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onBatteryLevelChanged:"), UIDevice.BatteryLevelDidChangeNotification, null);
            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onOrientationChanged:"), UIDevice.OrientationDidChangeNotification, null);
            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onPowerSourceChanged:"), UIDevice.BatteryStateDidChangeNotification, null);
        }

        /// <summary></summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, UIDevice.BatteryLevelDidChangeNotification, null);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, UIDevice.OrientationDidChangeNotification, null);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, UIDevice.BatteryStateDidChangeNotification, null);

            base.Dispose(disposing);
        }

        [Export("onBatteryLevelChanged:")]
        private void OnBatteryLevelChanged(NSNotification notification)
        {
            BatteryLevelChanged(this, EventArgs.Empty);
        }

        [Export("onOrientationChanged:")]
        private void OnOrientationChanged(NSNotification notification)
        {
            OrientationChanged(this, EventArgs.Empty);
        }

        [Export("onPowerSourceChanged:")]
        private void OnPowerSourceChanged(NSNotification notification)
        {
            PowerSourceChanged(this, EventArgs.Empty); 
        }
    }
}

