// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Editor.Packages;
using RealityCollective.ServiceFramework.Services;
using RealityToolkit.Boundary.Definitions;
using System.Linq;
using UnityEditor;

namespace RealityToolkit.Boundary.Editor
{
    /// <summary>
    /// Installs <see cref="IBoundaryServiceModule"/>s coming from a third party package
    /// into the <see cref="BoundaryServiceProfile"/> in the <see cref="ServiceManager.ActiveProfile"/>.
    /// </summary>
    [InitializeOnLoad]
    public sealed class BoundaryPackageModulesInstaller : IPackageModulesInstaller
    {
        /// <summary>
        /// Static initializer for the installer instance.
        /// </summary>
        static BoundaryPackageModulesInstaller()
        {
            if (Instance == null)
            {
                Instance = new BoundaryPackageModulesInstaller();
            }

            PackageInstaller.RegisterModulesInstaller(Instance);
        }

        /// <summary>
        /// Internal singleton instance of the installer.
        /// </summary>
        private static BoundaryPackageModulesInstaller Instance { get; }

        /// <inheritdoc/>
        public bool Install(ServiceConfiguration serviceConfiguration)
        {
            if (!typeof(IBoundaryServiceModule).IsAssignableFrom(serviceConfiguration.InstancedType.Type))
            {
                // This module installer does not accept the configuration type.
                return false;
            }

            if (!ServiceManager.IsActiveAndInitialized)
            {
                UnityEngine.Debug.LogWarning($"Could not install {serviceConfiguration.InstancedType.Type.Name}.{nameof(ServiceManager)} is not initialized.");
                return false;
            }

            if (!ServiceManager.Instance.HasActiveProfile)
            {
                UnityEngine.Debug.LogWarning($"Could not install {serviceConfiguration.InstancedType.Type.Name}.{nameof(ServiceManager)} has no active profile.");
                return false;
            }

            if (!ServiceManager.Instance.TryGetServiceProfile<IBoundaryService, BoundaryServiceProfile>(out var BoundaryProfile))
            {
                UnityEngine.Debug.LogWarning($"Could not install {serviceConfiguration.InstancedType.Type.Name}.{nameof(BoundaryProfile)} not found.");
                return false;
            }

            // Setup the configuration.
            var typedServiceConfiguration = new ServiceConfiguration<IBoundaryServiceModule>(serviceConfiguration.InstancedType.Type, serviceConfiguration.Name, serviceConfiguration.Priority, serviceConfiguration.RuntimePlatforms, serviceConfiguration.Profile);

            // Make sure it is not already in the target profile.
            if (BoundaryProfile.ServiceConfigurations.All(sc => sc.InstancedType.Type != serviceConfiguration.InstancedType.Type))
            {
                BoundaryProfile.AddConfiguration(typedServiceConfiguration);
                UnityEngine.Debug.Log($"Successfully installed the {serviceConfiguration.InstancedType.Type.Name} to {BoundaryProfile.name}.");
            }
            else
            {
                UnityEngine.Debug.Log($"Skipped installing the {serviceConfiguration.InstancedType.Type.Name} to {BoundaryProfile.name}. Already installed.");
            }

            return true;
        }
    }
}
