using Hyperion.Platform.Tests.Core.ExedraLib.Config;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views
{
    public sealed partial class UC_LandingPage : UserControl
    {
        public event RoutedEventHandler FinishedWork;

        public EnvironmentHandler.Type EnvironmentT { get; set; }

        private TimeSpan Offset { get; set; }

        public UC_LandingPage()
        {
            this.InitializeComponent();

            SB_Intro.Completed += SB_Intro_Completed;

            Offset = new TimeSpan(0);
            SB_Intro.Begin();
        }

        private void SB_Intro_Completed(object sender, object e)
        {
            GRD_Environments.Visibility = Visibility.Visible;
            SB_ShowEnvironments.Begin();
        }

        private void PointerEnteredHandler(string type, bool show)
        {
            Debug.WriteLine($"Offset -> {Offset}");

            switch (type)
            {
                case "DEV": // DEV
                    IMG_DEV_1.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
                    IMG_DEV_2.Visibility = show ? Visibility.Visible : Visibility.Collapsed;

                    if (show)
                    {
                        switch (SB_Anim_Right_REV.GetCurrentState())
                        {
                            case Windows.UI.Xaml.Media.Animation.ClockState.Active:
                                SB_Anim_Right_REV.Resume();
                                break;
                            case Windows.UI.Xaml.Media.Animation.ClockState.Stopped:
                                SB_Anim_Right_REV.Begin();
                                SB_Anim_Right.Stop();
                                break;
                            default:                                
                                break;
                        }                        
                    }
                    else
                    {
                        SB_Anim_Right_REV.Pause();
                        Offset = SB_Anim_Right_REV.GetCurrentTime();
                    }

                    break;
                default: // TST
                    IMG_TST_1.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
                    IMG_TST_2.Visibility = show ? Visibility.Visible : Visibility.Collapsed;

                    

                    if (show)
                    {
                        switch (SB_Anim_Right.GetCurrentState())
                        {
                            case Windows.UI.Xaml.Media.Animation.ClockState.Active:
                                SB_Anim_Right.Resume();
                                break;
                            case Windows.UI.Xaml.Media.Animation.ClockState.Stopped:
                                SB_Anim_Right.Begin();
                                SB_Anim_Right_REV.Stop();
                                break;
                            default:
                                break;
                        }

                    }
                    else
                    {
                        SB_Anim_Right.Pause();
                        Offset = SB_Anim_Right.GetCurrentTime();
                    }             

                    break;
            }
        }

        private void BT_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            StackPanel stackPanel = sender as StackPanel;

            PointerEnteredHandler(stackPanel.Tag.ToString(), true);
        }

        private void BT_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            StackPanel stackPanel = sender as StackPanel;

            PointerEnteredHandler(stackPanel.Tag.ToString(), false);
        }

        private void BT_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            StackPanel stackPanel = sender as StackPanel;

            EnvironmentT = stackPanel.Tag.ToString().Equals("DEV", StringComparison.Ordinal) ? EnvironmentHandler.Type.DEV : EnvironmentHandler.Type.TST;

            FinishedWork?.Invoke(sender, null);
        }
    }
}
