using G4Studio.Models;
using G4Studio.Utils;
using Hyperion.Platform.Tests.Core.ExedraLib;
using Hyperion.Platform.Tests.Core.ExedraLib.Handlers;
using Hyperion.Platform.Tests.Core.ExedraLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views.UIElements
{
    public sealed partial class UC_RunBook_Item : UserControl
    {
        private static ResourceLoader resourceLoader;

        public RunBookItem Item { get; set; }
        private RunBookItem.Status Status { get; set; }

        private bool IsAllowedStart { get; set; }
        private bool IsAllowedPause { get; set; }
        private bool IsAllowedStop{ get; set; }

        public long ElapsedTime { get; set; }
        public long AverageTime { get; set; }

        public double Total { get; set; }
        public int Succeed { get; set; }
        public int Failed { get; set; }
        public double DeliveredPercentage { get; set; }

        public event RoutedEventHandler UpdateStats;
        public event RoutedEventHandler Done;
        public event RoutedEventHandler DoneAndUpdate;
        public event RoutedEventHandler DoneUpdateAndClean;
        public event RoutedEventHandler DoneAndUpdateStyle;

        public UC_RunBook_Item()
        {
            InitializeComponent();

            resourceLoader = ResourceLoader.GetForCurrentView();

            Item = new RunBookItem();
            Status = RunBookItem.Status.INI;
            IsAllowedStart = true;
            IsAllowedPause = false;
            IsAllowedStop = false;

            Succeed = 0;
            Failed = 0;

            SB_DoWork_Done.Completed += SB_DoWork_Done_Completed;
        }

        private void SetCategoryControlsStyle(string color)
        {
            BRD_Category.Background = new SolidColorBrush(ColorHandler.FromHex(color));
            BRD_Category.BorderBrush = new SolidColorBrush(ColorHandler.FromHex(color));
        }

        private async void SetStatusControlsStyle()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                switch (Status)
                {
                    case RunBookItem.Status.INI:
                        SP_Status.Background = new SolidColorBrush(ColorHandler.FromHex("#FFF5F5F5"));
                        BRD_BTN_Done.BorderBrush = new SolidColorBrush(ColorHandler.FromHex("#35000000"));
                        TB_Status.Foreground = new SolidColorBrush(ColorHandler.FromHex("#FF333333"));
                        TB_Status.Text = "NOT STARTED";

                        IsAllowedStart = true;
                        IsAllowedPause = false;
                        IsAllowedStop = true;

                        IMG_Media_Start.Opacity = 1;
                        IMG_Media_Pause.Opacity = .21;
                        IMG_Media_Stop.Opacity = 1;
                        break;
                    case RunBookItem.Status.RUN:
                        SP_Status.Background = new SolidColorBrush(ColorHandler.FromHex("#FFC90000"));
                        BRD_BTN_Done.BorderBrush = new SolidColorBrush(ColorHandler.FromHex("#FFFFFFFF"));
                        TB_Status.Foreground = new SolidColorBrush(ColorHandler.FromHex("#FFFFFFFF"));
                        TB_Status.Text = "RUNNING";

                        IsAllowedStart = false;
                        IsAllowedPause = true;
                        IsAllowedStop = true;

                        IMG_Media_Start.Opacity = .21;
                        IMG_Media_Pause.Opacity = 1;
                        IMG_Media_Stop.Opacity = 1;
                        break;
                    case RunBookItem.Status.PAU:
                        SP_Status.Background = new SolidColorBrush(ColorHandler.FromHex("#FFF5F5F5"));
                        BRD_BTN_Done.BorderBrush = new SolidColorBrush(ColorHandler.FromHex("#35000000"));
                        TB_Status.Foreground = new SolidColorBrush(ColorHandler.FromHex("#FF333333"));
                        TB_Status.Text = "PAUSED";

                        IsAllowedStart = true;
                        IsAllowedPause = false;
                        IsAllowedStop = true;

                        IMG_Media_Start.Opacity = 1;
                        IMG_Media_Pause.Opacity = .21;
                        IMG_Media_Stop.Opacity = 1;
                        break;
                    case RunBookItem.Status.WAI:
                        SP_Status.Background = new SolidColorBrush(ColorHandler.FromHex("#FFF5F5F5"));
                        BRD_BTN_Done.BorderBrush = new SolidColorBrush(ColorHandler.FromHex("#35000000"));
                        TB_Status.Foreground = new SolidColorBrush(ColorHandler.FromHex("#FF333333"));
                        TB_Status.Text = "HOLDING";

                        IsAllowedStart = false;
                        IsAllowedPause = false;
                        IsAllowedStop = true;

                        IMG_Media_Start.Opacity = 1;
                        IMG_Media_Pause.Opacity = .21;
                        IMG_Media_Stop.Opacity = 1;
                        break;
                    case RunBookItem.Status.STP:
                        SP_Status.Background = new SolidColorBrush(ColorHandler.FromHex("#FFF5F5F5"));
                        BRD_BTN_Done.BorderBrush = new SolidColorBrush(ColorHandler.FromHex("#35000000"));
                        TB_Status.Foreground = new SolidColorBrush(ColorHandler.FromHex("#FF333333"));
                        TB_Status.Text = "CANCELED";

                        IsAllowedStart = false;
                        IsAllowedPause = false;
                        IsAllowedStop = false;

                        IMG_Media_Start.Opacity = .21;
                        IMG_Media_Pause.Opacity = .21;
                        IMG_Media_Stop.Opacity = .21;
                        break;
                    case RunBookItem.Status.FIN:
                        SP_Status.Background = new SolidColorBrush(ColorHandler.FromHex("#FF2F5F00"));
                        BRD_BTN_Done.BorderBrush = new SolidColorBrush(ColorHandler.FromHex("#FFFFFFFF"));
                        TB_Status.Foreground = new SolidColorBrush(ColorHandler.FromHex("#FFFFFFFF"));
                        TB_Status.Text = "FINISHED";

                        IsAllowedStart = false;
                        IsAllowedPause = false;
                        IsAllowedStop = false;

                        IMG_Media_Start.Opacity = .21;
                        IMG_Media_Pause.Opacity = .21;
                        IMG_Media_Stop.Opacity = .21;
                        break;
                    default:
                        break;
                }
            });
        }

        public void BindData()
        {
            ElapsedTime = 0;
            AverageTime = 0;
            DeliveredPercentage = 0;
            Total = 0;
            Succeed = 0;
            Failed = 0;

            switch (Item.D2CMessagesKind)
            {
                case D2CMessagesConfig.Kind.REG:
                    IMG_Category.Source = new BitmapImage(new Uri(resourceLoader.GetString("Map_Actions_REG_IMG_B")));
                    SetCategoryControlsStyle("#FFC90000");
                    break;
                case D2CMessagesConfig.Kind.TEL:
                    IMG_Category.Source = new BitmapImage(new Uri(resourceLoader.GetString("Map_Actions_TEL_IMG_B")));
                    SetCategoryControlsStyle("#FF535353");
                    break;
                case D2CMessagesConfig.Kind.ALR:
                    IMG_Category.Source = new BitmapImage(new Uri(resourceLoader.GetString("Map_Actions_ALR_IMG_B")));
                    SetCategoryControlsStyle("#FFA4A4A4");
                    break;
                case D2CMessagesConfig.Kind.BREG:
                    IMG_Category.Source = new BitmapImage(new Uri(resourceLoader.GetString("Map_Actions_REG_IMG_B")));
                    SetCategoryControlsStyle("#FFC90000");
                    break;
                case D2CMessagesConfig.Kind.BTEL:
                    IMG_Category.Source = new BitmapImage(new Uri(resourceLoader.GetString("Map_Actions_TEL_IMG_B")));
                    SetCategoryControlsStyle("#FF535353");
                    break;
                default:
                    IMG_Category.Source = new BitmapImage(new Uri(resourceLoader.GetString("Map_Actions_ALR_IMG_B")));
                    SetCategoryControlsStyle("#FFA4A4A4");
                    break;
            }

            Total = Item.NMessages;

            TB_Title.Text = Item.D2CMessagesKindDesc.ToUpperInvariant();
            TB_Environment.Text = Item.EnvironmentDesc;
            TB_Project.Text = Item.Project.Name;
            TB_NMessages.Text = string.Format(CultureInfo.InvariantCulture, "{0:### ### ###.#}", Item.NMessages).Trim();
            TB_N_Runs.Text = string.Format(CultureInfo.InvariantCulture, "{0:### ### ###.#}", Item.NRuns).Trim();
            TB_N_Seconds.Text = Item.NSeconds.ToString(CultureInfo.InvariantCulture);

            SetStatusControlsStyle();
        }

        private async void DoWork()
        {
            int index = 1;

            KeyValuePair<bool, string> result = new KeyValuePair<bool, string>();

            double stopAt = Math.Floor(Item.Devices.Count / Item.NRuns);

            foreach (var device in Item.Devices)
            {
                if (Status.Equals(RunBookItem.Status.STP))
                {
                    Status = RunBookItem.Status.STP;
                    SetStatusControlsStyle();
                    return;
                }

                Stopwatch timer = new Stopwatch();

                timer.Start();

                while (Status.Equals(RunBookItem.Status.PAU))
                {
                    await Task.Run(() => Thread.Sleep(1000)).ConfigureAwait(false);

                    Debug.WriteLine("PAUSED...");
                }

                if (index % stopAt == 0)
                {
                    Status = RunBookItem.Status.WAI;
                    SetStatusControlsStyle();

                    await Task.Run(() => Thread.Sleep((int)Item.NSeconds * 1000)).ConfigureAwait(false);

                    Status = RunBookItem.Status.RUN;
                    SetStatusControlsStyle();
                }

                switch (Item.D2CMessagesKind)
                {
                    case D2CMessagesConfig.Kind.REG:
                        result = ExedraLibCoreHandler.RegisterDevice(device.DeviceName, new BasicGeoposition() { Latitude = device.DevicePosition.Latitude, Longitude = device.DevicePosition.Longitude }, Item.Environment);
                        break;
                    case D2CMessagesConfig.Kind.TEL:
                        result = ExedraLibCoreHandler.SendDimmingProfile(device.DeviceID, DateTime.Now, 100, 0, 0, 0, 0, Item.Environment);
                        break;
                    case D2CMessagesConfig.Kind.ALR:
                        result = ExedraLibCoreHandler.SendAlarm(device.DeviceID, DateTime.Now, 0, Item.Environment);
                        break;
                    case D2CMessagesConfig.Kind.BTEL:
                        result = ExedraLibCoreHandler.SendDimmingProfile(device.DeviceID, DateTime.Now, 100, 0, 0, 0, 0, Item.Environment);
                        break;
                    case D2CMessagesConfig.Kind.BALR:
                        result = ExedraLibCoreHandler.SendAlarm(device.DeviceID, DateTime.Now, 0, Item.Environment);
                        break;
                    case D2CMessagesConfig.Kind.BREG:
                        result = ExedraLibCoreHandler.RegisterDevice(device.DeviceName, new BasicGeoposition() { Latitude = device.DevicePosition.Latitude, Longitude = device.DevicePosition.Longitude }, Item.Environment);
                        break;
                    default:
                        Debug.WriteLine($"DoWork() -> {Item.D2CMessagesKind}");
                        break;
                }

                if (!result.Key)
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                    () =>
                    {
                        Failed++;
                        TB_N_Failed.Text = string.Format(CultureInfo.InvariantCulture, "{0:### ### ###.#}", Failed).Trim();
                    });
                }
                else
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                    () =>
                    {
                        Succeed++;
                        TB_N_Succeed.Text = string.Format(CultureInfo.InvariantCulture, "{0:### ### ###.#}", Succeed).Trim();
                    });
                    
                    Debug.WriteLine(result.Value);
                }                

                timer.Stop();

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                () =>
                {
                    ElapsedTime += timer.ElapsedMilliseconds;
                    AverageTime = ElapsedTime / index;
                    DeliveredPercentage = Math.Round((double)index / Item.NMessages * 100, 0);
                    double elapsedTime = Math.Round((double)ElapsedTime / 1000, 2);

                    TB_N_Delivered.Text = string.Format(CultureInfo.InvariantCulture, "{0:###}", DeliveredPercentage);
                    TB_N_AVG.Text = TextHandler.GetTimeFormattedFromMilseconds(AverageTime);
                    TB_N_Elapsed.Text = TextHandler.GetTimeFormattedFromMilseconds(ElapsedTime);                   
                });

                index++;
            }
            
            Status = RunBookItem.Status.FIN;
            SetStatusControlsStyle();

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                SB_DoWork_Done.Begin();
            });
        }

        private void CreateDevicesForBulkRegistration()
        {
            List<BasicGeoposition> coordinates = Item.Project.Coordinates;

            BasicGeoposition centroid = GeopositionHandler.GetPolygonCentroid(coordinates);
            BasicGeoposition northwest = GeopositionHandler.GetNorthWestPosition(coordinates, centroid);
            BasicGeoposition southeast = GeopositionHandler.GetSouthEastPosition(coordinates, centroid);

            var projectName = Item.Project.Name.Substring(0, 3).ToUpper(CultureInfo.InvariantCulture);
            var length = 16;

            Item.Devices.Clear();

            for (int i = 0; i < Item.NMessages; i++)
            {
                var now = DateTime.Now;

                var datetime = now.Ticks.ToString(CultureInfo.InvariantCulture);
                var deviceID = projectName + i + datetime.Substring(datetime.Length - length, length);

                Windows.Devices.Geolocation.BasicGeoposition devicePosition = new Windows.Devices.Geolocation.BasicGeoposition()
                {
                    Latitude = MathHandler.GetRandomNumber(southeast.Latitude, northwest.Latitude),
                    Longitude = MathHandler.GetRandomNumber(northwest.Longitude, southeast.Longitude),
                };

                Item.Devices.Add(new Device(deviceID, devicePosition));
            }
        }

        private async void Run()
        {
            if (Item.D2CMessagesKind.Equals(D2CMessagesConfig.Kind.BREG))
            {
                CreateDevicesForBulkRegistration();
            }

            await Task.Run(() => DoWork()).ConfigureAwait(false);
        }

        private void BRD_Play_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Status.Equals(RunBookItem.Status.RUN) || !IsAllowedStart) return;

            if (!Status.Equals(RunBookItem.Status.PAU))
            {
                Run();
            }

            Status = RunBookItem.Status.RUN;
            SetStatusControlsStyle();            
        }

        private void BRD_Pause_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Status.Equals(RunBookItem.Status.PAU) || !IsAllowedPause) return;

            Status = RunBookItem.Status.PAU;
            SetStatusControlsStyle();
        }

        private void BRD_Stop_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Status.Equals(RunBookItem.Status.STP) || !IsAllowedStop) return;

            Status = RunBookItem.Status.STP;
            SetStatusControlsStyle();
        }

        private void SB_DoWork_Done_Completed(object sender, object e)
        {
            if (UpdateStats != null)
            {
                UpdateStats?.Invoke(this, null);
            }

            switch (Item.D2CMessagesKind)
            {
                case D2CMessagesConfig.Kind.REG:
                    if (DoneUpdateAndClean != null)
                    {
                        DoneUpdateAndClean?.Invoke(this, null);
                    }
                    break;
                case D2CMessagesConfig.Kind.TEL:
                    if (DoneAndUpdateStyle != null)
                    {
                        DoneAndUpdateStyle?.Invoke(this, null);
                    }
                    break;
                case D2CMessagesConfig.Kind.ALR:
                    if (DoneAndUpdateStyle != null)
                    {
                        DoneAndUpdateStyle?.Invoke(this, null);
                    }
                    break;
                case D2CMessagesConfig.Kind.BREG:
                    if (DoneAndUpdate != null)
                    {
                        DoneAndUpdate?.Invoke(this, null);
                    }
                    break;
                default:
                    if (Done != null)
                    {
                        Done?.Invoke(this, null);
                    }
                    break;
            }

            Debug.WriteLine($"UC_RunBook_Item -> SB_DoWork_Done_Completed -> {Item.D2CMessagesKind}");
        }
    }
}
