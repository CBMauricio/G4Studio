using G4Studio.Views.UIElements;
using Hyperion.Platform.Tests.Core.ExedraLib;
using Hyperion.Platform.Tests.Core.ExedraLib.Config;
using Hyperion.Platform.Tests.Core.ExedraLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views.UserControls
{
    public sealed partial class UCControlPanel : UserControl
    {
        public EnvironmentHandler.Type EnvironmentT { get; set; }
        private bool IsShowingInfo { get; set; }

        public List<Tenant> Projects { get; set; }
        public List<Twin> SelectedDevices { get; set; }

        public UCControlPanel()
        {
            InitializeComponent();

            IsShowingInfo = false;

            Projects = new List<Tenant>();
            SelectedDevices = new List<Twin>();

            SB_ShowInfo.Completed += SB_ShowInfo_Completed;
            SB_HideInfo.Completed += SB_HideInfo_Completed;

            UC_Selector_Environment.EnvironmentSelected += UC_Selector_Environment_EnvironmentSelected;
        }

        public void BindData()
        {
            UC_Selector_Environment.BindData();
        }

        public void Reset()
        {
            IsShowingInfo = false;
        }

        public void Show()
        {
            GRD_Main.Visibility = Visibility.Visible;
            SB_ShowInfo.Begin();
        }

        public void Hide()
        {
            SB_HideInfo.Begin();
        }

        private void GetProjects()
        {
            Projects = new List<Tenant>();

            TenantIList tenantsObj = Task.Run(async () => await ExedraLibCoreHandler.GetTenantsAsync(EnvironmentT).ConfigureAwait(false)).Result;

            Projects = tenantsObj.Tenants;

            Debug.WriteLine(tenantsObj.Logs.Value);
        }

        private void SB_ShowInfo_Completed(object sender, object e)
        {
            IsShowingInfo = true;
        }

        private void SB_HideInfo_Completed(object sender, object e)
        {
            IsShowingInfo = false;
            GRD_Main.Visibility = Visibility.Collapsed;
        }

        private void UC_Selector_Environment_EnvironmentSelected(object sender, RoutedEventArgs e)
        {
            UIETextItem item = sender as UIETextItem;

            EnvironmentT = item.ID.ToString().Equals("DEV", StringComparison.Ordinal) ? EnvironmentHandler.Type.DEV : EnvironmentHandler.Type.TST;

            GetProjects();
            UC_Selector_Projects.BindData(Projects);
        }

        private void BRD_NDevices_Select_All_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TB_FilterDevices.Text = string.Empty;
            SelectedDevices.Clear();

            foreach (UC_Devices_Item item in GRD_Devices_List.Children)
            {
                item.SetItemStyle(true);
                SelectedDevices.Add(item.Device);
            }
        }

        private void BRD_NDevices_UnSelect_All_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TB_FilterDevices.Text = string.Empty;
            SelectedDevices.Clear();

            foreach (UC_Devices_Item item in GRD_Devices_List.Children)
            {
                item.SetItemStyle(false);

                SelectedDevices.Remove(item.Device);
            }
        }

        private void TB_FilterDevices_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;

            foreach (UC_Devices_Item item in GRD_Devices_List.Children)
            {
                var index = item.Device.DeviceID.IndexOf(textbox.Text, StringComparison.InvariantCulture);

                if (index < 0)
                {
                    SelectedDevices.Remove(item.Device);
                    item.Opacity = 0.2;
                }
                else
                {
                    item.Opacity = 1.0;
                }
            }
        }
    }
}
