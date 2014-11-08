using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace KinectLibrary.Fingers
{
    public sealed class FingerRecognition : IFingerRecognition
    {
        private readonly RangeFinder rangeFinder;

        public FingerRecognition(RangeFinder rangeFinder)
        {
            this.rangeFinder = rangeFinder;
            MinimumPixelsForValidFingerSegment = 0;
        }

        public IEnumerable<Fingertip> FindFingertipLocations(IEnumerable<CurvePoint> curves, Pixel[] pixels, int width, int height)
        {
            Width = width;
            Height = height;

            IEnumerable<Fingertip> points = FindFingertips((IList<CurvePoint>)curves, pixels);
            ShowCurvePoints(curves, points);
            FingertipLocationsReady(points); // Tracking completed and fingers detected; fire event.
            
            return points;
        }

        [Conditional("DEBUG")]
        private void ShowCurvePoints(IEnumerable<CurvePoint> curves, IEnumerable<Fingertip> points)
        {
            var fingerTipCurveList = new List<CurvePoint>(curves).Select(c => new Fingertip { Position = c.Point });
            FingertipLocationsReady(fingerTipCurveList);

            var fingerBisects = new List<Fingertip>(points).Select(tip => new Fingertip { Position = tip.Bisect });
            FingertipLocationsReady(fingerBisects);
        }
        
        private IEnumerable<Fingertip> FindFingertips(IList<CurvePoint> curvePoints, Pixel[] pixels)
        {
            // Rearrange curve point list if there is a cure that spans between the end and start of the curve list.
            curvePoints = RearrangeCurvePointsList(curvePoints);

            var fingertips = new List<Fingertip>(10);

            int startPointIndex = 0;
            Vector prevPoint = null;

            for (int i = 0; i < curvePoints.Count; i++)
            {
                var currentPoint = curvePoints[i].Point;

                if (prevPoint == null)
                    prevPoint = currentPoint;
                
                bool isNotLastPoint = i != curvePoints.Count - 1;
                
                if (IsContinuationOfSegment(prevPoint, currentPoint) && isNotLastPoint)
                    prevPoint = currentPoint;
                else
                {
                    // Found the last point of a curve.
                    // Need to find the middel of that curve segment.
                    int prevIndex = i - 1;
                    int indexMiddle = ((prevIndex - startPointIndex)/2) + startPointIndex;
                    CurvePoint midPoint = curvePoints[indexMiddle];
                    
                    Fingertip fingertip;
                    if (IsPointFingertip(midPoint, pixels, out fingertip) && (prevIndex - startPointIndex) > MinimumPixelsForValidFingerSegment)
                        fingertips.Add(fingertip);

                    // Start a new curve segment.
                    startPointIndex = i;
                    prevPoint = null;
                }
            }
            
            return fingertips;
        }

        /// <summary>
        /// Minimum number of pixels a line segment must have to be considered for finger recognition.
        /// </summary>
        public int MinimumPixelsForValidFingerSegment { get; set; }

        /// <summary>
        /// Checks if there is a curve segment that spans between the start and end index of the curve point list.
        /// If there is, it will return a list where the segment is offset to the beginning of the list.
        /// </summary>
        private IList<CurvePoint> RearrangeCurvePointsList(IList<CurvePoint> curvePoints)
        {
            if (curvePoints.Count < 4)
                return curvePoints;

            bool segmentIsCut = IsContinuationOfSegment(curvePoints[curvePoints.Count - 1].Point, curvePoints[0].Point);

            if (segmentIsCut)
            {
                int i = 1;
                int indexPrevPoint = (curvePoints.Count - 1) - (i - 1);
                int indexCurrentPoint = (curvePoints.Count - 1) - i;

                while (IsContinuationOfSegment(curvePoints[indexPrevPoint].Point, curvePoints[indexCurrentPoint].Point))
                {
                    if(indexCurrentPoint == 0)
                        break; // We have been through the whole list.

                    i++;
                    indexPrevPoint = (curvePoints.Count - 1) - (i - 1);
                    indexCurrentPoint = (curvePoints.Count - 1) - i;
                }

                i = (curvePoints.Count - 1) - i; // Curve segment start index.
                List<CurvePoint> rearrangedCurvePointsList = new List<CurvePoint>(curvePoints.Count);

                for (int j = 0; j < curvePoints.Count; j++, i++)
                {
                    if (i == curvePoints.Count)
                        i = 0;
                    
                    rearrangedCurvePointsList.Add(curvePoints[i]);
                }

                return rearrangedCurvePointsList;
            }

            return curvePoints;
        }

        private bool IsContinuationOfSegment(Vector prevPoint, Vector currentPoint)
        {
            // Max pixel distance to be in the current segment.
            // These numbers where chosen by trail and error.
            const int verticalPixelDistanceThreshold = 5;
            const int horizontalPixelDistanceThreshold = 5;

            double deltaX = currentPoint.X - prevPoint.X;
            double deltaY = currentPoint.Y - prevPoint.Y;

            bool withinHorizontalDistance = deltaX < horizontalPixelDistanceThreshold &&
                                            deltaX > -horizontalPixelDistanceThreshold;
            bool withinVerticalDistance = deltaY < verticalPixelDistanceThreshold &&
                                          deltaY > -verticalPixelDistanceThreshold;

            return withinHorizontalDistance && withinVerticalDistance;
        }

        private bool IsPointFingertip(CurvePoint point, Pixel[] pixels, out Fingertip fingertip)
        {
            const int bisectMultiplier = 25; // We use this to get a line that is a few pixels away from the origin point.
            Vector bisect = Vector.Bisect(point.LineSegmentA, point.LineSegmentB);
            Vector vector = Vector.Add(point.Point, bisect * bisectMultiplier);

            // Checks which direction the angle is pointing at.
            // If it hits a pixel that is in range; we have a fingertip. Otherwise we dont.
            if (!rangeFinder.PixelInRange(vector, Width, Height, pixels))
            {
                fingertip = new Fingertip
                                {
                                    Position = point.Point,
                                    Direction = FindFingerDirection(point),
                                    Bisect = vector,
                                };
                return true;
            }

            fingertip = null;
            return false;
        }

        /// <summary>
        /// Finds the pointing direction of the finger.
        /// </summary>
        /// <param name="point">Position of the finger.</param>
        /// <returns>Returns pointing direction.</returns>
        private Vector FindFingerDirection(CurvePoint point)
        {
            Vector halfCSegment = point.LineSegmentC / 2;
            Vector direction = halfCSegment - point.LineSegmentB;
            return direction;
        }

        private int Width { get; set; }
        private int Height { get; set; }

        public event FingertipPoints FingertipLocationsReady = delegate { };
    }
}