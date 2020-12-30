using G4Studio.Utils;
using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views.UIElements
{
    public sealed partial class UCActionsItem : UserControl
    {
        private static ResourceLoader resourceLoader;

        public string ID { get; set; }
        public string IMGSource { get; set; }
        public string IMGAltSource { get; set; }
        public bool IsSelected { get; set; }

        public event RoutedEventHandler Show;
        public event RoutedEventHandler Hide;

        public UCActionsItem()
        {
            InitializeComponent();

            IsSelected = false;
            resourceLoader = ResourceLoader.GetForCurrentView();
        }

        public void BindData()
        {
            if (string.IsNullOrEmpty(IMGSource))
            {
                IMGSource = resourceLoader.GetString("DEFAULTS_IMG_Tiles");
            }

            if (string.IsNullOrEmpty(IMGAltSource))
            {
                IMGAltSource = resourceLoader.GetString("DEFAULTS_IMG_Tiles_Alt");
            }

            IMG_Action.Source = new BitmapImage() { UriSource = new Uri(IMGSource) };
        }

        public void Reset()
        {
            IsSelected = false;
            BRD_Action.Background = new SolidColorBrush(ColorHandler.FromHex("#7FFFFFFF"));
            IMG_Action.Source = new BitmapImage() { UriSource = new Uri(IMGSource) };
        }

        private void SetImage()
        {
            if (IsSelected)
            {
                BRD_Action.Background = new SolidColorBrush(ColorHandler.FromHex("#FF333333"));
                IMG_Action.Source = new BitmapImage() { UriSource = new Uri(IMGAltSource) };
            }
            else
            {
                BRD_Action.Background = new SolidColorBrush(ColorHandler.FromHex("#7FFFFFFF"));
                IMG_Action.Source = new BitmapImage() { UriSource = new Uri(IMGSource) };
            }
        }

        private void BRD_Action_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (!IsSelected)
            {
                BRD_Action.Background = new SolidColorBrush(ColorHandler.FromHex("#FFFFFFFF"));
            }
        }

        private void BRD_Action_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (!IsSelected)
            {
                BRD_Action.Background = new SolidColorBrush(ColorHandler.FromHex("#7FFFFFFF"));
            }
        }

        private void BRD_Action_Tapped(object sender, TappedRoutedEventArgs e)
        {
            IsSelected = !IsSelected;

            SetImage();

            if (IsSelected)
            {
                if (Show != null)
                {
                    Show?.Invoke(this, e);
                }
            }
            else
            {
                if (Hide != null)
                {
                    Hide?.Invoke(this, e);
                }
            }
        }
    }
}
