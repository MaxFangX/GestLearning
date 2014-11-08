using Microsoft.Xna.Framework;

namespace KinectLibrary.Helpers
{
    public static class ColorImageHelper
    {
        public static Color[] ToColorArray(this ColorImage imageFrame)
        {
            var colors = new Color[imageFrame.Height * imageFrame.Width];
            int index = 0;

            for (int y = 0; y < imageFrame.Height; y++)
            {
                for (int x = 0; x < imageFrame.Width; x++, index += 4)
                {
                    colors[y * imageFrame.Width + x] = new Color(imageFrame.PixelData[index + 2],
                                                               imageFrame.PixelData[index + 1],
                                                               imageFrame.PixelData[index + 0]);
                }
            }

            return colors;
        }
    }
}