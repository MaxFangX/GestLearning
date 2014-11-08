using System;
using System.Collections.Generic;

namespace KinectLibrary.DTWGestureRecognition
{
    public class Gesture
    {
        private readonly List<Hand> fingerPositions;

        public Gesture(string name) : this()
        {
            Name = name;
        }

        public Gesture(IEnumerable<Hand> fingerPositions)
        {
            this.fingerPositions = new List<Hand>(fingerPositions);
        }

        public Gesture()
        {
            fingerPositions = new List<Hand>();
        }

        public void AddFrame(Hand handPosition)
        {
            fingerPositions.Add(handPosition);
        }

        public List<Hand> Frames { get { return fingerPositions; } }
        public Hand LastFrame { get { return fingerPositions[fingerPositions.Count - 1]; } }

        private string name;
        public string Name { get { return string.IsNullOrEmpty(name) ? "NO_NAME" : name; } set { name = value; } }
    }
}