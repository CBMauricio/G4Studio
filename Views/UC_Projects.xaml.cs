using G4Studio.Utils;
using Hyperion.Platform.Tests.Core.ExedraLib.Models;
using System.Collections.Generic;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views
{
    public sealed partial class UC_Projects : UserControl
    {
        private bool IsShowingInfo { get; set; }
        public List<Tenant> Projects { get; private set; }
        public Tenant SelectedProject { get; set; }

        public double ItemWidth { get; set; }
        public double ItemHeight { get; set; }
        public double DefaultMargin { get; set; }
        public int NColumns { get; set; }

        public event RoutedEventHandler ItemSelected;

        public UC_Projects()
        {
            this.InitializeComponent();

            IsShowingInfo = false;
            ItemWidth = 150;
            ItemHeight = 150;
            DefaultMargin = 5;
            NColumns = 10;
            Projects = new List<Tenant>();

            SB_ShowInfo.Completed += SB_ShowInfo_Completed;
            SB_HideInfo.Completed += SB_HideInfo_Completed;
        }

        public void BindData(List<Tenant> projects)
        {
            Projects = projects;

            BindProjects();
        }

        public void Reset()
        {
            IsShowingInfo = false;
        }

        private void BindProjects()
        {
            GRD_Projects_List.Children.Clear();

            int column = 1;
            int line = 1;

            double defaultMargin = 5;

            double marginLeft;
            double marginTop = defaultMargin;

            foreach (var project in Projects)
            {
                if (project != null)
                {
                    //project.GetNTwins().Wait();

                    if (column <= NColumns)
                    {
                        marginLeft = (column - 1) * ItemWidth + column * DefaultMargin;

                        column++;
                    }
                    else
                    {
                        marginLeft = DefaultMargin;
                        marginTop = line * ItemHeight + (line + 1) * DefaultMargin;

                        column = 2;
                        line++;
                    }

                    UC_Projects_Item project_item = new UC_Projects_Item()
                    {
                        Margin = new Thickness(marginLeft, marginTop, defaultMargin, 0),
                        ItemWidth = ItemWidth,
                        ItemHeight = ItemHeight,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        //BGColor = new SolidColorBrush(ColorHandler.FromHex(project.FillColor, project.FillOpacity * 100)),
                        BRDColor = new SolidColorBrush(Windows.UI.Colors.White),
                        BRDThickness = new Thickness(1),
                        BGColor = new SolidColorBrush(ColorHandler.FromHex(project.FillColor)),
                        Text = project.Name,
                        NProjects = project.NDevices.ToString(CultureInfo.InvariantCulture),
                        Project = project
                    };

                    project_item.ItemTapped += Project_item_ItemTapped;

                    project_item.BindData();

                    GRD_Projects_List.Children.Add(project_item);
                }
            }
        }

        private void Project_item_ItemTapped(object sender, RoutedEventArgs e)
        {  
            UC_Projects_Item selectedItem = sender as UC_Projects_Item;
            SelectedProject = selectedItem.Project;
            
            //IsShowingInfo = true;

            //TB_Title.Text = SelectedProject.name;
            //TB_Hostname.Text = SelectedProject.hostnames[0];
            //TB_NProjects.Text = SelectedProject.Count.ToString(CultureInfo.InvariantCulture);

            //SP_ProjectInfo.Visibility = Visibility.Visible;
            //BRD_Projects_List.Visibility = Visibility.Collapsed;
            //SB_ShowProject_Info.Begin();

            SB_HideInfo.Begin();

            if (ItemSelected != null)
            {
                ItemSelected?.Invoke(sender, null);
            }
        }

        public void Show()
        {
            SP_Main.Visibility = Visibility.Visible;
            SB_ShowInfo.Begin();
        }

        public void Hide()
        {
            SB_HideInfo.Begin();
        }

        private void SB_ShowInfo_Completed(object sender, object e)
        {
            IsShowingInfo = true;
        }

        private void SB_HideInfo_Completed(object sender, object e)
        {
            IsShowingInfo = false;
            SP_Main.Visibility = Visibility.Collapsed;
        }
    }
}
