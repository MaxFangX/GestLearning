using System;
using System.Collections.Generic;

namespace KinectLibrary.DTWGestureRecognition
{
    public class DtwGestureRecognizer
    {
        private const double FrameDistanceThreshold = 30;
        private const int VerticalMovementThreshold = 10;
        private const int HorizontalMovementThreshold = 10;

        private readonly double pathCostThreshold;

        public DtwGestureRecognizer()
        {
            pathCostThreshold = 8;
            LatestPathCost = double.PositiveInfinity;
        }

        public bool RecognizeGesture(Gesture observations, Gesture gestureCandidate, out Gesture recognizedGesture)
        {
            recognizedGesture = null;

            double[,] dtwMatrix = FindDtwMatrix(observations, gestureCandidate);

            double pathCost;
            bool foundLowestCostPath = FindLowestCostPath(dtwMatrix, out pathCost);

            if (!foundLowestCostPath)
                return false;

            pathCost = pathCost / observations.Frames.Count;
            LatestPathCost = pathCost;

            recognizedGesture = gestureCandidate;
            if (pathCost < pathCostThreshold)
                return true;

            return false;
        }

        /// <summary>
        /// Finds a gesture where the last frame is a relatively good match with the last observation frame.
        /// </summary>
        /// <param name="lastObservationFrame">Last frame in the current observation.</param>
        /// <param name="storedGestures">All prerecorded gestures.</param>
        /// <param name="gestureCandidate">A gesture where the last frame has a relatively good match with the last observation frame.</param>
        /// <returns>Returns true if a gesture candidate was found, otherwise it returns false.</returns>
        public bool FindGestureCandidate(Hand lastObservationFrame, IEnumerable<Gesture> storedGestures, out Gesture gestureCandidate)
        {
            gestureCandidate = null;
            double lowestFrameDistance = double.MaxValue;
            
            foreach (Gesture gesture in storedGestures)
            {
                if (gesture.Frames.Count < 1)
                    continue;

                Hand lastGestureFrame = gesture.Frames[gesture.Frames.Count - 1];
                double frameDistance = TotalEuclideanDistance(lastObservationFrame, lastGestureFrame);

                if (frameDistance < FrameDistanceThreshold && frameDistance < lowestFrameDistance)
                {
                    lowestFrameDistance = frameDistance;
                    gestureCandidate = gesture;
                }

                GestureCandidateDistance = frameDistance;
            }
            if (gestureCandidate != null)
                return true;

            return false;
        }

        public double GestureCandidateDistance { get; set; }

        /// <summary>
        /// Calculate total euclidean distance between two frame.
        /// </summary>
        /// <param name="observation">The observation frame.</param>
        /// <param name="gesture">The prerecorded frame.</param>
        /// <returns>Returns total euclidean distance between the two frames.</returns>
        private double TotalEuclideanDistance(Hand observation, Hand gesture)
        {
            double totalFrameDistance = 0;

            for (int i = 0; i < Hand.MaxFingers; i++)
            {
                double deltaX = observation.Fingers[i].Position.X - gesture.Fingers[i].Position.X;
                double deltaY = observation.Fingers[i].Position.Y - gesture.Fingers[i].Position.Y;
                double deltaZ = observation.Fingers[i].Position.Z - gesture.Fingers[i].Position.Z;

                totalFrameDistance += EuclideanDistance(deltaX, deltaY, deltaZ);
            }

            return totalFrameDistance;
        }

        /// <summary>
        /// Calculates Euclidean distance.
        /// </summary>
        /// <param name="deltaX">Change in x-direction.</param>
        /// <param name="deltaY">Change in y-direction.</param>
        /// <param name="deltaZ">Change in z-direction.</param>
        /// <returns>Returns the euclidean distance.</returns>
        private double EuclideanDistance(double deltaX, double deltaY, double deltaZ)
        {
            return Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2) + Math.Pow(deltaZ, 2));
        }

        /// <summary>
        /// Calculates Euclidean distance with a weighing factor on each dimension.
        /// </summary>
        /// <param name="deltaX">Change in x-direction.</param>
        /// <param name="deltaY">Change in y-direction.</param>
        /// <param name="deltaZ">Change in z-direction.</param>
        /// <param name="weighing">Weight for each direction.</param>
        /// <returns>Returns the euclidean distance.</returns>
        private double EuclideanDistance(double deltaX, double deltaY, double deltaZ, Vector weighing)
        {
            return Math.Sqrt(weighing.X*Math.Pow(deltaX, 2) 
                           + weighing.Y*Math.Pow(deltaY, 2)
                           + weighing.Z*Math.Pow(deltaZ, 2));
        }

        private double[,] FindDtwMatrix(Gesture observations, Gesture gestureCandidate, Vector weighing)
        {
            double[,] costMatrix = new double[observations.Frames.Count, gestureCandidate.Frames.Count];

            double[,] accumulatedCostMatrix = new double[observations.Frames.Count, gestureCandidate.Frames.Count];
            accumulatedCostMatrix[0, 0] = 0;

            // Compute lowest cost matrix
            for (int n = 0; n < observations.Frames.Count; n++)
            {
                for (int m = 0; m < gestureCandidate.Frames.Count; m++)
                {
                    double distance = TotalEuclideanDistance(observations.Frames[n], gestureCandidate.Frames[m]);
                    costMatrix[n, m] = distance;


                    if (n == 0 && m != 0) // The leftmost column.
                        accumulatedCostMatrix[0, m] = distance + accumulatedCostMatrix[0, m - 1];

                    if (n != 0 && m == 0) // The lowest row.
                        accumulatedCostMatrix[n, 0] = distance + accumulatedCostMatrix[n - 1, 0];


                    if (n != 0 && m != 0)
                    {
                        double costLeft = weighing.X * distance + accumulatedCostMatrix[n - 1, m];
                        double costBelow = weighing.Y * distance + accumulatedCostMatrix[n, m - 1];
                        double costDiagonal = weighing.Z * distance + accumulatedCostMatrix[n - 1, m - 1];

                        double lowestCost = MinimumCost(costLeft, costBelow, costDiagonal);

                        accumulatedCostMatrix[n, m] = lowestCost;
                    }
                }
            }

            return accumulatedCostMatrix;
        }

        private double[,] FindDtwMatrix(Gesture observations, Gesture gestureCandidate)
        {
            return FindDtwMatrix(observations, gestureCandidate, new Vector(0, 0, 0.5));
        }

        private double MinimumCost(double left, double below, double diagonal)
        {
            Vector v; Direction d; // Variables are not used.
            return MinimumCost(left, below, diagonal, out v, out d);
        }

        private double MinimumCost(double left, double below, double diagonal, out Vector directionVector, out Direction direction)
        {
            if (diagonal <= left && diagonal <= below)
            {
                directionVector = new Vector(-1, -1, 0);
                direction = Direction.Diagonal;
                return diagonal;
            }

            if (below <= left && below <= diagonal)
            {
                directionVector = new Vector(0, -1, 0);
                direction = Direction.Below;
                return below;
            }

            directionVector = new Vector(-1, 0, 0);
            direction = Direction.Left;
            return left;
        }

        /// <summary>
        /// Minimum cost path direction.
        /// </summary>
        private enum Direction
        {
            Left = 0,
            Below,
            Diagonal,
        }

        private bool FindLowestCostPath(double[,] accumulatedCost, out double totalPathCost)
        {
            int nStart = accumulatedCost.GetLength(dimension: 0) - 1;
            int mStart = accumulatedCost.GetLength(dimension: 1) - 1;

            int n = nStart;
            int m = mStart;

            Vector positionInMatrix = new Vector(nStart, mStart, 0);

            int verticalMovementCounter = 0;
            int horizontalMovementCounter = 0;

            totalPathCost = 0;

            while (n > 0 && m > 0)
            {
                double costLeft = double.MaxValue, costBelow = double.MaxValue, costDiagonal = double.MaxValue;

                if (m > 0)
                    costBelow = accumulatedCost[n, m - 1];

                if (n > 0)
                    costLeft= accumulatedCost[n - 1, m];

                if (n > 0 && m > 0)
                    costDiagonal = accumulatedCost[n - 1, m - 1];

                Direction lowestCostDirection;
                Vector lowestCostVectorDirection;
                double lowestCost = MinimumCost(costLeft, costBelow, costDiagonal, out lowestCostVectorDirection, out lowestCostDirection);

                if (double.IsPositiveInfinity(lowestCost))
                    return false;

                totalPathCost += lowestCost;

                positionInMatrix = positionInMatrix.Add(lowestCostVectorDirection);

                n = (int)positionInMatrix.X;
                m = (int)positionInMatrix.Y;

                // Check if we are over the horizontal or vertical threshold limit.
                // If we are over the limit, abort the path finding; The two feature sets are too different to each other.

                if (lowestCostDirection == Direction.Left)
                    horizontalMovementCounter++;
                if (lowestCostDirection == Direction.Below)
                    verticalMovementCounter++;

                // Reset counters.
                if (lowestCostDirection == Direction.Diagonal)
                {
                    horizontalMovementCounter = 0;
                    verticalMovementCounter = 0;
                }

                if (horizontalMovementCounter > HorizontalMovementThreshold)
                    return false;
                if (verticalMovementCounter > VerticalMovementThreshold)
                    return false;
            }

            return true;
        }

        public double LatestPathCost { get; private set; }
    }
}