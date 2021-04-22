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
            if (FinishedWork != null)
            {
                FinishedWork?.Invoke(sender, null);
            }
        }
    }
}
