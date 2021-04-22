using G4Studio.Utils;
using Hyperion.Platform.Tests.Core.ExedraLib;
using Hyperion.Platform.Tests.Core.ExedraLib.Config;
using Hyperion.Platform.Tests.Core.ExedraLib.Models;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views.UIElements
{
#pragma warning disable CA1707 // Identifiers should not contain underscores
    public sealed partial class UC_Environment_Item : UserControl
#pragma warning restore CA1707 // Identifiers should not contain underscores
    {
        public EnvironmentHandler.Type EnvironmentT { get; set; }
        public TenantIList Tenants { get; set; }
        public string Title { get; set; }
        public string HostName { get; set; }
        private double NTenants { get; set; }
        private double NDevices { get; set; }
        private int NGroupedTenants { get; set; }
        private int NGroupedDevices { get; set; }
        private double DefaultScoreBoxWidth { get; set; }
        private double DefaultScoreBoxHeight { get; set; }
        public bool IsSelected { get; set; }
        private bool LoadedProjects { get; set; }
        public bool Available { get; set; }
        public double BorderCornerRadius { get; set; }

        public event RoutedEventHandler ItemSelected;

        public UC_Environment_Item()
        {
            InitializeComponent();

            BorderCornerRadius = 150;
            DefaultScoreBoxWidth = 15;
            DefaultScoreBoxHeight = 9;
            NTenants = 0;
            NDevices = 0;
            NGroupedTenants = 30;
            NGroupedDevices = 2000;
            Available = false;
            IsSelected = false;
            LoadedProjects = false;
            EnvironmentT = EnvironmentHandler.Type.DEV;
            Tenants = new TenantIList();
        }

        public async void BindData()
        {
            BRD_Main.CornerRadius = new CornerRadius(BorderCornerRadius);

            TB_Environment.Text = string.IsNullOrEmpty(Title) ? string.Empty : Title;
            TB_HostName.Text = string.IsNullOrEmpty(HostName) ? string.Empty : HostName;

            if (!LoadedProjects)
            {
                CTRL_Animation_Tenants.Start();
                CTRL_Animation_Devices.Start();

                await Task.Run(() => DoWork()).ConfigureAwait(false);
            }
        }

        private async void DoWork()
        {
            try
            {
                Tenants = Task.Run(async () => await ExedraLibCoreHandler.GetTenantsAsync(EnvironmentT).ConfigureAwait(true)).Result;

                LoadedProjects = true;

                foreach (var tenant in Tenants.Tenants)
                {
                    NDevices += tenant.NDevices;
                }

                NTenants = Tenants.Tenants.Count;

                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    TB_NTenants.Text = string.Format(CultureInfo.InvariantCulture, "{0:### ### ###}", NTenants).Trim();
                    TB_NDevices.Text = string.Format(CultureInfo.InvariantCulture, "{0:### ### ###}", NDevices).Trim();

                    UIElementsHandler.DrawScoreElements(GRD_NTenants, 5, 5, NTenants / NGroupedTenants, DefaultScoreBoxWidth, DefaultScoreBoxHeight);
                    UIElementsHandler.DrawScoreElements(GRD_NDevices, 5, 5, NDevices / NGroupedDevices, DefaultScoreBoxWidth, DefaultScoreBoxHeight);

                    CTRL_Animation_Tenants.Stop();
                    CTRL_Animation_Devices.Stop();
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine($"UC_Environment_Item::DoWork()::{e.Message}");
            }
            
        }

        public void Clean()
        {
            IsSelected = false;
            SetStyleTextboxes(false);
            SetStyleScoreElements(GRD_NTenants, false, (int)NTenants / NGroupedTenants);
            SetStyleScoreElements(GRD_NDevices, false, (int)NDevices / NGroupedDevices);
        }   

        private void SetStyleTextboxes(bool isSelected)
        {
            BRD_Main.Background = isSelected ? new SolidColorBrush(ColorHandler.FromHex("#FF333333")) : new SolidColorBrush(ColorHandler.FromHex("#FFFFFFFF"));
            TB_Environment.Foreground = isSelected ? new SolidColorBrush(ColorHandler.FromHex("#F2FFFFFF")) : new SolidColorBrush(ColorHandler.FromHex("#FF333333"));
            TB_HostName.Foreground = isSelected ? new SolidColorBrush(ColorHandler.FromHex("#CCFFFFFF")) : new SolidColorBrush(ColorHandler.FromHex("#FF333333"));
            TB_NTenants.Foreground = isSelected ? new SolidColorBrush(ColorHandler.FromHex("#F2FFFFFF")) : new SolidColorBrush(ColorHandler.FromHex("#FF333333"));
            TB_NDevices.Foreground = isSelected ? new SolidColorBrush(ColorHandler.FromHex("#F2FFFFFF")) : new SolidColorBrush(ColorHandler.FromHex("#FF333333"));
            TB_NTenants_Lead.Foreground = isSelected ? new SolidColorBrush(ColorHandler.FromHex("#F2FFFFFF")) : new SolidColorBrush(ColorHandler.FromHex("#FF333333"));
            TB_NDevices_Lead.Foreground = isSelected ? new SolidColorBrush(ColorHandler.FromHex("#F2FFFFFF")) : new SolidColorBrush(ColorHandler.FromHex("#FF333333"));
        }

        private static void SetStyleScoreElements(Grid gridElement, bool isSelected, int nElements)
        {
            int i = 0;

            foreach (Rectangle child in gridElement.Children)
            {
                if (isSelected)
                {
                    child.Fill = i <= nElements ? new SolidColorBrush(ColorHandler.FromHex("#F2FFFFFF")) : new SolidColorBrush(ColorHandler.FromHex("#FF333333"));
                    child.Stroke = new SolidColorBrush(ColorHandler.FromHex("#AAFFFFFF"));
                }
                else
                {
                    child.Fill = i <= nElements ? new SolidColorBrush(ColorHandler.FromHex("#FF333333")) : new SolidColorBrush(ColorHandler.FromHex("#4CFFFFFF"));
                    child.Stroke = new SolidColorBrush(ColorHandler.FromHex("#4C333333"));
                }

                i++;
            }
        }

        private void BRD_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (IsSelected) return;

            Border border = sender as Border;
            border.Background = new SolidColorBrush(ColorHandler.FromHex("#0C000000"));
        }

        private void BRD_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (IsSelected) return;

            Border border = sender as Border;

            border.Background = IsSelected ? new SolidColorBrush(ColorHandler.FromHex("#FF333333")) : new SolidColorBrush(ColorHandler.FromHex("#00FFFFFF"));
        }

        private void BRD_Tapped(object sender, TappedRoutedEventArgs e)
        {
            IsSelected = true;

            SetStyleTextboxes(true);
            SetStyleScoreElements(GRD_NTenants, true, (int)NTenants / NGroupedTenants);
            SetStyleScoreElements(GRD_NDevices, true, (int)NDevices / NGroupedDevices);

            if (ItemSelected != null)
            {
                ItemSelected?.Invoke(this, null);
            }
        }
    }
}
