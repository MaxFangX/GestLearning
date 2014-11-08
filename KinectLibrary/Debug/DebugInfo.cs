using System;
using KinectLibrary.DTWGestureRecognition;

namespace KinectLibrary.Debug
{
    /// <summary>
    /// This class holds information usefull for debugging.
    /// </summary>
    public class DebugInfo
    {
        public bool AutoscanInProgress { get; set; }

        public bool FoundHandInconsistencies { get; set; }

        public long FingertipLocationsReadyCounter { get; set; }

        public bool GestureRecognized { get; set; }

        public bool DebugMode { get; set; }

        public DistanceThreshold DistanceThreshold { get; set; }

        public Pixel[] RangeData { get; set; }

        public IGestureRecognitionDebug GestureDebugInfo { get; set; }
    }
}