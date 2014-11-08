using System.Collections.Generic;
using System.Linq;
using KinectLibrary.Contour;
using KinectLibrary.Curves;
using KinectLibrary.Debug;
using KinectLibrary.DTWGestureRecognition;
using KinectLibrary.Enhancements;
using KinectLibrary.Fingers;

namespace KinectLibrary
{
    public sealed class Main
    {
        private readonly IKinect kinectDevice;
        private readonly RangeFinder rangeFinder;
        private readonly IContourTracking contourTracking;
        private readonly ICurveDetection curveDetection;
        private readonly IFingerRecognition fingerRecognition;
        private readonly DistanceScanner distanceScanner;
        private readonly IGestureRecognition gestureRecognition;
        private readonly IPrediction prediction;
        private readonly HandEnhancements handEnhancements;
        private readonly DebugInfo debugInfo;
        private Hand prevHand;

        public Main(IKinect kinectDevice)
        { 
            // Distances in millimeter.
            const int minDepthDistance = 800; // The minimum distance where the Kinect for Xbox 360 can detect objects.
            const int maxDepthDistance = 4000;

            this.kinectDevice    = kinectDevice;
            var sensorDepthRange = new DistanceThreshold { MinDistance = minDepthDistance, MaxDistance = maxDepthDistance };
            rangeFinder          = new RangeFinder();
            contourTracking      = new ContourTracking();
            curveDetection       = new CurveDetection();
            fingerRecognition    = new FingerRecognition(rangeFinder);
            distanceScanner      = new DistanceScanner(sensorDepthRange);
            gestureRecognition   = new GestureRecognition();
            prediction           = new Prediction();
            handEnhancements     = new HandEnhancements(prediction, gestureRecognition);
            debugInfo            = new DebugInfo();
            

            InitializeDistanceThreshold(minDepthDistance);
            InitializeDebugInfo();
            CreateEvents();
        }

        public IContourTracking ContourTracking { get { return contourTracking; } }
        public ICurveDetection CurveDetection { get { return curveDetection; } }
        public IFingerRecognition FingerRecognition { get { return fingerRecognition; } }
        public IGestureRecognition GestureRecognition { get { return gestureRecognition; } }
        public DebugInfo DebugInfo { get { return debugInfo; } }

        private void InitializeDistanceThreshold(int minDepthDistance)
        {
            const int distanceInterval = 150;
            DistanceThreshold = new DistanceThreshold
                                    {
                                        MinDistance = minDepthDistance,
                                        MaxDistance = minDepthDistance + distanceInterval
                                    };

            debugInfo.DistanceThreshold = DistanceThreshold;
        }

        private void InitializeDebugInfo()
        {
            debugInfo.GestureDebugInfo = gestureRecognition as IGestureRecognitionDebug;
        }

        private void CreateEvents()
        {
            kinectDevice.DepthDistanceUpdated         += new DepthDistanceEventHandler(kinectDevice_DepthDistanceUpdated);
            contourTracking.ContourDataReady          += new ContourReady(contourTracking_ContourDataReady);
            fingerRecognition.FingertipLocationsReady += new FingertipPoints(fingerRecognition_FingertipLocationsReady);
            gestureRecognition.GestureRecognized      += new GestureReady(gestureRecognition_GestureRecognized);
            gestureRecognition.GestureRecorded        += new GestureRecorded(gestureRecognition_GestureRecorded);
        }

        private void kinectDevice_DepthDistanceUpdated(short[] depthDistanceData, int width, int height)
        {
            Width = width;
            Height = height;

            Pixel[] pixelsInRange = rangeFinder.PixelsInRange(depthDistanceData, DistanceThreshold.MinDistance, DistanceThreshold.MaxDistance);

            debugInfo.RangeData = pixelsInRange;

            contourTracking.StartTracking(pixelsInRange, width, height);
        }


        private void contourTracking_ContourDataReady(IEnumerable<Vector> contourPoints, Pixel[] pixels)
        {
            IEnumerable<CurvePoint> curves = curveDetection.FindCurves(contourPoints);

            IEnumerable<Fingertip> points = fingerRecognition.FindFingertipLocations(curves, pixels, Width, Height);
            int fingerCount = ((IList<Fingertip>)points).Count;

            if (UpdateDepthDistaceThreshold)
            {
                AutoscanForDistanceThreshold(fingerCount);
                return; // Dont do anything else while scan is in progress.
            }
        }

        private void AutoscanForDistanceThreshold(int fingerCount)
        {
            DistanceThreshold newDistanceThreshold;

            bool scanFinished = distanceScanner.TwoHandScan(fingerCount, DistanceThreshold, out newDistanceThreshold);
            DistanceThreshold = newDistanceThreshold;

            UpdateDepthDistaceThreshold = !scanFinished;

            debugInfo.AutoscanInProgress = UpdateDepthDistaceThreshold;
            debugInfo.DistanceThreshold = DistanceThreshold;
        }
        
        private void fingerRecognition_FingertipLocationsReady(IEnumerable<Fingertip> points)
        {
            var fingertips = new List<Fingertip>(points);
            Hand currentHand = new Hand(fingertips.Take(5));

            if (EnableSmoothing)
                currentHand = Smoothing.ExponentialSmoothing(currentHand, prevHand, SmoothingFactor);

            if(PreventHandInconsitencies)
                handEnhancements.PreventHandIncosistency(currentHand);
            else if (gestureRecognition.Recognizing || gestureRecognition.Recording)
                gestureRecognition.AnalyzeFrame(currentHand);

            debugInfo.FoundHandInconsistencies = handEnhancements.FixedInconsistencies;

            debugInfo.FingertipLocationsReadyCounter++;

            prevHand = currentHand;
        }

        private void gestureRecognition_GestureRecognized(Gesture recognizedGesture)
        {
            debugInfo.GestureRecognized = true;
        }

        private void gestureRecognition_GestureRecorded(Gesture recordedGesture)
        {
            gestureRecognition.StoreGesture(recordedGesture);
        }

        public double SmoothingFactor { get; set; }

        public bool EnableSmoothing { get; set; }

        public bool PreventHandInconsitencies { get; set; }
        
        private DistanceThreshold DistanceThreshold { get; set; }
        public bool UpdateDepthDistaceThreshold { get; set; }

        private int Height { get; set; }
        private int Width { get; set; }
    }
}