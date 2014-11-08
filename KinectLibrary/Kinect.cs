using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Kinect;

namespace KinectLibrary
{
    public class Kinect : IDisposable, IKinect
    {
        public Kinect()
        {
            Initialize();
        }

        public void Stop()
        {
            Dispose();
        }

        ~Kinect()
        {
            Dispose();
        }

        public void Dispose()
        {
            Sensor.Stop();
        }

        private void Initialize()
        {
            // Initialize Kinect Sensor
            Sensor = KinectSensor.KinectSensors[0];
            
            Sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            Sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            Sensor.SkeletonStream.Enable();

            Sensor.Start();


            // Create events
            CreateEvents();

            // Initialize Properties
            IsTrackingSkeleton = false;
        }

        private void CreateEvents()
        {
            Sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);
            Sensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorFrameReady);
            Sensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthFrameReady);
        }

        private int depthImageWidth;
        private int depthImageHeight;
        private short[] depthPixelData;
        private short[] depthDistances;
        private void DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            bool recievedData = false;

            using (DepthImageFrame depthImageFrame = e.OpenDepthImageFrame())
            {
                if (depthImageFrame != null)
                {
                    if (depthPixelData == null)
                        depthPixelData = new short[depthImageFrame.PixelDataLength];

                    depthImageFrame.CopyPixelDataTo(depthPixelData);

                    depthImageHeight = depthImageFrame.Height;
                    depthImageWidth = depthImageFrame.Width;

                    recievedData = true;
                }
                else
                {
                    // apps processing of image data took too long; it got more than 2 frames behind.
                    // the data is no longer avabilable.
                }
            }

            depthDistances = new short[depthImageWidth * depthImageHeight];

            if (recievedData)
            {
                var depthIndex = 0;
                for (var y = 0; y < depthImageHeight; y++)
                {
                    for (var x = 0; x < depthImageWidth; x++, depthIndex++)
                    {
                        var distance = (short)(depthPixelData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth);
                        depthDistances[depthIndex] = distance;
                    }
                }

                DepthDistanceUpdated(depthDistances, depthImageWidth, depthImageHeight);



                DepthImageData = new DepthImage
                                 {DepthData = depthDistances, Height = depthImageHeight, Width = depthImageWidth};
            }
        }

        private byte[] pixelData;
        private void ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            bool receivedData = false;
            int imageHeight = 0, imageWidth = 0;

            using (ColorImageFrame colorImageFrame = e.OpenColorImageFrame())
            {
                if (colorImageFrame != null)
                {
                    if (pixelData == null)
                        pixelData = new byte[colorImageFrame.PixelDataLength];

                    colorImageFrame.CopyPixelDataTo(pixelData);

                    imageHeight = colorImageFrame.Height;
                    imageWidth = colorImageFrame.Width;

                    receivedData = true;
                }
                else
                {
                    // Apps processing of skeleton data took too long; it got more than 2 frames behind.
                    // The data is no longer available.
                }
            }
            if (receivedData)
            {
                ColorImage = new ColorImage {PixelData = pixelData, Height = imageHeight, Width = imageWidth};
            }
        }

        private Tuple<float, float, float, float> FloorClipPLane;
        private Skeleton[] skeletons;
        private void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            IsTrackingSkeleton = false;

            bool receivedData = false;
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if (skeletons == null) // Allocate the first time.
                        skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];

                    skeletonFrame.CopySkeletonDataTo(skeletons);

                    FloorClipPLane = skeletonFrame.FloorClipPlane;

                    receivedData = true;
                }
                else
                {
                    // Apps processing of skeleton data took too long; it got more than 2 frames behind.
                    // The data is no longer available.
                }
            }
            if (receivedData)
            {
                Skeleton trackedSkeleton = null;

                if (TrackClosestSkeleton)
                {
                    if (!Sensor.SkeletonStream.AppChoosesSkeletons)
                        Sensor.SkeletonStream.AppChoosesSkeletons = true;

                    ClosestSkeleton = FindClosestSkeleton(skeletons);

                    if (ClosestSkeleton != null)
                        SetSkeletonToTrack(ClosestSkeleton.TrackingId);

                    trackedSkeleton = ClosestSkeleton;
                }
                else
                {
                    // Get the tracked skeleton. Only one skeleton at a time can be tracked.
                    trackedSkeleton =
                        skeletons.Where(skeleton =>
                                        skeleton != null && skeleton.TrackingState == SkeletonTrackingState.Tracked)
                            .FirstOrDefault();

                    if (Sensor.SkeletonStream.AppChoosesSkeletons)
                        Sensor.SkeletonStream.AppChoosesSkeletons = false;
                }

                TrackedSkeleton = trackedSkeleton;

                if (trackedSkeleton != null)
                {
                    Joint head = trackedSkeleton.Joints[JointType.Head];
                    HeadPosition = head.Position;
                    
                    IsTrackingSkeleton = true;
                }
            }
        }

        /// <summary>
        /// Find the closest skeleton to the Kinect.
        /// </summary>
        /// <param name="skeletons">All skeletons recognized from the Kinect.</param>
        /// <returns>Returns the closest skeleton.</returns>
        private Skeleton FindClosestSkeleton(IEnumerable<Skeleton> skeletons)
        {
            List<Skeleton> recognizedSkeletons =
                skeletons.ToList();

            Skeleton closestSkeleton = null;
            float closestPosition = float.PositiveInfinity;

            foreach (Skeleton recognizedSkeleton in recognizedSkeletons)
            {
                float skeletonPosition = recognizedSkeleton.Position.Z; // Position in meters.
                if (skeletonPosition < closestPosition && skeletonPosition > 0.15f)
                {
                    closestPosition = skeletonPosition;
                    closestSkeleton = recognizedSkeleton;
                }
            }

            return closestSkeleton;
        }

        /// <summary>
        /// Track the skeleton with the specified tracking ID.
        /// </summary>
        /// <param name="trackingID">The tracking ID to the skeleton we want to track.</param>
        private void SetSkeletonToTrack(int trackingID)
        {
            Sensor.SkeletonStream.ChooseSkeletons(trackingID);
        }

        public bool TrackClosestSkeleton { get; set; }
        public Skeleton ClosestSkeleton { get; set; }
        public Skeleton TrackedSkeleton { get; private set; }

        public Vector HeadPositionRelativeToScreen(int horizontalOffset, int floorToKinectOffset, int screenToKinectOffset, int screenWidth)
        {
            float trackedSkeletonHeight = FloorClipPLane.Item1 * HeadPosition.X + FloorClipPLane.Item2 * HeadPosition.Y +
                         FloorClipPLane.Item3 * HeadPosition.Z + FloorClipPLane.Item4;

            float relativeX = (HeadPosition.X - horizontalOffset) / screenWidth;
            float relativeY = trackedSkeletonHeight - floorToKinectOffset - screenToKinectOffset;

            return new Vector(HeadPosition.X, trackedSkeletonHeight, HeadPosition.Z);
        }

        public KinectSensor Sensor { get; private set; }

        public SkeletonPoint HeadPosition { get; set; }

        public bool IsTrackingSkeleton { get; set; }
        public ColorImage ColorImage { get; private set; }

        public DepthImage DepthImageData { get; private set; }

        public event DepthDistanceEventHandler DepthDistanceUpdated = delegate { };
    }
}