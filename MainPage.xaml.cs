using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace G4Studio
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            UCLandingPage.FinishedWork += UC_LandingPage_FinishedWork;


            //VER UC_LANDINGPAGE DESCOMENTAR SB
            //UCLandingPage.Visibility = Visibility.Collapsed;
        }

        private void UC_LandingPage_FinishedWork(object sender, RoutedEventArgs e)
        {
            UCLandingPage.Visibility = Visibility.Collapsed;

            UCMapControl.StartAnimation(UCLandingPage.EnvironmentT);
        }
    }
}
