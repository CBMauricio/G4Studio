using ExedraCoreLibrary;
using G4Studio.Models;
using G4Studio.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views
{
    public sealed partial class UC_MapControl : UserControl
    {
        private string DeviceIP_CoAPSERVER { get; set; }
       
        private static ResourceLoader resourceLoader;
        private string CEnvironment { get; set; }

        private Geopoint UserLocation { get; set; }

        List<BasicGeoposition> Coordinates { get; set; }
        private List<Project> Projects { get; set; }
        private List<MapPolygon> MapPolygons { get; set; }
        private List<MapElementsLayer> MapLayers { get; set; }
        private List<MapElementsLayer> MapLayersOverlayers { get; set; }
        private List<MapIcon> MapIcons { get; set; }
        private List<string> ListOfActionsToPerform { get; set; }


        private Project SelectedProject { get; set; }
        private Device SelectedDevice { get; set; }

        private bool ProjectLayerSelected { get; set; }


        public UC_MapControl()
        {
            this.InitializeComponent();

            resourceLoader = ResourceLoader.GetForCurrentView();

            CEnvironment = resourceLoader.GetString("CONFS_ENVIRONMENT");
            CTRL_Map_Main.MapServiceToken = resourceLoader.GetString("Map_ServiceToken" + CEnvironment);

            DeviceIP_CoAPSERVER = resourceLoader.GetString("CoAPSERVER_IP_DEV");
                       

            UserLocation = new Geopoint(new BasicGeoposition());

            Coordinates = new List<BasicGeoposition>();

            MapPolygons = new List<MapPolygon>();
            MapLayers = new List<MapElementsLayer>();
            MapLayersOverlayers = new List<MapElementsLayer>();
            MapIcons = new List<MapIcon>();

            ListOfActionsToPerform = new List<string>();

            ProjectLayerSelected = false;
            SelectedProject = new Project();
            SelectedDevice = new Device();

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

        public void StartAnimation()
        {
            SB_MapInitialization.Begin();
            CTRL_Map_Animation.StartAnimation();
        }

        private void SB_MapInitialization_Completed(object sender, object e)
        {
            InitializeMapControl();
            InitializeProjects();
            CTRL_Projects.BindData(Projects);
            
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
                UserLocation = new Geopoint(new BasicGeoposition() { Latitude = (double)localSettings.Values["UserLocation_Latitude"], Longitude = (double)localSettings.Values["UserLocation_Longitude"] });
            }


            // Set the map location.
            CTRL_Map_Main.Center = UserLocation;
            CTRL_Map_Main.ZoomLevel = 12;
            CTRL_Map_Main.LandmarksVisible = true;
            CTRL_Map_Main.PedestrianFeaturesVisible = true;
            CTRL_Map_Main.WatermarkMode = MapWatermarkMode.Automatic;
            CTRL_Map_Main.StyleSheet = MapStyleSheet.ParseFromJson(resourceLoader.GetString("Map_Style" + CEnvironment));

            CTRL_Map_Animation.Visibility = Visibility.Collapsed;
            CTRL_Map_Animation.StopAnimation();
            SB_ShowMap.Begin();
        }

        private void InitializeProjects()
        {
            Projects = new List<Project>();
            Projects = JsonConvert.DeserializeObject<List<Project>>(resourceLoader.GetString("TENANTS" + CEnvironment));
        }

        

        private void InitializeTwins(Project project)
        {
            project.SetCoordinates();
            project.LoadTwins().Wait();

            //MOVE THIS TO DELETE DEVICES
            //project.DeleteZombieDevicesOnTableStorage();
            //project.DeleteZombieDevicesOnKeyVault();

            //CoAPServer.TestServer(DeviceIP_Local);
            //CoAPServer.TestServer("20.56.53.23");
            //CoAPServer.TestServer("95.136.78.21");
        }

        private void CTRL_Projects_ItemSelected(object sender, RoutedEventArgs e)
        {
            LoadProjectAndTwins();
        }

        private void LoadProjectAndTwins()
        {
            Project selectedProject = CTRL_Projects.SelectedProject;

            if (!selectedProject.TwinsLoaded)
            {
                InitializeTwins(selectedProject);
            }

            SetViewToSelectedProject(selectedProject);
            
            CTRL_ProjectDetail.BindData(selectedProject);

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
            Debug.WriteLine("---- ListListOfActionsToPerform ----");
            foreach (var item in ListOfActionsToPerform)
            {
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Item: {0}", item));
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
                    CTRL_NewDevices.BindData(SelectedProject);

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
            
            SendAlarms(SelectedProject, alarmsToDeliver, initDateTime);
        }

        private void CTRL_ProjectDetail_Telemetry_Bulk_PerformAction(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("CTRL_ProjectDetail_Telemetry_Bulk_PerformAction -> " + CTRL_ProjectDetail.ActionToPerform);

            DateTime initDateTime = new DateTime(2020, 11, 1, 0, 0, 1);
            DateTime endDateTime = DateTime.Now;
            var currentTotalConsumption = 0.0;

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

            SendDimLevels(SelectedProject, messagesToDeliver, initDateTime, endDateTime, false, currentTotalConsumption);
            //SendDimLevels_BoisdelaCambre_Default(messagesToDeliver, initDateTime, endDateTime, true, currentTotalConsumption);
            //SendDimLevels_BoisdelaCambre_Default_V2(messagesToDeliver, initDateTime, endDateTime, true);
        }

        private async void CTRL_ProjectDetail_DeleteDevices(object sender, RoutedEventArgs e)
        {
            double totalMessages = SelectedProject.Devices.Count;

            SelectedProject.TwinDeleted += SelectedProject_TwinDeleted;

            await Task.Run(async () =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                () =>
                {
                    CTRL_DoWork.Title = resourceLoader.GetString("STR_UC_Map_DELETE_Title");
                    CTRL_DoWork.Description = resourceLoader.GetString("STR_UC_Map_DELETE_Description");
                    CTRL_DoWork.MaxItems = totalMessages.ToString(CultureInfo.InvariantCulture);
                    CTRL_DoWork.Visibility = Visibility.Visible;
                    CTRL_DoWork.BindData();

                    SB_HideProjectDetail.Begin();
                });

            }).ConfigureAwait(false);


            //await SelectedProject.DeleteTwins(SelectedProject.Items).ConfigureAwait(false);
            await SelectedProject.DeleteTwinsByTenantID().ConfigureAwait(false);
            //await SelectedProject.DeleteTwinsBulk().ConfigureAwait(false);

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                Thread.Sleep(5000);
                SelectedProject.TwinsLoaded = false;
                LoadProjectAndTwins();

                CTRL_DoWork.ShowDoneControl();
            });
        }

        private async void SelectedProject_TwinDeleted(object sender, RoutedEventArgs e)
        {
            var project = sender as Project;

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                CTRL_DoWork.UpdateCounter(project.Index.ToString(CultureInfo.InvariantCulture));
            });
        }

        private void CTRL_NewDevices_RegisterDevices(object sender, RoutedEventArgs e)
        {
            StringBuilder logs = new StringBuilder();

            var initDate = DateTime.Now;

            //Debug.WriteLine("STARTED AT: " + initDate.ToLongTimeString() + "." + initDate.Millisecond);

            foreach (var item in CTRL_NewDevices.NewDevices)
            {
                //bool isRegistered = G3Gateway.RegisterDevice(item.DeviceName, item.DevicePosition, DateTime.Now, string.Format(CultureInfo.InvariantCulture, "{0}", DeviceIP_CoAPSERVER));

                bool isRegistered = D2CHandler.RegisterDevice(item.DeviceName, item.DevicePosition, DateTime.Now, string.Format(CultureInfo.InvariantCulture, "{0}", DeviceIP_CoAPSERVER));

                if (isRegistered)
                {
                    Debug.WriteLine("DEVICE REGISTERED::UPDATING MAP -> " + item.DeviceName);
                    //await Task.Delay(3000).ConfigureAwait(true);
                }
                else
                {
                    Debug.WriteLine("ERROR REGISTERING DEVICE! -> " + item.DeviceName);
                }
            }

            CTRL_NewDevices.NewDevices.Clear();

            //InitializeTwins(SelectedProject.name);

            //SetViewToSelectedDevice(CTRL_ProjectDetail.SelectedDevices, CTRL_ProjectDetail.SelectedDevice, false);
            //SetMapElements_Devices_Selected(CTRL_ProjectDetail.SelectedDevice.DeviceId, false);

            //CTRL_ProjectDetail.BindData(SelectedProject);
            //ListOfActionsToPerform.Clear();

            //var endDate = DateTime.Now;

            //Debug.WriteLine("ENDED AT: " + endDate.ToLongTimeString() + "." + endDate.Millisecond);

           
        }

        private async void CreateDevicesBulk(Project project, long nDevices)
        {
            StringBuilder logs = new StringBuilder();

            List<string> FirstAndLastDevice = new List<string>();

            BasicGeoposition centroid = GeopositionHandler.GetPolygonCentroid(project.Coordinates);
            BasicGeoposition northwest = GeopositionHandler.GetNorthWestPosition(project.Coordinates, centroid);
            BasicGeoposition southeast = GeopositionHandler.GetSouthEastPosition(project.Coordinates, centroid);
            
            var clickedProjectName = SelectedProject.name;
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

                if (i == 0 || i == nDevices - 1)
                {
                    FirstAndLastDevice.Add(i + 1 + " -> " + deviceID);
                }

                BasicGeoposition devicePosition = new BasicGeoposition()
                {
                    Latitude = MathHandler.GetRandomNumber(southeast.Latitude, northwest.Latitude),
                    Longitude = MathHandler.GetRandomNumber(northwest.Longitude, southeast.Longitude),
                };

                //Debug.WriteLine("BOX ->");



                //bool success = G3Gateway.RegisterDevice(deviceID, devicePosition, DateTime.Now, string.Format(CultureInfo.InvariantCulture, "{0}:{1}", DeviceIP_CoAPSERVER, messageIndex));
                bool success = D2CHandler.RegisterDevice(deviceID, devicePosition, DateTime.Now, string.Format(CultureInfo.InvariantCulture, "{0}:{1}", DeviceIP_CoAPSERVER, messageIndex));

                DateTime end = DateTime.Now;

                TimeSpan diff = end - now;
                TimeSpan elapsed = end - initDate;

                logs.AppendLine( "[" + (i + 1) + "] | " + deviceID + " -> " + diff.TotalSeconds + " -> " + elapsed.TotalSeconds);

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
                Thread.Sleep(1000);
                SelectedProject.TwinsLoaded = false;
                LoadProjectAndTwins();

                CTRL_DoWork.ShowDoneControl();
            });

            var endDate = DateTime.Now;

            //Debug.Write("STARTED AT: " + initDate.ToLongTimeString() + "." + initDate.Millisecond);
            //Debug.WriteLine(" -> " + FirstAndLastDevice[0] != null ? FirstAndLastDevice[0] : "FirstAndLastDevice[0]");
            
            //Debug.Write("ENDED AT: " + endDate.ToLongTimeString() + "." + endDate.Millisecond);
            //Debug.WriteLine(" -> " + FirstAndLastDevice[1] != null ? FirstAndLastDevice[1] : "FirstAndLastDevice[1]");

            TimeSpan elapsedDiff = endDate - initDate;

            logs.Insert(0, "TOTAL ELAPSED -> " + Math.Round(elapsedDiff.TotalSeconds, 3) + "s  |  " + Math.Round(elapsedDiff.TotalSeconds / nDevices, 3) + "s / device" + Environment.NewLine + Environment.NewLine);
            //logs.Insert(0, "CityLinx ->  [FIRST AT ]" + Environment.NewLine);
            //logs.Insert(0, "CoAP Clients ->  [FIRST AT ]" + Environment.NewLine);
            logs.Insert(0, "BOT -> " + nDevices + Environment.NewLine);
            logs.Insert(0, "STARTED AT -> " + initDate.ToLongTimeString() + Environment.NewLine);
            logs.Insert(0, resourceLoader.GetString("CONFS_ENVIRONMENT") + "_" + Environment.NewLine);

            //TOTAL ELAPSED [FROM 1 - 100 CoAP Requests @ BOT side] -> 724.6868905 | AVG -> 7.2s / device

            Debug.WriteLine(logs.ToString());

            //Debug.WriteLine("TOTAL DIFF -> " + elapsedDiff.TotalSeconds);
        }
        
        private async void SendDimLevels_BoisdelaCambre_Default(int messagesPerDay, DateTime initDateTime, DateTime endDateTime, bool usesTimeZoneHandler, double totalConsumption)
        {
            //List<Twin> selectedDevices = new List<Twin>();

            List<string> DevicesBasline = new List<string>() { "BOI56168040" };
            //List<string> DevicesAvBelleAlliance = new List<string>() { "BOI31542458", "BOI48086484", "BOI64659725", "BOI65606604", "BOI94165412" }; //AV. BElle Alliance

            List<string> Devices = new List<string>();
            Devices.AddRange(DevicesBasline);
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
                        //Debug.WriteLine(device + ": " + item.Date + " -> " + item.Date.DayOfWeek + " |" + item.Date.IsDaylightSavingTime() + "| -> " + item.DimLevel + " -> " + item.EnergyConsumption + " -> " + (totalConsumption + dailyConsumption + item.EnergyConsumption));

                        bool success = D2CHandler.SendDimmingProfile(device, new BasicGeoposition(), item.Date, item.DimLevel, (totalConsumption + dailyConsumption + item.EnergyConsumption), item.ACPower, item.ACCurrent, item.ACCPowerFactor, DeviceIP_CoAPSERVER);

                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                        () =>
                        {
                            CTRL_DoWork.UpdateCounter(messageIndex.ToString(CultureInfo.InvariantCulture));
                        });

                        messageIndex++;

                        dailyConsumption += Math.Round(item.EnergyConsumption, 2);
                    }

                    Debug.WriteLine("DAILY -> " + dailyConsumption);
                    Debug.WriteLine(Environment.NewLine);
                    
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

            Debug.WriteLine("MESSAGES SENT: " + messageIndex);
        }

        private async void SendDimLevels_BoisdelaCambre_Default_V2(int messagesPerDay, DateTime initDateTime, DateTime endDateTime, bool usesTimeZoneHandler)
        {
            List<string> Devices = new List<string>();

            //Avenue de Diane
            Devices.Add("BOI85615447");
            Devices.Add("BOI43928512");
            Devices.Add("BOI35059266");
            Devices.Add("BOI17470110");
            //Devices.Add("BOI07608732");

            List<DimFeedback> dimFeedbacks = new List<DimFeedback>();

            //dimFeedbacks.Add(new DimFeedback(new DateTime(2020, 10, 23, 16, 15, 27), 0, 21.7, 0.0, 0.0, 0.36));
            //dimFeedbacks.Add(new DimFeedback(new DateTime(2020, 10, 23, 16, 45, 44), 100, 21.72145, 85.8, 0.379, 0.36));
            dimFeedbacks.Add(new DimFeedback(new DateTime(2020, 10, 24, 08, 01, 38), 0, 22.87975, 0.0, 0.0, 0.36));
            dimFeedbacks.Add(new DimFeedback(new DateTime(2020, 10, 24, 16, 45, 44), 100, 22.9012, 85.8, 0.379, 0.36));
            dimFeedbacks.Add(new DimFeedback(new DateTime(2020, 10, 25, 07, 22, 42), 0, 24.0595, 0.0, 0.0, 0.36));
            dimFeedbacks.Add(new DimFeedback(new DateTime(2020, 10, 25, 16, 45, 57), 100, 24.08095, 85.8, 0.379, 0.36));
            dimFeedbacks.Add(new DimFeedback(new DateTime(2020, 10, 25, 23, 14, 54), 50, 24.627925, 85.8 * 0.5, 0.379 * 0.5, 0.36));
            dimFeedbacks.Add(new DimFeedback(new DateTime(2020, 10, 26, 05, 00, 32), 100, 24.885325, 85.8, 0.379, 0.36));
            dimFeedbacks.Add(new DimFeedback(new DateTime(2020, 10, 26, 07, 23, 58), 0, 24.992575, 0.0, 0.0, 0.36));


            /*
            2020/10/23 16:15:27.671659 Date: 2020-10-23 16:15:27.644 +0000 UTC, Telemetry: {"DeltaTime":null,"Priority":0,"Source":"Lamp","Index":1,"DimFeedback":{"EnergyMeter":21.7,"ACPower":1.1,"ACCurrent":0.013,"ACVolt":235,"ACPF":0.36,"FeedbackDIMLevel":"0","MinAcPower":46.3,"MaxAcPower":47.6},"DeviceID":"BOI35059266"}
            2020/10/23 16:45:44.042188 Date: 2020-10-23 16:45:44.007 +0000 UTC, Telemetry: {"DeltaTime":null,"Priority":0,"Source":"Lamp","Index":1,"DimFeedback":{"EnergyMeter":21.72145,"ACPower":1.1,"ACCurrent":0.013,"ACVolt":235,"ACPF":0.36,"FeedbackDIMLevel":"100","MinAcPower":46.3,"MaxAcPower":47.6},"DeviceID":"BOI85615447"}            
            2020/10/24 06:30:38.781736 Date: 2020-10-24 06:30:38.76 +0000 UTC, Telemetry: {"DeltaTime":null,"Priority":0,"Source":"Lamp","Index":1,"DimFeedback":{"EnergyMeter":22.879750000000083,"ACPower":1.1,"ACCurrent":0.013,"ACVolt":235,"ACPF":0.36,"FeedbackDIMLevel":"0","MinAcPower":46.3,"MaxAcPower":47.6},"DeviceID":"BOI85615447"}
            2020/10/24 16:45:44.214730 Date: 2020-10-24 16:45:44.174 +0000 UTC, Telemetry: {"DeltaTime":null,"Priority":0,"Source":"Lamp","Index":1,"DimFeedback":{"EnergyMeter":22.901200000000085,"ACPower":1.1,"ACCurrent":0.013,"ACVolt":235,"ACPF":0.36,"FeedbackDIMLevel":"100","MinAcPower":46.3,"MaxAcPower":47.6},"DeviceID":"BOI07608732"}
            2020/10/25 06:30:42.143978 Date: 2020-10-25 06:30:42.113 +0000 UTC, Telemetry: {"DeltaTime":null,"Priority":0,"Source":"Lamp","Index":1,"DimFeedback":{"EnergyMeter":24.059500000000167,"ACPower":1.1,"ACCurrent":0.013,"ACVolt":235,"ACPF":0.36,"FeedbackDIMLevel":"0","MinAcPower":46.3,"MaxAcPower":47.6},"DeviceID":"BOI85615447"}
            2020/10/25 16:45:57.860996 Date: 2020-10-25 16:45:57.837 +0000 UTC, Telemetry: {"DeltaTime":null,"Priority":0,"Source":"Lamp","Index":1,"DimFeedback":{"EnergyMeter":24.08095000000017,"ACPower":1.1,"ACCurrent":0.013,"ACVolt":235,"ACPF":0.36,"FeedbackDIMLevel":"100","MinAcPower":46.3,"MaxAcPower":47.6},"DeviceID":"BOI17470110"}
            2020/10/25 23:15:54.041892 Date: 2020-10-25 23:15:54.009 +0000 UTC, Telemetry: {"DeltaTime":null,"Priority":0,"Source":"Lamp","Index":1,"DimFeedback":{"EnergyMeter":24.627925000000207,"ACPower":1.1,"ACCurrent":0.013,"ACVolt":235,"ACPF":0.36,"FeedbackDIMLevel":"50","MinAcPower":46.3,"MaxAcPower":47.6},"DeviceID":"BOI17470110"}
            2020/10/26 05:00:32.166805 Date: 2020-10-26 05:00:32.145 +0000 UTC, Telemetry: {"DeltaTime":null,"Priority":0,"Source":"Lamp","Index":1,"DimFeedback":{"EnergyMeter":24.885325000000226,"ACPower":1.1,"ACCurrent":0.013,"ACVolt":235,"ACPF":0.36,"FeedbackDIMLevel":"100","MinAcPower":46.3,"MaxAcPower":47.6},"DeviceID":"BOI07608732"}
            2020/10/26 06:30:58.052801 Date: 2020-10-26 06:30:58.026 +0000 UTC, Telemetry: {"DeltaTime":null,"Priority":0,"Source":"Lamp","Index":1,"DimFeedback":{"EnergyMeter":24.992575000000233,"ACPower":1.1,"ACCurrent":0.013,"ACVolt":235,"ACPF":0.36,"FeedbackDIMLevel":"0","MinAcPower":46.3,"MaxAcPower":47.6},"DeviceID":"BOI07608732"}
             */


            foreach (var dimFeedback in dimFeedbacks)
            {
                foreach (string device in Devices)
                {
                    //Debug.WriteLine(device + ": " + dimFeedback.Date + " -> " + dimFeedback.Date.DayOfWeek + " |" + dimFeedback.Date.IsDaylightSavingTime() + "| -> " + dimFeedback.DimLevel + " -> " + dimFeedback.EnergyConsumption);

                    bool success = D2CHandler.SendDimmingProfile(device, new BasicGeoposition(), dimFeedback.Date, dimFeedback.DimLevel, dimFeedback.EnergyConsumption, dimFeedback.ACPower, dimFeedback.ACCurrent, dimFeedback.ACCPowerFactor, DeviceIP_CoAPSERVER);
                }

                Debug.WriteLine(Environment.NewLine);
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

        private async void SendDimLevels(Project project, int messagesPerDay, DateTime initDateTime, DateTime endDateTime, bool usesTimeZoneHandler, double totalConsumption)
        {
            StringBuilder logs = new StringBuilder();

            double dailyConsumption = 0;

            var initDate = DateTime.Now;
            List<string> FirstAndLastDevice = new List<string>();

            int i = 1;
            int counter = 0;
            int messageIndex = 1;
            double totalMessages = Math.Ceiling(new TimeSpan(endDateTime.Ticks - initDateTime.Ticks).TotalDays) * messagesPerDay * project.Devices.Count;

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
                foreach (Device device in project.Devices)
                {
                    var now = DateTime.Now;

                    dailyConsumption = 0;
                    BasicGeoposition twinPosition = device.DevicePosition;

                    List<DimFeedback> dimFeedbacks = DimFeedback.GetDimFeedbacks(initDateTime, twinPosition.Latitude, twinPosition.Longitude, usesTimeZoneHandler);
                    if (counter == 0 || counter == totalMessages - 1)
                    {
                        FirstAndLastDevice.Add(counter + 1 + " -> " + device.DeviceID);
                    }

                    foreach (var item in dimFeedbacks)
                    {
                        //Debug.WriteLine(twin.DeviceId + ": " + item.Date + " -> " + item.Date.DayOfWeek + " |" + item.Date.IsDaylightSavingTime() + "| -> " + item.DimLevel + " -> " + item.EnergyConsumption + " -> " + (totalConsumption + dailyConsumption + item.EnergyConsumption));

                        bool success = D2CHandler.SendDimmingProfile(device.DeviceID, device.DevicePosition, item.Date, item.DimLevel, (totalConsumption + dailyConsumption + item.EnergyConsumption), item.ACPower, item.ACCurrent, item.ACCPowerFactor, DeviceIP_CoAPSERVER);

                        DateTime end = DateTime.Now;

                        TimeSpan diff = end - now;
                        TimeSpan elapsed = end - initDate;

                        logs.AppendLine("[" + (i++) + "] | " + device.DeviceID + " -> " + diff.TotalSeconds + " -> " + elapsed.TotalSeconds);

                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                        () =>
                        {
                            CTRL_DoWork.UpdateCounter(messageIndex.ToString(CultureInfo.InvariantCulture));
                        });

                        messageIndex++;

                        dailyConsumption += Math.Round(item.EnergyConsumption, 2);
                    }

                    //Debug.WriteLine(Environment.NewLine);
                    //Debug.WriteLine("DAILY -> " + dailyConsumption);
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

            //Debug.WriteLine("MESSAGES SENT: " + messageIndex);
            //Debug.Write("STARTED AT: " + initDate.ToLongTimeString() + "." + initDate.Millisecond);
            //Debug.WriteLine(" -> " + FirstAndLastDevice[0] != null ? FirstAndLastDevice[0] : "FirstAndLastDevice[0]");

            //Debug.Write("ENDED AT: " + endDate.ToLongTimeString() + "." + endDate.Millisecond);
            //Debug.WriteLine(" -> " + FirstAndLastDevice[1] != null ? FirstAndLastDevice[1] : "FirstAndLastDevice[1]");

            TimeSpan elapsedDiff = endDate - initDate;

            logs.Insert(0, "TOTAL ELAPSED -> " + elapsedDiff.TotalSeconds + "  |  " + Math.Round(elapsedDiff.TotalSeconds / totalMessages, 3) + "s / device" + Environment.NewLine + Environment.NewLine);
            //logs.Insert(0, "CityLinx ->  [FIRST AT ]" + Environment.NewLine);
            //logs.Insert(0, "CoAP Clients ->  [FIRST AT ]" + Environment.NewLine);
            logs.Insert(0, "BOT -> " + totalMessages + Environment.NewLine);
            logs.Insert(0, "STARTED AT -> " + initDate.ToLongTimeString() + Environment.NewLine);
            logs.Insert(0, "TELEMETRY" + Environment.NewLine);
            logs.Insert(0, resourceLoader.GetString("CONFS_ENVIRONMENT") + "_" + Environment.NewLine);

            //TOTAL ELAPSED [FROM 1 - 100 CoAP Requests @ BOT side] -> 724.6868905 | AVG -> 7.2s / device

            Debug.WriteLine(logs.ToString());
        }

        private async void SendAlarms(Project project, int messagesPerDay, DateTime initDateTime)
        {
            var initDate = DateTime.Now;
            List<string> FirstAndLastDevice = new List<string>();

            int counter = 0;
            int messageIndex = 1;
            double workingHours = 12;
            double totalMessages = Math.Ceiling(new TimeSpan(DateTime.Now.Ticks - initDateTime.Ticks).TotalDays) * messagesPerDay * project.Devices.Count;

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
                foreach (Device device in project.Devices)
                {
                    List<AlarmFeedback> alarmFeedbacks = AlarmFeedback.GetAlarmFeedbacks(initDateTime, messagesPerDay, workingHours);

                    if (counter == 0 || counter == totalMessages - 1)
                    {
                        FirstAndLastDevice.Add(counter + 1 + " -> " + device.DeviceID);
                    }

                    foreach (var item in alarmFeedbacks)
                    {
                        bool success = D2CHandler.SendAlarm(device.DeviceID, item.Type, item.Date, DeviceIP_CoAPSERVER);

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

            var endDate = DateTime.Now;

            Debug.WriteLine("ALARMS SENT: " + messageIndex);
            Debug.Write("STARTED AT: " + initDate.ToLongTimeString() + "." + initDate.Millisecond);
            Debug.WriteLine(" -> " + FirstAndLastDevice[0] != null ? FirstAndLastDevice[0] : "FirstAndLastDevice[0]");

            Debug.Write("ENDED AT: " + endDate.ToLongTimeString() + "." + endDate.Millisecond);
            Debug.WriteLine(" -> " + FirstAndLastDevice[1] != null ? FirstAndLastDevice[1] : "FirstAndLastDevice[1]");
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
                return new GeoboundingBox(new BasicGeoposition() { Latitude = 90, Longitude = 90 }, new BasicGeoposition() { Latitude = -90, Longitude = -90 });
            }

            Geopoint topLeft;
            Geopoint bottomRight;


            try
            {
                map.GetLocationFromOffset(new Windows.Foundation.Point(0, 0), out topLeft);
            }
            catch
            {
                var topOfMap = new Geopoint(new BasicGeoposition()
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
                var bottomOfMap = new Geopoint(new BasicGeoposition()
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

        private void AddOverlayer(List<BasicGeoposition> basePositions)
        {
            var fillColor = ColorHandler.FromHex("#99333333");
            var strokeColor = ColorHandler.FromHex("#FF000000");

            CleanOverlayers();

            GeoboundingBox box = GetBounds(CTRL_Map_Main, true);

            var mapProjects = new List<MapElement>();
            var mapProjectsOuter = new List<MapElement>();
            var positions = new List<BasicGeoposition>();
            var positionsOuterLayer = new List<BasicGeoposition>();

            positions.Add(box.SoutheastCorner);
            positions.Add(new BasicGeoposition() { Latitude = box.NorthwestCorner.Latitude, Longitude = box.SoutheastCorner.Longitude });
            positions.Add(box.NorthwestCorner);
            positions.Add(new BasicGeoposition() { Latitude = box.SoutheastCorner.Latitude, Longitude = box.NorthwestCorner.Longitude });

            for (int i = basePositions.Count - 1; i >= 0; i--)
            {
                positions.Add(basePositions[i]);
            }

            positions.Add(new BasicGeoposition() { Latitude = box.SoutheastCorner.Latitude, Longitude = box.NorthwestCorner.Longitude });


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

            positionsOuterLayer.Add(new BasicGeoposition() { Latitude = -85, Longitude = 85 });
            positionsOuterLayer.Add(new BasicGeoposition() { Latitude = 85, Longitude = 85 });
            positionsOuterLayer.Add(new BasicGeoposition() { Latitude = 85, Longitude = -180 });
            positionsOuterLayer.Add(new BasicGeoposition() { Latitude = -85, Longitude = -180 });
            positionsOuterLayer.Add(new BasicGeoposition() { Latitude = -85, Longitude = 180 });
            positionsOuterLayer.Add(new BasicGeoposition() { Latitude = 85, Longitude = 180 });
            positionsOuterLayer.Add(new BasicGeoposition() { Latitude = 85, Longitude = -85 });
            positionsOuterLayer.Add(new BasicGeoposition() { Latitude = -85, Longitude = -85 });

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
                if (project.name.Equals(projectName, StringComparison.InvariantCulture))
                {
                    var fillColor = ColorHandler.FromHex(project.fence.properties.fillColor, project.fence.properties.fillOpacity * 100);
                    var strokeColor = ColorHandler.FromHex(project.fence.properties.fillColor);

                    Coordinates.AddRange(project.Coordinates);

                    var mapProjects = new List<MapElement>();

                    var mapPolygon = new MapPolygon
                    {
                        Tag = project.name,
                        Path = new Geopath(project.Coordinates),
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
                Coordinates = new List<BasicGeoposition>();
            }

            foreach (var project in Projects)
            {
                if (project.name.Equals(projectName, StringComparison.InvariantCulture))
                {
                    var mapDevices = new List<MapElement>();

                    foreach (var device in project.Devices)
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
                            Location = new Geopoint(position),
                            NormalizedAnchorPoint = new Point(0.5, 1),
                            ZIndex = 0,
                            Title = device.DeviceID,
                            Image = RandomAccessStreamReference.CreateFromUri(new Uri(resourceLoader.GetString("Map_Pushpin_Black"))),
                        };

                        MapIcons.Add(pushpin);
                        mapDevices.Add(pushpin);

                        Coordinates.Add(position);
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


        private void AddPushpin(string deviceID, BasicGeoposition position)
        {
            var mapDevices = new List<MapElement>();
            
            BasicGeoposition snPosition = new BasicGeoposition { Latitude = position.Latitude, Longitude = position.Longitude };

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

        private async void SetViewToCoordinates(List<BasicGeoposition> coordinates)
        {
            if (coordinates.Count > 0)
            {
                await CTRL_Map_Main.TrySetViewBoundsAsync(GeoboundingBox.TryCompute(coordinates), new Thickness(30), MapAnimationKind.Bow);
            }
        }

        private async void SetViewToSelectedProject(Project project)
        {
            SelectedProject = project;

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Coordinates = new List<BasicGeoposition>();

                MapPolygons.Clear();
                CTRL_Map_Main.Layers.Clear();

                AddMapElements_Projects(SelectedProject.name, true);
                AddMapElements_Devices(SelectedProject.name, false);
            });
        }

        private async void SetViewToSelectedDevice(List<Device> devices, Device selectedDevice, bool selected)
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
                await CTRL_Map_Main.TrySetViewBoundsAsync(GeoboundingBox.TryCompute(SelectedProject.Coordinates), new Thickness(30), MapAnimationKind.Bow);
            }
            else
            {
                await CTRL_Map_Main.TrySetViewAsync(new Geopoint(new BasicGeoposition() { Latitude = selectedDevicePosition.latitude, Longitude = selectedDevicePosition.longitude }), 17);
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
    }
}
