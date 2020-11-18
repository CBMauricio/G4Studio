using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;

namespace G4Studio.Models
{
    public class Properties
    {
        [JsonProperty("fill-color")]
        public string fillColor { get; set; }

        [JsonProperty("fill-opacity")]
        public double fillOpacity { get; set; }
    }

    [JsonConverter(typeof(CoordinateConverter))]
    public class Coordinate
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
    }

    public class CoordinateConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType.Name.Equals("Coordinate", StringComparison.InvariantCulture);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JArray array = JArray.Load(reader);

            return new Coordinate
            {
                latitude = array[1].ToObject<double>(),
                longitude = array[0].ToObject<double>()
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var defaultLatitude = 0;
            var defaultLongitude = 0;

            var coordinate = value as Coordinate;
            JArray arra = new JArray();

            if (coordinate != null)
            {
                arra.Add(coordinate.latitude);
                arra.Add(coordinate.longitude);
            }
            else
            {
                arra.Add(defaultLatitude);
                arra.Add(defaultLongitude);
            }
            
            arra.WriteTo(writer);
        }
    }

    public class Geometry
    {
        public string type { get; set; }
        public List<List<Coordinate>> coordinates { get; set; }
    }

    public class Fence
    {
        public Geometry geometry { get; set; }
        public Properties properties { get; set; }
    }

    public class Project
    {
        private string CEnvironment { get; set; }
        public string id { get; set; }
        public Fence fence { get; set; }
        public string timezone { get; set; }
        public string group { get; set; }
        public string name { get; set; }
        public string parent { get; set; }
        public List<string> hostnames { get; set; }

        public bool TwinsLoaded { get; set; }

        public List<BasicGeoposition> Coordinates { get; private set; }
        //public List<Twin> Items { get; private set; }
        public List<Device> Devices { get; set; }
        public List<G3GatewayTableEntity> TableStorageEntities { get; private set; }
        public List<SecretItem> KeyVaultEntities { get; private set; }
        public long Count { get; set; }
        public long Index { get; set; }

        private static RegistryManager registryManager;
        private static ResourceLoader resourceLoader;

        public event RoutedEventHandler TwinDeleted;

        private CloudStorageAccount StorageAccount { get; set; }
        private CloudTableClient DevicesCloudTableClient { get; set; }
        private CloudTable DevicesStorageTable { get; set; }

        private string KEYVAULT_APP_CLIENT_ID { get; set; }
        private string KEYVAULT_APP_CLIENT_SECRET { get; set; }
        private string KEYVAULT_BASE_URI { get; set; }

        private KeyVaultClient KeyVaultC { get; set; }

        public Project()
        {
            resourceLoader = ResourceLoader.GetForCurrentView();

            CEnvironment = resourceLoader.GetString("CONFS_ENVIRONMENT");

            KEYVAULT_APP_CLIENT_ID = resourceLoader.GetString("KeyVault_App_ID" + CEnvironment);
            KEYVAULT_APP_CLIENT_SECRET = resourceLoader.GetString("KeyVault_App_SECRET" + CEnvironment);
            KEYVAULT_BASE_URI = resourceLoader.GetString("KeyVault_BaseURI" + CEnvironment);

            KeyVaultC = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(
                   async (string authority, string resource, string scope) =>
                   {
                       var authContext = new AuthenticationContext(authority);
                       var credential = new ClientCredential(KEYVAULT_APP_CLIENT_ID, KEYVAULT_APP_CLIENT_SECRET);

#pragma warning disable CS0618 // Type or member is obsolete
                       AuthenticationResult result = await authContext.AcquireTokenAsync(resource, credential).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete

                       if (result == null)
                       {
                           throw new InvalidOperationException("Failed to retrieve JWT token");
                       }

                       return result.AccessToken;
                   }
               ));


            TwinsLoaded = false;
            Coordinates = new List<BasicGeoposition>();

            //Items = new List<Twin>();
            Devices = new List<Device>();

            TableStorageEntities = new List<G3GatewayTableEntity>();
            KeyVaultEntities = new List<SecretItem>();
            Count = 0;
            Index = 0;

            
            registryManager = RegistryManager.CreateFromConnectionString(resourceLoader.GetString("Azure_IoTHub_Connection" + CEnvironment));

            StorageAccount = new CloudStorageAccount(new StorageCredentials(resourceLoader.GetString("TableStorage_AccountName" + CEnvironment), resourceLoader.GetString("TableStorage_KeyValue" + CEnvironment)), true);

            DevicesCloudTableClient = StorageAccount.CreateCloudTableClient();
            DevicesStorageTable = DevicesCloudTableClient.GetTableReference(resourceLoader.GetString("TableStorage_TableName" + CEnvironment));
        }

        public void SetCoordinates()
        {
            foreach (var item in fence.geometry.coordinates[0])
            {
                Coordinates.Add(new BasicGeoposition() { Latitude = item.latitude, Longitude = item.longitude });
            }
        }

        public async Task GetNTwins()
        {
            try
            {
                //string strQuery = "SELECT COUNT() AS NDevices FROM devices WHERE tags.tenant = '" + name + "'";
                string strQuery = string.Format(CultureInfo.InvariantCulture, "SELECT COUNT() AS NDevices FROM devices WHERE tags.tenant = '{0}' or STARTSWITH(deviceId,'{1}')", name, name.Substring(0, 3).ToUpper(CultureInfo.InvariantCulture));

                IQuery query = registryManager.CreateQuery(strQuery);

                string json = (await query.GetNextAsJsonAsync().ConfigureAwait(false)).FirstOrDefault();
                Dictionary<string, long> data = JsonConvert.DeserializeObject<Dictionary<string, long>>(json);

                Count = data["NDevices"];
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Project.cs -> GetNTwins() -> " + ex.Message);
                    
                Count = 0;
            }
        }

        public async Task LoadTwins()
        {
            string strQuery = string.Format(CultureInfo.InvariantCulture, resourceLoader.GetString("QUERY_GetDevices_ByTenant"), name, name.Substring(0, 3).ToUpper(CultureInfo.InvariantCulture));

            IQuery query = registryManager.CreateQuery(strQuery, 10000);

            Devices = new List<Device>();

            while (query.HasMoreResults)
            {
                var result = await query.GetNextAsJsonAsync().ConfigureAwait(false);

                foreach (var item in result)
                {
                    Devices.Add(JsonConvert.DeserializeObject<Device>(item));
                }
            }

            TwinsLoaded = true;            
        }

        //public async Task LoadTwinsV2()
        //{
        //    DateTime start = DateTime.Now;

        //    string strQuery = string.Format(CultureInfo.InvariantCulture, resourceLoader.GetString("QUERY_GetDevices_ByTenant_V2"), name, name.Substring(0, 3).ToUpper(CultureInfo.InvariantCulture));

        //    IQuery query = registryManager.CreateQuery(strQuery, 10000);

        //    Items = new List<Twin>();

        //    while (query.HasMoreResults)
        //    {
        //        var page = await query.GetNextAsTwinAsync().ConfigureAwait(false);

        //        foreach (var twin in page)
        //        {
        //            Items.Add(twin);
        //        }

        //        Count = Items.Count;

        //        Debug.WriteLine("RESULTS " + Count);
        //    }

        //    DateTime end = DateTime.Now;

        //    TimeSpan elapsed = end - start;

        //    Debug.WriteLine("QUERY 2 -> " + elapsed.TotalSeconds);



        //    TwinsLoaded = true;
        //}

        public async Task DeleteTwinsBulk()
        {
            Index = 0;

            List<Microsoft.Azure.Devices.Device> Devices = new List<Microsoft.Azure.Devices.Device>();

            string prefix = name.Substring(0, 3).ToUpper(CultureInfo.InvariantCulture);

            //string strQuery = "SELECT * FROM devices WHERE tags.tenant = '" + name + "'";
            string strQuery = "SELECT * FROM devices WHERE tags.tenant = '" + name + "' OR STARTSWITH(deviceId,'" + prefix + "')";
            //string strQuery = "SELECT * FROM devices WHERE STARTSWITH(deviceId, 'SCH') OR STARTSWITH(deviceId, 'NOV') OR STARTSWITH(deviceId, 'CAN') OR STARTSWITH(deviceId, 'ALG') OR STARTSWITH(deviceId, 'CAR') OR STARTSWITH(deviceId, 'SYD')";


            IQuery query = registryManager.CreateQuery(strQuery, 10000);

            DateTime initDate = DateTime.Now;
            

            int skip = 0;
            int skipInc = 100;


            while (query.HasMoreResults)
            {
                var page = await query.GetNextAsTwinAsync().ConfigureAwait(false);

                foreach (var twin in page)
                {
                    Index++;

                    Microsoft.Azure.Devices.Device device = await registryManager.GetDeviceAsync(twin.DeviceId).ConfigureAwait(false);

                    Devices.Add(device);

                    if (Index % skipInc == 0)
                    {
                        Debug.WriteLine("DELETING -> " + Index + " -> " + DateTime.Now.ToLongTimeString());

                        await registryManager.RemoveDevices2Async(Devices.Skip(skip).Take(skipInc)).ConfigureAwait(false);

                        skip += skipInc;
                    }

                    if (TwinDeleted != null)
                    {
                        TwinDeleted?.Invoke(this, null);
                    }
                }
            }

            DateTime endDate = DateTime.Now;
            Debug.WriteLine("STARTED -> " + initDate.ToLongTimeString());
            Debug.WriteLine("ENDED -> " + endDate.ToLongTimeString());
        }

        public async Task DeleteTwinsFromList(List<Twin> devicesToDelete)
        {
            if (devicesToDelete == null) return;

            Index = 0;

            foreach (var twin in devicesToDelete)
            {
                try
                {
                    Index++;

                    await registryManager.RemoveDeviceAsync(twin.DeviceId).ConfigureAwait(false);

                    if (TwinDeleted != null)
                    {
                        TwinDeleted?.Invoke(this, null);
                    }
                }
                catch (DeviceNotFoundException ex)
                {
                    Debug.WriteLine("DEVICE NOT FOUND -> " + ex.Message);
                }
            }

            Devices = new List<Device>();
        }

        //public async Task DeleteTwinsByTenantIDV1()
        //{
        //    Index = 0;

        //    string strQuery = "SELECT * FROM devices WHERE tags.tenant = '" + name + "'";
            
        //    IQuery query = registryManager.CreateQuery(strQuery, 10000);

        //    while (query.HasMoreResults)
        //    {
        //        var page = await query.GetNextAsTwinAsync().ConfigureAwait(false);

        //        foreach (var twin in page)
        //        {
        //            try
        //            {
        //                Index++;

        //                await registryManager.RemoveDeviceAsync(twin.DeviceId).ConfigureAwait(false);
        //                await DeleteEntityByDeviceIDAsync(DevicesStorageTable, resourceLoader.GetString("TableStorage_PartitionKey" + CEnvironment), twin.DeviceId).ConfigureAwait(false);
        //                await DeleteKeyVaultSecretAsync(KeyVaultC, KEYVAULT_BASE_URI, resourceLoader.GetString("KeyVault_G3DevicePrefix") + twin.DeviceId).ConfigureAwait(false);

        //                if (TwinDeleted != null)
        //                {
        //                    TwinDeleted?.Invoke(this, null);
        //                }

        //            }
        //            catch (DeviceNotFoundException ex)
        //            {
        //                Debug.WriteLine("DEVICE NOT FOUND -> " + ex.Message);
        //            }
        //        }
        //    }

        //    Items = new List<Twin>();
        //}

        public async Task DeleteTwinsByTenantID()
        {
            Index = 0;

            #region ParisTenants
            List<string> ParisTenantIDS = new List<string>();

            ParisTenantIDS.Add("PAR90768506");
            ParisTenantIDS.Add("PAR53488324");
            ParisTenantIDS.Add("PAR43889117");
            ParisTenantIDS.Add("PAR63238605");
            ParisTenantIDS.Add("PAR73016307");
            ParisTenantIDS.Add("PAR00370973");
            ParisTenantIDS.Add("PAR84532943");
            ParisTenantIDS.Add("PAR10537124");
            ParisTenantIDS.Add("PAR35484974");
            ParisTenantIDS.Add("PAR55902631");
            ParisTenantIDS.Add("PAR66943862");
            ParisTenantIDS.Add("PAR90919814");
            ParisTenantIDS.Add("PAR08516968");
            ParisTenantIDS.Add("PAR36451672");
            ParisTenantIDS.Add("PAR21255143");
            ParisTenantIDS.Add("PAR49008828");
            ParisTenantIDS.Add("PAR07327331");
            ParisTenantIDS.Add("PAR22041383");
            ParisTenantIDS.Add("PAR31974127");
            ParisTenantIDS.Add("PAR48779112");
            ParisTenantIDS.Add("PAR58999102");
            ParisTenantIDS.Add("PAR74920485");
            ParisTenantIDS.Add("PAR06757538");
            ParisTenantIDS.Add("PAR23564810");
            ParisTenantIDS.Add("PAR34438025");
            ParisTenantIDS.Add("PAR33950170");
            ParisTenantIDS.Add("PAR52296110");
            ParisTenantIDS.Add("PAR87334178");
            ParisTenantIDS.Add("PAR96842669");
            ParisTenantIDS.Add("PAR59180046");
            ParisTenantIDS.Add("PAR74454225");
            ParisTenantIDS.Add("PAR07090046");
            ParisTenantIDS.Add("PAR43203085");
            ParisTenantIDS.Add("PAR73743780");
            ParisTenantIDS.Add("PAR84208746");
            ParisTenantIDS.Add("PAR23577016");
            ParisTenantIDS.Add("PAR93747583");
            ParisTenantIDS.Add("PAR62212882");
            ParisTenantIDS.Add("PAR07902166");
            ParisTenantIDS.Add("PAR19033946");
            ParisTenantIDS.Add("PAR33817717");
            ParisTenantIDS.Add("PAR50304036");
            ParisTenantIDS.Add("PAR34536788");
            ParisTenantIDS.Add("PAR48454150");
            ParisTenantIDS.Add("PAR63812161");
            ParisTenantIDS.Add("PAR78067608");
            ParisTenantIDS.Add("PAR18289201");
            ParisTenantIDS.Add("PAR32763093");
            ParisTenantIDS.Add("PAR45661532");
            ParisTenantIDS.Add("PAR58064490");
            ParisTenantIDS.Add("PAR76518607");
            ParisTenantIDS.Add("PAR89243406");
            ParisTenantIDS.Add("PAR02681896");
            ParisTenantIDS.Add("PAR13813284");
            ParisTenantIDS.Add("PAR28207475");
            ParisTenantIDS.Add("PAR45177739");
            ParisTenantIDS.Add("PAR55414256");
            ParisTenantIDS.Add("PAR52284299");
            ParisTenantIDS.Add("PAR94139965");
            ParisTenantIDS.Add("PAR83606883");
            ParisTenantIDS.Add("PAR72290632");
            ParisTenantIDS.Add("PAR08684842");
            ParisTenantIDS.Add("PAR22762045");
            ParisTenantIDS.Add("PAR36603049");
            ParisTenantIDS.Add("PAR47817303");
            ParisTenantIDS.Add("PAR86529170");
            ParisTenantIDS.Add("PAR75662134");
            ParisTenantIDS.Add("PAR58631043");
            ParisTenantIDS.Add("PAR01337704");
            ParisTenantIDS.Add("PAR31518191");
            ParisTenantIDS.Add("PAR40100170");
            ParisTenantIDS.Add("PAR90149752");
            ParisTenantIDS.Add("PAR56521544");
            ParisTenantIDS.Add("PAR99911288");
            ParisTenantIDS.Add("PAR13753548");
            ParisTenantIDS.Add("PAR31207824");
            ParisTenantIDS.Add("PAR45321530");
            ParisTenantIDS.Add("PAR68924159");
            ParisTenantIDS.Add("PAR82442285");
            ParisTenantIDS.Add("PAR94546066");
            ParisTenantIDS.Add("PAR12157941");
            ParisTenantIDS.Add("PAR28204696");
            ParisTenantIDS.Add("PAR57443275");
            ParisTenantIDS.Add("PAR79025476");
            ParisTenantIDS.Add("PAR69439510");
            ParisTenantIDS.Add("PAR89777260");
            ParisTenantIDS.Add("PAR99036249");
            ParisTenantIDS.Add("PAR11514908");
            ParisTenantIDS.Add("PAR43321535");
            ParisTenantIDS.Add("PAR53826714");
            ParisTenantIDS.Add("PAR65416985");
            ParisTenantIDS.Add("PAR79753007");
            ParisTenantIDS.Add("PAR03343722");
            ParisTenantIDS.Add("PAR89659456");
            ParisTenantIDS.Add("PAR38531387822");
            #endregion

            string prefix = name.Substring(0, 3).ToUpper(CultureInfo.InvariantCulture);
            string strQuery = "SELECT * FROM devices WHERE tags.tenant = '" + name + "' OR STARTSWITH(deviceId,'" + prefix + "')";

            IQuery query = registryManager.CreateQuery(strQuery, 10000);

            var countKeep = 0;
            var countGo = 0;

            while (query.HasMoreResults)
            {
                var page = await query.GetNextAsTwinAsync().ConfigureAwait(false);

                foreach (var twin in page)
                {
                    try
                    {
                        if (!ParisTenantIDS.Contains(twin.DeviceId))
                        {
                            Index++;

                            await registryManager.RemoveDeviceAsync(twin.DeviceId).ConfigureAwait(false);
                            await DeleteEntityByDeviceIDAsync(DevicesStorageTable, resourceLoader.GetString("TableStorage_PartitionKey" + CEnvironment), twin.DeviceId).ConfigureAwait(false);
                            await DeleteKeyVaultSecretAsync(KeyVaultC, KEYVAULT_BASE_URI, resourceLoader.GetString("KeyVault_G3DevicePrefix") + twin.DeviceId).ConfigureAwait(false);

                            if (TwinDeleted != null)
                            {
                                TwinDeleted?.Invoke(this, null);
                            }

                            Debug.WriteLine("GO -> " + twin.DeviceId);

                            countGo++;
                        }
                        else
                        {
                            Debug.WriteLine("KEEP -> " + twin.DeviceId);

                            countKeep++;
                        }
                    }
                    catch (DeviceNotFoundException ex)
                    {
                        Debug.WriteLine("DEVICE NOT FOUND -> " + ex.Message);
                    }
                }
            }

            Devices = new List<Device>();

            Debug.WriteLine(Environment.NewLine);
            Debug.WriteLine("GO -> " + countGo + "\nKEEP -> " + countKeep);
        }

        public void DeleteZombieDevicesOnKeyVault()
        {
            GetKeyVaultSecrets().Wait();

            Debug.WriteLine(Environment.NewLine);

            foreach (var item in KeyVaultEntities)
            {
                string id = item.Identifier.Name.Replace(resourceLoader.GetString("KeyVault_G3DevicePrefix"), string.Empty, StringComparison.InvariantCulture).Trim();

                if (!TableStorageEntityExistsInTwinCollection(id))
                {
                    DeleteKeyVaultSecretAsync(KeyVaultC, KEYVAULT_BASE_URI, item.Identifier.Name).Wait();
                }
            }

            Debug.WriteLine(Environment.NewLine);
        }

        public async Task GetKeyVaultSecrets()
        {
            KeyVaultEntities = new List<SecretItem>();

            var prefix = name.Substring(0, 3).ToUpper(CultureInfo.InvariantCulture);
            var secrets = await KeyVaultC.GetSecretsAsync(KEYVAULT_BASE_URI).ConfigureAwait(false);

            do
            {
                foreach (var item in secrets)
                {
                    if (item.Identifier.Name.IndexOf(prefix, StringComparison.InvariantCulture) > -1)
                    {
                        KeyVaultEntities.Add(item);
                    }   
                }

                secrets = secrets.NextPageLink != null ? await KeyVaultC.GetSecretsNextAsync(secrets.NextPageLink).ConfigureAwait(false) : null;
            } 
            while (secrets != null);
        }

        public static async Task DeleteKeyVaultSecretAsync(KeyVaultClient keyVaultClient, string keyVault, string secretID)
        {
            try
            {
                if (keyVaultClient != null)
                {
                    DateTime initDate = DateTime.Now;

                    await keyVaultClient.DeleteSecretAsync(keyVault, secretID).ConfigureAwait(false);

                    DateTime endDate = DateTime.Now;


                    TimeSpan elapsedDiff = endDate - initDate;

                    Debug.WriteLine("DELETED -> " + secretID + " -> " + elapsedDiff);
                }

            }
            catch (KeyVaultErrorException e)
            {
                Debug.WriteLine("ERROR DELETING ENTITY ON KEY VAULT -> " + e.Message);
            }
        }


        public void DeleteZombieDevicesOnTableStorage()
        {
            GetTableStorageDevicesAsync().Wait();

            foreach (var item in TableStorageEntities)
            {
                if (!TableStorageEntityExistsInTwinCollection(item.RowKey))
                {
                    DeleteEntityAsync(DevicesStorageTable, item).Wait();
                }
            }
        }

        public async Task GetTableStorageDevicesAsync()
        {
            var entities = new List<G3GatewayTableEntity>();

            #region Filters
            /*
                var filterTenant = TableQuery.GenerateFilterCondition(
                  nameof(G3GatewayTableEntity.tenant),
                  QueryComparisons.Equal, name);

                var filterDeviceID = TableQuery.GenerateFilterCondition(
                  nameof(G3GatewayTableEntity.RowKey),
                  QueryComparisons.GreaterThanOrEqual, name.Substring(0, 3));

                var allFilters = TableQuery.CombineFilters(
                            filterTenant,
                            TableOperators.Or,
                            filterDeviceID);         
                            
            
                var queryResult = await table.ExecuteQuerySegmentedAsync(new TableQuery<G3GatewayTableEntity>().Where(filterTenant), token).ConfigureAwait(false);
             */
            #endregion

            TableContinuationToken token = null;

            do
            {
                var queryResult = await DevicesStorageTable.ExecuteQuerySegmentedAsync(new TableQuery<G3GatewayTableEntity>(), token).ConfigureAwait(false);

                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            }
            while (token != null);


            foreach (var item in entities)
            {
                if (item.RowKey.Substring(0, 3).ToUpper(CultureInfo.InvariantCulture).Equals(name.Substring(0, 3).ToUpper(CultureInfo.InvariantCulture), StringComparison.InvariantCulture))
                {
                    TableStorageEntities.Add(item);
                }
            }

            entities.Clear();
        }

        public async Task<G3GatewayTableEntity> DeleteEntityByDeviceIDAsync(CloudTable table, string partitionKey, string rowKey)
        {
            try
            {
                if (table == null) return new G3GatewayTableEntity();

                TableOperation retrieveOperation = TableOperation.Retrieve<G3GatewayTableEntity>(partitionKey, rowKey);
                TableResult result = await table.ExecuteAsync(retrieveOperation).ConfigureAwait(false);

                G3GatewayTableEntity device = result.Result as G3GatewayTableEntity;

                if (device != null)
                {
                    await DeleteEntityAsync(table, device).ConfigureAwait(false);
                }

                return device;
            }
            catch (StorageException e)
            {
                Debug.WriteLine("[RetrieveEntityUsingPointQueryAsync] -> " + e.Message);
                throw;
            }
        }

        public static async Task DeleteEntityAsync(CloudTable table, G3GatewayTableEntity record)
        {
            try
            {
                if (table != null && record != null)
                {
                    TableOperation deleteOperation = TableOperation.Delete(record);
                    TableResult result = await table.ExecuteAsync(deleteOperation).ConfigureAwait(false);
                }

            }
            catch (StorageException e)
            {
                Debug.WriteLine("ERROR DELETING ENTITY ON TABLE STORAGE -> " + e.Message);
            }
        }

        public bool TableStorageEntityExistsInTwinCollection(string ID)
        {
            foreach (var item in Devices)
            {
                if (item.DeviceID.Equals(ID, StringComparison.InvariantCulture))
                {
                    return true;
                }
            }

            return false;
        }

    }
}
