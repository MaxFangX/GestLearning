using System.Collections.Generic;
using System.Linq;
using KinectLibrary.DTWGestureRecognition;

namespace KinectLibrary.Enhancements
{
    public class HandEnhancements
    {
        private Hand prevHand;
        private readonly Queue<Hand> handQueue; // Correct hand frames to be used in the prediction algorithm.
        private readonly List<Hand> inconsistentHands;
        private readonly IPrediction predictionModule;
        private readonly IGestureRecognition gestureRecognitionModule;
        private const int MaxHandQueueCount = 40;
        
        public HandEnhancements(IPrediction predictionModule, IGestureRecognition gestureRecognitionModule)
        {
            FrameLimit = 10;
            PredicitionWeight = 0.8d;

            prevHand = new Hand();
            handQueue = new Queue<Hand>(60);
            inconsistentHands = new List<Hand>(FrameLimit + 1);

            this.predictionModule = predictionModule;
            this.gestureRecognitionModule = gestureRecognitionModule;
        }

        /// <summary>
        /// This will add fingers to previous frames that have fewer fingers than the hand from the current frame.
        /// The missing finger data will be calculated from a prediction based approach.
        /// It will also add all correct frames to the gesture recognition stream.
        /// </summary>
        /// <param name="currentHand">Representation of the current hand.</param>
        public void PreventHandIncosistency(Hand currentHand)
        {
            if (QueueIsSaturated() && currentHand.FingerCount != prevHand.FingerCount)
            {
                // The queue needs to be saturated so the prediction algorithm has some data to work with. This will give better results.

                inconsistentHands.Add(currentHand);

                // Finger count has changed, this is not an inconsistency.
                if (inconsistentHands.Count > FrameLimit)
                {
                    foreach (Hand hand in inconsistentHands)
                    {
                        handQueue.Enqueue(hand);
                        gestureRecognitionModule.AnalyzeFrame(hand);
                    }

                    inconsistentHands.Clear();
                }
            }
            // Finger count has changed, this is an inconsistency.
            else if (inconsistentHands.Count > 0)
            {
                inconsistentHands.Add(currentHand);

                // Fix inconsistencies.
                List<Hand> fixedHands = FixMissingFingers(inconsistentHands);

                // Add all fixed hands to the gesture analyzer.
                foreach (Hand fixedHand in fixedHands)
                    gestureRecognitionModule.AnalyzeFrame(fixedHand);

                inconsistentHands.Clear();
                FixedInconsistencies = true;
            }
            // Finger count has not not changed, all is good.
            else
            {
                handQueue.Enqueue(currentHand);
                gestureRecognitionModule.AnalyzeFrame(currentHand);

                // Keep the number of elements in the queue to a specified limit.
                if (handQueue.Count > MaxHandQueueCount)
                {
                    int handsToDequeue = handQueue.Count - 1 - MaxHandQueueCount;
                    for (int i = 0; i < handsToDequeue; i++)
                        handQueue.Dequeue();
                }

                FixedInconsistencies = false;
            }

            prevHand = currentHand;
        }

        private bool QueueIsSaturated()
        {
            return handQueue.Count > 30;
        }

        /// <summary>
        /// This will fix missing fingers by using previous correct frames to predict where it should be.
        /// </summary>
        /// <param name="handsToFix">The frames with missing fingers.</param>
        /// <returns>Returns a list with fixed hands.</returns>
        private List<Hand> FixMissingFingers(IEnumerable<Hand> handsToFix)
        {
            List<Hand> fixedHands = new List<Hand>(FrameLimit);

            foreach (Hand hand in handsToFix)
            {
                Hand fixedHand = new Hand();
                Hand predictedHand = predictionModule.PredictedHandState(handQueue.ToList(), PredicitionWeight);

                for (int i = 0; i < Finger.FingerCount; i++)
                {
                    int fingerID = i;
                    bool hasFinger = hand.HasFinger(i);

                    if (!hasFinger)
                        fixedHand.Fingers[i] = predictedHand.Fingers[fingerID];
                    else
                        fixedHand.Fingers[i] = hand.Fingers[i];
                }

                fixedHands.Add(fixedHand);
                handQueue.Enqueue(fixedHand);
            }

            return fixedHands;
        }

        /// <summary>
        /// Max frames that will temporarily be stored before new hands is not 
        /// considered inconsisten but a normal change of finger count.
        /// </summary>
        public int FrameLimit { get; set; }

        /// <summary>
        /// Weighting factor used by the prediction algorithm.
        /// </summary>
        public double PredicitionWeight { get; set; }

        /// <summary>
        /// If inconsistencies was detected and fixed.
        /// </summary>
        public bool FixedInconsistencies { get; private set; }
    }
}