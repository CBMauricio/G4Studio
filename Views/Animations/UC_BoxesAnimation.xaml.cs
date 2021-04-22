using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace G4Studio.Views.Animations
{
    public sealed partial class UC_BoxesAnimation : UserControl
    {
        public UC_BoxesAnimation()
        {
            InitializeComponent();

            SB_Anim.RepeatBehavior = RepeatBehavior.Forever;
        }

        public void Start()
        {
            SB_Anim.Begin();
        }

        public void Stop()
        {
            SB_Anim.Stop();
        }
    }
}
