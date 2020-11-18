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
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views
{
    public sealed partial class UC_MapControlAnimation : UserControl
    {
        public UC_MapControlAnimation()
        {
            this.InitializeComponent();

            var repeatBehavior = new RepeatBehavior();
            repeatBehavior.Type = RepeatBehaviorType.Forever;

            SB_Animation_2.Completed += SB_Animation_2_Completed;

            SB_Animation_3.RepeatBehavior = repeatBehavior;
        }

        public void StartAnimation()
        {
            SB_Animation.Begin();
            SB_Animation_2.Begin();
        }

        private void SB_Animation_2_Completed(object sender, object e)
        {
            SB_Animation_3.Begin();
        }

        public void StopAnimation()
        {
            SB_Animation.Stop();
            SB_Animation_2.Stop();
            SB_Animation_3.Stop();
        }

        public void SetTraceMessage(string prefix, string message)
        {
            Run run_prefix = new Run();
            run_prefix.Text = prefix;

            Bold bold = new Bold();
            Run run_message = new Run();
            run_message.Text = message;
            bold.Inlines.Add(run_message);


            TB_Teaser.Text = string.Empty;
            TB_Teaser.Inlines.Add(run_prefix);
            TB_Teaser.Inlines.Add(bold);
        }
    }
}
