using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing
{
    public class Processors
    {
        public Bitmap inputBitmap;

        public Processors(string imagePath)
        {
            inputBitmap = new Bitmap(imagePath);
        }

        ~Processors()
        {
            inputBitmap.Dispose();
        }

        private class Centroid
        {
            public double X { get; set; }
            public double Y { get; set; }
            public int Count { get; set; }
        }

        private class BoundingBox
        {
            public double LeftTopX { get; set; } = -1;
            public double LeftTopY { get; set; } = -1;
            public double RightBottomX { get; set; } = -1;
            public double RightBottomY { get; set; } = -1;
            public double Width => RightBottomX - LeftTopX;
            public double Height => RightBottomY - LeftTopY;
        }

        public void Regionprops()
        {
            var path = GetDestinationPath();
            var centroids = new Centroid[256];
            var boundingBoxes = new BoundingBox[256];
            var equivDiameters = new double[256];

            for (int i = 0; i < 256; i++)
            {
                centroids[i] = new Centroid();
                boundingBoxes[i] = new BoundingBox();
            }

            for (int x = 0; x < inputBitmap.Width; x++)
            {
                for (int y = 0; y < inputBitmap.Height; y++)
                {
                    var id = inputBitmap.GetPixel(x, y).R;
                    centroids[id].X += x;
                    centroids[id].Y += y;
                    centroids[id].Count++;

                    boundingBoxes[id].LeftTopX = 
                        (boundingBoxes[id].LeftTopX == -1 || x < boundingBoxes[id].LeftTopX) ? x : boundingBoxes[id].LeftTopX;
                    boundingBoxes[id].LeftTopY =
                        (boundingBoxes[id].LeftTopY == -1 || y < boundingBoxes[id].LeftTopY) ? y : boundingBoxes[id].LeftTopY;

                    boundingBoxes[id].RightBottomX =
                        (boundingBoxes[id].RightBottomX == -1 || x > boundingBoxes[id].RightBottomX) ? x : boundingBoxes[id].RightBottomX;
                    boundingBoxes[id].RightBottomY =
                        (boundingBoxes[id].RightBottomY == -1 || y > boundingBoxes[id].RightBottomY) ? y : boundingBoxes[id].RightBottomY;

                }
            }

            for (int i = 0; i < 256; i++)
            {
                if (centroids[i].Count == 0)
                {
                    centroids[i].X = -1;
                    centroids[i].Y = -1;
                    equivDiameters[i] = 0;
                }
                else
                {
                    centroids[i].X /= centroids[i].Count;
                    centroids[i].Y /= centroids[i].Count;
                    equivDiameters[i] = Math.Sqrt(4 * centroids[i].Count / Math.PI);
                }
            }

            using (var writer = File.CreateText(path))
            {
                writer.WriteLine("ID\tCentroid\tBounding Box\t");
                for (int i = 0; i < 256; i++)
                {
                    writer.WriteLine(
                        $"{i}\t" +
                        $"[{centroids[i].X}; {centroids[i].Y}]\t" +
                        $"[{boundingBoxes[i].LeftTopX}; {boundingBoxes[i].LeftTopY}; {boundingBoxes[i].Width}; {boundingBoxes[i].Height}]\t" +
                        $"{equivDiameters[i]}");
                }
            }
        }

        private string GetDestinationPath()
        {
            Console.WriteLine("Write destination path: ");
            return Console.ReadLine();
        }
    }
}
