using G4Studio.Views.UIElements;
using Hyperion.Platform.Tests.Core.ExedraLib.Config;
using Hyperion.Platform.Tests.Core.ExedraLib.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views.UserControls
{
    public sealed partial class UC_Environments : UserControl
    {
        public EnvironmentHandler.Type EnvironmentT { get; set; }
        public event RoutedEventHandler ItemSelected;
        public TenantIList Tenants { get; set; }

        public UC_Environments()
        {
            InitializeComponent();

            CTRL_Environment_DEV.ItemSelected += CTRL_Environment_ItemSelected;
            CTRL_Environment_TST.ItemSelected += CTRL_Environment_ItemSelected;
            CTRL_Environment_PRD.ItemSelected += CTRL_Environment_ItemSelected;

            Tenants = new TenantIList();
        }

        private void CTRL_Environment_ItemSelected(object sender, RoutedEventArgs e)
        {
            UC_Environment_Item selectedEnvironment = sender as UC_Environment_Item;
            EnvironmentT = selectedEnvironment.EnvironmentT;

            switch (EnvironmentT)
            {
                case EnvironmentHandler.Type.DEV:
                    CTRL_Environment_TST.Clean();
                    CTRL_Environment_PRD.Clean();
                    break;
                case EnvironmentHandler.Type.TST:
                    CTRL_Environment_DEV.Clean();
                    CTRL_Environment_PRD.Clean();
                    break;
                default:
                    CTRL_Environment_DEV.Clean();
                    CTRL_Environment_TST.Clean();
                    break;
            }

            if (ItemSelected != null)
            {
                Tenants = selectedEnvironment.Tenants;

                ItemSelected?.Invoke(sender, null);
            }
        }

        public void Show()
        {
            SB_Init.Begin();

            CTRL_Environment_PRD.BindData();
            CTRL_Environment_DEV.BindData();
            CTRL_Environment_TST.BindData();
            
        }
    }
}
