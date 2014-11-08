using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace $safeprojectname$
{
    public partial class MainWindow : Window
    {
        //Instantiate the Kinect runtime. Required to initialize the device.
        //IMPORTANT NOTE: You can pass the device ID here, in case more than one Kinect device is connected.
        KinectSensor sensor = KinectSensor.KinectSensors.First();
        short[] pixelData;

        public MainWindow()
        {
            InitializeComponent();

            //Runtime initialization is handled when the window is opened. When the window
            //is closed, the runtime MUST be unitialized.
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.Unloaded += new RoutedEventHandler(MainWindow_Unloaded);

            sensor.DepthStream.Enable();
            sensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(sensor_DepthFrameReady);
        }

        void sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            bool receivedData = false;

            using (DepthImageFrame DFrame = e.OpenDepthImageFrame())
            {
                if (DFrame == null)
                {
                    // The image processing took too long. More than 2 frames behind.
                }
                else
                {
                    pixelData = new short[DFrame.PixelDataLength];
                    DFrame.CopyPixelDataTo(pixelData);
                    receivedData = true;
                }
            }

            if (receivedData)
            {
                BitmapSource source = BitmapSource.Create(640, 480, 96, 96,
                        PixelFormats.Gray16, null, pixelData, 640 * 2);

                depthImage.Source = source;
            }
        }

        void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            sensor.Stop();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //RuntimeOptions.UseDepth is used because I am obtaining depth data
            sensor.Start();
        }

    }
}
