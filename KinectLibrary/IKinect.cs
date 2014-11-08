using Microsoft.Kinect;

namespace KinectLibrary
{
    public interface IKinect
    {
        Skeleton TrackedSkeleton { get; }
        Skeleton ClosestSkeleton { get; }
        bool TrackClosestSkeleton { get; set; }

        void Stop();
        Vector HeadPositionRelativeToScreen(int horizontalOffset, int floorToKinectOffset, int screenToKinectOffset, int screenWidth);
        bool IsTrackingSkeleton { get; set; }
        ColorImage ColorImage { get; }
        event DepthDistanceEventHandler DepthDistanceUpdated;
    }
}