using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

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

        #region Regionprops

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
            Console.WriteLine("Write destination path: ");
            var path = Console.ReadLine();
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

        #endregion

        public void KirschFiltration()
        {
            Console.WriteLine("Write output file name: ");
            var path = Console.ReadLine();
            Console.WriteLine("Is image RGB(1) or mono(2)?");
            var type = Convert.ToInt32(Console.ReadLine());

            var result = ConvolutionFilter(inputBitmap, Kirsch3x3x8);
            
            if (type == 2)
            {
                result.Save(path);
            }
            else if (type == 1)
            {
                var redBitmap = GetOneChannelImage(result, 0);
                var greenBitmap = GetOneChannelImage(result, 1);
                var blueBitmap = GetOneChannelImage(result, 2);
                redBitmap.Save("R" + path);
                greenBitmap.Save("G" + path);
                blueBitmap.Save("B" + path);
            }
        }

        private Bitmap GetOneChannelImage(Bitmap rgbBitmap, int channel)
        {
            var resultBitmap = new Bitmap(rgbBitmap);
            for (int y = 0; y < rgbBitmap.Height; y++)
            {
                for (int x = 0; x < rgbBitmap.Width; x++)
                {
                    var inputColor = rgbBitmap.GetPixel(x, y);
                    switch (channel)
                    {
                        case 0:
                            resultBitmap.SetPixel(x, y, Color.FromArgb(inputColor.R, inputColor.R, inputColor.R));
                            break;
                        case 1:
                            resultBitmap.SetPixel(x, y, Color.FromArgb(inputColor.G, inputColor.G, inputColor.G));
                            break;
                        case 2:
                            resultBitmap.SetPixel(x, y, Color.FromArgb(inputColor.B, inputColor.B, inputColor.B));
                            break;

                    }
                }
            }
            return resultBitmap;
        }

        private double[,,] Kirsch3x3x8
        {
            get
            {
                double[,] baseKernel = new double[,]
                { { -3, -3,  5, },
                  { -3,  0,  5, },
                  { -3, -3,  5, }, };

                double[,,] kernel = RotateMatrix(baseKernel, 45);


                return kernel;
            }
        }

        public double[,,] RotateMatrix(double[,] baseKernel,
                                                     double degrees)
        {
            double[,,] kernel = new double[(int)(360 / degrees),
               baseKernel.GetLength(0), baseKernel.GetLength(1)];

            int xOffset = baseKernel.GetLength(1) / 2;
            int yOffset = baseKernel.GetLength(0) / 2;

            for (int y = 0; y < baseKernel.GetLength(0); y++)
            {
                for (int x = 0; x < baseKernel.GetLength(1); x++)
                {
                    for (int compass = 0; compass <
                        kernel.GetLength(0); compass++)
                    {
                        double radians = compass * degrees *
                                         Math.PI / 180.0;

                        int resultX = (int)(Math.Round((x - xOffset) *
                                   Math.Cos(radians) - (y - yOffset) *
                                   Math.Sin(radians)) + xOffset);

                        int resultY = (int)(Math.Round((x - xOffset) *
                                   Math.Sin(radians) + (y - yOffset) *
                                   Math.Cos(radians)) + yOffset);

                        kernel[compass, resultY, resultX] =
                                                    baseKernel[y, x];
                    }
                }
            }

            return kernel;
        }

        private Bitmap ConvolutionFilter(Bitmap sourceBitmap, double[,,] filterMatrix)
        {
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0,
                                     sourceBitmap.Width, sourceBitmap.Height),
                                                       ImageLockMode.ReadOnly,
                                                 PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);

            int filterWidth = filterMatrix.GetLength(1);

            int filterOffset = (filterWidth - 1) / 2;

            for (int offsetY = filterOffset; offsetY <
                sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX <
                    sourceBitmap.Width - filterOffset; offsetX++)
                {
                    double blue = 0;
                    double green = 0;
                    double red = 0;

                    int byteOffset = offsetY *
                                 sourceData.Stride +
                                 offsetX * 4;

                    for (int compass = 0; compass <
                         filterMatrix.GetLength(0); compass++)
                    {

                        double blueCompass = 0.0;
                        double greenCompass = 0.0;
                        double redCompass = 0.0;

                        for (int filterY = -filterOffset;
                            filterY <= filterOffset; filterY++)
                        {
                            for (int filterX = -filterOffset;
                                filterX <= filterOffset; filterX++)
                            {


                                int calcOffset = byteOffset +
                                             (filterX * 4) +
                                             (filterY * sourceData.Stride);


                                blueCompass += pixelBuffer[calcOffset] *
                                                filterMatrix[compass,
                                                filterY + filterOffset,
                                                filterX + filterOffset];

                                greenCompass += pixelBuffer[calcOffset + 1] *
                                                filterMatrix[compass,
                                                filterY + filterOffset,
                                                filterX + filterOffset];

                                redCompass += pixelBuffer[calcOffset + 2] *
                                                filterMatrix[compass,
                                                filterY + filterOffset,
                                                filterX + filterOffset];
                            }
                        }

                        blue = blueCompass > blue ? blueCompass : blue;
                        green = greenCompass > green ? greenCompass : green;
                        red = redCompass > red ? redCompass : red;
                    }

                    if (blue > 255)
                    { blue = 255; }
                    else if (blue < 0)
                    { blue = 0; }

                    if (green > 255)
                    { green = 255; }
                    else if (green < 0)
                    { green = 0; }

                    if (red > 255)
                    { red = 255; }
                    else if (red < 0)
                    { red = 0; }

                    resultBuffer[byteOffset] = (byte)blue;
                    resultBuffer[byteOffset + 1] = (byte)green;
                    resultBuffer[byteOffset + 2] = (byte)red;
                    resultBuffer[byteOffset + 3] = 255;
                }
            }

            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);

            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0,
                                     resultBitmap.Width, resultBitmap.Height),
                                                      ImageLockMode.WriteOnly,
                                                 PixelFormat.Format32bppArgb);

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);

            return resultBitmap;
        }
    }
}
