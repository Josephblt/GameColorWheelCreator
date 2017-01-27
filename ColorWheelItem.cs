using System.Drawing;

namespace GameColorWheelCreator
{
    public class ColorWheelItem
    {
        #region Constructor

        public ColorWheelItem(Color color, float occurency)
        {
            Color = color;
            OriginalOccurency = occurency;
            CustomOccurency = occurency;

            CreateColorImage(color);
        }

        #endregion

        #region Attributes and Properties

        public Color Color { get; private set; }
        public float OriginalOccurency { get; private set; }
        public float CustomOccurency { get; set; }

        public Bitmap GridImage { get; private set; }
        public string GridOriginalOccurency { get { return OriginalOccurency.ToString("0.##"); } }
        public string GridCustomOccurency { get { return CustomOccurency.ToString("0.##"); } }
        public string GridRGB { get { return Color.ToRGBValue(); } }
        public string GridHex { get { return Color.ToHexValue(); } }

        #endregion

        #region Private Methods

        private void CreateColorImage(Color color)
        {
            GridImage = new Bitmap(1 , 1);
            GridImage.SetPixel(0, 0, color);
        }

        #endregion
    }
}
