using G4Studio.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views
{
    public sealed partial class UC_Action_BTN : UserControl
    {
        private static ResourceLoader resourceLoader;

        public bool IsSelected { get; set; }

        public double ItemWidth { get; set; }
        public double ItemHeight { get; set; }

        public string Title { get; set; }
        public string Label { get; set; }
        public string ImageSource { get; set; }

        public event RoutedEventHandler ItemSelected;
        public event RoutedEventHandler ItemDeselected;

        public UC_Action_BTN()
        {
            this.InitializeComponent();

            resourceLoader = ResourceLoader.GetForCurrentView();

            IsSelected = false;

            Title = string.Empty;
            Label = string.Empty;

            ItemWidth = 40;
            ItemHeight = 40;
            ImageSource = "BTN_Action_Default";
        }

        public void BindaData()
        {
            BRD_BTN_Main.Width = ItemWidth;
            BRD_BTN_Main.Height = ItemHeight;

            TB_Label.Text = Label;

            if (string.IsNullOrEmpty(ImageSource))
            {
                //IMG_BTN.Visibility = Visibility.Collapsed;
                TB_Title.Text = Title;

                BRD_BTN_Main.BorderBrush = new SolidColorBrush(ColorHandler.FromHex("#FF363636"));
                BRD_BTN_Main.BorderThickness = new Thickness(0.5);
                BRD_BTN_Main.CornerRadius = new CornerRadius(3);
            }
            else
            {
                IMG_BTN.Source = new BitmapImage(new Uri(resourceLoader.GetString(ImageSource)));
            }
            
        }

        private void UserControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (IsSelected && ItemDeselected != null)
            {
                ItemDeselected?.Invoke(sender, null);
            }

            if (!IsSelected && ItemSelected != null)
            {
                ItemSelected?.Invoke(sender, null);
            }

            IsSelected = !IsSelected;
        }

        private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (!IsSelected)
                BRD_BTN_Main.Background = new SolidColorBrush(ColorHandler.FromHex(resourceLoader.GetString("BTN_Action_Pointer_Entered_BGColor")));
        }

        private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (!IsSelected)
                BRD_BTN_Main.Background = new SolidColorBrush(Colors.Transparent);
        }
    }
}
