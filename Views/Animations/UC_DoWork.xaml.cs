using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class UC_DoWork : UserControl
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string MaxItems { get; set; }

        public event RoutedEventHandler Done;

        public UC_DoWork()
        {
            Title = string.Empty;
            Description = string.Empty;
            MaxItems = string.Empty;

            this.InitializeComponent();
        }

        public void BindData()
        {
            TB_DoWork_Counter.Text = "0";
            TB_DoWork_Title.Text = Title;
            TB_DoWork_Description.Text = Description;
            TB_DoWork_Counter_Max.Text = MaxItems;

            BTN_Done.Opacity = 0;
        }

        public void UpdateCounter()
        {
            int c;

            if (int.TryParse(TB_DoWork_Counter.Text, out c))
            {
                c += 1;
                TB_DoWork_Counter.Text = c.ToString(CultureInfo.InvariantCulture);
            }

            //if (TB_DoWork_Counter.Text.Equals(TB_DoWork_Counter_Max.Text, StringComparison.InvariantCulture))
            //{
            //    BTN_Done.Opacity = 1;
            //}
        }


        public void UpdateCounter(string counter)
        {
            TB_DoWork_Counter.Text = counter;

            //if (TB_DoWork_Counter.Text.Equals(TB_DoWork_Counter_Max.Text, StringComparison.InvariantCulture))
            //{
            //    BTN_Done.Opacity = 1;
            //}
        }

        private void BTN_Done_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Done != null)
            {
                Done?.Invoke(sender, null);
            }
        }

        public void ShowDoneControl()
        {
            BTN_Done.Opacity = 1;
        }
    }
}
