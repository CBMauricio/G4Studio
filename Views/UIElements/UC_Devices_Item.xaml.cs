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

        public string DefaultProjectColor { get; set; }

        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        private string Type { get; set; }

        public double ItemWidth { get; set; }
        public double ItemHeight { get; set; }


        public event RoutedEventHandler Remove;

        public UC_Devices_Item()
        {
            InitializeComponent();

            resourceLoader = ResourceLoader.GetForCurrentView();

            DefaultProjectColor = "#00FFFFFF";
        }

        public void BindData()
        {
            TB_Name.Text = DeviceName;

            TB_Position.Text = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", Latitude.Substring(0, Math.Min(11, Latitude.Length)), Latitude.Substring(0, Math.Min(11, Latitude.Length)));
            BRD_Main.Width = ItemWidth;
            BRD_Main.Height = ItemHeight;
            //BRD_DeviceType.Background = new SolidColorBrush(ColorHandler.FromHex(DefaultProjectColor));
        }


        private void BRD_Remove_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Remove != null)
            {
                Remove?.Invoke(this, null);
            }
        }

        private void BRD_Remove_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            TB_Remove.Foreground = new SolidColorBrush(ColorHandler.FromHex("#FFDC0505"));
        }

        private void BRD_Remove_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            TB_Remove.Foreground = new SolidColorBrush(ColorHandler.FromHex("#FF404040"));
        }
    }
}
