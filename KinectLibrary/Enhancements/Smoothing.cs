using System;

namespace KinectLibrary.Enhancements
{
    public static class Smoothing
    {
        /// <summary>
        /// Applies exponential smoothing between two points.
        /// </summary>
        /// <param name="currentPoint">Current point.</param>
        /// <param name="previousPoint">Previous point.</param>
        /// <param name="smoothingFactor">
        /// Smoothing factor. This value must be greater than 0 and less than 1. Values closer to 1 will apply less smoothing.
        /// </param>
        /// <returns>Returns a new point where smoothing is applied to the current point.</returns>
        public static Vector ExponentialSmoothing(Vector currentPoint, Vector previousPoint, double smoothingFactor)
        {
            if(smoothingFactor <= 0)
                throw new ArgumentOutOfRangeException("smoothingFactor", "It must be greater than zero.");
            if(smoothingFactor >= 1)
                throw new ArgumentOutOfRangeException("smoothingFactor", "It must be less than one.");

            Vector vectorWithSmoothing = new Vector();
            vectorWithSmoothing.X = previousPoint.X + smoothingFactor * (currentPoint.X - previousPoint.X);
            vectorWithSmoothing.Y = previousPoint.Y + smoothingFactor * (currentPoint.Y - previousPoint.Y);
            vectorWithSmoothing.Z = previousPoint.Z + smoothingFactor * (currentPoint.Z - previousPoint.Z);
            return vectorWithSmoothing;
        }

        /// <summary>
        /// Applies exponential smoothing between two hands.
        /// </summary>
        /// <param name="currentHand">Current hand.</param>
        /// <param name="previousHand">Previous hand.</param>
        /// <param name="smoothingFactor">
        /// Smoothing factor. This value must be greater than 0 and less than 1. Values closer to 1 will apply less smoothing.
        /// </param>
        /// <returns>Returns a new hand where smoothing is applied to the current hand.</returns>
        public static Hand ExponentialSmoothing(Hand currentHand, Hand previousHand, double smoothingFactor)
        {
            Hand handWithSmoothing = new Hand();
            for (int fingerID = 0; fingerID < Finger.FingerCount; fingerID++)
            {
                Vector currentFingerPosition = currentHand.Fingers[fingerID].Position;
                Vector prevFingerPosition = previousHand.Fingers[fingerID].Position;

                handWithSmoothing.Fingers[fingerID].Position = ExponentialSmoothing(currentFingerPosition, prevFingerPosition, smoothingFactor);

                Vector currentFingerDirection = currentHand.Fingers[fingerID].Direction;
                Vector prevFingerDirection = previousHand.Fingers[fingerID].Direction;

                handWithSmoothing.Fingers[fingerID].Direction = ExponentialSmoothing(currentFingerDirection, prevFingerDirection, smoothingFactor);
            }
            return handWithSmoothing;
        }
    }
}