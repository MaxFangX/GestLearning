using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace KinectLibrary
{
    public static class ImageSaver
    {
        public static void SaveDepthImageSequence(IEnumerable<Pixel[]> depthFrames, string fileName, string filePath, int frameWidth, int frameHeight)
        {
            Parallel.ForEach(depthFrames, (frame, loopState, index) =>
                                              {
                                                  using (var bitmap = CreateBitmap(frame, frameWidth, frameHeight))
                                                  {

                                                      SaveBitmap(bitmap, fileName, filePath, (int) index);
                                                  }
                                              });
        }

        public static Bitmap CreateBitmap(Pixel[] rangeData, int width, int height)
        {
            Color color;
            Bitmap image = new Bitmap(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * (width)) + x;

                    if (rangeData[index] == Pixel.InRange)
                        color = Color.White;
                    else
                        color = Color.Black;

                    image.SetPixel(x, y, color);
                }
            }
            return image;
        }

        public static void SaveBitmap(Bitmap bitmap, string fileName, string filePath, int sequenceNumber)
        {
            fileName = string.Format("{0}_{1}.{2}", fileName, sequenceNumber, "png");
            string path = Path.Combine(filePath, fileName);
            //bitmap.Save(@"E:\Frames\image_" + sequenceNumber + ".png");
            bitmap.Save(path, ImageFormat.Png);
        }
    }
}