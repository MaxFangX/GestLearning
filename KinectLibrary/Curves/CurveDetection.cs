using System;
using System.Collections.Generic;

namespace KinectLibrary.Curves
{
    public sealed class CurveDetection : ICurveDetection
    {
        private List<CurvePoint> curvePoints;

        public CurveDetection()
        {
            // The 'k' constant is how many pixels we want to travel from the origin point
            // to a new pixel to create a line segment. 
            // This value will vary from application to application and has been found by trail and error.
            K = 20;

            // This value will vary from application to application and has been found by trail and error.
            MaxAngle = 55;
            MinAngle = 25;
        }

        /// <summary>
        /// Find curves using the k-curvature algorithm.
        /// </summary>
        /// <param name="points">Array of points.</param>
        /// <returns>Returns the curve points found.</returns>
        public IEnumerable<CurvePoint> FindCurves(IEnumerable<Vector> points)
        {
            double maxAngle = DegreesToRadians(MaxAngle);
            double minAngle = DegreesToRadians(MinAngle);

            return this.FindCurves(points, K, maxAngle, minAngle);
        }

        /// <summary>
        /// Find curves using the k-curvature algorithm.
        /// </summary>
        /// <param name="points">Array of points.</param>
        /// <param name="k">Line segment distance.</param>
        /// <param name="maxAngle">Maximum angle.</param>
        /// <param name="minAngle">Minimum angle.</param>
        /// <returns>Returns the curve points found.</returns>
        public IEnumerable<CurvePoint> FindCurves(IEnumerable<Vector> points, int k, double maxAngle, double minAngle)
        {
            curvePoints = new List<CurvePoint>(100);
            var position = new List<Vector>(points);
            int positionCount = position.Count - 1;

            for (int i = 0; i < positionCount; i++)
            {
                Vector lineSegment, lineSegmentA, lineSegmentB;

                if (i < k)
                {
                    if (StartAndEndpointIsConected(position, i, k, out lineSegment))
                        lineSegmentA = lineSegment;
                    else
                        lineSegmentA = CreateLineSegment(position[i], position[0]);
                }
                else
                    lineSegmentA = CreateLineSegment(position[i], position[i - k]);

                if (i > positionCount - k)
                {
                    if (StartAndEndpointIsConected(position, i, k, out lineSegment))
                        lineSegmentB = lineSegment;
                    else
                        lineSegmentB = CreateLineSegment(position[i], position[positionCount]);
                }
                else
                    lineSegmentB = CreateLineSegment(position[i], position[i + k]);

                Vector lineSegmentC = CreateLineSegment(lineSegmentB, lineSegmentA);

                double theta = Vector.Theta(lineSegmentA, lineSegmentB);

                if (theta <= maxAngle && theta >= minAngle)
                    curvePoints.Add(new CurvePoint { Point = position[i], LineSegmentA = lineSegmentA, LineSegmentB = lineSegmentB, LineSegmentC = lineSegmentC});
            }

            CurvesReady(curvePoints);
            return curvePoints;
        }

        private static Vector CreateLineSegment(Vector vectorA, Vector vectorB)
        {
            return new Vector(vectorA.X - vectorB.X, vectorA.Y - vectorB.Y, 0d);
        }

        /// <summary>
        /// Checks if two points is within a specified range.
        /// </summary>
        /// <param name="pixelRangeLimit">The range limiter.</param>
        private bool NextPointInRange(Vector pointA, Vector pointB, int pixelRangeLimit)
        {
            double deltaX = pointB.X - pointA.X;
            double deltaY = pointB.Y - pointB.Y;

            bool horizontalInRange = deltaX <= pixelRangeLimit && deltaX >= -pixelRangeLimit;
            bool verticalInRange = deltaY <= pixelRangeLimit && deltaY >= -pixelRangeLimit;

            return horizontalInRange && verticalInRange;
        }

        /// <summary>
        /// Checks if the start and end point in a vector list is withing a specified range and constructs a line segment.
        /// </summary>
        private bool StartAndEndpointIsConected(IList<Vector> positions, int currentIndex, int pixelRangeLimit, out Vector lineSegment)
        {
            lineSegment = null;
            int listCount = (positions.Count - 1);

            int nextIndex;
            if (currentIndex > listCount - pixelRangeLimit) // currentIndex is at the end of the list.
                nextIndex = pixelRangeLimit - listCount + currentIndex;
            else if (currentIndex < pixelRangeLimit) // currentIndex is at the beginning of the list.
                nextIndex = listCount - pixelRangeLimit + currentIndex;
            else
                return false;

            bool indexOutOfBounds = !(nextIndex >= 0 && nextIndex <= listCount);

            int pixelRangeLimitZeroBased = pixelRangeLimit + 1; // To account for zero based index in the vector list.

            if (!indexOutOfBounds && NextPointInRange(positions[currentIndex], positions[nextIndex], pixelRangeLimitZeroBased))
            {
                lineSegment = CreateLineSegment(positions[currentIndex], positions[nextIndex]);
                return true;
            }

            return false;
        }

        private double DegreesToRadians(float angle)
        {
            return angle / (180 / Math.PI);
        }

        /// <summary>
        /// The 'k' constant is how many pixels we want to travel from the origin point to a new pixel to create a line segment. 
        /// </summary>
        public int K { get; set; }

        /// <summary>
        /// Maximum angle to be a valid curve.
        /// </summary>
        public int MaxAngle { get; set; }

        /// <summary>
        /// Minimum angle to be a valid curve.
        /// </summary>
        public int MinAngle { get; set; }

        /// <summary>
        /// Event fired when the algorithm is finished finding the curve points.
        /// </summary>
        public event CurvesReady CurvesReady = delegate { };
    }
}