using System;
using System.Collections.Generic;
using System.Diagnostics;
using KinectLibrary.Helpers;

namespace KinectLibrary.DTWGestureRecognition
{
    public class StoredGestures
    {
        public StoredGestures()
        {
            Gestures = new List<Gesture>();
            NotPersistedGestures = new List<Gesture>();
        }

        private List<Gesture> NotPersistedGestures { get; set; }
        public bool HasNotPersistedGestures { get { return NotPersistedGestures.Count != 0; } }

        public List<Gesture> Gestures { get; private set; }

        public void AddGesture(Gesture gesture)
        {
            Gestures.Add(gesture);
            NotPersistedGestures.Add(gesture);
        }

        public void RemoveGesture(Gesture gesture)
        {
            Gestures.Remove(gesture);
            NotPersistedGestures.Remove(gesture);
        }

        /// <summary>
        /// Load gestures from the specified file overriding current gestures in memory.
        /// </summary>
        /// <param name="path">Gestures file path.</param>
        /// <returns>Returns true if loading were successfull; otherwise false.</returns>
        public bool LoadGesturesFromFile(string path)
        {
            List<Gesture> readGestures;
            bool success = GetGesturesFromFile(path, out readGestures);

            if (success)
                Gestures = readGestures;

            return success;
        }

        /// <summary>
        /// Get gestures from the specified file.
        /// </summary>
        /// <param name="path">Gestures file path.</param>
        /// <param name="gesturesFromFile">Gestures from the specified file.</param>
        /// <returns>Returns true if gestures were retrieved successfully; otherwise false.</returns>
        public bool GetGesturesFromFile(string path, out List<Gesture> gesturesFromFile)
        {
            bool success = XmlHelpers.ReadFromFile<List<Gesture>>(path, out gesturesFromFile);
            Trace.WriteLine(String.Format("Read gestures from file. File path: {0}", path));
            return success;
        }

        /// <summary>
        /// Save all gestures in memory to the specified file.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <returns>Return true if gestures were stored successfully; otherwise false.</returns>
        public bool SaveToFile(string path)
        {
            bool success = XmlHelpers.WriteToFile<List<Gesture>>(path, Gestures);
            return success;
        }
    }
}