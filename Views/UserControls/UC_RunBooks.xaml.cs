using G4Studio.Models;
using G4Studio.Utils;
using G4Studio.Views.UIElements;
using Hyperion.Platform.Tests.Core.ExedraLib.Config;
using Hyperion.Platform.Tests.Core.ExedraLib.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views.UserControls
{
    public sealed partial class UC_RunBooks : UserControl
    {
        public List<RunBookItem> RunBookItems { get; private set; }
        public List<Device> DevicesToClean { get; private set; }

        private int NActiveRuns { get; set; }
        public int NColumns { get; set; }
        public double ItemWidth { get; set; }
        public double ItemHeight { get; set; }
        private double DefaultMarginRight { get; set; }
        private double DefaultMarginTop { get; set; }
        private long ElapsedTime { get; set; }
        private long AverageTime { get; set; }
        public double Total { get; set; }
        private int Succeed { get; set; }
        private int Failed { get; set; }
        private double DeliveredPercentage { get; set; }

        public event RoutedEventHandler RunFinished;
        public event RoutedEventHandler RunFinishedDoClean;
        public event RoutedEventHandler RunFinishedDoChangeStyle;

        public UC_RunBooks()
        {
            InitializeComponent();

            NActiveRuns = 0;

            ElapsedTime = 0;
            AverageTime = 0;
            DeliveredPercentage = 0;
            Total = 0;
            Succeed = 0;
            Failed = 0;

            RunBookItems = new List<RunBookItem>();
            DevicesToClean = new List<Device>();
            NColumns = 1;

            ItemWidth = 700;
            ItemHeight = 74;

            DefaultMarginRight = 2;
            DefaultMarginTop = 13;
        }

        public void AddRun(EnvironmentHandler.Type environment, Tenant project, D2CMessagesConfig.Category D2CMessagesCategory, D2CMessagesConfig.Kind D2CMessagesKind, double nMessages, double nRuns, double nSeconds, List<Device> devices)
        {
            if (devices is null) return;

            RunBookItem newItem = new RunBookItem(environment, project, D2CMessagesCategory, D2CMessagesKind, nMessages, nRuns, nSeconds, devices, DateTime.Now);

            RunBookItems.Add(newItem);
            NActiveRuns++;

            var run = new UC_RunBook_Item()
            {
                Item = newItem,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            run.UpdateStats += Run_UpdateStats;
            run.Done += Run_Done;
            run.DoneAndUpdate += Run_DoneAndUpdate;
            run.DoneUpdateAndClean += Run_DoneUpdateAndClean;
            run.DoneAndUpdateStyle += Run_DoneAndUpdateStyle;

            run.BindData();

            GRD_Runs_List.Children.Insert(0, run);

            int counter = 0;

            foreach (UC_RunBook_Item item in GRD_Runs_List.Children)
            {
                item.Margin = new Thickness(0, counter * ItemHeight + counter * DefaultMarginTop, DefaultMarginRight, 0);

                counter++;
            }

            SetAlertInfo();

            SB_Show_BTN.Begin();
        }

        private void UpdateAlertCounter()
        {
            NActiveRuns--;

            SetAlertInfo();
        }

        private void SetAlertInfo()
        {
            TB_NDevices.Text = NActiveRuns.ToString(CultureInfo.InvariantCulture);
            BRD_Alert.Visibility = NActiveRuns > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public void Hide()
        {
            BRD_Alert.Visibility = NActiveRuns > 0 ? Visibility.Visible : Visibility.Collapsed;
            SB_Hide.Begin();
        }

        private void BRD_Remove_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SB_Hide.Begin();
        }

        private void BRD_Remove_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            BRD_Remove.Background = new SolidColorBrush(ColorHandler.FromHex("#FFDC0505"));
            TB_Remove.Foreground = new SolidColorBrush(ColorHandler.FromHex("#FFFFFFFF"));
        }

        private void BRD_Remove_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            BRD_Remove.Background = new SolidColorBrush(ColorHandler.FromHex("#FFFFFFFF"));
            TB_Remove.Foreground = new SolidColorBrush(ColorHandler.FromHex("#FF404040"));
        }

        private void BRD_Action_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SB_Show.Begin();
        }

        private void BRD_Action_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            BRD_Action.Background = new SolidColorBrush(ColorHandler.FromHex("#FFFFFFFF"));
        }

        private void BRD_Action_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            BRD_Action.Background = new SolidColorBrush(ColorHandler.FromHex("#B2FFFFFF"));
        }

        private void Run_UpdateStats(object sender, RoutedEventArgs e)
        {
            UC_RunBook_Item reporter = sender as UC_RunBook_Item;

            Total += reporter.ElapsedTime;
            Succeed += reporter.Succeed;
            Failed += reporter.Failed;
            ElapsedTime += reporter.ElapsedTime;
            AverageTime += reporter.AverageTime;
            DeliveredPercentage += reporter.DeliveredPercentage;

            TB_NMessages.Text = string.Format(CultureInfo.InvariantCulture, "{0:### ### ###.#}", Total).Trim();
            TB_N_Delivered.Text = string.Format(CultureInfo.InvariantCulture, "{0:###}", DeliveredPercentage);
            TB_N_AVG.Text = TextHandler.GetTimeFormattedFromMilseconds(AverageTime);
            TB_N_Elapsed.Text = TextHandler.GetTimeFormattedFromMilseconds(ElapsedTime);
            TB_N_Failed.Text = string.Format(CultureInfo.InvariantCulture, "{0:### ### ###.#}", Failed).Trim();
            TB_N_Succeed.Text = string.Format(CultureInfo.InvariantCulture, "{0:### ### ###.#}", Succeed).Trim();

        }

        private void Run_Done(object sender, RoutedEventArgs e)
        {
            UpdateAlertCounter();
        }

        private void Run_DoneAndUpdate(object sender, RoutedEventArgs e)
        {
            UpdateAlertCounter();

            if (RunFinished != null)
            {
                RunFinished?.Invoke(this, null);
            }
        }
        private void Run_DoneUpdateAndClean(object sender, RoutedEventArgs e)
        {
            var item = sender as UC_RunBook_Item;

            UpdateAlertCounter();

            DevicesToClean = item.Item.Devices;

            if (RunFinishedDoClean != null)
            {
                RunFinishedDoClean?.Invoke(this, null);
            }
        }

        private void Run_DoneAndUpdateStyle(object sender, RoutedEventArgs e)
        {
            var item = sender as UC_RunBook_Item;

            UpdateAlertCounter();

            DevicesToClean = item.Item.Devices;

            if (RunFinishedDoChangeStyle != null)
            {
                RunFinishedDoChangeStyle?.Invoke(this, null);
            }
        }
    }
}
