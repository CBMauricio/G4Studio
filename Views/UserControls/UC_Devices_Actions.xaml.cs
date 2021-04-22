using G4Studio.Models;
using G4Studio.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views.UserControls
{
    public sealed partial class UC_Devices_Actions : UserControl
    {
        #region Private Members
        private static ResourceLoader resourceLoader;
        private bool IsShowing { get; set; }
        private bool IsShowingDevices { get; set; }
        private bool HasLoaded { get; set; }
        private int NColumns { get; set; }
        private double ItemWidth { get; set; }
        private double ItemHeight { get; set; }
        private double DefaultMarginRight { get; set; }
        private double DefaultMarginTop { get; set; }
        private string DefaultProjectColor { get; set; }
        #endregion

        #region Public Members
        public D2CMessagesConfig.Category D2CMessagesCategory { get; set; }
        public D2CMessagesConfig.Kind D2CMessagesKind { get; set; }
        public List<Device> Devices { get; }
        
        public string RemovedDeviceID { get; set; }  
        public bool IsEditingAllowed { get; set; }
        public bool AvoidTextValidation { get; set; }
        public bool AllowToExpandDevices { get; set; }
        public bool IsTappedWithoutDevicesAllowed { get; set; }
        public bool HasLoadedDevices { get; set; }
        public double BaseNDevices { get; set; }
        public double NMessages { get; set; }
        public double NRuns { get; set; }
        public double NSeconds { get; set; }
        #endregion

        #region Routed Events
        public event RoutedEventHandler Focused;
        public event RoutedEventHandler DeviceRemoved;
        public event RoutedEventHandler DoWork;
        #endregion

        public UC_Devices_Actions()
        {
            InitializeComponent();

            resourceLoader = ResourceLoader.GetForCurrentView();

            HasLoaded = false;

            D2CMessagesCategory = D2CMessagesConfig.Category.REG;
            D2CMessagesKind = D2CMessagesConfig.Kind.REG;
            
            Devices = new List<Device>();

            BaseNDevices = 0;

            AvoidTextValidation = true;
            HasLoadedDevices = false;
            IsEditingAllowed = true;
            IsShowing = false;
            IsShowingDevices = false;
            AllowToExpandDevices = false;
            IsTappedWithoutDevicesAllowed = false;

            ItemWidth = 282;
            ItemHeight = 65;
            NColumns = 1;
            DefaultMarginRight = 2;
            DefaultMarginTop = 1;
            DefaultProjectColor = "#FF333333";

            SB_Hide.Completed += SB_Hide_Completed;
            SB_Hide_Devices.Completed += SB_Hide_Devices_Completed;
        }

        public void BindData()
        {            
            TB_NMessages.IsReadOnly = !IsEditingAllowed;
            BRD_Expand.Visibility = AllowToExpandDevices ? Visibility.Visible : Visibility.Collapsed;

            switch (D2CMessagesCategory)
            {
                case D2CMessagesConfig.Category.REG:
                    IMG_Action.Source = new BitmapImage(new Uri(resourceLoader.GetString("Map_Actions_REG_IMG_B")));
                    SL_Type.IsEnabled = false;
                    D2CMessagesKind = D2CMessagesConfig.Kind.REG;
                    break;
                case D2CMessagesConfig.Category.TEL:
                    IMG_Action.Source = new BitmapImage(new Uri(resourceLoader.GetString("Map_Actions_TEL_IMG_B")));
                    SL_Type.Value = 2;
                    SL_Type.Minimum = 2;
                    D2CMessagesKind = D2CMessagesConfig.Kind.TEL;
                    break;
                default:
                    IMG_Action.Source = new BitmapImage(new Uri(resourceLoader.GetString("Map_Actions_BLK_IMG_B")));
                    SL_Type.Value = 1;
                    D2CMessagesKind = D2CMessagesConfig.Kind.BREG;
                    break;
            }

            HasLoaded = true;
        }

        public void AddDevice(string deviceID, string deviceName, Windows.Devices.Geolocation.BasicGeoposition position)
        {
            if (Devices.Exists(e => e.DeviceID.Equals(deviceID))) return;            

            Windows.Devices.Geolocation.BasicGeoposition snPosition = new Windows.Devices.Geolocation.BasicGeoposition { Latitude = position.Latitude, Longitude = position.Longitude };

            Devices.Insert(0, new Device { DeviceID = deviceID, DeviceName = deviceName, DevicePosition = snPosition, Latitude = snPosition.Latitude, Longitude = snPosition.Longitude });

            ListDevices();
        }

        public void AddDeviceButNoListings(string deviceID, string deviceName, Windows.Devices.Geolocation.BasicGeoposition position)
        {
            if (Devices.Exists(e => e.DeviceID.Equals(deviceID))) return;

            Windows.Devices.Geolocation.BasicGeoposition snPosition = new Windows.Devices.Geolocation.BasicGeoposition { Latitude = position.Latitude, Longitude = position.Longitude };

            Devices.Insert(0, new Device { DeviceID = deviceID, DeviceName = deviceName, DevicePosition = snPosition, Latitude = snPosition.Latitude, Longitude = snPosition.Longitude });
        }

        public void Reset()
        {
            RemoveAllDevices();
            ClearAndHide();
        }

        public void RemoveDevice(string deviceID)
        {
            Device device = Devices.Single(i => i.DeviceID.Equals(deviceID));

            if (device is null) return;

            if (Devices.Count < 2)
            {
                ClearAndHide();

                return;
            }

            Devices.Remove(device);
            ListDevices();
        }        

        private void RemovedDevice(object sender, RoutedEventArgs e)
        {
            UC_Devices_Item item = sender as UC_Devices_Item;

            RemovedDeviceID = item.DeviceID;

            RemoveDevice(RemovedDeviceID);

            if (DeviceRemoved != null)
            {
                DeviceRemoved?.Invoke(this, null);
            }
        }

        private void RemoveAllDevices()
        {
            foreach (var item in Devices)
            {
                RemovedDeviceID = item.DeviceID;

                if (DeviceRemoved != null)
                {
                    DeviceRemoved?.Invoke(this, null);
                }
            }
        }

        public void CleanDevices()
        {
            if (!D2CMessagesCategory.Equals(D2CMessagesConfig.Category.BLK))
            {
                Devices.Clear();
            }
        }
        
        private void ListDevices()
        {
            double maxHeight = SP_Devices.MaxHeight - 58;

            GRD_Devices_List.Children.Clear();

            int column = 0;
            int line = 0;

            double marginLeft;
            double marginTop = 0;

            foreach (var item in Devices.OrderByDescending(i => i.DeviceName))
            {
                if (column < NColumns)
                {
                    marginLeft = column * ItemWidth + column * DefaultMarginRight;

                    column++;
                }
                else
                {
                    column = 1;
                    line++;

                    marginLeft = 0;
                    marginTop = line * ItemHeight + (line + 1) * DefaultMarginTop;
                }

                var device = new UC_Devices_Item()
                {
                    ItemWidth = ItemWidth,
                    ItemHeight = ItemHeight,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(marginLeft, marginTop, DefaultMarginRight, 0),
                    DeviceID = item.DeviceID,
                    DeviceName = item.DeviceName,
                    Latitude = item.Latitude.ToString(CultureInfo.InvariantCulture),
                    Longitude = item.Longitude.ToString(CultureInfo.InvariantCulture),
                    DefaultProjectColor = DefaultProjectColor
                };

                device.Remove += RemovedDevice;

                device.BindData();

                GRD_Devices_List.Children.Insert(0, device);
            }

            SV_Devices.Height = Math.Min(maxHeight, (Devices.Count * ItemHeight + (Devices.Count + 1) * DefaultMarginTop) + 3);

            TB_NMessages.Text = Devices.Count.ToString(CultureInfo.InvariantCulture);
            TB_NDevices.Text = Devices.Count.ToString(CultureInfo.InvariantCulture);

            BRD_Alert.Visibility = Devices.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            BRD_Expand.Visibility = AllowToExpandDevices ? Visibility.Visible : Visibility.Collapsed;
        }
        
        public void Show()
        {
            SB_Show.Begin();
        }

        public void Hide()
        {
            if (IsShowing)
            {
                SB_Hide.Begin();
            }

            if (IsShowingDevices)
            {
                SB_Hide_Devices.Begin();
            }
        }

        private void Clear()
        {
            GRD_Devices_List.Children.Clear();
            Devices.Clear();

            IsShowing = false;
            IsShowingDevices = false;
            HasLoadedDevices = false;

            ResetTextBoxes();
        }

        private void ResetTextBoxes()
        {
            TB_NMessages.Text = D2CMessagesKind.Equals(D2CMessagesConfig.Kind.BREG) || D2CMessagesKind.Equals(D2CMessagesConfig.Kind.REG) ? string.Empty : BaseNDevices.ToString(CultureInfo.InvariantCulture);
            TB_N_Runs.Text = string.Empty;
            TB_N_Seconds.Text = string.Empty;
        }

        public void ClearAndHide()
        {
            IsShowing = true;
            IsShowingDevices = true;

            Hide();
            Clear();
        }

        private void SB_Hide_Completed(object sender, object e)
        {
            IsShowing = false;

            BRD_Alert.Visibility = Visibility.Collapsed;
        }

        private void SB_Hide_Devices_Completed(object sender, object e)
        {
            IsShowingDevices = false;
        }

        private void On_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (!AvoidTextValidation)
            {
                args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
            }

            AvoidTextValidation = false;
        }

        private void BRD_Expand_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!IsShowingDevices)
            {
                SB_Show_Devices.Begin();
            }
            else
            {
                SB_Hide_Devices.Begin();
            }

            IsShowingDevices = !IsShowingDevices;
        }

        private void BRD_Action_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Devices.Count < 1 && !IsTappedWithoutDevicesAllowed) return;

            if (IsShowing)
            {
                Hide();
            }
            else
            {
                Show();

                if (Focused != null)
                {
                    Focused?.Invoke(this, null);
                }
            }

            IsShowing = !IsShowing;
        }

        private void BRD_Add_Tapped(object sender, TappedRoutedEventArgs e)
        {
            int nMessages = 0;
            int nRuns = 0;
            int nSeconds = 0;

            bool valid = int.TryParse(TextHandler.RemoveWhitespace(TB_NMessages.Text), out nMessages);
            valid = int.TryParse(TextHandler.RemoveWhitespace(TB_N_Runs.Text), out nRuns);
            valid = int.TryParse(TextHandler.RemoveWhitespace(TB_N_Seconds.Text), out nSeconds);

            if (nMessages < 1) return;

            NMessages = nMessages;
            NRuns = Math.Max(Math.Min(nRuns, nMessages), 1);
            NSeconds = NRuns > 1 ? Math.Max(0, nSeconds) : 0;

            Hide();
            ResetTextBoxes();

            if (DoWork != null)
            {
                DoWork?.Invoke(this, null);
            }
        }

        private void BRD_Close_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Reset();
        }

        private void BRD_Action_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            BRD_Action.Background = new SolidColorBrush(ColorHandler.FromHex("#FFFFFFFF"));
        }

        private void BRD_Action_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            BRD_Action.Background = new SolidColorBrush(ColorHandler.FromHex("#B2FFFFFF"));
        }

        private void SL_Type_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (!HasLoaded) return;

            AvoidTextValidation = true;

            switch (e.NewValue)
            {
                case 1:
                    TB_Step.Text = "REGISTRATIONS";
                    D2CMessagesKind = D2CMessagesConfig.Kind.REG;
                    break;
                case 2:
                    TB_Step.Text = "D2C TELEMETRY";
                    D2CMessagesKind = D2CMessagesConfig.Kind.TEL;
                    break;
                default:
                    TB_Step.Text = "D2C ALARMS";
                    D2CMessagesKind = D2CMessagesConfig.Kind.ALR;
                    break;
            }

            if (D2CMessagesCategory.Equals(D2CMessagesConfig.Category.BLK))
            {
                if (e.NewValue > 1)
                {
                    D2CMessagesKind = e.NewValue.Equals(2) ? D2CMessagesConfig.Kind.BTEL :  D2CMessagesConfig.Kind.BALR;
                    TB_NMessages.IsReadOnly = true;
                    TB_NMessages.Text = string.Format(CultureInfo.InvariantCulture, "{0:### ### ###.#}", BaseNDevices);
                }
                else
                {
                    D2CMessagesKind = D2CMessagesConfig.Kind.BREG;
                    TB_NMessages.Text = "0";
                    TB_NMessages.IsReadOnly = false;
                }
            }
        }
    }
}
