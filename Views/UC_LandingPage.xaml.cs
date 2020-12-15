using Hyperion.Platform.Tests.Core.ExedraLib.Config;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views
{
    public sealed partial class UC_LandingPage : UserControl
    {
        public event RoutedEventHandler FinishedWork;

        public EnvironmentHandler.Type EnvironmentT { get; set; }

        public UC_LandingPage()
        {
            this.InitializeComponent();

            SB_Intro.Begin();
        }

        private void PointerEnteredHandler(string type, bool show)
        {
            switch (type)
            {
                case "DEV": // DEV
                    IMG_DEV_1.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
                    IMG_DEV_2.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
                    break;
                default: // TST
                    IMG_TST_1.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
                    IMG_TST_2.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
                    break;
            }
        }

        private void BT_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            StackPanel stackPanel = sender as StackPanel;

            PointerEnteredHandler(stackPanel.Tag.ToString(), true);
        }

        private void BT_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            StackPanel stackPanel = sender as StackPanel;

            PointerEnteredHandler(stackPanel.Tag.ToString(), false);
        }

        private void BT_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            StackPanel stackPanel = sender as StackPanel;

            EnvironmentT = stackPanel.Tag.ToString().Equals("DEV", StringComparison.Ordinal) ? EnvironmentHandler.Type.DEV : EnvironmentHandler.Type.TST;

            FinishedWork?.Invoke(sender, null);
        }
    }
}
