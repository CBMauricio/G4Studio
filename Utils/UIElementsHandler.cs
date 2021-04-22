using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace G4Studio.Utils
{
    public static class UIElementsHandler
    {
        public static void DrawScoreElements(Grid gridElement, int rows, int columns, double nElements, double defaultScoreBoxWidth, double defaultScoreBoxHeight)
        {
            int elementsToDraw = rows * columns;
            int currentRowIndex = 1;
            int currentColumnIndex = 1;
            double horizontalMargin = 0;
            double verticalMargin = 0;

            if (gridElement is null) return;

            gridElement.Children.Clear();

            for (int i = 0; i < elementsToDraw; i++)
            {
                string tag = i.ToString(CultureInfo.InvariantCulture);

                Rectangle rect = new Rectangle
                {
                    Name = tag,
                    Tag = i,
                    Width = defaultScoreBoxWidth,
                    Height = defaultScoreBoxHeight,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(horizontalMargin, 0, 0, verticalMargin),
                    Fill = i <= nElements ? new SolidColorBrush(ColorHandler.FromHex("#FF333333")) : new SolidColorBrush(ColorHandler.FromHex("#FFFFFFFF")),
                    Stroke = new SolidColorBrush(ColorHandler.FromHex("#4C333333")),
                    StrokeThickness = 1
                };

                if (currentRowIndex++ % rows == 0)
                {
                    verticalMargin += defaultScoreBoxHeight + 1;
                }

                if (currentColumnIndex++ < columns)
                {
                    horizontalMargin += defaultScoreBoxWidth + 1;
                }
                else
                {
                    horizontalMargin = 0;
                    currentColumnIndex = 1;
                }

                gridElement.Children.Add(rect);
            }
        }
    }
}
