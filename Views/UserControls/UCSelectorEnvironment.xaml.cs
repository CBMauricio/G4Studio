using G4Studio.Views.UIElements;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views.UserControls
{
    public sealed partial class UCSelectorEnvironment : UserControl
    {
        public event RoutedEventHandler EnvironmentSelected;

        public UCSelectorEnvironment()
        {
            InitializeComponent();

            UC_DEV.Selected += Item_Selected;
            UC_TST.Selected += Item_Selected;            
        }

        public void BindData()
        {
            UC_DEV.BindData();
            UC_TST.BindData();
        }

        private void Item_Selected(object sender, RoutedEventArgs e)
        {
            UIETextItem item = sender as UIETextItem;

            switch (item.ID)
            {
                case "DEV":
                    UC_TST.Reset();
                    break;
                default:
                    UC_DEV.Reset();
                    break;
            }

            if (EnvironmentSelected != null)
            {
                EnvironmentSelected?.Invoke(sender, e);
            }
        }
    }
}
