using System;
using System.Threading.Tasks;

namespace KinectLibrary
{
    public sealed class RangeFinder
    {
        /// <summary>
        /// Check if pixels is in the spesified range interval.
        /// </summary>
        /// <param name="depthData">An array of distances.</param>
        /// <param name="minDepthDistance">Minimum distance.</param>
        /// <param name="maxDepthDistance">Maximum distance.</param>
        /// <returns></returns>
        public Pixel[] PixelsInRange(short[] depthData, int minDepthDistance, int maxDepthDistance)
        {
            Pixel[] pixelDepthRange = new Pixel[depthData.Length];
            Parallel.For(0, depthData.Length, i =>
            {
                short depth = depthData[i];

                if (depth > minDepthDistance && depth < maxDepthDistance)
                    pixelDepthRange[i] = Pixel.InRange;
                else
                    pixelDepthRange[i] = Pixel.OutOfRange;
            });

            return pixelDepthRange;
        }

        /// <summary>
        /// Checks if a pixel is in range.
        /// </summary>
        /// <param name="position">Position of the pixel to check.</param>
        /// <param name="width">Widht of image.</param>
        /// <param name="height">Height of image.</param>
        /// <param name="pixels">All pixels.</param>
        /// <returns>Returns true if in range, otherwise false.</returns>
        public bool PixelInRange(Vector position, int width, int height, Pixel[] pixels)
        {
            if(width < 1)
                throw new ArgumentOutOfRangeException("width", width, "Must be greater than zero.");
            if(height < 1)
                throw new ArgumentOutOfRangeException("height", height, "Must be greater than zero.");

            // Convert from cartesian screen coordinates to array index position.
            int index = ((int)position.Y * width) + (int)position.X;

            int upperArrayBound = width*height;

            if (index >= upperArrayBound || index < 0)
                return false;

            if (pixels[index] == Pixel.InRange)
                return true;

            return false;
        }
    }
}