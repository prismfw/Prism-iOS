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


using CoreMotion;

namespace Prism.iOS.Systems.Sensors
{
    /// <summary>
    /// Represents the base class for sensors.
    /// </summary>
    public abstract class Sensor
    {
        /// <summary>
        /// Gets the <see cref="CMMotionManager"/> instance that manages the sensor data for the device.
        /// </summary>
        protected static CMMotionManager MotionManager { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sensor"/> class.
        /// </summary>
        public Sensor()
        {
            if (MotionManager == null)
            {
                MotionManager = new CMMotionManager();
            }
        }
    }
}
