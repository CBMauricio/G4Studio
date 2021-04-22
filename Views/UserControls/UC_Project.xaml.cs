using G4Studio.Models;
using G4Studio.Utils;
using Hyperion.Platform.Tests.Core.ExedraLib;
using Hyperion.Platform.Tests.Core.ExedraLib.Config;
using Hyperion.Platform.Tests.Core.ExedraLib.Handlers;
using Hyperion.Platform.Tests.Core.ExedraLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views.UserControls
{
    public sealed partial class UC_Project : UserControl
    {
        private static ResourceLoader resourceLoader;
        public EnvironmentHandler.Type EnvironmentT { get; set; }
        public Tenant Project { get; set; }
        public List<Twin> Twins { get; private set; }
        private List<Device> DevicesFromRunBooksToClean { get; set; }
        private List<Windows.Devices.Geolocation.BasicGeoposition> Coordinates { get; set; }
        private List<MapElementsLayer> MapLayers { get; set; }
        
        private bool MapControlLoaded { get; set; }
        private bool IsShowingProjectDetail { get; set; }
        private bool IsTwinsRefreshAllowed { get; set; }
        private Color FillColor { get; set; }


        public event RoutedEventHandler GoBack;

        private System.Timers.Timer Scheduler { get; set; }

        public UC_Project()
        {
            InitializeComponent();

            EnvironmentT = EnvironmentHandler.Type.DEV;

            Scheduler = new System.Timers.Timer();

            resourceLoader = ResourceLoader.GetForCurrentView();

            MapControlLoaded = false;
            IsShowingProjectDetail = false;
            IsTwinsRefreshAllowed = true;

            Coordinates = new List<Windows.Devices.Geolocation.BasicGeoposition>();
            
            FillColor = new Color();

            CTRL_Map_Main.MapServiceToken = resourceLoader.GetString("Map_ServiceToken");

            Scheduler.Interval = 10000;
            Scheduler.Elapsed += Scheduler_Elapsed;

            Project = new Tenant();
            Twins = new List<Twin>();
            DevicesFromRunBooksToClean = new List<Device>();

            CTRL_NewDevices.D2CMessagesCategory = D2CMessagesConfig.Category.REG;
            CTRL_NewDevices.IsEditingAllowed = false;
            CTRL_NewDevices.AllowToExpandDevices = true;
            CTRL_NewDevices.IsTappedWithoutDevicesAllowed = false;
            CTRL_NewDevices.Focused += CTRL_NewDevices_Focused;
            CTRL_NewDevices.DeviceRemoved += CTRL_NewDevices_DeviceRemoved;
            CTRL_NewDevices.DoWork += CTRL_ProjectActions_DoWork;

            CTRL_Telemetry.D2CMessagesCategory = D2CMessagesConfig.Category.TEL;
            CTRL_Telemetry.IsEditingAllowed = false;
            CTRL_Telemetry.AllowToExpandDevices = true;
            CTRL_Telemetry.IsTappedWithoutDevicesAllowed = false;
            CTRL_Telemetry.Focused += CTRL_Telemetry_Focused;
            CTRL_Telemetry.DeviceRemoved += CTRL_Telemetry_DeviceRemoved;
            CTRL_Telemetry.DoWork += CTRL_ProjectActions_DoWork;

            CTRL_Bulk.D2CMessagesCategory = D2CMessagesConfig.Category.BLK;
            CTRL_Bulk.Focused += CTRL_Bulk_Focused;
            CTRL_Bulk.IsEditingAllowed = true;
            CTRL_Bulk.AllowToExpandDevices = false;
            CTRL_Bulk.IsTappedWithoutDevicesAllowed = true;
            CTRL_Bulk.DoWork += CTRL_ProjectActions_DoWork;

            CTRL_Runbooks.RunFinished += CTRL_Runbooks_RunFinished;
            CTRL_Runbooks.RunFinishedDoClean += CTRL_Runbooks_RunFinishedDoClean;
            CTRL_Runbooks.RunFinishedDoChangeStyle += CTRL_Runbooks_RunFinishedDoChangeStyle;

            SB_ShowInfo_R.Completed += SB_ShowInfo_R_Completed;
            SB_HideInfo_R.Completed += SB_HideInfo_R_Completed;
        }

        public void BindData(Tenant project, EnvironmentHandler.Type environment)
        {
            if (project is null) return;

            MapLayers = new List<MapElementsLayer>();         

            Project = project;
            EnvironmentT = environment;

            var area = GeopositionHandler.GetAreaInKmFromCoordinates(Project.Coordinates, 1);
            var perimeter = GeopositionHandler.GetPerimeterInKmFromCoordinates(Project.Coordinates, 1);
            
            var projectName = TextHandler.GetOnlyAlphabetChars(Project.Name);
            var projectNameLength = Math.Min(projectName.Length, 3);

            TB_Title_Prefix.Text = projectNameLength > 0 ? projectName.Substring(0, projectNameLength).ToUpperInvariant() : string.Empty;
            TB_ProjectName.Text = Project.Name;
            TB_Title.Text = Project.Name;
            TB_Timezone.Text = Project.Timezone;
            TB_NDevices.Text = Project.NDevices.ToString(CultureInfo.InvariantCulture);
            TB_Area.Text = string.Format(CultureInfo.InvariantCulture, "{0:### ### ###.#}", area);
            TB_Perimeter.Text = string.Format(CultureInfo.InvariantCulture, "{0:### ### ###.#}", perimeter);
            FillColor = ColorHandler.FromHex(Project.FillColor, Project.FillOpacity * 255);            

            BRD_Info_BT.Background = new SolidColorBrush(ColorHandler.FromHex(Project.FillColor, 200));
            BRD_Info_Title.Background = new SolidColorBrush(ColorHandler.FromHex(Project.FillColor, 200));

            Scheduler.Start();

            if (!MapControlLoaded)
            {
                InitializeMapControl();
            }
        }

        private async void BindDataControls()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                TB_NDevices.Text = string.Format(CultureInfo.InvariantCulture, "{0:### ### ###}", Twins.Count).Trim();
            });            

            CTRL_NewDevices.BaseNDevices = Twins.Count;
            CTRL_Telemetry.BaseNDevices = Twins.Count;
            CTRL_Bulk.BaseNDevices = Twins.Count;

            foreach (var twin in Twins)
            {
                CTRL_Bulk.AddDeviceButNoListings(twin.DeviceID, twin.DeviceID, GeoPositionConversor.Parse(twin.DevicePosition));
            }

            CTRL_NewDevices.BindData();
            CTRL_Telemetry.BindData();
            CTRL_Bulk.BindData();
        }

        private async void InitializeMapControl()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            Geopoint UserLocation = new Geopoint(new Windows.Devices.Geolocation.BasicGeoposition());

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

            CTRL_Map_Main.Center = UserLocation;
            CTRL_Map_Main.ZoomLevel = 1;
            CTRL_Map_Main.LandmarksVisible = false;
            CTRL_Map_Main.PedestrianFeaturesVisible = false;
            CTRL_Map_Main.WatermarkMode = MapWatermarkMode.Automatic;
            CTRL_Map_Main.StyleSheet = MapStyleSheet.ParseFromJson(resourceLoader.GetString("Map_Style"));
            CTRL_Map_Main.CacheMode = new BitmapCache();

            GRD_Map.Visibility = Visibility.Visible;

            MapControlLoaded = true;
        }

        private async void AddMapElements_Projects(bool centerOnTarget)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                Coordinates = new List<Windows.Devices.Geolocation.BasicGeoposition>();
                var zindex = 1000;
                var strokeColor = ColorHandler.FromHex(Project.FillColor);
                var mapProjects = new List<MapElement>();

                Coordinates.AddRange(GeoPositionConversor.Parse(Project.Coordinates));

                var mapPolygon = new MapPolygon
                {
                    Tag = Project.Name,
                    Path = new Geopath(Coordinates),
                    ZIndex = zindex,
                    FillColor = FillColor,
                    StrokeColor = strokeColor,
                    StrokeThickness = 3,
                    StrokeDashed = false,
                };

                mapProjects.Add(mapPolygon);


                var mapProjectsLayer = new MapElementsLayer
                {
                    ZIndex = zindex,
                    MapElements = mapProjects
                };

                MapLayers.Add(mapProjectsLayer);
                CTRL_Map_Main.Layers.Add(mapProjectsLayer);
            });

            if (centerOnTarget)
            {
                await CTRL_Map_Main.TrySetViewBoundsAsync(GeoboundingBox.TryCompute(Coordinates), new Thickness(200), MapAnimationKind.Bow);
            }
        }

        private void GetTwinsAsync()
        {
            try
            {
                Twins = Task.Run(async () => await ExedraLibCoreHandler.GetTwinsAsync(EnvironmentT, Project.Name).ConfigureAwait(false)).Result.Twins;
                bool a = Task.Run(async () => await DrawPushpins(Twins.ToList()).ConfigureAwait(true)).Result;
                Task.Run(async () => await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => { BindDataControls(); }));                
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine($"GetTwinsAsync - > {ex.Message}");
            }
        }

        private void AddTwinsAsync(List<Twin> newTwins)
        {
            try
            {
                Twins.AddRange(newTwins);
                bool a = Task.Run(async () => await DrawPushpins(newTwins).ConfigureAwait(true)).Result;
                Task.Run(async () => await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => { BindDataControls(); }));
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine($"AddTwinsAsync - > {ex.Message}");
            }
        }

        private async Task<bool> DrawPushpins(List<Twin> twins)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                var mapDevices = new List<MapElement>();

                foreach (var device in twins)
                {
                    var position = device.DevicePosition;

                    var pushpin = new MapIcon
                    {
                        Location = new Geopoint(GeoPositionConversor.Parse(position)),
                        NormalizedAnchorPoint = new Point(0.5, 1),
                        ZIndex = 0,
                        Tag = "EXISTING",
                        Title = device.DeviceID,
                        Image = RandomAccessStreamReference.CreateFromUri(new Uri(resourceLoader.GetString("Map_Pushpin_Default")))
                    };

                    mapDevices.Add(pushpin);

                    //ExistingDeviceMapIcons.Add(pushpin);
                }

                var mapDevicesLayer = new MapElementsLayer
                {
                    ZIndex = 1,
                    MapElements = mapDevices
                };

                MapLayers.Add(mapDevicesLayer);
                CTRL_Map_Main.Layers.Add(mapDevicesLayer);
            });

            return true;
        }

        private void AddPushpin(string deviceID, Windows.Devices.Geolocation.BasicGeoposition position)
        {
            var mapDevices = new List<MapElement>();

            Windows.Devices.Geolocation.BasicGeoposition snPosition = new Windows.Devices.Geolocation.BasicGeoposition { Latitude = position.Latitude, Longitude = position.Longitude };

            var pushpin = new MapIcon
            {
                Location = new Geopoint(snPosition),
                NormalizedAnchorPoint = new Point(0.5, 1),
                ZIndex = 1000,
                Tag = "NEW",
                Title = deviceID,
                Image = RandomAccessStreamReference.CreateFromUri(new Uri(resourceLoader.GetString("Map_Pushpin_New"))),
            };

            mapDevices.Add(pushpin);

            var mapDevicesLayer = new MapElementsLayer
            {
                ZIndex = 1,
                MapElements = mapDevices
            };

            MapLayers.Add(mapDevicesLayer);
            CTRL_Map_Main.Layers.Add(mapDevicesLayer);
        }

        private async void RemovePushpin(string deviceID)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                foreach (MapElementsLayer item in CTRL_Map_Main.Layers)
                {
                    var selectedMapIcons = item.MapElements.Where(x => x is MapIcon).ToList();

                    if (selectedMapIcons != null)
                    {
                        MapIcon selectedMapIcon = selectedMapIcons.FirstOrDefault(x => x is MapIcon && ((MapIcon)x).Title == deviceID) as MapIcon;

                        if (selectedMapIcon != null)
                        {
                            Debug.WriteLine($"FOUND -> {selectedMapIcon.Title}");

                            CTRL_Map_Main.Layers.Remove(item);

                            return;
                        }
                    }
                }
            });            
        }

        public void ChangePushpinStyleList(List<Device> items)
        {
            if (items is null) return;

            foreach (var item in items)
            {
                ChangePushpinStyle(item.DeviceID);
            }
        }

        private async void ChangePushpinStyle(string deviceID)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                foreach (MapElementsLayer item in CTRL_Map_Main.Layers)
                {
                    var selectedMapIcons = item.MapElements.Where(x => x is MapIcon).ToList();

                    if (selectedMapIcons != null)
                    {
                        MapIcon selectedMapIcon = selectedMapIcons.FirstOrDefault(x => x is MapIcon && ((MapIcon)x).Title == deviceID) as MapIcon;

                        if (selectedMapIcon != null)
                        {
                            Debug.WriteLine($"FOUND -> {selectedMapIcon.Title}");

                            selectedMapIcon.Image = RandomAccessStreamReference.CreateFromUri(new Uri(resourceLoader.GetString("Map_Pushpin_Default")));
                        }
                    }
                }
            });            
        }

        private void CleanDevicesFromRunbooks()
        {
            foreach (var device in DevicesFromRunBooksToClean)
            {
                RemovePushpin(device.DeviceID);
            }

            DevicesFromRunBooksToClean.Clear();
        }

        private static void SetBorderBackgroundColor(Border border, bool pointerEntered)
        {
            border.Background = new SolidColorBrush(pointerEntered ? ColorHandler.FromHex("#FFFFFFFF") : ColorHandler.FromHex("#7FFFFFFF"));
        }

        private void SetIMGBreadcrumbVisibility(bool pointerEntered)
        {
            SetBorderBackgroundColor(BRD_Breadcrumb, pointerEntered);

            TB_Breadcrumb.Visibility = pointerEntered ? Visibility.Visible : Visibility.Collapsed;
            IMG_Breadcrumb_1.Visibility = pointerEntered ? Visibility.Collapsed : Visibility.Visible;
            IMG_Breadcrumb_2.Visibility = pointerEntered ? Visibility.Visible : Visibility.Collapsed;
        }

        public void Show()
        {
            SB_ShowInfo_R.Begin();
        }

        public void Hide()
        {
            SB_HideInfo_R.Begin();
        }

        private void ClearMapLayers()
        {
            foreach (var item in MapLayers)
            {
                CTRL_Map_Main.Layers.Remove(item);
            }

            MapLayers.Clear();
        }

        private async void CTRL_Map_Main_MapElementClick(Windows.UI.Xaml.Controls.Maps.MapControl sender, MapElementClickEventArgs args)
        {
            if (args.MapElements.Count < 1 || args.MapElements[0] == null) return;

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                MapIcon selectedMapIcon = args.MapElements.FirstOrDefault(x => x is MapIcon) as MapIcon;

                if (selectedMapIcon is null)
                {
                    var clickedProjectName = args.MapElements[0].Tag.ToString();
                    var position = args.Location.Position;

                    var minLengthSufix = Math.Min(clickedProjectName.Length, 3);
                    var length = 16;
                    var datetime = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
                    var projectName = clickedProjectName.Substring(0, minLengthSufix).ToUpper(CultureInfo.InvariantCulture);
                    var deviceID = projectName + datetime.Substring(datetime.Length - length, length);


                    AddPushpin(deviceID, position);

                    CTRL_NewDevices.AddDevice(deviceID, deviceID, position);
                }
                else
                {
                    string tag = selectedMapIcon.Tag.ToString();

                    if (tag.Equals("NEW")) return;

                    selectedMapIcon.Image = RandomAccessStreamReference.CreateFromUri(new Uri(resourceLoader.GetString("Map_Pushpin_Red")));

                    CTRL_Telemetry.AddDevice(selectedMapIcon.Title, selectedMapIcon.Title, selectedMapIcon.Location.Position);
                }
            });            
        }

        private async void SB_ShowInfo_R_Completed(object sender, object e)
        {
            await Task.Run(() => AddMapElements_Projects(true)).ConfigureAwait(false);
            await Task.Run(() => GetTwinsAsync()).ConfigureAwait(true);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => { SB_ShowMap.Begin(); });
        }

        private void SB_HideInfo_R_Completed(object sender, object e)
        {
            IsShowingProjectDetail = false;

            SB_HideProjectInfo.Begin();
            ClearMapLayers();
            CTRL_NewDevices.ClearAndHide();
            CTRL_Telemetry.ClearAndHide();
            CTRL_Bulk.Hide();
            CTRL_Runbooks.Hide();
        }       

        private void SP_Breadcrumb_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            SetIMGBreadcrumbVisibility(true);
        }

        private void SP_Breadcrumb_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            SetIMGBreadcrumbVisibility(false);
        }

        private void SP_Breadcrumb_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Scheduler.Stop();

            Hide();

            if (GoBack != null)
            {
                GoBack?.Invoke(sender, e);
            }

            CTRL_NewDevices.Reset();
            CTRL_Telemetry.Reset();
            CTRL_Bulk.Reset();

            DevicesFromRunBooksToClean.Clear();
        }

        private void BRD_Reset_Position_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            SetBorderBackgroundColor(BRD_Reset_Position, true);
        }

        private void BRD_Reset_Position_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            SetBorderBackgroundColor(BRD_Reset_Position, false);
        }

        private async void BRD_Reset_Position_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await CTRL_Map_Main.TrySetViewBoundsAsync(GeoboundingBox.TryCompute(Coordinates), new Thickness(200), MapAnimationKind.Bow);
        }

        private void BRD_Info_BT_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!IsShowingProjectDetail)
            {
                SB_ShowProjectInfo.Begin();
            }
            else
            {
                SB_HideProjectInfo.Begin();
            }

            IsShowingProjectDetail = !IsShowingProjectDetail;
        }

        private void BRD_Info_BT_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            BRD_Info_BT.Background = new SolidColorBrush(ColorHandler.FromHex(Project.FillColor));
            BRD_Info_Title.Background = new SolidColorBrush(ColorHandler.FromHex(Project.FillColor));
        }

        private void BRD_Info_BT_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            BRD_Info_BT.Background = new SolidColorBrush(ColorHandler.FromHex(Project.FillColor, 200));
            BRD_Info_Title.Background = new SolidColorBrush(ColorHandler.FromHex(Project.FillColor, 200));
        }        

        private void CTRL_NewDevices_DeviceRemoved(object sender, RoutedEventArgs e)
        {
            RemovePushpin(CTRL_NewDevices.RemovedDeviceID);
        }

        private void CTRL_Telemetry_DeviceRemoved(object sender, RoutedEventArgs e)
        {
            ChangePushpinStyle(CTRL_Telemetry.RemovedDeviceID);
        }

        private void CTRL_Bulk_Focused(object sender, RoutedEventArgs e)
        {   
            CTRL_NewDevices.Hide();
            CTRL_Telemetry.Hide();
        }

        private void CTRL_Telemetry_Focused(object sender, RoutedEventArgs e)
        {
            CTRL_NewDevices.Hide();
            CTRL_Bulk.Hide();
        }

        private void CTRL_NewDevices_Focused(object sender, RoutedEventArgs e)
        {
            CTRL_Bulk.Hide();
            CTRL_Telemetry.Hide();
        }

        private void CTRL_ProjectActions_DoWork(object sender, RoutedEventArgs e)
        {
            UC_Devices_Actions control = sender as UC_Devices_Actions;

            CTRL_Runbooks.AddRun(EnvironmentT, Project, control.D2CMessagesCategory, control.D2CMessagesKind, control.NMessages, control.NRuns, control.NSeconds, new List<Device>(control.Devices));

            control.CleanDevices();
        }

        private void CTRL_Runbooks_RunFinishedDoClean(object sender, RoutedEventArgs e)
        {
            UC_RunBooks runBook = sender as UC_RunBooks;

            DevicesFromRunBooksToClean.AddRange(runBook.DevicesToClean);
        }

        private void CTRL_Runbooks_RunFinished(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("CTRL_Runbooks_RunFinished -> NOT IMPLEMENTED");
        }

        private void CTRL_Runbooks_RunFinishedDoChangeStyle(object sender, RoutedEventArgs e)
        {
            UC_RunBooks runBook = sender as UC_RunBooks;

            ChangePushpinStyleList(runBook.DevicesToClean);
        }

        private async void Scheduler_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!IsTwinsRefreshAllowed)
            {
                await Task.Run(() => Thread.Sleep(1000)).ConfigureAwait(true);
            }            

            IsTwinsRefreshAllowed = false;

            List<Twin> newTwins = Task.Run(async () => await ExedraLibCoreHandler.GetTwinsAsync(EnvironmentT, Project.Name).ConfigureAwait(false)).Result.Twins;

            var result = newTwins.Where(n => Twins.All(o => o.DeviceID != n.DeviceID)).ToList();

            Debug.WriteLine($"Scheduler_Elapsed -> REFRESHING TWINS -> O: {Twins.Count} -> {result.Count}");

            if (result.Count > 0)
            {
                await Task.Run(() => CleanDevicesFromRunbooks()).ConfigureAwait(false);
                await Task.Run(() => AddTwinsAsync(result.ToList())).ConfigureAwait(false);
            }            

            IsTwinsRefreshAllowed = true;
        }
    }
}
