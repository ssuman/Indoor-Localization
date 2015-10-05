using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Indoor_Localization
{
    class KinectDepthData
    {
        public double fieldOfViewHorizontal;
        public double fieldOfViewVertical;
        public int width;
        public int height;
        public int f = 600;

        public double maxDepth;

        public double [] depthLookup = new double[65536];
        public List<Vector3D> pointCloudLookups;

        double a = 3.008;
        double b = -0.002745;

        public int getWidth()
        {
            return width;
        }

        public int getHeight()
        {
            return height;
        }
        public KinectDepthData()
        {
            this.width = 640;
            this.height = 480;
            this.fieldOfViewHorizontal = ConvertToRadians(57.0 / 2);
            this.fieldOfViewVertical = ConvertToRadians(43.00 / 2);
            this.pointCloudLookups =  new List<Vector3D>();
            this.depthLookup = new double[65536];
            this.maxDepth = 1500;
            initDepthReconstructionLookups();
        }

        public double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        public Vector3D depthPixelTo3D(int row, int col, short[] depthImage)
        {
            int index = row * width + col;
            short depthImageVal = depthImage[index];
            return depthLookup[depthImageVal] * pointCloudLookups.ElementAt(index);
        }

        public Vector3D depthPixelTo3d(int index, short[] depthImage)
        {
            short depthImageVal = depthImage[index];
            return depthLookup[depthImageVal] * pointCloudLookups.ElementAt(index);
        }

        public void initDepthReconstructionLookups()
        {
            double h = 1.0 / f;
            double v = 1.0 / f;
            double tanOfFieldOfViewHorizontal = Math.Tan(fieldOfViewHorizontal);
            double tanOfFieldOfViewVertical = Math.Tan(fieldOfViewVertical);
            double x, y, z = 1.0;
            int ind;

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    ind = row * width + col;
                    x = ((((double)col / (double)(width - 1.0)) * 0.5) - 1.0) * tanOfFieldOfViewHorizontal;
                    y = (((double)row / (double)(height - 1.0) * 0.5) - 1.0) * tanOfFieldOfViewVertical;
                    this.pointCloudLookups.Insert(ind, new Vector3D(x, y, z));
                }

            }

            for(int i=0; i < 65536; i++)
            {
                depthLookup[i] = 1.0 / (a + b * i);
            }
        }

        public bool isValidDepthValue(int index, short[] depthImage)
        {
            short depthImageVal = depthImage[index];
            return depthImageVal <= maxDepth && depthImageVal > 0;
        }
    }
}
