using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Indoor_Localization
{
    class PlaneFilter
    {
        // Parameters for basic FSPF
        int maxPoints;
        double numSamples;
        int numLocalSamples;
        int planeSize;
        double worldPlaneSize;

        double maxError;
        int numNeighborSamples;
        double minInlierFraction;
        double maxDepthDiff;
        double MAXRETRIES;

        int numGlobalSamples;
        double minOffsetError;
        int numSampleLocations;
  
        public PlaneFilter()
        {
            this.maxPoints = 2000;
            this.numSamples = 200000;
            this.numNeighborSamples = 20000;
            this.numLocalSamples = 50;
            this.numGlobalSamples = 60;
            this.planeSize = 60;
            this.maxDepthDiff = 1.8;
            this.maxError = 0.03;
            this.worldPlaneSize = 2;
            this.minOffsetError = 0.02;
            this.MAXRETRIES = 2;
            this.minInlierFraction = 0.8;

        }

        bool sampleLocation(KinectDepthData data, short[] depthImage, int index, int row, int col, Vector3D p,
          int rMin, int height, int cMin, int width)
        {
            int retries = 0;
            bool valid = false;

            //initialize MAX RETRIES
            while ((!valid) && retries <= MAXRETRIES)
            {
                Random rndm = new Random();
                row = rMin + rndm.Next() % height;
                col = cMin + rndm.Next() % width;
                index = row * data.getWidth() + col;
                valid = data.isValidDepthValue(index, depthImage);
                retries++;
                numSampleLocations++;
            }

            if (valid)
            {
                Vector3D v3D = data.depthPixelTo3d(index, depthImage);
                p = new Vector3D(v3D.X, v3D.Y, v3D.Z);
            }
            return valid;
        }

        public void GenerateFilteredPointCloud(short[] depthImage, List<Vector3D> filteredPointCloud,
            List<Vector> pixelLocs, List<Vector3D> pointCloudNormals, List<Vector3D> outlierCloud, KinectDepthData data)
        {

            filteredPointCloud.Clear();
            pixelLocs.Clear();
            pointCloudNormals.Clear();
            outlierCloud.Clear();

            double minInliers = this.numLocalSamples;
            double maxOutliers = (1 - this.minInlierFraction) * this.numLocalSamples;
            double planeSizeH = 0.5 * this.planeSize;
            double PlanseSize = this.planeSize;
            double w2 = data.width - this.planeSize;
            double h2 = data.height - this.planeSize;

            int numPlanes = 0;
            int numPoints = 0;

            int ind1 = 0, ind2 = 0, ind3 = 0;

            // Call the math.tan function
            double sampleRadiusHFactor = this.worldPlaneSize * data.width / (4.0 * Math.Tan(data.fieldOfViewHorizontal));
            double sampleRadiusVFactor = this.worldPlaneSize * data.height / (4.0 * Math.Tan(data.fieldOfViewVertical));

            Vector3D p1, p2, p3, p;
            Vector l1, l2, l3, l;

            p1 = new Vector3D();
            p2 = new Vector3D();
            p3 = new Vector3D();
            p = new Vector3D();

            l1 = new Vector();
            l2 = new Vector();
            l3 = new Vector();
            l = new Vector();

            List<Vector3D> neighborhoodInliers = new List<Vector3D>();
            List<Vector> neighborhoodPixelLocs = new List<Vector>();

            double d;
            int sampleRadiusH, sampleRadiusV, rMin, rMax, cMin, cMax, dR, dC;

            // numSamples not present
            for (int i = 0; numPoints < this.maxPoints && i < this.numSamples; i++)
            {
                if (!sampleLocation(data, depthImage, ind1, (int)l1.Y, (int)l1.X, p1, (int)planeSizeH, (int)h2, (int)planeSizeH, (int)w2))
                    continue;
                if (!sampleLocation(data, depthImage, ind2, (int)l2.Y, (int)l2.X, p2, (int)(l1.Y - planeSizeH), planeSize, (int)(l1.X - planeSizeH), planeSize))
                    continue;
                if (!sampleLocation(data, depthImage, ind3, (int)l3.Y, (int)l3.X, p3, (int)(l1.Y - planeSizeH), planeSize, (int)(l1.X - planeSizeH), planeSize))
                    continue;

                Vector3D n = Vector3D.CrossProduct((p1 - p2), (p3 - p2));

                d = Vector3D.DotProduct(p1, n);

                double meanDepth = (p1.X + p2.X + p3.X) * 0.3333;

                sampleRadiusH = (int)Math.Ceiling(sampleRadiusHFactor / meanDepth * Math.Sqrt(1.0 - (n.Y * n.Y)));
                sampleRadiusV = (int)Math.Ceiling(sampleRadiusVFactor / meanDepth * Math.Sqrt(1 - n.Z * n.Z));
                rMin = (int)Math.Max(0, l1.Y - sampleRadiusV);
                rMax = (int)Math.Min(data.height - 1, l1.Y + sampleRadiusV);
                cMin = (int)Math.Max(0, l1.X - sampleRadiusH);
                cMax = (int)Math.Min(data.width - 1, l1.X + sampleRadiusH);
                dR = rMax - rMin;
                dC = cMax - cMin;

                if (sampleRadiusH < 2.0 && sampleRadiusV < 2.0)
                {
                    continue;
                }

                int inliers = 0, outliers = 0;
                neighborhoodInliers.Clear();
                neighborhoodPixelLocs.Clear();
                for (int j = 0; outliers < maxOutliers && j < this.numLocalSamples; j++)
                {

                    int r, c, ind = 0;
                    if (!sampleLocation(data, depthImage, ind, (int)l.Y, (int)l.X, p, rMin, dR, cMin, dC))
                        continue;

                    double err = Math.Abs(Vector3D.DotProduct(n, p) - d);

                    // What is max DEpth Diff ?? What is filtered Parmas
                    if (err < maxError && p.X < meanDepth + maxDepthDiff && p.X > meanDepth - maxDepthDiff)
                    {
                        inliers++;
                        neighborhoodInliers.Add(new Vector3D(p.X, p.Y, p.Z));
                        neighborhoodPixelLocs.Add(new Vector(l.X, l.Y));
                    }
                    else
                    {
                        outliers++;
                    }
                }

                if (inliers >= minInliers && inliers > 3)
                {

                    Vector3D ng = n;


                    //ng.w = 0;

                    for (int k = 0; k < neighborhoodInliers.Count; k++)
                    {
                        filteredPointCloud.Add(neighborhoodInliers.ElementAt(i));
                        pointCloudNormals.Add(ng);
                        pixelLocs.Add(neighborhoodPixelLocs.ElementAt(i));
                    }

                    filteredPointCloud.Add(new Vector3D(p1.X, p1.Y, p1.Z));
                    filteredPointCloud.Add(new Vector3D(p2.X, p2.Y, p2.Z));
                    filteredPointCloud.Add(new Vector3D(p3.X, p2.Y, p3.Z));

                    pointCloudNormals.Add(ng);
                    pointCloudNormals.Add(ng);
                    pointCloudNormals.Add(ng);

                    pixelLocs.Add(new Vector(l1.X, l1.Y));
                    pixelLocs.Add(new Vector(l2.X, l2.Y));
                    pixelLocs.Add(new Vector(l3.X, l3.Y));

                    numPoints += neighborhoodInliers.Count + 3;
                    numPlanes += 1;
                }
                else
                {

                    for (int j = 0; j < neighborhoodInliers.Count; j++)
                    {
                        outlierCloud.Add(neighborhoodInliers.ElementAt(i));
                    }

                    outlierCloud.Add(p1);
                    outlierCloud.Add(p2);
                    outlierCloud.Add(p3);
                }
            }

        }
    }
}
