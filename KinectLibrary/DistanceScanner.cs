namespace KinectLibrary
{
    public sealed class DistanceScanner
    {
        private readonly int searchDistance;
        private readonly int searchDistanceInterval;

        private int prevDistance;
        private bool findMinDistance;

        /// <param name="sensorRange">Maximum and minimum sensor range distance, in millimeters.</param>
        public DistanceScanner(DistanceThreshold sensorRange)
        {
            searchDistance           = 150; // Hand width. Distance in millimeters.
            searchDistanceInterval   = searchDistance / 2;
            MinSensorDistance        = sensorRange.MinDistance;
            MaxSensorDistance        = sensorRange.MaxDistance;
            DefaultDistanceThreshold = new DistanceThreshold { MinDistance = MinSensorDistance, MaxDistance = MinSensorDistance + searchDistance };
            ResetScan();
        }

        /// <summary>
        /// Scans for a hand and sets its position as the minimum depth distance.
        /// The maximum distance will then be the minimum distance pluss the specified distance interval.
        /// </summary>
        public bool OneHandScan(int fingerCount, int distanceInterval, DistanceThreshold currentDistance, out DistanceThreshold distanceThreshold)
        {
            const bool isTwoHandScan = false;
            bool thresholdFound = FindThreshold(isTwoHandScan, fingerCount, currentDistance, out distanceThreshold);

            if(thresholdFound && distanceThreshold != DefaultDistanceThreshold)
                distanceThreshold.MaxDistance = distanceThreshold.MinDistance + distanceInterval;

            return thresholdFound;
        }

        /// <summary>
        /// Scans for a hand and sets its position as the minimum depth distance. 
        /// It will then scan for a second hand further away and set its distance as the maximum depth distance.
        /// </summary>
        public bool TwoHandScan(int fingerCount, DistanceThreshold currentDistance, out DistanceThreshold distanceThreshold)
        {
            const bool isTwoHandScan = true;
            return FindThreshold(isTwoHandScan, fingerCount, currentDistance, out distanceThreshold);
        }
        
        private bool FindThreshold(bool findTwoHands, int fingerCount, DistanceThreshold currentDistance, out DistanceThreshold scanDistance)
        {
            const int acceptableFingerCount = 4; // Four fingers in case of bad results from finger recognition.

            bool newScan = currentDistance.MinDistance < prevDistance;
            if (newScan)
                return InitializeScan(out scanDistance);

            if (fingerCount >= acceptableFingerCount)
            {
                if (findMinDistance)
                {
                    DistanceThreshold.MinDistance = currentDistance.MinDistance;
                    findMinDistance = false;

                    if (!findTwoHands) // Find distance threshold using only one hand.
                        return ScanHasFinished(out scanDistance);

                    // Skip overlap at the next pass to avoid the possibility of detecting the first hand again.
                    currentDistance = SkipOverlap(currentDistance);
                    return ContinueScan(currentDistance, out scanDistance);
                }

                // Found second hand.
                DistanceThreshold.MaxDistance = currentDistance.MaxDistance;
                return ScanHasFinished(out scanDistance);
            }

            bool reachedMaxDistance = currentDistance.MaxDistance > MaxSensorDistance;
            if (reachedMaxDistance)
                return DefaultValues(out scanDistance);

            return ContinueScan(currentDistance, out scanDistance);
        }

        private bool InitializeScan(out DistanceThreshold distanceThreshold)
        {
            distanceThreshold = DefaultDistanceThreshold;
            prevDistance = distanceThreshold.MinDistance;
            return false;
        }

        private bool ScanHasFinished(out DistanceThreshold distanceThreshold)
        {
            distanceThreshold = DistanceThreshold;
            ResetScan();
            return true;
        }
        
        private DistanceThreshold SkipOverlap(DistanceThreshold current)
        {
            current.MinDistance += searchDistanceInterval;
            return current;
        }

        private bool ContinueScan(DistanceThreshold currentDistance, out DistanceThreshold nextScanDistance)
        {
            nextScanDistance = NextDistanceThreshold(currentDistance);
            prevDistance = currentDistance.MinDistance;
            return false;
        }

        private DistanceThreshold NextDistanceThreshold(DistanceThreshold current)
        {
            int newMinDistance = current.MinDistance + searchDistanceInterval;
            return new DistanceThreshold
                       {
                           MinDistance = newMinDistance,
                           MaxDistance = newMinDistance + searchDistance
                       };
        }


        private void ResetScan()
        {
            findMinDistance = true;
            prevDistance = int.MaxValue;
            DistanceThreshold = new DistanceThreshold();
        }

        private bool DefaultValues(out DistanceThreshold distanceThreshold)
        {
            distanceThreshold = DefaultDistanceThreshold;
            ResetScan();
            return true;
        }
        
        public int MaxSensorDistance { get; private set; }
        public int MinSensorDistance { get; private set; }
        public DistanceThreshold DefaultDistanceThreshold { get; private set; }

        private DistanceThreshold DistanceThreshold { get; set; }
    }
}