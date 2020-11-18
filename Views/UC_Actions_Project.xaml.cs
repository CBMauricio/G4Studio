using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace G4Studio.Views
{
    public sealed partial class UC_Actions_Project : UserControl
    {
        public bool IsActionsVisible { get; set; }

        public UC_Actions_Project()
        {
            this.InitializeComponent();

            IsActionsVisible = false;
            SB_Tooltip.Completed += SB_Tooltip_Completed;
        }

        private void SB_Tooltip_Completed(object sender, object e)
        {
            
        }

        private void BD_Actions_Main_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            SB_Tooltip.Begin();
        }

        private void BD_Actions_Main_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            SB_Tooltip_Hide.Begin();
        }

        private void BD_Actions_Main_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SP_Tooltip.Opacity = 0;

            if (!IsActionsVisible)
            {
                SB_Actions_Show.Begin();
            }
            else
            {
                SB_Actions_Hide.Begin();
            }

            IsActionsVisible = !IsActionsVisible;
        }
    }
}
