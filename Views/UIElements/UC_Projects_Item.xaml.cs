using G4Studio.Models;
using Hyperion.Platform.Tests.Core.ExedraLib.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views
{
    public sealed partial class UC_Projects_Item : UserControl
    {
        public Tenant Project { get; set; } 
        public double ItemWidth { get; set; }
        public double ItemHeight { get; set; }
        public string Text { get; set; }
        public string NProjects { get; set; }
        public SolidColorBrush BGColor { get; set; }
        public SolidColorBrush BRDColor { get; set; }
        public Thickness BRDThickness { get; set; }

        public event RoutedEventHandler ItemTapped;

        public UC_Projects_Item()
        {
            this.InitializeComponent();

            Project = new Tenant();

            ItemWidth = 100;
            ItemHeight = 100;
        }

        public void BindData()
        {
            TB_Project.Text = Text;
            TB_NProjects.Text = NProjects;

            BRD_Main.Width = ItemWidth;
            BRD_Main.Height = ItemHeight;
            //BRD_Main.Background = BGColor;
            BRD_Main.BorderThickness = BRDThickness;
            BRD_Main.BorderBrush = BGColor;
        }

        private void UserControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ItemTapped != null)
            {
                ItemTapped?.Invoke(sender, null);
            }
        }
    }
}
