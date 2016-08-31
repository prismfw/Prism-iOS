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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreLocation;
using Foundation;
using Prism.Native;
using Prism.Systems.Geolocation;

namespace Prism.iOS.Systems.Geolocation
{
    /// <summary>
    /// Represents an iOS implementation of an <see cref="INativeGeolocator"/>.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Register(typeof(INativeGeolocator), IsSingleton = true)]
    public class Geolocator : CLLocationManager, INativeGeolocator
    {
        /// <summary>
        /// Occurs when the location is updated.
        /// </summary>
        public event EventHandler<GeolocationUpdatedEventArgs> LocationUpdated;

        /// <summary>
        /// Gets or sets the desired level of accuracy when reading geographic coordinates.
        /// </summary>
        public new GeolocationAccuracy DesiredAccuracy
        {
            get { return base.DesiredAccuracy > CLLocation.AccuracyHundredMeters ? GeolocationAccuracy.Approximate : GeolocationAccuracy.Precise; }
            set { base.DesiredAccuracy = value == GeolocationAccuracy.Precise ? CLLocation.AccuracyBest : CLLocation.AccuracyKilometer; }
        }

        /// <summary>
        /// Gets or sets the minimum distance, in meters, that should be covered before the location is updated again.
        /// </summary>
        public double DistanceThreshold
        {
            get { return distanceThreshold; }
            set
            {
                distanceThreshold = value;
                AllowDeferredLocationUpdatesUntil(double.IsNaN(distanceThreshold) ? CLLocationDistance.MaxDistance : distanceThreshold,
                    double.IsNaN(updateInterval) ? MaxTimeInterval : updateInterval / 1000);
            }
        }
        private double distanceThreshold = double.NaN;

        /// <summary>
        /// Gets or sets the amount of time, in milliseconds, that should pass before the location is updated again.
        /// </summary>
        public double UpdateInterval
        {
            get { return updateInterval; }
            set
            {
                updateInterval = value;
                AllowDeferredLocationUpdatesUntil(double.IsNaN(distanceThreshold) ? CLLocationDistance.MaxDistance : distanceThreshold,
                    double.IsNaN(updateInterval) ? MaxTimeInterval : updateInterval / 1000);
            }
        }
        private double updateInterval = double.NaN;
        
        private readonly ManualResetEventSlim authorizer = new ManualResetEventSlim(false);
        private ManualResetEventSlim retriever;
        private CLHeading lastHeading;
        private CLLocation lastLocation;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Geolocator"/> class.
        /// </summary>
        public Geolocator()
        {
            AllowsBackgroundLocationUpdates = true;
            Delegate = new GeolocatorDelegate();
        }

        /// <summary>
        /// Signals the geolocation service to begin listening for location updates.
        /// </summary>
        public void BeginLocationUpdates()
        {
            StartUpdatingLocation();
            StartUpdatingHeading();
        }

        /// <summary>
        /// Signals the geolocation service to stop listening for location updates.
        /// </summary>
        public void EndLocationUpdates()
        {
            StopUpdatingLocation();
            StopUpdatingHeading();
        }

        /// <summary>
        /// Makes a singular request to the geolocation service for the current location.
        /// </summary>
        /// <returns>A <see cref="Coordinate"/> representing the current location.</returns>
        public Task<Coordinate> GetCoordinateAsync()
        {
            return Task.Run(() =>
            {
                retriever = new ManualResetEventSlim(false);
                if (UIKit.UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
                {
                    RequestLocation();
                }
                else
                {
                    StartUpdatingLocation();
                }
                
                retriever?.Wait();
                
                if (!UIKit.UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
                {
                    StopUpdatingLocation();
                }
                
                return BuildCoordinate(lastLocation, lastHeading);
            });
        }

        /// <summary>
        /// Requests access to the device's geolocation service.
        /// </summary>
        /// <returns><c>true</c> if access is granted; otherwise, <c>false</c>.</returns>
        public Task<bool> RequestAccessAsync()
        {
            return Task.Run(() =>
            {
                if (Status == CLAuthorizationStatus.Denied || Status == CLAuthorizationStatus.Restricted)
                {
                    return false;
                }
                
                authorizer.Reset();
                if (NSBundle.MainBundle.ObjectForInfoDictionary("NSLocationAlwaysUseDescription") != null && Status != CLAuthorizationStatus.AuthorizedAlways)
                {
                    RequestAlwaysAuthorization();
                }
                else if (Status == CLAuthorizationStatus.NotDetermined)
                {
                    RequestWhenInUseAuthorization();
                }
                else
                {
                    return true;
                }
                
                authorizer.Wait(Timeout.Infinite);
                return (Status == CLAuthorizationStatus.AuthorizedAlways || Status == CLAuthorizationStatus.AuthorizedWhenInUse) && LocationServicesEnabled;
            });
        }
        
        private Coordinate BuildCoordinate(CLLocation location, CLHeading heading)
        {
            if (location == null)
            {
                return null;
            }
            
            return new Coordinate(((DateTime)location.Timestamp).ToLocalTime(), location.Coordinate.Latitude, location.Coordinate.Longitude,
                location.Altitude, heading?.TrueHeading, location.Speed < 0 ? null : (double?)location.Speed,
                location.HorizontalAccuracy < 0 ? null : (double?)location.HorizontalAccuracy,
                location.VerticalAccuracy < 0 ? null : (double?)location.VerticalAccuracy);
        }
        
        private void OnLocationUpdated(CLLocation location, CLHeading heading)
        {
            if (location != null)
            {
                lastLocation = location;
            }
            if (heading != null)
            {
                lastHeading = heading;
            }
            
            if (lastLocation == null || (lastHeading == null && HeadingAvailable))
            {
                return;
            }
            
            if (retriever != null)
            {
                retriever.Set();
                retriever = null;
                return;
            }
            
            LocationUpdated(this, new GeolocationUpdatedEventArgs(BuildCoordinate(lastLocation, lastHeading)));
            
            lastLocation = null;
            lastHeading = null;
        }
        
        private class GeolocatorDelegate : CLLocationManagerDelegate
        {
            public override void AuthorizationChanged(CLLocationManager manager, CLAuthorizationStatus status)
            {
                if (status != CLAuthorizationStatus.NotDetermined)
                {
                    ((Geolocator)manager).authorizer?.Set();
                }
            }
            
            public override void Failed (CLLocationManager manager, NSError error)
            {
                Prism.Utilities.Logger.Error("Error encountered in Geolocator: " + error.LocalizedDescription);
                
                var geolocator = (Geolocator)manager;
                geolocator.lastLocation = null;
                geolocator.retriever?.Set();
            }
            
            public override void LocationsUpdated(CLLocationManager manager, CLLocation[] locations)
            {
                var geolocator = (Geolocator)manager;
                geolocator.OnLocationUpdated(locations.LastOrDefault(), null);
                geolocator.AllowDeferredLocationUpdatesUntil(double.IsNaN(geolocator.distanceThreshold) ? CLLocationDistance.MaxDistance : geolocator.distanceThreshold,
                    double.IsNaN(geolocator.updateInterval) ? CLLocationManager.MaxTimeInterval : geolocator.updateInterval / 1000);
            }
            
            public override void UpdatedHeading(CLLocationManager manager, CLHeading newHeading)
            {
                ((Geolocator)manager).OnLocationUpdated(null, newHeading);
            }
        }
    }
}

