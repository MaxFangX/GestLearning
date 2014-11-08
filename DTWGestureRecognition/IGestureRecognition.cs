namespace KinectLibrary.DTWGestureRecognition
{
    public interface IGestureRecognition
    {
        void StartRecording();
        void StopRecording();

        void AnalyzeFrame(Hand currentFingerPositions);

        void StartRecognizer();
        void StopRecognizer();

        bool Recording { get; }
        bool Recognizing { get; }

        void StoreGesture(Gesture gesture);

        bool SaveGesturesToFile(string path);
        bool LoadGesturesFromFile(string path);

        event GestureReady GestureRecognized;
        event GestureRecorded GestureRecorded;
    }

    public interface IGestureRecognitionDebug
    {
        double LatestPathCost { get; }
        long AccumulatedFrameCount { get; }
        double GestureCandidateDistance { get; }
    }
}