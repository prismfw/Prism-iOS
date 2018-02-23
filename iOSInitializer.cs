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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Foundation;
using Prism.Native;
using UIKit;

namespace Prism.iOS
{
    /// <summary>
    /// Represents a platform initializer for iOS.
    /// </summary>
    public sealed class iOSInitializer : PlatformInitializer
    {
        private iOSInitializer()
        {
        }

        /// <summary>
        /// Initializes the platform and loads the specified <see cref="Prism.Application"/> instance.
        /// </summary>
        /// <param name="appInstance">The application instance to be loaded.</param>
        public static void Initialize(Prism.Application appInstance)
        {
            System.Threading.SynchronizationContext.SetSynchronizationContext(System.Threading.SynchronizationContext.Current);

            List<Assembly> appAssemblies = null;
            if (!HasInitialized)
            {
                UI.Window.KeyWindowRef = new Prism.iOS.UI.CoreWindow(UIScreen.MainScreen.Bounds)
                {
                    BackgroundColor = UIColor.White,
                    RootViewController = new UIViewController()
                };
                UI.Window.KeyWindowRef.MakeKeyAndVisible();

                // Any libraries that are not referenced by the entry assembly will be excluded from this list.
                // This is somewhat intentional since the iOS linker apparently strips these assemblies away anyway.
                appAssemblies = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies());

                var exeAssembly = Assembly.GetExecutingAssembly();
                FilterAssemblies(exeAssembly.GetName(), appAssemblies);
                appAssemblies.Insert(0, exeAssembly);
            }

            Initialize(appInstance, appAssemblies?.ToArray());
        }

        private static void FilterAssemblies(AssemblyName name, List<Assembly> loadedAssemblies)
        {
            var assembly = loadedAssemblies.FirstOrDefault(a => a.FullName == name.FullName);
            if (assembly != null)
            {
                loadedAssemblies.Remove(assembly);
                foreach (var refAssembly in assembly.GetReferencedAssemblies())
                {
                    FilterAssemblies(refAssembly, loadedAssemblies);
                }
            }
        }
    }
}

