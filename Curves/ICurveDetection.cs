using System.Collections.Generic;

namespace KinectLibrary.Curves
{
    public interface ICurveDetection
    {
        /// <summary>
        /// Find curves using the k-curvature algorithm.
        /// </summary>
        /// <param name="points">Array of points.</param>
        /// <returns>Returns the curve points found.</returns>
        IEnumerable<CurvePoint> FindCurves(IEnumerable<Vector> points);

        /// <summary>
        /// Find curves using the k-curvature algorithm.
        /// </summary>
        /// <param name="points">Array of points.</param>
        /// <param name="k">Line segment distance.</param>
        /// <param name="maxAngle">Maximum angle.</param>
        /// <param name="minAngle">Minimum angle.</param>
        /// <returns>Returns the curve points found.</returns>
        IEnumerable<CurvePoint> FindCurves(IEnumerable<Vector> points, int k, double maxAngle, double minAngle);

        /// <summary>
        /// The 'k' constant is how many pixels we want to travel from the origin point to a new pixel to create a line segment. 
        /// </summary>
        int K { get; set; }

        /// <summary>
        /// Maximum angle to be a valid curve.
        /// </summary>
        int MaxAngle { get; set; }

        /// <summary>
        /// Minimum angle to be a valid curve.
        /// </summary>
        int MinAngle { get; set; }

        /// <summary>
        /// Event fired when the algorithm is finished finding the curve points.
        /// </summary>
        event CurvesReady CurvesReady;
    }
}