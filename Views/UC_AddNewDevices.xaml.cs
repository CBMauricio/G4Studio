using G4Studio.Models;
using G4Studio.Utils;
using Hyperion.Platform.Tests.Core.ExedraLib.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
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

namespace G4Studio.Views
{
    public sealed partial class UC_AddNewDevices : UserControl
    {
        public List<Device> NewDevices { get; set; }
        public Tenant Project { get; set; }
        public List<Twin> Twins { get; set; }

        public event RoutedEventHandler RegisterDevices;

        public UC_AddNewDevices()
        {
            this.InitializeComponent();

            NewDevices = new List<Device>(); 
            Project = new Tenant();
            Twins = new List<Twin>();
        }

        public void BindData(Tenant project, List<Twin> twins)
        {
            if (project != null)
            {
                Project = project;
                Twins = twins;

                TB_Title.Text = Project.Name;
                TB_Hostname.Text = Project.Hostnames[0];
                TB_NProjects.Text = Twins.Count.ToString(CultureInfo.InvariantCulture);

                RC_Color.Fill = new SolidColorBrush(ColorHandler.FromHex(project.FillColor, project.FillOpacity * 100));
                RC_Color.Stroke = new SolidColorBrush(ColorHandler.FromHex(project.FillColor));

                //BD_Actions_Main_Inner.Background = new SolidColorBrush(ColorHandler.FromHex(resourceLoader.GetString("BRD_Light_BGColor_Selected")));
                //IMG_Main.Source = new BitmapImage(new Uri(resourceLoader.GetString("BRD_Actions_Devices_IMG_Selected")));

                //CTRL_Action_REG.BindaData();
                //CTRL_Action_RREG.BindaData();
                //CTRL_Action_UPD.BindaData();

                //CTRL_Action_PHIGH.BindaData();
                //CTRL_Action_PF.BindaData();
                //CTRL_Action_PLOW.BindaData();

                //CTRL_Action_TEL0.BindaData();
                //CTRL_Action_TEL50.BindaData();
                //CTRL_Action_TEL100.BindaData();

                //BindData_Devices(project.FillColor);

                //IsVisible = true;
            }
        }

        public void AddNewDevice(string deviceID, Windows.Devices.Geolocation.BasicGeoposition position)
        {
            NewDevices.Add(new Device(deviceID, position));

            TB_NewDevices.Text = NewDevices.Count.ToString(CultureInfo.InvariantCulture);
        }

        private void BTN_Register_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (RegisterDevices != null)
            {
                RegisterDevices?.Invoke(sender, null);
            }
        }
    }
}
