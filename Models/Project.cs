using Hyperion.Platform.Tests.Core.ExedraLib.Models;
using Microsoft.Azure.Devices;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace G4Studio.Models
{

    public class Project
    {
        private string CEnvironment { get; set; }

        public Tenant Tenant { get; set; }

        public long Index { get; set; }

        private static RegistryManager registryManager;
        private static ResourceLoader resourceLoader;

        public Project()
        {
            resourceLoader = ResourceLoader.GetForViewIndependentUse();

            CEnvironment = resourceLoader.GetString("CONFS_ENVIRONMENT");

           
            Index = 0;

            
            registryManager = RegistryManager.CreateFromConnectionString(resourceLoader.GetString("Azure_IoTHub_Connection" + CEnvironment));

        }
    }
}
