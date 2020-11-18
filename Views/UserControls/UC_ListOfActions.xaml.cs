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

namespace G4Studio.Views.UserControls
{
    public sealed partial class ItemAction
    {
        public string Title { get; set; }
        public string Label { get; set; }
        public string ImageSource { get; set; }

        public ItemAction(string title, string label, string imageSource)
        {
            Title = title;
            Label = label;
            ImageSource = imageSource;
        }
    }

    public sealed partial class UC_ListOfActions : UserControl
    {
        public string Title { get; set; }
        private List<ItemAction> ListOfActions { get; set; }
        public bool AllowMultipleSelection { get; set; }

        public event RoutedEventHandler PerformAction;
        public string SelectedValue { get; set; }

        public UC_ListOfActions()
        {
            ListOfActions = new List<ItemAction>();
            AllowMultipleSelection = false;
            SelectedValue = string.Empty;

            this.InitializeComponent();
        }

        public void AddActions(List<ItemAction> actions)
        {
            if (actions == null) return;

            foreach (var item in actions)
            {
                AddAction(item);
            }
        }

        public void AddAction(string title, string label, string imageSource)
        {
            AddAction(new ItemAction(title, label, imageSource));
        }

        public void AddAction(ItemAction action)
        {
            ListOfActions.Add(action);
        }

        public void ClearActions()
        {
            ListOfActions.Clear();
            SP_Actions.Children.Clear();
        }

        public void BindData()
        {
            SP_Actions.Children.Clear();

            TB_Title.Text = Title;

            Debug.WriteLine("ADDING " + ListOfActions.Count + " ACTIONS");

            foreach (var item in ListOfActions)
            {
                UC_Action_BTN actionBT = new UC_Action_BTN()
                {
                    ImageSource = item.ImageSource,
                    Title = item.Title,
                    Label = item.Label,
                    IsSelected = false,
                    Margin = new Thickness(0, 0, 7, 0),
                    ItemWidth = 67,
                    ItemHeight = 43
                };

                actionBT.ItemSelected += ActionBT_ItemSelected;
                actionBT.ItemDeselected += ActionBT_ItemDeselected;

                actionBT.BindaData();

                SP_Actions.Children.Add(actionBT);
                //CB_Options.Items.Add(actionBT);
            }
        }

        private void ActionBT_ItemDeselected(object sender, RoutedEventArgs e)
        {
            UC_Action_BTN button = sender as UC_Action_BTN;

            Debug.WriteLine("DESELECTED-> " + button.Title);
        }

        private void ActionBT_ItemSelected(object sender, RoutedEventArgs e)
        {
            UC_Action_BTN button = sender as UC_Action_BTN;

            SelectedValue = button.Title;

            //string name = button.Text;

            //if (!AllowMultipleSelection)
            //{
            //    foreach (var item in SP_Actions.Children)
            //    {
            //        try
            //        {
            //            UC_Action_BTN itemBT = item as UC_Action_BTN;

            //            itemBT.IsSelected = itemBT.Text.Equals(name, StringComparison.InvariantCulture) ? true : false;
            //        }
            //        catch
            //        {
            //            Debug.WriteLine("ActionBT_ItemSelected: Item is not of type UC_Action_BTN");
            //        }
            //    }
            //}

            Debug.WriteLine("SELECTED-> " + button.Title);
        }

        private void BRD_BTN_Run_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //PerformAction

            if (PerformAction != null)
            {
                PerformAction?.Invoke(sender, null);
            }
        }
    }
}
