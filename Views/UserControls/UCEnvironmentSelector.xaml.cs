using G4Studio.Utils;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views.UserControls
{
    public sealed partial class UCEnvironmentSelector : UserControl
    {
        public UCEnvironmentSelector()
        {
            this.InitializeComponent();

            SB_Intro.Completed += SB_Intro_Completed;
        }

        public void StartAnim()
        {
            SB_Intro.Begin();
        }

        private void SB_Intro_Completed(object sender, object e)
        {
            SP_DEV.Opacity = 100;
            SP_TST.Opacity = 100;
        }

        private void SP_DEV_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Debug.WriteLine("SP_DEV_Tapped");
        }

        private void SP_TST_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Debug.WriteLine("SP_TST_Tapped");
        }
    }
}
