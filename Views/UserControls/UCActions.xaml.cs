using G4Studio.Utils;
using G4Studio.Views.UIElements;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views.UserControls
{
    public sealed partial class UCActions : UserControl
    {
        public event RoutedEventHandler Show;
        public event RoutedEventHandler Hide;

        private bool IsVisible { get; set; }

        public UCActions()
        {
            this.InitializeComponent();

            IsVisible = false;

            SB_Show.Completed += SB_Show_Completed;
            SB_Hide.Completed += SB_Hide_Completed;

            UC_Action_Main_Selector.Show += UC_Action_Show;
            UC_Action_Tenant_Selector.Show += UC_Action_Show;
            UC_Action_Devices_Selector.Show += UC_Action_Show;

            UC_Action_Main_Selector.Hide += UC_Action_Hide;
            UC_Action_Tenant_Selector.Hide += UC_Action_Hide;
            UC_Action_Devices_Selector.Hide += UC_Action_Hide;
        }

        public void BindData()
        {
            UC_Action_Main_Selector.BindData();
            UC_Action_Tenant_Selector.BindData();
            UC_Action_Devices_Selector.BindData();
        }

        private void ResetAndKeepSelectedItem(string itemID)
        {
            switch (itemID)
            {
                case "SL_TEN":
                    UC_Action_Main_Selector.Reset();
                    UC_Action_Devices_Selector.Reset();
                    break;
                case "SL_Main":
                    UC_Action_Tenant_Selector.Reset();
                    UC_Action_Devices_Selector.Reset();
                    break;
                default:
                    UC_Action_Main_Selector.Reset();
                    UC_Action_Tenant_Selector.Reset();
                    break;
            }
        }

        public void Reset()
        {
            IsVisible = false;

            UC_Action_Main_Selector.Reset();
            UC_Action_Tenant_Selector.Reset();
            UC_Action_Devices_Selector.Reset();
        }

        public void SetVisibility()
        {
            if (!IsVisible)
            {
                BRD_Main.Visibility = Visibility.Visible;
                SB_Show.Begin();
            }
            else
            {
                SB_Hide.Begin();
            }
        }

        private void SB_Show_Completed(object sender, object e)
        {
            IsVisible = true;
        }

        private void SB_Hide_Completed(object sender, object e)
        {
            BRD_Main.Visibility = Visibility.Collapsed;
            IsVisible = false;
        }

        private void UC_Action_Show(object sender, RoutedEventArgs e)
        {
            UCActionsItem UIElement = sender as UCActionsItem;

            ResetAndKeepSelectedItem(UIElement.ID);

            if (Show != null)
            {
                Show?.Invoke(sender, e);
            }
        }

        private void UC_Action_Hide(object sender, RoutedEventArgs e)
        {
            if (Hide != null)
            {
                Hide?.Invoke(sender, e);
            }
        }
    }
}
