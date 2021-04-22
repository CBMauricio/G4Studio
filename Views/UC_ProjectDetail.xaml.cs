using G4Studio.Models;
using G4Studio.Utils;
using Hyperion.Platform.Tests.Core.ExedraLib.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views
{
    public sealed partial class UC_ProjectDetail : UserControl
    {
        private static ResourceLoader resourceLoader;

        private bool AllSelected { get; set; }

        public List<Twin> SelectedDevices { get; set; }
        public List<Twin> Twins { get; set; }
        public Twin SelectedDevice { get; set; }
        public Tenant Project { get; set; }
        public event RoutedEventHandler ItemSelected;
        public event RoutedEventHandler ItemDeselected;

        public event RoutedEventHandler ShowDetails;
        public event RoutedEventHandler HideDetails;
        public event RoutedEventHandler BTN_Action_ItemSelected;
        public event RoutedEventHandler BTN_Action_ItemDeSelected;
        public event RoutedEventHandler BTN_Action_SendMessages;

        public event RoutedEventHandler RegisterDevices_Bulk_PerformAction;
        public event RoutedEventHandler Telemetry_Bulk_PerformAction;
        public event RoutedEventHandler Alarms_Bulk_PerformAction;
        public event RoutedEventHandler DeleteDevices;

        public int NColumns { get; set; }
        public int NRows { get; set; }
        public double ItemWidth { get; set; }
        public double ItemHeight { get; set; }
        public double DefaultMargin { get; set; }
        public double DefaultMarginTop { get; set; }

        public string ActionToPerform { get; set; }

        private bool IsVisible { get; set; }

        public UC_ProjectDetail()
        {
            this.InitializeComponent();

            resourceLoader = ResourceLoader.GetForCurrentView();

            AllSelected = false;

            ItemWidth = 151;
            ItemHeight = 51;
            NColumns = 3;
            NRows = 10;
            DefaultMargin = 7;
            DefaultMarginTop = 7;


            Project = new Tenant();
            SelectedDevices = new List<Twin>();
            SelectedDevice = new Twin();
            Twins = new List<Twin>();


            IsVisible = true;


            UC_Project_Register_Bulk.ClearActions();
            UC_Project_Register_Bulk.AddActions(new List<UserControls.ItemAction>()
                    { 
                        //new UserControls.ItemAction("1", ""),
                        new UserControls.ItemAction("30", "30", ""),
                        new UserControls.ItemAction("50", "50", ""),
                        new UserControls.ItemAction("100", "100", ""),
                        new UserControls.ItemAction("250", "250", ""),
                        new UserControls.ItemAction("500", "500", ""),
                        new UserControls.ItemAction("1000", "1000", ""),
                        new UserControls.ItemAction("2000", "2K", ""),
                        new UserControls.ItemAction("5000", "5K", ""),
                        new UserControls.ItemAction("10000", "10K", "")
                    });
            UC_Project_Register_Bulk.PerformAction += UC_Project_Register_Bulk_PerformAction;
            UC_Project_Register_Bulk.BindData();


            UC_Project_Telemetry_Bulk.ClearActions();
            UC_Project_Telemetry_Bulk.AddActions(new List<UserControls.ItemAction>()
                    { 
                        //new UserControls.ItemAction("1", ""),
                        new UserControls.ItemAction("3", "SMALL", ""),
                        new UserControls.ItemAction("12", "MEDIUM", ""),
                        new UserControls.ItemAction("24", "ADVANCED", ""),
                        new UserControls.ItemAction("36", "MASSIVE", ""),
                        new UserControls.ItemAction("64", "MASSIVE", "")
                    });
            UC_Project_Telemetry_Bulk.PerformAction += UC_Project_Telemetry_Bulk_PerformAction;
            UC_Project_Telemetry_Bulk.BindData();

            UC_Project_Alarms_Bulk.ClearActions();
            UC_Project_Alarms_Bulk.AddActions(new List<UserControls.ItemAction>()
                    { 
                        //new UserControls.ItemAction("1", ""),
                        new UserControls.ItemAction("2", "SMALL", ""),
                        new UserControls.ItemAction("5", "MEDIUM", ""),
                        new UserControls.ItemAction("15", "ADVANCED", ""),
                        new UserControls.ItemAction("30", "MASSIVE", "")
                    });
            UC_Project_Alarms_Bulk.PerformAction += UC_Project_Alarms_Bulk_PerformAction;
            UC_Project_Alarms_Bulk.BindData();

            CTRL_Action_REG.BindaData();
            CTRL_Action_RREG.BindaData();
            CTRL_Action_UPD.BindaData();

            CTRL_Action_PHIGH.BindaData();
            CTRL_Action_PF.BindaData();
            CTRL_Action_PLOW.BindaData();

            CTRL_Action_TEL0.BindaData();
            CTRL_Action_TEL50.BindaData();
            CTRL_Action_TEL100.BindaData();

            CTRL_Action_UPD.ItemSelected += CTRL_Action_UPD_ItemSelected;
        }

        public void BindData(Tenant project, List<Twin> twins)
        {
            if (project != null)
            {
                Project = project;

                Twins = twins;

                TB_Title.Text = Project.Name;
                TB_Hostname.Text = Project.Hostnames[0];
                //TB_NProjects.Text = _Project.Items.Count.ToString(CultureInfo.InvariantCulture);
                TB_ProjectInfo_Main_NProjects.Text = twins.Count.ToString(CultureInfo.InvariantCulture);

                RC_Color.Fill = new SolidColorBrush(ColorHandler.FromHex(project.FillColor, project.FillOpacity * 100));
                RC_Color.Stroke = new SolidColorBrush(ColorHandler.FromHex(project.FillColor));
                

                BRD_NDevices_Top.BorderBrush = new SolidColorBrush(ColorHandler.FromHex(project.FillColor));
                //BRD_NDevices_Top.Background = new SolidColorBrush(ColorHandler.FromHex(project.FillColor, project.FillOpacity * 100));

                //BindData_Devices(project.FillColor);

                IsVisible = true;
            }
        }

        private void BindData_Devices(string projectColor)
        {
            //double maxHeight = BRD_Main.ActualHeight;
            //double offsetTop = 133 +  BRD_Devices_Info.ActualOffset.Y + SP_ProjectInfo.ActualOffset.Y + SV_Devices.ActualOffset.Y + BRD_Devices_Info.Margin.Bottom;

            //GRD_Devices_List.Children.Clear();
            //SelectedDevices.Clear();
            //SelectedDevice = new Twin();

            //int column = 0;
            //int line = 0;

            //double marginLeft;
            //double marginTop = DefaultMarginTop;


            //foreach (var item in Twins)
            //{
            //    if (column < NColumns)
            //    {
            //        marginLeft = column * ItemWidth + column * DefaultMargin;

            //        column++;
            //    }
            //    else
            //    {
            //        column = 1;
            //        line++;

            //        marginLeft = 0;
            //        marginTop = line * ItemHeight + (line + 1) * DefaultMarginTop;
            //    }

            //    var device = new UC_Devices_Item()
            //    {
            //        ItemWidth = ItemWidth,
            //        ItemHeight = ItemHeight,
            //        HorizontalAlignment = HorizontalAlignment.Left,
            //        VerticalAlignment = VerticalAlignment.Top,
            //        Margin = new Thickness(marginLeft, marginTop, DefaultMargin, 0),
            //        SelectedItemColor = projectColor
            //    };

            //    device.ItemSelected += Device_ItemSelected;
            //    device.ItemDeselected += Device_ItemDeselected;

            //    device.BindData(item);

            //    GRD_Devices_List.Children.Insert(0, device);
            //}

            ////var offset = SV_Devices.ActualOffset.X;
            ////var maxWidth = SP_ProjectInfo.ActualWidth;
            ////SV_Devices.Width = (NColumns * ItemWidth) + ((NColumns + 1) * DefaultMargin);
            ////SV_Devices.Height = (Math.Min(NRows, line) * ItemHeight) + (Math.Min(NRows, line) * DefaultMarginTop);

            //double width = NColumns * ItemWidth + NColumns * DefaultMargin;
            //SV_Devices.Height = maxHeight - offsetTop;
            //TB_FilterDevices.Width = width;
            ////BRD_NDevices_Top.Width = width / 2;
            ////BRD_NewDevices_Top.Width = width / 2;
        }

        private void Device_ItemSelected(object sender, RoutedEventArgs e)
        {
            //UC_Devices_Item item = sender as UC_Devices_Item;

            //SelectedDevices.Add(item.Device);
            //SelectedDevice = item.Device;

            //SetActionsBasedOnNumberOfSelectedDevices();

            //if (ItemSelected != null)
            //{
            //    ItemSelected?.Invoke(sender, null);
            //}
        }

        private void Device_ItemDeselected(object sender, RoutedEventArgs e)
        {
            //UC_Devices_Item item = sender as UC_Devices_Item;

            //SelectedDevices.Remove(item.Device);
            //SelectedDevice = item.Device;

            //SetActionsBasedOnNumberOfSelectedDevices();

            //if (ItemDeselected != null)
            //{
            //    ItemDeselected?.Invoke(sender, null);
            //}
        }

        private void SetActionsBasedOnNumberOfSelectedDevices()
        {
            BRD_Project_Actions_Main.Opacity = SelectedDevices.Count > 0 ? 1 : 0;
        }

        private void BD_Actions_Main_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SetCTRLVisibility();
        }

        public void SetCTRLVisibility()
        {
            if (IsVisible)
            {
                //BD_Actions_Main_Inner.Background = new SolidColorBrush(ColorHandler.FromHex(resourceLoader.GetString("BRD_Light_BGColor_UnSelected")));
                //IMG_Main.Source = new BitmapImage(new Uri(resourceLoader.GetString("BRD_Actions_Devices_IMG_UnSelected")));

                ////if (HideDetails != null)
                ////{
                ////    HideDetails?.Invoke(sender, null);
                ////}

                //SB_HideDetails.Begin();
            }
            else
            {
                //BD_Actions_Main_Inner.Background = new SolidColorBrush(ColorHandler.FromHex(resourceLoader.GetString("BRD_Light_BGColor_Selected")));
                //IMG_Main.Source = new BitmapImage(new Uri(resourceLoader.GetString("BRD_Actions_Devices_IMG_Selected")));

                ////if (ShowDetails != null)
                ////{
                ////    ShowDetails?.Invoke(sender, null);
                ////}
                //SB_ShowDetails.Begin();
            }

            IsVisible = !IsVisible;
        }

        private void CTRL_Action_ItemSelected(object sender, RoutedEventArgs e)
        {
            if (BTN_Action_ItemSelected != null)
            {
                BTN_Action_ItemSelected?.Invoke(sender, null);
            }
        }

        private void CTRL_Action_ItemDeselected(object sender, RoutedEventArgs e)
        {
            if (BTN_Action_ItemDeSelected != null)
            {
                BTN_Action_ItemDeSelected?.Invoke(sender, null);
            }
        }

        private void BRD_BTN_Main_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (BTN_Action_SendMessages != null)
            {
                BTN_Action_SendMessages?.Invoke(sender, null);
            }
        }

        private void UC_Project_Alarms_Bulk_PerformAction(object sender, RoutedEventArgs e)
        {
            ActionToPerform = UC_Project_Alarms_Bulk.SelectedValue;

            if (Alarms_Bulk_PerformAction != null)
            {   
                Alarms_Bulk_PerformAction?.Invoke(sender, null);
            }
        }

        private void UC_Project_Telemetry_Bulk_PerformAction(object sender, RoutedEventArgs e)
        {
            ActionToPerform = UC_Project_Telemetry_Bulk.SelectedValue;

            if (Telemetry_Bulk_PerformAction != null)
            {
                Telemetry_Bulk_PerformAction?.Invoke(sender, null);
            }
        }

        private void UC_Project_Register_Bulk_PerformAction(object sender, RoutedEventArgs e)
        {
            ActionToPerform = UC_Project_Register_Bulk.SelectedValue;

            if (RegisterDevices_Bulk_PerformAction != null)
            {
                RegisterDevices_Bulk_PerformAction?.Invoke(sender, null);
            }
        }
               
        private void CTRL_Action_UPD_ItemSelected(object sender, RoutedEventArgs e)
        {
            if (DeleteDevices != null)
            {
                DeleteDevices?.Invoke(sender, null);
            }
        }

        private void BRD_NDevices_Select_All_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //TB_FilterDevices.Text = string.Empty;
            //SelectedDevices.Clear();

            //foreach (UC_Devices_Item item in GRD_Devices_List.Children)
            //{
            //    item.SetItemStyle(true);
            //    SelectedDevices.Add(item.Device);
            //}
        }

        private void BRD_NDevices_UnSelect_All_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //TB_FilterDevices.Text = string.Empty;
            //SelectedDevices.Clear();

            //foreach (UC_Devices_Item item in GRD_Devices_List.Children)
            //{
            //    item.SetItemStyle(false);

            //    SelectedDevices.Remove(item.Device);
            //}
        }

        private void TB_FilterDevices_TextChanged(object sender, TextChangedEventArgs e)
        {
            //TextBox textbox = sender as TextBox;

            //foreach (UC_Devices_Item item in GRD_Devices_List.Children)
            //{
            //    var index = item.Device.DeviceID.IndexOf(textbox.Text, StringComparison.InvariantCulture);

            //    if (index < 0)
            //    {
            //        SelectedDevices.Remove(item.Device);
            //        item.Opacity = 0.2;
            //    }
            //    else
            //    {
            //        item.Opacity = 1.0;
            //    }
            //}
        }
    }
}
