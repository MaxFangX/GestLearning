using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace KinectLibrary.DTWGestureRecognition
{
    public class GestureRecognition : IGestureRecognition, IGestureRecognitionDebug
    {
        private readonly StoredGestures storedGestures;
        private readonly GestureStream gestureStream;
        private readonly DtwGestureRecognizer dtwGestureRecognizer;

        public GestureRecognition()
        {
            const int maxStoredFrames = 40;

            storedGestures = new StoredGestures();
            gestureStream = new GestureStream(maxStoredFrames);

            
            dtwGestureRecognizer = new DtwGestureRecognizer();
        }

        public bool LoadGesturesFromFile(string path)
        {
            bool success = storedGestures.LoadGesturesFromFile(path);
            return success;
        }

        public bool SaveGesturesToFile(string path)
        {
            bool success = storedGestures.SaveToFile(path);
            return success;
        }
        
        /// <summary>
        /// Analyze the hand. 
        /// If gesture recognizing is enabled, it will search for a matching gesture.
        /// If gesture recording is enabled, it will produce a gesture object after the recording is completed.
        /// The gesture will be available through the GestureRecorded event.
        /// </summary>
        /// <param name="currentFingerPositions">The hand in the current frame.</param>
        public void AnalyzeFrame(Hand currentFingerPositions)
        {
            if (Recognizing)
            {
                gestureStream.AddFrame(currentFingerPositions);

                if (gestureStream.IsSaturated)
                {
                    Gesture gestureCandidate;
                    bool foundGestureCandidate = dtwGestureRecognizer.FindGestureCandidate(currentFingerPositions, storedGestures.Gestures, out gestureCandidate);

                    if (foundGestureCandidate)
                    {
                        Gesture recognizedGesture;
                        Gesture currentGesture = gestureStream.ToGesture();
                        bool successfullRecognition = dtwGestureRecognizer.RecognizeGesture(currentGesture, gestureCandidate, out recognizedGesture);

                        if (successfullRecognition)
                            GestureRecognized(recognizedGesture); // Fire event.
                    }
                }
            }

            if (Recording)
            {
                gestureStream.AddFrame(currentFingerPositions);
                System.Diagnostics.Debug.WriteLineIf(gestureStream.IsSaturated, "Overflow of frames in the stream.");
            }
        }

        public void StartRecording()
        {
            StopRecognizer();
            Recording = true;
            ClearGestureStream();
        }

        public void StopRecording()
        {
            Recording = false;

            Gesture newGesture = gestureStream.ToGesture();
            //gestureStream.MaxFrames = newGesture.Frames.Count;

            const int minimumGestureFrames = 10;
            if (newGesture.Frames.Count > minimumGestureFrames)
                GestureRecorded(newGesture);

            ClearGestureStream();
        }

        public void StoreGesture(Gesture gesture)
        {
            storedGestures.AddGesture(gesture);
        }

        public void StartRecognizer()
        {
            Recording = false;
            Recognizing = true;
            ClearGestureStream();
        }

        public void StopRecognizer()
        {
            Recognizing = false;
            ClearGestureStream();
        }

        /// <summary>
        /// Clear all frames from the gesture stream.
        /// </summary>
        private void ClearGestureStream()
        {
            gestureStream.Clear();
        }

        public GestureStream GestureStream { get { return gestureStream; } }

        public double LatestPathCost { get { return dtwGestureRecognizer.LatestPathCost; } }
        public long AccumulatedFrameCount { get { return gestureStream.AccumulatedFrameCount; } }
        public double GestureCandidateDistance { get { return dtwGestureRecognizer.GestureCandidateDistance; } }

        public bool Recording { get; private set; }
        public bool Recognizing { get; private set; }

        public event GestureReady GestureRecognized = delegate { };
        public event GestureRecorded GestureRecorded = delegate { };
    }
}