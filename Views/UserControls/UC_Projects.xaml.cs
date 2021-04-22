using G4Studio.Utils;
using Hyperion.Platform.Tests.Core.ExedraLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Timers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views
{
    public sealed partial class UC_Projects : UserControl
    {
        public List<Tenant> Projects { get; private set; }
        public Tenant SelectedProject { get; set; }

        public double ItemWidth { get; set; }
        public double ItemHeight { get; set; }
        public double DefaultMargin { get; set; }
        public double DefaultMarginTop { get; set; }
        public int NRows { get; set; }
        public int NColumns { get; set; }

        public event RoutedEventHandler ItemSelected;
        public event RoutedEventHandler GoBack;

        private static Timer Timer { get; set; }

        public UC_Projects()
        {
            InitializeComponent();

            Timer = new Timer(600);
            Timer.Elapsed += Timer_ElapsedAsync;

            ItemWidth = 262;
            ItemHeight = 107;
            DefaultMargin = 7;
            DefaultMarginTop = 7;
            NColumns = 7;
            NRows = 7;
            Projects = new List<Tenant>();
        }

        public void BindData(TenantIList tenantsObj)
        {
            if (tenantsObj == null) return;

            Projects = tenantsObj.Tenants;

            if (Projects == null || Projects.Count < 1) return;

            TB_NDevices_Total.Text = Projects.Count.ToString(CultureInfo.InvariantCulture);

            BindProjects(Projects);
        }

        private void BindProjects(List<Tenant> projects)
        {
            TB_NDevices_Filtered.Text = projects.Count.ToString(CultureInfo.InvariantCulture);
            GRD_Projects_List.Children.Clear();

            int column = 1;
            int line = 1;

            double marginLeft;
            double marginTop = DefaultMargin;

            int projectMaxSize = ProjectMaxSize();

            SP_Projects_Info_Top.Height = NRows * ItemHeight + (NRows - 2) * DefaultMarginTop - SP_Projects_Info_Top.Margin.Bottom - 100;
            TB_FilterProjects.Width = (NColumns * ItemWidth) - ((NColumns - 2) * DefaultMargin) + 2;

            foreach (var project in projects)
            {
                if (project != null)
                {
                    if (column <= NColumns)
                    {
                        marginLeft = (column - 1) * ItemWidth + column * DefaultMargin;

                        column++;
                    }
                    else
                    {
                        marginLeft = DefaultMargin;
                        marginTop = line * ItemHeight + (line + 1) * DefaultMarginTop;

                        column = 2;
                        line++;
                    }

                    UC_Projects_Item project_item = new UC_Projects_Item()
                    {
                        Margin = new Thickness(marginLeft, marginTop, DefaultMargin, 0),
                        ItemWidth = ItemWidth,
                        ItemHeight = ItemHeight,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        BRDColor = new SolidColorBrush(ColorHandler.FromHex(project.FillColor)),
                        BGColor = new SolidColorBrush(ColorHandler.FromHex(project.FillColor))
                    };

                    project_item.ItemTapped += Project_item_ItemTapped;

                    project_item.BindData(project, projectMaxSize);

                    GRD_Projects_List.Children.Add(project_item);

                }
            }
        }

        public void Show()
        {
            SB_ShowInfo_R.Begin();
        }

        public void Hide()
        {
            SB_HideInfo_R.Begin();
        }

        private int ProjectMaxSize()
        {
            int currentMax = 0;

            foreach (var item in Projects)
            {
                currentMax = Math.Max(currentMax, item.NDevices);
            }

            return currentMax;
        }

        private void SetIMGBreadcrumbVisibility(bool pointerEntered)
        {
            IMG_Breadcrumb_1.Visibility = pointerEntered ? Visibility.Collapsed : Visibility.Visible;
            IMG_Breadcrumb_2.Visibility = pointerEntered ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void Timer_ElapsedAsync(object sender, ElapsedEventArgs e)
        {
            Timer.Stop();

            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                var text = TB_FilterProjects.Text.ToUpperInvariant();

                BindProjects(Projects.FindAll(item => item.Name.ToUpperInvariant().IndexOf(text, StringComparison.InvariantCulture) >= 0
                    || item.Timezone.ToUpperInvariant().IndexOf(text, StringComparison.InvariantCulture) >= 0
                    || string.Join("", item.Hostnames).ToUpperInvariant().IndexOf(text, StringComparison.InvariantCulture) >= 0));
            });
        }
        private void Project_item_ItemTapped(object sender, RoutedEventArgs e)
        {
            UC_Projects_Item selectedItem = sender as UC_Projects_Item;
            SelectedProject = selectedItem.Project;

            if (ItemSelected != null)
            {
                ItemSelected?.Invoke(sender, null);
            }

            Debug.WriteLine("Project_item_ItemTapped");
        }

        private async void BT_Delete_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                TB_FilterProjects.Text = string.Empty;
            });
        }

        private void TB_FilterProjects_TextChanged(object sender, TextChangedEventArgs e)
        {   
            Timer.Start();

            BT_Delete.Opacity = string.IsNullOrEmpty(TB_FilterProjects.Text) ? 0 : 100;
        }

        private void SP_Breadcrumb_l_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            SetIMGBreadcrumbVisibility(true);
        }

        private void SP_Breadcrumb_l_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            SetIMGBreadcrumbVisibility(false);
        }

        private void SP_Breadcrumb_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Hide();

            if (GoBack != null)
            {
                GoBack?.Invoke(sender, e);
            }
        }
    }
}
