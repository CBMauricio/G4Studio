using G4Studio.Utils;
using System;
using System.Collections.Generic;
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

namespace G4Studio.Views.UIElements
{
    public sealed partial class UIETextItem : UserControl
    {
        public bool IsSelected { get; set; }
        public string Title { get; set; }
        public string ID { get; set; }

        public event RoutedEventHandler Selected;

        public UIETextItem()
        {
            this.InitializeComponent();

            IsSelected = false;
            Title = string.Empty;
            ID = string.Empty;
        }

        public void BindData()
        {
            TB_Title.Text = Title;
        }

        public void Reset()
        {
            IsSelected = false;

            SetLayout(false);
        }

        public void SetLayout(bool show)
        {
            BRD_Main.Background = new SolidColorBrush(ColorHandler.FromHex(show ? "#FF333333" : "#FFFFFFFF"));
            TB_Title.Foreground = new SolidColorBrush(ColorHandler.FromHex(show ? "#FFFFFFFF" : "#FF333333"));
        }

        private void BRD_Main_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (!IsSelected)
            {
                SetLayout(true);
            }
        }

        private void BRD_Main_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (!IsSelected)
            {
                SetLayout(false);
            }
        }

        private void BRD_Main_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!IsSelected)
            {
                IsSelected = true;

                SetLayout(true);

                if (Selected != null)
                {
                    Selected?.Invoke(this, e);
                }
            }
        }
    }
}
