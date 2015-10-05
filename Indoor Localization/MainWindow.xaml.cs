using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.IO;
using System.Windows.Media.Media3D;

namespace Indoor_Localization
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private KinectSensor sensor;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (this.sensor != null)
            {
                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                this.sensor.DepthFrameReady += this.SensorDepthFrameReady;
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }
        }

        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    short[] pixelData = new short[depthFrame.PixelDataLength];

                    int stride = depthFrame.Width * 2;
                    depthFrame.CopyPixelDataTo(pixelData);
                    int minDepth = depthFrame.MinDepth;
                    int maxDepth = depthFrame.MaxDepth;
                    KinectDepthData depthData = new KinectDepthData();
                    List<Vector3D> filteredPointCloud = new List<Vector3D>();
                    List<Vector> pixelLocs = new List<Vector>();
                    List<Vector3D> pointCloudNormals = new List<Vector3D>();
                    List<Vector3D> outlierCloud = new List<Vector3D>();
                    PlaneFilter filter = new PlaneFilter();
                    filter.GenerateFilteredPointCloud(pixelData, filteredPointCloud, pixelLocs, pointCloudNormals, outlierCloud, depthData);
                    //this.Image.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height,
                    //    96, 96, PixelFormats.Gray16, null, pixelData, stride);
                }
            }
        }

    }
}
