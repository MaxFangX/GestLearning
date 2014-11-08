using System.Collections.Generic;

namespace KinectLibrary.Fingers
{
    public interface IFingerRecognition
    {
        IEnumerable<Fingertip> FindFingertipLocations(IEnumerable<CurvePoint> curves, Pixel[] pixels, int width, int height);

        /// <summary>
        /// Minimum number of pixels a line segment must have to be considered for finger recognition.
        /// </summary>
        int MinimumPixelsForValidFingerSegment { get; set; }

        event FingertipPoints FingertipLocationsReady;
    }
}