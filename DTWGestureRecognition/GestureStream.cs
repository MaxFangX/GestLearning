using System;
using System.Collections.Generic;

namespace KinectLibrary.DTWGestureRecognition
{
    public class GestureStream
    {
        private  Queue<Hand> fingerPositions;
        private readonly int maxFrameCount;
        private long totalFrameCount;

        public GestureStream(int maxFrameCount)
        {
            this.maxFrameCount = maxFrameCount;
            fingerPositions = new Queue<Hand>(maxFrameCount + 1);
        }
        
        public void AddFrame(Hand handPosition)
        {
            fingerPositions.Enqueue(handPosition);
            if (fingerPositions.Count > maxFrameCount)
                fingerPositions.Dequeue();

            totalFrameCount++;
        }

        /// <summary>
        /// Convert frames stored in the stream to a gesture object.
        /// </summary>
        /// <returns></returns>
        public Gesture ToGesture()
        {
            return new Gesture(fingerPositions);
        }

        public List<Hand> ToList()
        {
            return new List<Hand>(fingerPositions);
        }

        /// <summary>
        /// Remove all stored frames from the stream.
        /// </summary>
        public void Clear()
        {
            fingerPositions.Clear();
        }

        /// <summary>
        /// Returns true if the stream has its maximum number of frames;
        /// Otherwise false.
        /// </summary>
        public bool IsSaturated
        {
            get { return fingerPositions.Count == maxFrameCount; }
        }

        public long AccumulatedFrameCount { get { return totalFrameCount; } }

        public int MaxFrames
        {
            set { fingerPositions = new Queue<Hand>(value);}
        }
    }
}