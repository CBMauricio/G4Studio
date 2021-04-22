using G4Studio.Utils;
using Hyperion.Platform.Tests.Core.ExedraLib.Models;
using System;
using System.Globalization;
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
        public SolidColorBrush BGColor { get; set; }
        public SolidColorBrush BRDColor { get; set; }
        public Thickness BRDThickness { get; set; }

        public event RoutedEventHandler ItemTapped;

        public UC_Projects_Item()
        {
            this.InitializeComponent();

            Project = new Tenant();
        }

        public void BindData(Tenant project, int projectMaxSize)
        {
            Project = project;

            double projectSize = Math.Min(1, Math.Round((double)Project.NDevices / Math.Min(1500, projectMaxSize), 1)) * 180;

            BRD_Fence.Background = BGColor;
            TB_Project.Text = Project.Name ?? string.Empty;
            TB_HostName.Text = Project.Hostnames != null && Project.Hostnames.Count > 0 ? Project.Hostnames[0] : string.Empty;
            TB_Timezone.Text = Project.Timezone ?? string.Empty;
            TB_NDevices.Text = string.Format(CultureInfo.InvariantCulture, "{0:### ### ##0}", Project.NDevices).Trim();

            SetToolTip(TB_Project, TB_Project.Text);
            SetToolTip(TB_HostName, TB_HostName.Text);

            BRD_Fence.Width = projectSize;

            BRD_Main.Width = ItemWidth;
            BRD_Main.Height = ItemHeight;
        }

        private static void SetToolTip(TextBlock textblock, string text)
        {
            ToolTip toolTip = new ToolTip
            {
                Content = text
            };

            ToolTipService.SetToolTip(textblock, toolTip);
        }

        private void UserControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ItemTapped != null)
            {
                ItemTapped?.Invoke(sender, null);
            }
        }

        private void BRD_Main_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            BRD_Main.Background = new SolidColorBrush(ColorHandler.FromHex("#0C000000"));
        }

        private void BRD_Main_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            BRD_Main.Background = new SolidColorBrush(ColorHandler.FromHex("#00FFFFFF"));
        }
    }
}
