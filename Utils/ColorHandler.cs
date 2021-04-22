using System;
using System.Diagnostics;
using System.Drawing;
using Windows.UI.Xaml.Markup;

namespace G4Studio.Utils
{
    static class ColorHandler
    {
        public static Windows.UI.Color FromHex(string hexString)
        {
            Windows.UI.Color x;

            try
            {
                x = (Windows.UI.Color)XamlBindingHelper.ConvertValue(typeof(Windows.UI.Color), hexString);
            }
            catch (ArgumentException)
            {
                x = Windows.UI.Colors.Black;
            }
            
            return x;
        }

        public static Windows.UI.Color FromHex(string fillColor, double opacity)
        {
            Windows.UI.Color x;

            try
            {
                x = (Windows.UI.Color)XamlBindingHelper.ConvertValue(typeof(Windows.UI.Color), fillColor);
                x.A = Convert.ToByte(opacity);
            }
            catch (ArgumentException)
            {
                x = Windows.UI.Colors.Black;
            }

            return x;
        }
    }
}
