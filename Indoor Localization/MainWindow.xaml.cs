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
        short[] pixelData;
        int minDepth;
        int maxDepth;


        int height;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
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
                    this.pixelData = pixelData;
                    this.maxDepth = depthFrame.MaxDepth;
                    this.minDepth = depthFrame.MinDepth;
                    this.Image.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height,
                        96, 96, PixelFormats.Gray16, null, pixelData, stride);

                    int temp = 0;
                    int i = 0;
                    for (int y = 0; y < 480; y += s)
                    {
                        for (int x = 0; x < 640; x += s)
                        {
                            temp = ((ushort)this.pixelData[x + y * 640]) >> 3;
                            ((TranslateTransform3D)points[i].Transform).OffsetZ = temp;
                            i++;
                        }

                    }
                }
            }
        }

        GeometryModel3D[] points = new GeometryModel3D[640 * 420];
        int s = 4;

        private void button_Click(object sender, RoutedEventArgs e)
        {
            DirectionalLight dirLight = new DirectionalLight();
            dirLight.Color = Colors.White;
            dirLight.Direction = new Vector3D(1, 1, 1);

            PerspectiveCamera camera1 = new PerspectiveCamera();
            camera1.FarPlaneDistance = 10000;
            camera1.NearPlaneDistance = 100;
            camera1.FieldOfView = 10;
            camera1.Position = new Point3D(160, 120, -1000);
            camera1.LookDirection = new Vector3D(0, 0, 1);
            camera1.UpDirection = new Vector3D(0, -1, 0);


            Model3DGroup group = new Model3DGroup();

            int i = 0;

            for (int y = 0; y < 480; y += s)
            {
                for (int x = 0; x < 640; x += s)
                {
                    points[i] = triangle(x, y, s);
                    points[i].Transform = new TranslateTransform3D(0, 0, 0);

                    group.Children.Add(points[i]);
                    i++;
                }
            }

            group.Children.Add(dirLight);
            ModelVisual3D modelsVisual = new ModelVisual3D();
            modelsVisual.Content = group;
            Viewport3D myViewport = new Viewport3D();
            myViewport.IsHitTestVisible = false;
            myViewport.Camera = camera1;
            myViewport.Children.Add(modelsVisual);
            canvas1.Children.Add(myViewport);

            myViewport.Height = canvas1.Height;
            myViewport.Width = canvas1.Width;
            Canvas.SetTop(myViewport, 0);
            Canvas.SetLeft(myViewport, 0);

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

            int minDepth = this.minDepth;
            int maxDepth = this.maxDepth;
            KinectDepthData depthData = new KinectDepthData();
            List<Vector3D> filteredPointCloud = new List<Vector3D>();
            List<Vector> pixelLocs = new List<Vector>();
            List<Vector3D> pointCloudNormals = new List<Vector3D>();
            List<Vector3D> outlierCloud = new List<Vector3D>();
            PlaneFilter filter = new PlaneFilter();
            //pointCloudNormals = filter.GenerateSampledPointCloud(depthData, this.pixelData, pointCloudNormals, 200000);
            Point3DCollection collection = new Point3DCollection();
            foreach (Vector3D v in pointCloudNormals)
            {
                collection.Add(new Point3D(v.X, v.Y, v.Z));
            }
           
            //filter.GenerateFilteredPointCloud(pixelData, filteredPointCloud, pixelLocs, pointCloudNormals, outlierCloud, depthData);
        }

        private GeometryModel3D triangle(double x, double y, double z)
        {
            Point3DCollection corners = new Point3DCollection();
            corners.Add(new Point3D(x, y, 0));
            corners.Add(new Point3D(x, y + s, 0));
            corners.Add(new Point3D(x + s, y + s, 0));

            Int32Collection Triangles = new Int32Collection();
            Triangles.Add(0);
            Triangles.Add(1);
            Triangles.Add(2);

            MeshGeometry3D tmesh = new MeshGeometry3D();
            tmesh.Positions = corners;
            tmesh.TriangleIndices = Triangles;
            tmesh.Normals.Add(new Vector3D(0, 0, -1));

            GeometryModel3D msheet = new GeometryModel3D();
            msheet.Geometry = tmesh;
            msheet.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
            return msheet;
        }
    }
}
