using G4Studio.Models;
using G4Studio.Utils;
using G4Studio.Views.UIElements;
using Hyperion.Platform.Tests.Core.ExedraLib;
using Hyperion.Platform.Tests.Core.ExedraLib.Config;
using Hyperion.Platform.Tests.Core.ExedraLib.Handlers;
using Hyperion.Platform.Tests.Core.ExedraLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using BasicGeoposition = Hyperion.Platform.Tests.Core.ExedraLib.Models.BasicGeoposition;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views
{
    public sealed partial class UC_MapControl : UserControl
    {
        EnvironmentHandler.Type EnvironmentT { get; set; }

        private static ResourceLoader resourceLoader;
        private Geopoint UserLocation { get; set; }

        List<Windows.Devices.Geolocation.BasicGeoposition> Coordinates { get; set; }
        private List<Tenant> Projects { get; set; }
        private List<Twin> Twins { get; set; }
        private List<MapPolygon> MapPolygons { get; set; }
        private List<MapElementsLayer> MapLayers { get; set; }
        private List<MapElementsLayer> MapLayersOverlayers { get; set; }
        private List<MapIcon> MapIcons { get; set; }
        private List<string> ListOfActionsToPerform { get; set; }


        private Tenant SelectedProject { get; set; }
        private Twin SelectedDevice { get; set; }

        private bool ProjectLayerSelected { get; set; }


        public UC_MapControl()
        {
            this.InitializeComponent();

            Projects = new List<Tenant>();
            Twins = new List<Twin>();            

            resourceLoader = ResourceLoader.GetForCurrentView();

            EnvironmentT = EnvironmentHandler.Type.TST;
            CTRL_Map_Main.MapServiceToken = resourceLoader.GetString("Map_ServiceToken");

            UserLocation = new Geopoint(new Windows.Devices.Geolocation.BasicGeoposition());

            Coordinates = new List<Windows.Devices.Geolocation.BasicGeoposition>();

            MapPolygons = new List<MapPolygon>();
            MapLayers = new List<MapElementsLayer>();
            MapLayersOverlayers = new List<MapElementsLayer>();
            MapIcons = new List<MapIcon>();

            ListOfActionsToPerform = new List<string>();

            ProjectLayerSelected = false;
            SelectedProject = new Tenant();
            SelectedDevice = new Twin();

            SB_MapInitialization.Completed += SB_MapInitialization_Completed;
            SB_ShowMap.Completed += SB_ShowMap_Completed;

            CTRL_Projects.ItemWidth = 130;
            CTRL_Projects.ItemHeight = 106;
            CTRL_Projects.ItemSelected += CTRL_Projects_ItemSelected;

            CTRL_ProjectDetail.ItemSelected += CTRL_ProjectDetail_ItemSelected;
            CTRL_ProjectDetail.ItemDeselected += CTRL_ProjectDetail_ItemDeselected;
            CTRL_ProjectDetail.BTN_Action_ItemSelected += CTRL_ProjectDetail_BTN_Action_ItemSelected;
            CTRL_ProjectDetail.BTN_Action_ItemDeSelected += CTRL_ProjectDetail_BTN_Action_ItemDeSelected;
            CTRL_ProjectDetail.BTN_Action_SendMessages += CTRL_ProjectDetail_BTN_Action_SendMessages;
            CTRL_ProjectDetail.Telemetry_Bulk_PerformAction += CTRL_ProjectDetail_Telemetry_Bulk_PerformAction;
            CTRL_ProjectDetail.Alarms_Bulk_PerformAction += CTRL_ProjectDetail_Alarms_Bulk_PerformAction;
            CTRL_ProjectDetail.RegisterDevices_Bulk_PerformAction += CTRL_ProjectDetail_RegisterDevices_Bulk_PerformAction;
            CTRL_ProjectDetail.DeleteDevices += CTRL_ProjectDetail_DeleteDevices;            

            CTRL_DoWork.Visibility = Visibility.Collapsed;
            CTRL_DoWork.Done += CTRL_DoWork_Done;
        }

        public void StartAnimation(EnvironmentHandler.Type environment)
        {
            EnvironmentT = environment;            

            SB_MapInitialization.Begin();
            CTRL_Map_Animation.StartAnimation();
        }

        private void SB_MapInitialization_Completed(object sender, object e)
        {
            InitializeMapControl();
            InitializeProjects();

            CTRL_Actions.BindData();
            CTRL_Projects.BindData(Projects);
            CTRL_ControlPanel.BindData();

            CTRL_Actions.SetVisibility();
        }

        private async void SB_ShowMap_Completed(object sender, object e)
        {
            await CTRL_Map_Main.TrySetViewAsync(UserLocation, 10, 0, 0, MapAnimationKind.Bow);
        }

        private async void InitializeMapControl()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (!localSettings.Values.Keys.Contains("UserLocation"))
            {
                var accessStatus = await Geolocator.RequestAccessAsync();

                switch (accessStatus)
                {
                    case GeolocationAccessStatus.Allowed:
                        // Get the current location.
                        Geolocator geolocator = new Geolocator();
                        Geoposition pos = await geolocator.GetGeopositionAsync();
                        UserLocation = pos.Coordinate.Point;

                        localSettings.Values["UserLocation"] = true;
                        localSettings.Values["UserLocation_Latitude"] = pos.Coordinate.Point.Position.Latitude;
                        localSettings.Values["UserLocation_Longitude"] = pos.Coordinate.Point.Position.Longitude;

                        break;

                    case GeolocationAccessStatus.Denied:
                        // Handle the case  if access to location is denied.
                        var messageDialog = new MessageDialog("Location services not enabled!");

                        await messageDialog.ShowAsync();


                        break;

                    case GeolocationAccessStatus.Unspecified:
                        // Handle the case if  an unspecified error occurs.
                        break;
                }
            }
            else 
            {
                UserLocation = new Geopoint(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = (double)localSettings.Values["UserLocation_Latitude"], Longitude = (double)localSettings.Values["UserLocation_Longitude"] });
            }

            // Set the map location.
            CTRL_Map_Main.Center = UserLocation;
            CTRL_Map_Main.ZoomLevel = 12;
            CTRL_Map_Main.LandmarksVisible = true;
            CTRL_Map_Main.PedestrianFeaturesVisible = true;
            CTRL_Map_Main.WatermarkMode = MapWatermarkMode.Automatic;
            CTRL_Map_Main.StyleSheet = MapStyleSheet.ParseFromJson(resourceLoader.GetString("Map_Style"));

            CTRL_Map_Animation.Visibility = Visibility.Collapsed;
            CTRL_Map_Animation.StopAnimation();
            SB_ShowMap.Begin();
        }

        private void InitializeProjects()
        {
            Projects = new List<Tenant>();
            
            TenantIList tenantsObj = Task.Run(async () => await ExedraLibCoreHandler.GetTenantsAsync(EnvironmentT).ConfigureAwait(false)).Result;

            Projects = tenantsObj.Tenants;

            Debug.WriteLine(tenantsObj.Logs.Value);

            //CleanUnhealthyDevices();

            //SendC2DDimCurve();
        }

        private void CleanUnhealthyDevices()
        {
            var unhealthyEntites = Task.Run(async () => await ExedraLibCoreHandler.GetUnhealthyEntities(EnvironmentT).ConfigureAwait(false)).Result;

            Debug.WriteLine(unhealthyEntites.Logs.Value);

            List<string> deviceIDs = new List<string>();

           foreach (var item in unhealthyEntites.Entities)
            {
                deviceIDs.Add(item.DeviceID);
            }

            DeleteDevicesByDeviceID(deviceIDs);
        }

        private void InitializeTwinsAsync()
        {
            DateTime date1 = DateTime.Now;

            TwinIList result = Task.Run(async () => await ExedraLibCoreHandler.GetTwinsAsync(EnvironmentT, SelectedProject.Name).ConfigureAwait(false)).Result;

            Twins = result.Twins;

            Debug.WriteLine(result.Logs.Value);            
        }
                
        private void LoadProjectAndTwins()
        {
            SelectedProject = CTRL_Projects.SelectedProject;

            InitializeTwinsAsync();

            SetViewToSelectedProject(SelectedProject);
            
            CTRL_ProjectDetail.BindData(SelectedProject, Twins);

            if (!ProjectLayerSelected)
            {
                SB_ShowProjectDetail.Begin();
                ProjectLayerSelected = true;
            }
        }

        private void CTRL_ProjectDetail_ShowDetails(object sender, RoutedEventArgs e)
        {
            SB_ShowProjectDetail.Begin();
        }

        private void CTRL_ProjectDetail_HideDetails(object sender, RoutedEventArgs e)
        {
            SB_HideProjectDetail.Begin();
        }

        private void CTRL_ProjectDetail_ItemSelected(object sender, RoutedEventArgs e)
        {
            SetViewToSelectedDevice(CTRL_ProjectDetail.SelectedDevices, CTRL_ProjectDetail.SelectedDevice, true);
            SetMapElements_Devices_Selected(CTRL_ProjectDetail.SelectedDevice.DeviceID, true);
        }

        private void CTRL_ProjectDetail_ItemDeselected(object sender, RoutedEventArgs e)
        {
            SetViewToSelectedDevice(CTRL_ProjectDetail.SelectedDevices, CTRL_ProjectDetail.SelectedDevice, false);
            SetMapElements_Devices_Selected(CTRL_ProjectDetail.SelectedDevice.DeviceID, false);
        }

        private void ListListOfActionsToPerform()
        {
            foreach (var item in ListOfActionsToPerform)
            {
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Item: {0}", item));


                //var url = $"{tenant}{tenant}";
            }
        }

        private void CTRL_ProjectDetail_BTN_Action_ItemSelected(object sender, RoutedEventArgs e)
        {
            UC_Action_BTN button = sender as UC_Action_BTN;

            switch (button.Tag.ToString())
            {
                case "CTRL_Action_REG":
                    //SPECIAL CASE - REGISTER NEW DEVICES

                    CTRL_ProjectDetail.SetCTRLVisibility();
                    CTRL_NewDevices.BindData(SelectedProject, Twins);

                    //AddOverlayer(SelectedProject.Coordinates);

                    Debug.WriteLine("CTRL_Action_REG");
                    break;
                default:
                    ListOfActionsToPerform.Add(button.Tag.ToString());

                    Debug.WriteLine("CTRL_ProjectDetail_BTN_Action_ItemSelected -> " + button.Tag + Environment.NewLine);
                    ListListOfActionsToPerform();
                    break;
            }
        }

        private void CTRL_ProjectDetail_BTN_Action_ItemDeSelected(object sender, RoutedEventArgs e)
        {
            UC_Action_BTN button = sender as UC_Action_BTN;

            ListOfActionsToPerform.Remove(button.Tag.ToString());

            Debug.WriteLine("CTRL_ProjectDetail_BTN_Action_ItemSelected -> " + button.Tag + Environment.NewLine);
            ListListOfActionsToPerform();
        }

        private void CTRL_ProjectDetail_BTN_Action_SendMessages(object sender, RoutedEventArgs e)
        {
            //ENERGY CONSUMPTION-> 0.2 KW / HOUR  |  1.68 KW / day

            //DateTime initDateTime = new DateTime(2020, 8, 13, 18, 0, 0);
            //List<Twin> selectedDevices = CTRL_ProjectDetail.SelectedDevices;

            //SendDimLevels(selectedDevices, 3, initDateTime);
            //SendAlarms(selectedDevices, 12, initDateTime);
            //CreateDevicesBulk(SelectedProject, 500);

            //CTRL_ProjectDetail.BindData(SelectedProject);
            //ListOfActionsToPerform.Clear();
        }

        private void CTRL_ProjectDetail_RegisterDevices_Bulk_PerformAction(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("CTRL_ProjectDetail_RegisterDevices_Bulk_PerformAction -> " + CTRL_ProjectDetail.ActionToPerform);

            long devicesToRegister = 0;

            switch (CTRL_ProjectDetail.ActionToPerform)
            {
                case "30":
                    devicesToRegister = 30;
                    break;
                case "50":
                    devicesToRegister = 50;
                    break;
                case "100":
                    devicesToRegister = 100;
                    break;
                case "250":
                    devicesToRegister = 250;
                    break;
                case "500":
                    devicesToRegister = 500;
                    break;
                case "2000":
                    devicesToRegister = 2000;
                    break;
                case "5000":
                    devicesToRegister = 5000;
                    break;
                case "10000":
                    devicesToRegister = 10000;
                    break;
                default:
                    devicesToRegister = 1000;
                    break;
            }

            CreateDevicesBulk(SelectedProject, devicesToRegister);
            //CreateDevicesBulkThreaded(SelectedProject, devicesToRegister);
        }
        
        private void CTRL_ProjectDetail_Telemetry_Bulk_PerformAction(object sender, RoutedEventArgs e)
        {
            //DateTime initDateTime = new DateTime(2020, 12, 4, 0, 0, 1);
            DateTime initDateTime = DateTime.Now.AddDays(-5);
            DateTime endDateTime = DateTime.Now;
            var currentTotalConsumption = 79.54;

            //List<Twin> selectedDevices = CTRL_ProjectDetail.SelectedDevices;
            int messagesToDeliver;

            switch (CTRL_ProjectDetail.ActionToPerform)
            {
                case "3":
                    messagesToDeliver = 3;
                    break;
                case "12":
                    messagesToDeliver = 12;
                    break;
                case "24":
                    messagesToDeliver = 24;
                    break;
                case "36":
                    messagesToDeliver = 36;
                    break;
                case "64":
                    messagesToDeliver = 64;
                    break;
                default:
                    messagesToDeliver = 0;
                    break;
            }

            SendDimLevels(messagesToDeliver, initDateTime, endDateTime, false, currentTotalConsumption);
            //SendDimLevels_BoisdelaCambre_Default(messagesToDeliver, initDateTime, endDateTime, true, currentTotalConsumption);
        }

        private void CTRL_ProjectDetail_Alarms_Bulk_PerformAction(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("CTRL_ProjectDetail_Alarms_Bulk_PerformAction -> " + CTRL_ProjectDetail.ActionToPerform);

            //DateTime initDateTime = new DateTime(2020, 8, 31, 18, 0, 0);
            DateTime initDateTime = DateTime.Now.AddDays(-2);
            //List<Twin> selectedDevices = CTRL_ProjectDetail.SelectedDevices;
            //List<Twin> selectedDevices = SelectedProject.Items;

            int alarmsToDeliver;

            switch (CTRL_ProjectDetail.ActionToPerform)
            {
                case "2":
                    alarmsToDeliver = 2;
                    break;
                case "5":
                    alarmsToDeliver = 5;
                    break;
                case "15":
                    alarmsToDeliver = 14;
                    break;
                case "30":
                    alarmsToDeliver = 30;
                    break;
                default:
                    alarmsToDeliver = 0;
                    break;
            }
            
            SendAlarms(alarmsToDeliver, initDateTime);
        }

        private async void DeleteDevicesByDeviceID(List<string> deviceIDs)
        {
            int index = 1;            

            await Task.Run(async () =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                () =>
                {
                    CTRL_DoWork.Title = resourceLoader.GetString("STR_UC_Map_DELETE_Title");
                    CTRL_DoWork.Description = resourceLoader.GetString("STR_UC_Map_DELETE_Description");
                    CTRL_DoWork.MaxItems = deviceIDs.Count.ToString(CultureInfo.InvariantCulture);
                    CTRL_DoWork.Visibility = Visibility.Visible;
                    CTRL_DoWork.BindData();

                    SB_HideProjectDetail.Begin();
                });

            }).ConfigureAwait(false);

            foreach (var ID in deviceIDs)
            {
                var result = await ExedraLibCoreHandler.DeleteDevice(EnvironmentT, ID).ConfigureAwait(false);

                Debug.WriteLine(result.Value);

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                () =>
                {
                    CTRL_DoWork.UpdateCounter(index++.ToString(CultureInfo.InvariantCulture));
                });
            }

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                CTRL_DoWork.ShowDoneControl();
            });
        }

        private void CTRL_ProjectDetail_DeleteDevices(object sender, RoutedEventArgs e)
        {
            if (SelectedProject.Name.IndexOf("Bois", StringComparison.InvariantCulture) > -1 || SelectedProject.Name.IndexOf("Paris", StringComparison.InvariantCulture) > -1)
            {
                return;
            }

            List<string> deviceIDs = new List<string>();

            foreach (var item in Twins)
            {
                deviceIDs.Add(item.DeviceID);
            }

            DeleteDevicesByDeviceID(deviceIDs);

            LoadProjectAndTwins();
        }

        private async void CTRL_NewDevices_RegisterDevices(object sender, RoutedEventArgs e)
        {
            int index = 1;

            await Task.Run(async () =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                () =>
                {
                    CTRL_DoWork.Title = resourceLoader.GetString("STR_UC_Map_REGISTER_Title");
                    CTRL_DoWork.Description = resourceLoader.GetString("STR_UC_Map_REGISTER_Description");
                    CTRL_DoWork.MaxItems = CTRL_NewDevices.NewDevices.Count.ToString(CultureInfo.InvariantCulture);
                    CTRL_DoWork.Visibility = Visibility.Visible;
                    CTRL_DoWork.BindData();

                    SB_HideProjectDetail.Begin();
                });

            }).ConfigureAwait(false);

            foreach (var device in CTRL_NewDevices.NewDevices)
            {
                KeyValuePair<bool, string> result1 = ExedraLibCoreHandler.RegisterDevice(device.DeviceName, new BasicGeoposition() { Latitude = device.DevicePosition.Latitude, Longitude = device.DevicePosition.Longitude }, EnvironmentT);

                Debug.WriteLine(result1.Value);

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                () =>
                {
                    CTRL_DoWork.UpdateCounter(index++.ToString(CultureInfo.InvariantCulture));
                });
            }            

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                CTRL_NewDevices.NewDevices.Clear();

                //Selectedproject.TwinsLoaded = false;
                LoadProjectAndTwins();

                CTRL_DoWork.ShowDoneControl();
            });


        }

        private async void CreateDevicesBulk(Tenant project, long nDevices)
        {
            List<string> FirstAndLastDevice = new List<string>();

            BasicGeoposition centroid = GeopositionHandler.GetPolygonCentroid(project.Coordinates);
            BasicGeoposition northwest = GeopositionHandler.GetNorthWestPosition(project.Coordinates, centroid);
            BasicGeoposition southeast = GeopositionHandler.GetSouthEastPosition(project.Coordinates, centroid);

            var clickedProjectName = SelectedProject.Name;

            var projectName = clickedProjectName.Substring(0, 3).ToUpper(CultureInfo.InvariantCulture);
            var length = 8;
            double totalMessages = nDevices;

            int messageIndex = 1;

            await Task.Run(async () =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                () =>
                {
                    CTRL_DoWork.Title = resourceLoader.GetString("STR_UC_Map_REGISTER_Title");
                    CTRL_DoWork.Description = resourceLoader.GetString("STR_UC_Map_REGISTER_Description");
                    CTRL_DoWork.MaxItems = totalMessages.ToString(CultureInfo.InvariantCulture);
                    CTRL_DoWork.Visibility = Visibility.Visible;
                    CTRL_DoWork.BindData();

                    SB_HideProjectDetail.Begin();
                });

            }).ConfigureAwait(false);


            var initDate = DateTime.Now;

            for (int i = 0; i < nDevices; i++)
            {
                var now = DateTime.Now;

                var datetime = now.Ticks.ToString(CultureInfo.InvariantCulture);
                var deviceID = projectName + i + datetime.Substring(datetime.Length - length, length);
                //var deviceID = "test-device-" + (i > 9 ? i.ToString(CultureInfo.InvariantCulture) : $"0{i}");

                if (i == 0 || i == nDevices - 1)
                {
                    FirstAndLastDevice.Add(i + 1 + " -> " + deviceID);
                }

                Windows.Devices.Geolocation.BasicGeoposition devicePosition = new Windows.Devices.Geolocation.BasicGeoposition()
                {
                    Latitude = MathHandler.GetRandomNumber(southeast.Latitude, northwest.Latitude),
                    Longitude = MathHandler.GetRandomNumber(northwest.Longitude, southeast.Longitude),
                };

                KeyValuePair<bool, string> result = ExedraLibCoreHandler.RegisterDevice(deviceID, new BasicGeoposition() { Latitude = devicePosition.Latitude, Longitude = devicePosition.Longitude }, EnvironmentT);

                Debug.WriteLine($"{result.Value}");

                DateTime end = DateTime.Now;

                TimeSpan diff = end - now;
                TimeSpan elapsed = end - initDate;

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                () =>
                {                    
                    AddPushpin(deviceID, devicePosition);
                    CTRL_DoWork.UpdateCounter(messageIndex.ToString(CultureInfo.InvariantCulture));
                });

                messageIndex++;

            }

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                //Selectedproject.TwinsLoaded = false;
                LoadProjectAndTwins();

                CTRL_DoWork.ShowDoneControl();
            });
        }
        
        private void SanitizeTelemetry()
        {
            string payload = resourceLoader.GetString("SANITIZE");
            KeyValuePair<bool, string> result = ExedraLibCoreHandler.SendDimmingProfile(payload, EnvironmentT);

            Debug.WriteLine("MESSAGES SENT: " + result.Value);
        }

        private void SendC2DDimCurve()
        {
            //List<string> IPS = new List<string>() { "11.0.211.96", "11.0.142.194", "11.0.231.18", "11.0.241.214", "11.0.74.27", "11.0.63.218", "11.0.173.240", "11.0.177.217", "11.0.153.195", "11.0.133.175", "12.41.179.94" };
            //List<string> IDS = new List<string>() { "0013A20041BBD742", "0013A20041BC0644", "0013A20041BC0634", "0013A20041BBD72C", "0013A20041BBD66E", "0013A20041BBD9C6", "0013A20041BC19DC", "0013A20041BBD78F", "0013A20041BC58EF", "0013A20041BC0A40", "0013A20041BC44F0" };

            List<string> IPS = new List<string>() { "11.0.211.96" };
            List<string> IDS = new List<string>() { "0013A20041BBD742" };

            int index = 0;

            foreach (var ip in IPS)
            {
                KeyValuePair<bool, string> result = ExedraLibCoreHandler.SendC2DDimCurve(ip, IDS[index++], EnvironmentT);

                Debug.WriteLine("MESSAGES SENT: " + result.Value);
            }            
        }

        private async void SendDimLevels_BoisdelaCambre_Default(int messagesPerDay, DateTime initDateTime, DateTime endDateTime, bool usesTimeZoneHandler, double totalConsumption)
        {
            List<string> DevicesBaseline = new List<string>() { "BOI56168040" };
            //List<string> DevicesAvBelleAlliance = new List<string>() { "BOI31542458", "BOI48086484", "BOI64659725", "BOI65606604", "BOI94165412" }; //AV. BElle Alliance

            List<string> Devices = new List<string>();
            Devices.AddRange(DevicesBaseline);
            //Devices.AddRange(DevicesAvBelleAlliance);

            double dailyConsumption = 0;
            int messageIndex = 0;
            int counter = 0;
            double totalMessages = Math.Ceiling(new TimeSpan(endDateTime.Ticks - initDateTime.Ticks).TotalDays) * messagesPerDay * Devices.Count;
            
            await Task.Run(async () =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                () =>
                {
                    CTRL_DoWork.Title = resourceLoader.GetString("STR_UC_Map_TELEMETRY_Title");
                    CTRL_DoWork.Description = resourceLoader.GetString("STR_UC_Map_TELEMETRY_Description");
                    CTRL_DoWork.MaxItems = totalMessages.ToString(CultureInfo.InvariantCulture);
                    CTRL_DoWork.Visibility = Visibility.Visible;
                    CTRL_DoWork.BindData();

                    SB_HideProjectDetail.Begin();
                });

            }).ConfigureAwait(false);

            while (initDateTime <= endDateTime)
            {
                foreach (string device in Devices)
                {
                    dailyConsumption = 0;
                    
                    List<DimFeedback> dimFeedbacks = DimFeedback.GetDimFeedbacks(initDateTime, 50.8066452649426, 4.37447303041323, usesTimeZoneHandler);

                    foreach (var item in dimFeedbacks)
                    {
                        KeyValuePair<bool, string> result = ExedraLibCoreHandler.SendDimmingProfile(device, item.Date, item.DimLevel, (totalConsumption + dailyConsumption + item.EnergyConsumption), item.ACPower, item.ACCurrent, item.ACCPowerFactor, EnvironmentT);

                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                        () =>
                        {
                            CTRL_DoWork.UpdateCounter(messageIndex.ToString(CultureInfo.InvariantCulture));
                        });

                        messageIndex++;

                        dailyConsumption += Math.Round(item.EnergyConsumption, 2);

                        Debug.WriteLine(result.Value);
                    }
                }

                counter++;
                totalConsumption = Math.Round(totalConsumption + dailyConsumption, 2);
                initDateTime = initDateTime.AddDays(1);
            }

            await Task.Run(async () =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                () =>
                {
                    CTRL_DoWork.ShowDoneControl();

                    SB_HideProjectDetail.Begin();
                });

            }).ConfigureAwait(false);

            var endDate = DateTime.Now;
        }

        private async void SendDimLevels(int messagesPerDay, DateTime initDateTime, DateTime endDateTime, bool usesTimeZoneHandler, double totalConsumption)
        {
            double dailyConsumption = 0;            
            int counter = 0;
            int messageIndex = 1;
            double totalMessages = Math.Ceiling(new TimeSpan(endDateTime.Ticks - initDateTime.Ticks).TotalDays) * messagesPerDay * Twins.Count;

            var initDate = DateTime.Now;

            await Task.Run(async () =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                () =>
                {
                    CTRL_DoWork.Title = resourceLoader.GetString("STR_UC_Map_TELEMETRY_Title");
                    CTRL_DoWork.Description = resourceLoader.GetString("STR_UC_Map_TELEMETRY_Description");
                    CTRL_DoWork.MaxItems = totalMessages.ToString(CultureInfo.InvariantCulture);
                    CTRL_DoWork.Visibility = Visibility.Visible;
                    CTRL_DoWork.BindData();

                    SB_HideProjectDetail.Begin();
                });

            }).ConfigureAwait(false);

            while (initDateTime <= endDateTime)
            {
                foreach (Twin device in Twins)
                {
                    var now = DateTime.Now;

                    dailyConsumption = 0;
                    BasicGeoposition twinPosition = device.DevicePosition;

                    List<DimFeedback> dimFeedbacks = DimFeedback.GetDimFeedbacks(initDateTime, twinPosition.Latitude, twinPosition.Longitude, usesTimeZoneHandler);
                    
                    foreach (var item in dimFeedbacks)
                    {
                        KeyValuePair<bool, string> result = ExedraLibCoreHandler.SendDimmingProfile(device.DeviceID, item.Date, item.DimLevel, (totalConsumption + dailyConsumption + item.EnergyConsumption), item.ACPower, item.ACCurrent, item.ACCPowerFactor, EnvironmentT);

                        Debug.WriteLine(result.Value);

                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                        () =>
                        {
                            CTRL_DoWork.UpdateCounter(messageIndex.ToString(CultureInfo.InvariantCulture));
                        });

                        messageIndex++;

                        dailyConsumption += Math.Round(item.EnergyConsumption, 2);
                    }
                }

                counter++;
                totalConsumption = Math.Round(totalConsumption + dailyConsumption, 2);
                initDateTime = initDateTime.AddDays(1);
            }

            await Task.Run(async () =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                () =>
                {
                    CTRL_DoWork.ShowDoneControl();

                    SB_HideProjectDetail.Begin();
                });

            }).ConfigureAwait(false);
        }

        private async void SendAlarms(int messagesPerDay, DateTime initDateTime)
        {
            var initDate = DateTime.Now;
            int counter = 0;
            int messageIndex = 1;
            double workingHours = 12;
            double totalMessages = Math.Ceiling(new TimeSpan(DateTime.Now.Ticks - initDateTime.Ticks).TotalDays) * messagesPerDay * Twins.Count;

            await Task.Run(async () =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                () =>
                {
                    CTRL_DoWork.Title = resourceLoader.GetString("STR_UC_Map_ALARMS_Title");
                    CTRL_DoWork.Description = resourceLoader.GetString("STR_UC_Map_ALARMS_Description");
                    CTRL_DoWork.MaxItems = totalMessages.ToString(CultureInfo.InvariantCulture);
                    CTRL_DoWork.Visibility = Visibility.Visible;
                    CTRL_DoWork.BindData();

                    SB_HideProjectDetail.Begin();
                });

            }).ConfigureAwait(false);

            while (initDateTime <= DateTime.Now)
            {
                foreach (Twin device in Twins)
                {
                    List<AlarmFeedback> alarmFeedbacks = AlarmFeedback.GetAlarmFeedbacks(initDateTime, messagesPerDay, workingHours);

                    foreach (var item in alarmFeedbacks)
                    {
                        KeyValuePair<bool, string> result = ExedraLibCoreHandler.SendAlarm(device.DeviceID, item.Date, item.Type, EnvironmentT);

                        Debug.WriteLine(result.Value);

                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                        () =>
                        {
                            CTRL_DoWork.UpdateCounter(messageIndex.ToString(CultureInfo.InvariantCulture));
                        });

                        messageIndex++;
                    }
                }

                counter++;
                initDateTime = initDateTime.AddDays(1);
            }

            await Task.Run(async () =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                () =>
                {
                    CTRL_DoWork.ShowDoneControl();

                    SB_HideProjectDetail.Begin();
                });

            }).ConfigureAwait(false);
        }

        private void CTRL_DoWork_Done(object sender, RoutedEventArgs e)
        {
            CTRL_DoWork.Visibility = Visibility.Collapsed;
            SB_ShowProjectDetail.Begin();
        }

        private async void SetMapView()
        {
            await CTRL_Map_Main.TrySetViewBoundsAsync(GeoboundingBox.TryCompute(Coordinates), new Thickness(100), MapAnimationKind.Bow);
        }

        public GeoboundingBox GetBounds(Windows.UI.Xaml.Controls.Maps.MapControl map, bool fullsize)
        {
            if (fullsize)
            {
                return new GeoboundingBox(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = 90, Longitude = 90 }, new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = -90, Longitude = -90 });
            }

            Geopoint topLeft;
            Geopoint bottomRight;


            try
            {
                map.GetLocationFromOffset(new Windows.Foundation.Point(0, 0), out topLeft);
            }
            catch
            {
                var topOfMap = new Geopoint(new Windows.Devices.Geolocation.BasicGeoposition()
                {
                    Latitude = 85,
                    Longitude = 0
                });

                Windows.Foundation.Point topPoint;
                map.GetOffsetFromLocation(topOfMap, out topPoint);
                map.GetLocationFromOffset(new Windows.Foundation.Point(0, topPoint.Y), out topLeft);
            }


            try
            {
                map.GetLocationFromOffset(new Windows.Foundation.Point(map.ActualWidth, map.ActualHeight), out bottomRight);
            }
            catch
            {
                var bottomOfMap = new Geopoint(new Windows.Devices.Geolocation.BasicGeoposition()
                {
                    Latitude = -85,
                    Longitude = 0
                });

                Windows.Foundation.Point bottomPoint;
                map.GetOffsetFromLocation(bottomOfMap, out bottomPoint);
                map.GetLocationFromOffset(new Windows.Foundation.Point(0, bottomPoint.Y), out bottomRight);
            }

            if (topLeft != null && bottomRight != null)
            {
                return new GeoboundingBox(topLeft.Position, bottomRight.Position);
            }



            return null;
        }        

        private void CleanMapLayers()
        {
            foreach (var item in MapLayers)
            {
                CTRL_Map_Main.Layers.Remove(item);
            }
        }
        
        private void CleanOverlayers()
        {
            foreach (var item in MapLayersOverlayers)
            {
                CTRL_Map_Main.Layers.Remove(item);
            }

            MapLayersOverlayers.Clear();
        }

        private void AddOverlayer(List<Windows.Devices.Geolocation.BasicGeoposition> basePositions)
        {
            var fillColor = ColorHandler.FromHex("#99333333");
            var strokeColor = ColorHandler.FromHex("#FF000000");

            CleanOverlayers();

            GeoboundingBox box = GetBounds(CTRL_Map_Main, true);

            var mapProjects = new List<MapElement>();
            var mapProjectsOuter = new List<MapElement>();
            var positions = new List<Windows.Devices.Geolocation.BasicGeoposition>();
            var positionsOuterLayer = new List<Windows.Devices.Geolocation.BasicGeoposition>();

            positions.Add(box.SoutheastCorner);
            positions.Add(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = box.NorthwestCorner.Latitude, Longitude = box.SoutheastCorner.Longitude });
            positions.Add(box.NorthwestCorner);
            positions.Add(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = box.SoutheastCorner.Latitude, Longitude = box.NorthwestCorner.Longitude });

            for (int i = basePositions.Count - 1; i >= 0; i--)
            {
                positions.Add(basePositions[i]);
            }

            positions.Add(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = box.SoutheastCorner.Latitude, Longitude = box.NorthwestCorner.Longitude });


            var mapPolygon = new MapPolygon
            {
                Path = new Geopath(positions),
                ZIndex = 1000,
                FillColor = fillColor,
                StrokeColor = strokeColor,
                StrokeThickness = 0,
                StrokeDashed = false
            };

            mapProjects.Add(mapPolygon);

            var mapProjectsLayer = new MapElementsLayer
            {
                ZIndex = 1000,
                MapElements = mapProjects
            };

            MapLayersOverlayers.Add(mapProjectsLayer);
            CTRL_Map_Main.Layers.Add(mapProjectsLayer);

            positionsOuterLayer.Add(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = -85, Longitude = 85 });
            positionsOuterLayer.Add(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = 85, Longitude = 85 });
            positionsOuterLayer.Add(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = 85, Longitude = -180 });
            positionsOuterLayer.Add(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = -85, Longitude = -180 });
            positionsOuterLayer.Add(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = -85, Longitude = 180 });
            positionsOuterLayer.Add(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = 85, Longitude = 180 });
            positionsOuterLayer.Add(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = 85, Longitude = -85 });
            positionsOuterLayer.Add(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = -85, Longitude = -85 });

            var mapPolygonOuterLayer = new MapPolygon
            {
                Path = new Geopath(positionsOuterLayer),
                ZIndex = 1000,
                FillColor = fillColor,
                StrokeColor = strokeColor,
                StrokeThickness = 0,
                StrokeDashed = false
            };

            mapProjectsOuter.Add(mapPolygonOuterLayer);

            var mapProjectsOuterLayer = new MapElementsLayer
            {
                ZIndex = 1000,
                MapElements = mapProjectsOuter
            };

            MapLayersOverlayers.Add(mapProjectsOuterLayer);
            CTRL_Map_Main.Layers.Add(mapProjectsOuterLayer);
        }

        private async void AddMapElements_Projects(string projectName, bool centerOnTarget)
        {
            var zindex = 0;

            foreach (var project in Projects)
            {
                if (project.Name.Equals(projectName, StringComparison.InvariantCulture))
                {
                    var fillColor = ColorHandler.FromHex(project.FillColor, project.FillOpacity * 100);
                    var strokeColor = ColorHandler.FromHex(project.FillColor);

                    Coordinates.AddRange(GeoPositionConversor.Parse(project.Coordinates));

                    var mapProjects = new List<MapElement>();

                    var mapPolygon = new MapPolygon
                    {
                        Tag = project.Name,
                        Path = new Geopath(Coordinates),
                        ZIndex = zindex,
                        FillColor = fillColor,
                        StrokeColor = strokeColor,
                        StrokeThickness = 3,
                        StrokeDashed = true,
                    };

                    mapProjects.Insert(0, mapPolygon);

                    var mapProjectsLayer = new MapElementsLayer
                    {
                        ZIndex = zindex,
                        MapElements = mapProjects
                    };


                    mapProjectsLayer.MapElementClick += MapProjectsLayer_MapElementClick;
                    mapProjectsLayer.MapContextRequested += MapProjectsLayer_MapContextRequested;

                    MapPolygons.Insert(0, mapPolygon);
                    MapLayers.Insert(0, mapProjectsLayer);

                    CTRL_Map_Main.Layers.Add(mapProjectsLayer);

                    zindex++;
                }
            }

            if (centerOnTarget)
            {
                await CTRL_Map_Main.TrySetViewBoundsAsync(GeoboundingBox.TryCompute(Coordinates), new Thickness(150), MapAnimationKind.Bow);
            }
        }

        private async void AddMapElements_Devices(string projectName, bool centerOnTarget)
        {
            MapIcons.Clear();

            if (centerOnTarget)
            {
                Coordinates = new List<Windows.Devices.Geolocation.BasicGeoposition>();
            }

            foreach (var project in Projects)
            {
                if (project.Name.Equals(projectName, StringComparison.Ordinal))
                {
                    var mapDevices = new List<MapElement>();

                    foreach (var device in Twins)
                    {
                        //foreach (KeyValuePair<string, object> item in twin.Properties.Desired)
                        //{
                        //    if (item.Key.Equals("location", StringComparison.InvariantCulture))
                        //    {
                        //        Position position = JsonConvert.DeserializeObject<Position>(item.Value.ToString());

                        //        BasicGeoposition snPosition = new BasicGeoposition { Latitude = position.latitude, Longitude = position.longitude };

                        //        var pushpin = new MapIcon
                        //        {  
                        //            Location = new Geopoint(snPosition),
                        //            NormalizedAnchorPoint = new Point(0.5, 1),
                        //            ZIndex = 0,
                        //            Title = twin.DeviceId,
                        //            Image = RandomAccessStreamReference.CreateFromUri(new Uri(resourceLoader.GetString("Map_Pushpin_Black"))),
                        //        };

                        //        MapIcons.Add(pushpin);
                        //        mapDevices.Add(pushpin);

                        //        Coordinates.Add(snPosition);
                        //    }
                        //}

                        var position = device.DevicePosition;

                        var pushpin = new MapIcon
                        {
                            Location = new Geopoint(GeoPositionConversor.Parse(position)),
                            NormalizedAnchorPoint = new Point(0.5, 1),
                            ZIndex = 0,
                            Title = device.DeviceID,
                            Image = RandomAccessStreamReference.CreateFromUri(new Uri(resourceLoader.GetString("Map_Pushpin_Black"))),
                        };

                        MapIcons.Add(pushpin);
                        mapDevices.Add(pushpin);

                        Coordinates.Add(GeoPositionConversor.Parse(position));
                    }

                    var mapDevicesLayer = new MapElementsLayer
                    {
                        ZIndex = 1,
                        MapElements = mapDevices
                    };

                    MapLayers.Add(mapDevicesLayer);
                    CTRL_Map_Main.Layers.Add(mapDevicesLayer);
                }   
            }

            if (centerOnTarget)
            {
                await CTRL_Map_Main.TrySetViewBoundsAsync(GeoboundingBox.TryCompute(Coordinates), new Thickness(50), MapAnimationKind.Bow);
            }
        }

        private void SetMapElements_Devices_Selected(string deviceID, bool selected)
        {
            foreach (var item in MapIcons)
            {
                if (item.Title.Equals(deviceID, StringComparison.InvariantCulture))
                {
                    item.Image = RandomAccessStreamReference.CreateFromUri(new Uri(resourceLoader.GetString(selected ? "Map_Pushpin_Selected" : "Map_Pushpin_Black")));
                    item.ZIndex = 1000;
                }
            }
        }

        //private void MyMap_MapElementClick(Windows.UI.Xaml.Controls.Maps.MapControl sender, Windows.UI.Xaml.Controls.Maps.MapElementClickEventArgs args)
        //{
        //    MapIcon myClickedIcon = args.MapElements.FirstOrDefault(x => x is MapIcon) as MapIcon;
        //    // do rest
        //}


        private void AddPushpin(string deviceID, Windows.Devices.Geolocation.BasicGeoposition position)
        {
            var mapDevices = new List<MapElement>();

            Windows.Devices.Geolocation.BasicGeoposition snPosition = new Windows.Devices.Geolocation.BasicGeoposition { Latitude = position.Latitude, Longitude = position.Longitude };

            var pushpin = new MapIcon
            {
                Location = new Geopoint(snPosition),
                NormalizedAnchorPoint = new Point(0.5, 1),
                ZIndex = 0,
                Title = deviceID,
                Image = RandomAccessStreamReference.CreateFromUri(new Uri(resourceLoader.GetString("Map_Pushpin_Red"))),
            };

            MapIcons.Add(pushpin);
            mapDevices.Add(pushpin);

            Coordinates.Add(snPosition);

            var mapDevicesLayer = new MapElementsLayer
            {
                ZIndex = 1,
                MapElements = mapDevices
            };

            MapLayers.Add(mapDevicesLayer);

            CTRL_Map_Main.Layers.Add(mapDevicesLayer);
            CTRL_NewDevices.AddNewDevice(deviceID, position);
        }

        private async void SetViewToCoordinates(List<Windows.Devices.Geolocation.BasicGeoposition> coordinates)
        {
            if (coordinates.Count > 0)
            {
                await CTRL_Map_Main.TrySetViewBoundsAsync(GeoboundingBox.TryCompute(coordinates), new Thickness(30), MapAnimationKind.Bow);
            }
        }

        private async void SetViewToSelectedProject(Tenant project)
        {
            SelectedProject = project;

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Coordinates = new List<Windows.Devices.Geolocation.BasicGeoposition>();

                MapPolygons.Clear();
                CTRL_Map_Main.Layers.Clear();

                AddMapElements_Projects(SelectedProject.Name, true);
                AddMapElements_Devices(SelectedProject.Name, false);
            });
        }

        private async void SetViewToSelectedDevice(List<Twin> devices, Twin selectedDevice, bool selected)
        {
            var selectedDevicePosition = new Position();
            List<BasicGeoposition> coordinates = new List<BasicGeoposition>();

            foreach (var device in devices)
            {
                coordinates.Add(new BasicGeoposition { Latitude = device.DevicePosition.Latitude, Longitude = device.DevicePosition.Longitude });

                if (device.DeviceID.Equals(selectedDevice.DeviceID, StringComparison.Ordinal))
                {
                    SelectedDevice = device;
                }
            }

            if (coordinates.Count < 1 || !selected)
            {
                await CTRL_Map_Main.TrySetViewBoundsAsync(GeoboundingBox.TryCompute(GeoPositionConversor.Parse(SelectedProject.Coordinates)), new Thickness(30), MapAnimationKind.Bow);
            }
            else
            {
                await CTRL_Map_Main.TrySetViewAsync(new Geopoint(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = selectedDevicePosition.latitude, Longitude = selectedDevicePosition.longitude }), 17);
            }
        }

        private void SetPolygonFillColor(List<Windows.UI.Xaml.Controls.Maps.MapPolygon> polygons, string projectName, string color, double opacity)
        {
            var fillColor = string.IsNullOrEmpty(color) ? Windows.UI.Colors.Transparent : ColorHandler.FromHex(color, opacity * 100);
            var strokeColor = string.IsNullOrEmpty(color) ? Windows.UI.Colors.Red : ColorHandler.FromHex(color);

            foreach (var item in polygons)
            {
                if (projectName.Equals(item.Tag.ToString(), StringComparison.InvariantCulture))
                {
                    item.FillColor = fillColor;
                    //item.StrokeColor = strokeColor;
                }
            }
        }

        private void CTRL_Map_Main_MapRightTapped(Windows.UI.Xaml.Controls.Maps.MapControl sender, MapRightTappedEventArgs args)
        {
            //Debug.WriteLine(string.Format("CLICKED ON MAP! -> ({0},{1})", args.Location.Position.Latitude, args.Location.Position.Longitude));

            //ShowDialog("SCH", args.Location.Position);

            //if (args.MapElements.Count < 1 || args.MapElements[0] == null)
            //    return;

            //var clickedProjectName = args.MapElements[0].Tag.ToString();

            //Debug.WriteLine("CLICKED ON PROJECT RIGHT LAYER + " + clickedProjectName);

            //var clickedProjectName = "MADRID";

            ////var mapDevices = new List<MapElement>();
            //var length = 8;
            //var datetime = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
            //var projectName = clickedProjectName.Substring(0, 3).ToUpper(CultureInfo.InvariantCulture);
            //var deviceID = projectName + datetime.Substring(datetime.Length - length, length);
            //var position = args.Location.Position;

            //AddPushpin(deviceID, position);
        }

        private void MapProjectsLayer_MapContextRequested(MapElementsLayer sender, MapElementsLayerContextRequestedEventArgs args)
        {
            var project = args.MapElements[0].Tag.ToString();


            Debug.WriteLine("CLICKED ON PROJECT RIGHT LAYER + " + args.MapElements[0].Tag);



            //ShowDialog(project, args.Location.Position);

            //var datetime = DateTime.Now;
            //var nHours = 1;

            //while (nHours > 0)
            //{
            //    SimulateSendTelemetry(0, datetime.AddHours(-1 * nHours));
            //    SimulateSendTelemetry(1, datetime.AddHours(-1 * nHours).AddMinutes(-20));
            //    SimulateSendTelemetry(2, datetime.AddHours(-1 * nHours).AddMinutes(-40));
            //    SimulateSendTelemetry(3, datetime.AddHours(-1 * nHours).AddMinutes(-60));

            //    nHours--;
            //}

        }

        private void MapProjectsLayer_MapElementClick(MapElementsLayer sender, MapElementsLayerClickEventArgs args)
        {
            if (args.MapElements.Count < 1 || args.MapElements[0] == null)
                return;

            var clickedProjectName = args.MapElements[0].Tag.ToString();

            //var mapDevices = new List<MapElement>();
            var length = 8;
            var datetime = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
            var projectName = clickedProjectName.Substring(0, 3).ToUpper(CultureInfo.InvariantCulture);
            var deviceID = projectName + datetime.Substring(datetime.Length - length, length);
            var position = args.Location.Position;

            AddPushpin(deviceID, position);

            //BasicGeoposition snPosition = new BasicGeoposition { Latitude = position.Latitude, Longitude = position.Longitude };

            //var pushpin = new MapIcon
            //{
            //    Location = new Geopoint(snPosition),
            //    NormalizedAnchorPoint = new Point(0.5, 1.0),
            //    ZIndex = 0,
            //    Title = deviceID,
            //    Image = RandomAccessStreamReference.CreateFromUri(new Uri(resourceLoader.GetString("Map_Pushpin_Red"))),
            //};

            //MapIcons.Add(pushpin);
            //mapDevices.Add(pushpin);

            //Coordinates.Add(snPosition);

            //var mapDevicesLayer = new MapElementsLayer
            //{
            //    ZIndex = 1,
            //    MapElements = mapDevices
            //};

            //MapLayers.Add(mapDevicesLayer);

            //CTRL_Map_Main.Layers.Add(mapDevicesLayer);
            //CTRL_NewDevices.AddNewDevice(deviceID, position);

        }

        private void CTRL_Map_Main_MapTapped(Windows.UI.Xaml.Controls.Maps.MapControl sender, MapInputEventArgs args)
        {
            //var length = 8;
            //var datetime = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
            //var projectName = "MREG";
            //var deviceID = projectName + datetime.Substring(datetime.Length - length, length);
            //var position = args.Location.Position;

            //AddPushpin(deviceID, position);
        }


        #region Routed Events

        private void CTRL_Actions_Show(object sender, RoutedEventArgs e)
        {
            UCActionsItem UIElement = sender as UCActionsItem;

            switch (UIElement.ID)
            {
                case "SL_TEN":
                    CTRL_Projects.Show();
                    CTRL_ControlPanel.Hide();
                    break;
                case "SL_Main":
                    CTRL_Projects.Hide();
                    CTRL_ControlPanel.Show();
                    break;
                default:
                    CTRL_Projects.Hide();
                    CTRL_ControlPanel.Hide();
                    break;
            }
        }

        private void CTRL_Actions_Hide(object sender, RoutedEventArgs e)
        {
            //UCActionsItem UIElement = sender as UCActionsItem;

            //switch (UIElement.ID)
            //{
            //    case "SL_TEN":
            //        CTRL_Projects.SetVisibility();
            //        break;
            //    default:
            //        CTRL_Projects.ResetAndSetVisibility();
            //        break;
            //}

            CTRL_Projects.Hide();
            CTRL_ControlPanel.Hide();
        }

        private void CTRL_Projects_ItemSelected(object sender, RoutedEventArgs e)
        {
            CTRL_Projects.Reset();
            CTRL_Actions.Reset();
            LoadProjectAndTwins();
        }
        
        #endregion
    }
}
