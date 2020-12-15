using G4Studio.Models;
using G4Studio.Utils;
using Hyperion.Platform.Tests.Core.ExedraLib.Models;
using System;
using System.Globalization;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views
{
    public sealed partial class UC_Devices_Item : UserControl
    {
        private static ResourceLoader resourceLoader;

        private bool IsSelected { get; set; }
        //public Twin _Twin { get; set; }
        public Twin Device { get; set; }
        public string SelectedItemColor { get; set; }

        public string DeviceName { get; set; }
        private string Type { get; set; }

        public double ItemWidth { get; set; }
        public double ItemHeight { get; set; }


        public event RoutedEventHandler ItemSelected;
        public event RoutedEventHandler ItemDeselected;

        public UC_Devices_Item()
        {
            this.InitializeComponent();

            resourceLoader = ResourceLoader.GetForCurrentView();

            IsSelected = false;
            SelectedItemColor = "#00FFFFFF";

            ItemWidth = 161;
            ItemHeight = 51;
        }

        public void BindData(Twin device)
        {
            //if (twin == null)
            //    return;

            if (device == null) return;

            BRD_Main.Width = ItemWidth;
            BRD_Main.Height = ItemHeight;

            DeviceName = device.DeviceID;

            //_Twin = twin;
            Device = device;

            TB_Name.Text = Device.DeviceID.ToUpper(CultureInfo.InvariantCulture);
            //TB_Type.Text = string.Format("{0}", _Twin.Tags["device_type"]);
            //TB_IMEI.Text = string.Format("{0}", _Twin.Tags["imei"]);

            //foreach (KeyValuePair<string, object> item in twin.Properties.Desired)
            //{
            //    if (item.Key.Equals("location", StringComparison.InvariantCulture))
            //    {
            //        Position position = JsonConvert.DeserializeObject<Position>(item.Value.ToString());

            //        var latitude = position.latitude.ToString(CultureInfo.InvariantCulture);
            //        var longitude = position.longitude.ToString(CultureInfo.InvariantCulture);

            //        TB_Position.Text = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", latitude.Substring(0, Math.Min(6, latitude.Length)), longitude.Substring(0, Math.Min(6, longitude.Length)));
            //    }
            //}

            var latitude = Device.DevicePosition.Latitude.ToString(CultureInfo.InvariantCulture);
            var longitude = Device.DevicePosition.Longitude.ToString(CultureInfo.InvariantCulture);

            TB_Position.Text = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", latitude.Substring(0, Math.Min(6, latitude.Length)), longitude.Substring(0, Math.Min(6, longitude.Length)));
        }

        private void UserControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            IsSelected = !IsSelected;

            if (IsSelected)
            {
                SetItemStyle(true);                

                if (ItemSelected != null)
                {
                    ItemSelected?.Invoke(sender, null);
                }
            }
            else
            {
                SetItemStyle(false);

                if (ItemDeselected != null)
                {
                    ItemDeselected?.Invoke(sender, null);
                }
            }
        }

        public void SetItemStyle(bool isEnabled)
        {
            if (isEnabled)
            {
                BRD_Main.Background = new SolidColorBrush(ColorHandler.FromHex(resourceLoader.GetString("BTN_Action_Pointer_Entered_BGColor")));
                //IMG_Default.Visibility = Visibility.Collapsed;
                //IMG_Selected.Visibility = Visibility.Visible;

                IsSelected = true;
            }
            else
            {
                BRD_Main.Background = new SolidColorBrush(Colors.Transparent);
                //IMG_Default.Visibility = Visibility.Visible;
                //IMG_Selected.Visibility = Visibility.Collapsed;

                IsSelected = false;
            }   
        }
    }
}
