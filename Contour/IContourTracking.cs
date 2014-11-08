using System.Collections.Generic;

namespace KinectLibrary.Contour
{
    public interface IContourTracking
    {
        /// <summary>
        /// Begin tracking the contour. The event 'ContourDataReady' will fire when the tracking is done.
        /// </summary>
        IEnumerable<Vector> StartTracking(Pixel[] pixelsInRange, int width, int height);

        int MaxEdgePixelCount { get; set; }

        /// <summary>
        /// Number of rows to skip when scanning for the inital contour pixel.
        /// </summary>
        int NumberOfRowsToSkip { get; set; }

        int MaxPixelsToBacktrack { get; set; }
        bool EnableScanFromRight { get; set; }
        bool EnableScanFromLeft { get; set; }

        /// <summary>
        /// Height offset in percentage of image height.
        /// This offsets the height where the scan will start.
        /// </summary>
        double ScanHeightOffset { get; set; }

        IEnumerable<Vector> EdgePosition { set; }
        event ContourDataUpdated EdgePointsUpdated;
        event ContourReady ContourDataReady;
    }
}