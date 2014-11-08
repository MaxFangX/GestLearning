using System;

namespace KinectLibrary.VR
{
    /// <summary>
    /// Calculates the screen dimensions based on the screen diameter and aspect ratio.
    /// Can also give position values relative to the screen.
    /// </summary>
    public class Screen
    {
        /// <summary>
        /// Calculate the screen dimensions based on the screen diameter and aspect ratio.
        /// </summary>
        /// <param name="screenDiameter">Screen diameter in millimeters.</param>
        /// <param name="aspectRatio">Aspect ratio.</param>
        public Screen(double screenDiameter, double aspectRatio)
        {
            CalculateDimensions(screenDiameter, aspectRatio);
            Diameter = screenDiameter;
            AspectRatio = AspectRatio;
        }

        private void CalculateDimensions(double screenDiameter, double aspectRatio)
        {
            Height = screenDiameter / (Math.Sqrt((Math.Pow(aspectRatio, 2) + 1)));
            Width = Math.Sqrt(Math.Pow(screenDiameter, 2) - Math.Pow(Height, 2));
        }

        /// <summary>
        /// Converts physical coordinates to screen coordinates. 
        /// Screen position values goes from -1 to 1 where [0,0] is center of the screen.
        /// </summary>
        /// <param name="x">Horizontal distance from center of the screen in millimeters.</param>
        /// <param name="y">Vertical distance from center of the screen in millimeters.</param>
        /// <param name="z">
        /// Distance from center of the screen and outwards in millimeters.
        /// This distance is mesured in screen height units.
        /// </param>
        /// <returns>Returns a vector containing the X, Y and Z screen coordinates.</returns>
        public Vector PhysicalToScreenCoordinates(double x, double y, double z)
        {
            double relativeXPos = x / (Width / 2);
            double relativeYPos = y / (Height / 2);
            double relativeZPos = z / Height; // This is correct.

            return new Vector(relativeXPos, relativeYPos, relativeZPos);
        }

        /// <summary>
        /// Width of the screen in millimeters.
        /// </summary>
        public double Width { get; private set; }
        /// <summary>
        /// Height of the screen in millimeters.
        /// </summary>
        public double Height { get; private set; }
        /// <summary>
        /// Screen diameter in millimeters.
        /// </summary>
        public double Diameter { get; private set; }
        /// <summary>
        /// Screen aspect ratio.
        /// </summary>
        public double AspectRatio { get; private set; }
    }
}