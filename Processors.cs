using System;
using System.Drawing;
using System.IO;

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
            public double Width => RightBottomX - LeftTopX + 1;
            public double Height => RightBottomY - LeftTopY + 1;
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
                    centroids[id].X += x + 1;
                    centroids[id].Y += y + 1;
                    centroids[id].Count++;

                    boundingBoxes[id].LeftTopX =
                        (boundingBoxes[id].LeftTopX == -1 || x < boundingBoxes[id].LeftTopX) ? x + 1 : boundingBoxes[id].LeftTopX;
                    boundingBoxes[id].LeftTopY =
                        (boundingBoxes[id].LeftTopY == -1 || y < boundingBoxes[id].LeftTopY) ? y + 1 : boundingBoxes[id].LeftTopY;

                    boundingBoxes[id].RightBottomX =
                        (boundingBoxes[id].RightBottomX == -1 || x > boundingBoxes[id].RightBottomX) ? x + 1 : boundingBoxes[id].RightBottomX;
                    boundingBoxes[id].RightBottomY =
                        (boundingBoxes[id].RightBottomY == -1 || y > boundingBoxes[id].RightBottomY) ? y + 1 : boundingBoxes[id].RightBottomY;

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
                writer.WriteLine("ID\tCentroid\tBounding Box\tEquivalent diameter");
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

        #region Kirsch filter

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

        private double[,,] RotateMatrix(double[,] baseKernel, double degrees)
        {
            double[,,] kernel = new double[(int)(360 / degrees), baseKernel.GetLength(0), baseKernel.GetLength(1)];

            int xOffset = baseKernel.GetLength(1) / 2;
            int yOffset = baseKernel.GetLength(0) / 2;

            for (int y = 0; y < baseKernel.GetLength(0); y++)
            {
                for (int x = 0; x < baseKernel.GetLength(1); x++)
                {
                    for (int compass = 0; compass < kernel.GetLength(0); compass++)
                    {
                        double radians = compass * degrees * Math.PI / 180.0;

                        int resultX = (int)(Math.Round((x - xOffset) *
                                   Math.Cos(radians) - (y - yOffset) *
                                   Math.Sin(radians)) + xOffset);

                        int resultY = (int)(Math.Round((x - xOffset) *
                                   Math.Sin(radians) + (y - yOffset) *
                                   Math.Cos(radians)) + yOffset);

                        kernel[compass, resultY, resultX] = baseKernel[y, x];
                    }
                }
            }

            return kernel;
        }

        private Bitmap ConvolutionFilter(Bitmap sourceBitmap, double[,,] filterMatrix)
        {
            var result = new Bitmap(sourceBitmap);
            int offset = filterMatrix.GetLength(1);
            int offsetCenter = (offset - 1) / 2;

            for (int y = 0; y < sourceBitmap.Height; y++)
            {
                for (int x = 0; x < sourceBitmap.Width; x++)
                {
                    double blue = 0;
                    double green = 0;
                    double red = 0;

                    for (int compass = 0; compass < filterMatrix.GetLength(0); compass++)
                    {
                        double blueCompass = .0;
                        double greenCompass = .0;
                        double redCompass = .0;

                        for (int yFilter = -offsetCenter; yFilter <= offsetCenter; yFilter++)
                        {
                            for (int xFilter = -offsetCenter; xFilter <= offsetCenter; xFilter++)
                            {
                                int pixelX = x + xFilter, 
                                    pixelY = y + yFilter;
                                if (pixelX < 0)
                                {
                                    pixelX = sourceBitmap.Width + pixelX;
                                }
                                else if (pixelX >= sourceBitmap.Width)
                                {
                                    pixelX -= sourceBitmap.Width;
                                }

                                if (pixelY < 0)
                                {
                                    pixelY = sourceBitmap.Height + pixelY;
                                }
                                else if (pixelY >= sourceBitmap.Height)
                                {
                                    pixelY -= sourceBitmap.Height;
                                }

                                var color = sourceBitmap.GetPixel(pixelX, pixelY);
                                var filterMultiplication = filterMatrix[compass, yFilter + offsetCenter, xFilter + offsetCenter];
                                blueCompass += color.B * filterMultiplication;
                                greenCompass += color.G * filterMultiplication;
                                redCompass += color.R * filterMultiplication;
                            }
                        }

                        blue = Math.Max(blue, blueCompass);
                        green = Math.Max(green, greenCompass);
                        red = Math.Max(red, redCompass);
                    }

                    blue = Math.Min(Math.Max(0, blue), 255);
                    green = Math.Min(Math.Max(0, green), 255);
                    red = Math.Min(Math.Max(0, red), 255);
                    result.SetPixel(x, y, Color.FromArgb((int)red, (int)green, (int)blue));
                }
            }

            return result;
        }

        #endregion

        #region Open with circle

        public void OpenWithCircle()
        {
            Console.WriteLine("Write output file name: ");
            var path = Console.ReadLine();

            Console.WriteLine("Write radius of structural element (>= 1): ");
            var radius = Convert.ToInt32(Console.ReadLine());

            var seSize = radius * 2 + 1;
            var SE = new int[seSize, seSize];

            var radius2 = radius * radius;
            var center = (seSize - 1) / 2;
            for (int y = 0; y < seSize; y++)
            {
                for (int x = 0; x < seSize; x++)
                {
                    var dist = (x - center) * (x - center) + (y - center) * (y - center);
                    if (dist <= radius2)
                    {
                        SE[y, x] = 1;
                    }
                }
            }

            var resultBitmap = ErodeDilate(inputBitmap, SE, MorphologyType.Erosion);
            resultBitmap = ErodeDilate(resultBitmap, SE, MorphologyType.Dilation);
            resultBitmap.Save(path);
        }

        private Bitmap ErodeDilate(Bitmap sourceBitmap, int[,] SE, MorphologyType method)
        {
            var result = new Bitmap(sourceBitmap);

            int offset = SE.GetLength(0);
            int offsetCenter = (offset - 1) / 2;

            int morphResetValue = (method == MorphologyType.Erosion ? 255 : 0);

            for (int y = 0; y < sourceBitmap.Height; y++)
            {
                for (int x = 0; x < sourceBitmap.Width; x++)
                {
                    var newColor = morphResetValue;

                    for (int ySE = -offsetCenter; ySE <= offsetCenter; ySE++)
                    {
                        for (int xSE = -offsetCenter; xSE <= offsetCenter; xSE++)
                        {
                            if (
                                SE[ySE + offsetCenter, xSE + offsetCenter] != 1 ||
                                x + xSE < 0 || x + xSE >= sourceBitmap.Width ||
                                y + ySE < 0 || y + ySE >= sourceBitmap.Height
                            )
                            {
                                continue;
                            }

                            newColor = method == MorphologyType.Erosion
                                ? Math.Min(newColor, sourceBitmap.GetPixel(x + xSE, y + ySE).R)
                                : Math.Max(newColor, sourceBitmap.GetPixel(x + xSE, y + ySE).R);
                        }
                    }

                    result.SetPixel(x, y, Color.FromArgb(newColor, newColor, newColor));
                }
            }

            return result;
        }

        private enum MorphologyType { Erosion, Dilation }

        #endregion

        #region Labeling

        public void Labeling()
        {
            Console.WriteLine("Write output txt file name: ");
            var path = Console.ReadLine();

            Console.WriteLine("Choose neighbourhood value (4 or 8)?");
            var choice = Convert.ToInt32(Console.ReadLine());
            int[,] directions = choice == 4
                ? new int[,] { { 1, 0, -1, 0 }, { 0, 1, 0, -1 } }
                : new int[,] { { -1, -1, -1, 0, 0, 0, 1, 1, 1 }, { -1, 0, 1, -1, 0, 1, -1, 0, 1 } };


            var labels = new int[inputBitmap.Height, inputBitmap.Width];
            var count = 1;

            for (int y = 0; y < inputBitmap.Height; y++)
            {
                for (int x = 0; x < inputBitmap.Width; x++)
                {
                    if (labels[y, x] == 0 && inputBitmap.GetPixel(x, y).R == 255)
                    {
                        Dfs(x, y, count, directions, labels);
                        count++;
                    }
                }
            }

            using (var writer = File.CreateText(path))
            {
                for (int y = 0; y < inputBitmap.Height; y++)
                {
                    for (int x = 0; x < inputBitmap.Width; x++)
                    {
                        writer.Write(labels[y, x]);
                        writer.Write("\t");
                    }
                    writer.Write("\n");
                }
            }

            
        }

        private void Dfs(int x, int y, int currentLabel, int[,] directions, int[,] labels)
        {
            if (
                x < 0 || x >= inputBitmap.Width ||
                y < 0 || y >= inputBitmap.Height ||
                labels[y, x] != 0 || inputBitmap.GetPixel(x, y).R != 255
            )
            {
                return;
            }

            labels[y, x] = currentLabel;

            for (int direction = 0; direction < directions.GetLength(1); direction++)
            {
                Dfs(x + directions[0, direction], y + directions[1, direction], currentLabel, directions, labels);
            }
        }

        #endregion
    }
}
