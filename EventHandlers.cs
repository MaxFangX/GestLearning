using System.Collections.Generic;
using System.Drawing;
using KinectLibrary.DTWGestureRecognition;

namespace KinectLibrary
{
    public delegate void DepthDistanceEventHandler(short[] depthDistanceData, int width, int height);

    public delegate void ContourDataUpdated(IEnumerable<Vector> edgePosition);

    public delegate void ContourReady(IEnumerable<Vector> contourPoints, Pixel[] pixels);

    public delegate void FingertipPoints(IEnumerable<Fingertip> points);

    public delegate void GestureReady(Gesture recognizedGesture);

    public delegate void GestureRecorded(Gesture recordedGesture);

    public delegate void PredictionReady(IEnumerable<Fingertip> points);

    public delegate void CurvesReady(IEnumerable<CurvePoint> curves);
}