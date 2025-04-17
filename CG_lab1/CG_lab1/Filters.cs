using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace CG_lab1
{
    public abstract class Filters
    {
        public abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);

        public int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                value = min;
            }
            if (value > max)
            {
                value = max;
            }
            return value;
        }

        public Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }

            return resultImage;
        }



    }

    public class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusX; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            return Color.FromArgb(
            Clamp((int)resultR, 0, 255),
            Clamp((int)resultG, 0, 255),
            Clamp((int)resultB, 0, 255)
            );
        }
    }

    class InvertFilter : Filters
    {
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);
            return resultColor;
        }

    }

    class GrayScaleFilter : Filters
    {
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int grayValue = (int)(0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B);
            grayValue = Clamp(grayValue, 0, 255);
            return Color.FromArgb(grayValue, grayValue, grayValue);
        }
    }

    class SepiaFilter : Filters
    {
        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int grayValue = (int)(0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B);
            grayValue = Clamp(grayValue, 0, 255);
            float k = (float)(0.6);
            int r = (int)(2 * k * sourceColor.R + grayValue);
            int g = (int)(0.5 * k * sourceColor.G + grayValue);
            int b = (int)(grayValue - (k * sourceColor.B));

            r = Clamp(r, 0, 255);
            g = Clamp(g, 0, 255);
            b = Clamp(b, 0, 255);

            return Color.FromArgb(r, g, b);
        }
    }

    class Brightness : Filters
    {
        public override Color calculateNewPixelColor(Bitmap sourceimage, int i, int j)
        {
            Color sourcecolor = sourceimage.GetPixel(i, j);
            double R = sourcecolor.R;
            double G = sourcecolor.G;
            double B = sourcecolor.B;
            int k = 60;
            Color resultcolor = Color.FromArgb(Clamp((int)(R + k), 0, 255),
                                               Clamp((int)(G + k), 0, 255),
                                               Clamp((int)(G + k), 0, 255));
            return resultcolor;
        }
    }


    class BlurFilter : MatrixFilter
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
                }
            }
        }
    }

    class GaussianFilter : MatrixFilter
    {
        public void createGaussianKernel(int radius, float sigma)
        {
            int size = 2 * radius + 1;
            kernel = new float[size, size];
            float norm = 0;

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            }

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    kernel[i, j] /= norm;
                }
            }
        }

        public GaussianFilter()
        {
            createGaussianKernel(3, 2);
        }
    }

    class Sobel : MatrixFilter
    {
        public Sobel()
        {
            int sizex = 3;
            int sizey = 3;
            kernel = new float[sizex, sizey];
            kernel[0, 0] = -1;
            kernel[0, 1] = -2;
            kernel[0, 2] = -1;
            kernel[1, 0] = 0;
            kernel[1, 1] = 0;
            kernel[1, 2] = 0;
            kernel[2, 0] = 1;
            kernel[2, 1] = 2;
            kernel[2, 2] = 1;
        }
    }

    class Sharpness : MatrixFilter
    {
        public Sharpness()
        {
            int sizex = 3;
            int sizey = 3;
            kernel = new float[sizex, sizey];
            kernel[0, 0] = 0;
            kernel[0, 1] = -1;
            kernel[0, 2] = 0;
            kernel[1, 0] = -1;
            kernel[1, 1] = 5;
            kernel[1, 2] = -1;
            kernel[2, 0] = 0;
            kernel[2, 1] = -1;
            kernel[2, 2] = 0;
        }
    }

    class embossing : MatrixFilter
    {
        public embossing()
        {
            int sizex = 3;
            int sizey = 3;
            kernel = new float[sizex, sizey];
            kernel[0, 0] = 0;
            kernel[0, 1] = 1;
            kernel[0, 2] = 0;
            kernel[1, 0] = 1;
            kernel[1, 1] = 0;
            kernel[1, 2] = -1;
            kernel[2, 0] = 0;
            kernel[2, 1] = -1;
            kernel[2, 2] = 0;
        }
        protected Color grayscale(Bitmap sourceimage, int i, int j)
        {
            Color sourcecolor = sourceimage.GetPixel(i, j);
            double R = sourcecolor.R;
            double G = sourcecolor.G;
            double B = sourcecolor.B;
            double Intensity = 0.299 * R + 0.587 * G + 0.114 * B;
            int k = 5;
            Color resultcolor = Color.FromArgb(Clamp((int)(Intensity + 2 * k), -255, 255),
                                               Clamp((int)(Intensity + 0.5 * k), -255, 255),
                                               Clamp((int)(Intensity - k), -255, 255));
            return resultcolor;
        }
        protected Bitmap processimage(Bitmap sourceimage)
        {
            Bitmap resultimage = new Bitmap(sourceimage.Width, sourceimage.Height);
            for (int i = 0; i < sourceimage.Width; i++)
            {
                for (int j = 0; j < sourceimage.Height; j++)
                {
                    resultimage.SetPixel(i, j, grayscale(sourceimage, i, j));
                }
            }
            return resultimage;
        }
        public override Color calculateNewPixelColor(Bitmap resultimage, int i, int j)
        {
            Bitmap sourceimage;
            sourceimage = resultimage;
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idx = Clamp(i + k, 0, sourceimage.Width - 1);
                    int idy = Clamp(j + l, 0, sourceimage.Height - 1);
                    Color neighborcolor = sourceimage.GetPixel(idx, idy);
                    resultR += neighborcolor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborcolor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborcolor.B * kernel[k + radiusX, l + radiusY];
                }
            resultR = (resultR + 255) / 2;
            resultG = (resultG + 255) / 2;
            resultB = (resultB + 255) / 2;
            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );
        }
    }

    class Reflection : Filters
    {
        protected byte[] FindMaxValue(Bitmap sourceimage)
        {
            byte maxR = sourceimage.GetPixel(0, 0).R, maxG = sourceimage.GetPixel(0, 0).G, maxB = sourceimage.GetPixel(0, 0).B;
            for (int i = 0; i < sourceimage.Width; i++)
                for (int j = 0; j < sourceimage.Height; j++)
                {
                    if ((float)sourceimage.GetPixel(i, j).R > (float)maxR)
                        maxR = sourceimage.GetPixel(i, j).R;
                    if ((float)sourceimage.GetPixel(i, j).G > (float)maxG)
                        maxG = sourceimage.GetPixel(i, j).G;
                    if ((float)sourceimage.GetPixel(i, j).B > (float)maxB)
                        maxB = sourceimage.GetPixel(i, j).B;
                }
            byte[] result = new byte[3];
            result[0] = maxR;
            result[1] = maxG;
            result[2] = maxB;
            return result;
        }
        public override Color calculateNewPixelColor(Bitmap sourceimage, int i, int j)
        {
            Color sourcecolor = sourceimage.GetPixel(i, j);
            byte[] colors = new byte[3];
            colors = FindMaxValue(sourceimage);
            double R = sourcecolor.R;
            double G = sourcecolor.G;
            double B = sourcecolor.B;
            Color resultcolor = Color.FromArgb(Clamp((int)(R * 255 / colors[0]), 0, 255),
                                               Clamp((int)(G * 255 / colors[1]), 0, 255),
                                               Clamp((int)(B * 255 / colors[2]), 0, 255));
            return resultcolor;
        }
    }

    class GlassEffect : Filters
    {
        Random rnd = new Random();
        public override Color calculateNewPixelColor(Bitmap sourceimage, int i, int j)
        {
            
            double k = rnd.NextDouble();
            int newcoloronx = (int)(i + (k - 0.5) * 10);
            int newcolorony = (int)(j + (k - 0.5) * 10);
            Color resultcolor = sourceimage.GetPixel(Clamp(newcoloronx, 0, sourceimage.Width - 1), Clamp(newcolorony, 0, sourceimage.Height - 1));
            //Color resultcolor = sourceimage.GetPixel(newcoloronx, newcolorony);
            return resultcolor;
        }
    }



    public class DilationFilter : Filters
    {
        private int[,] structuringElement;
        private int elementSize;

        public DilationFilter(int size = 3)
        {
            elementSize = size;
            structuringElement = new int[size, size];

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    structuringElement[i, j] = 1;
        }

        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int max = 0;
            int offset = elementSize / 2;

            for (int i = 0; i < elementSize; i++)
            {
                for (int j = 0; j < elementSize; j++)
                {
                    int pixelX = x + i - offset;
                    int pixelY = y + j - offset;

                    if (pixelX >= 0 && pixelX < sourceImage.Width && pixelY >= 0 && pixelY < sourceImage.Height)
                    {
                        Color pixelColor = sourceImage.GetPixel(pixelX, pixelY);
                        int intensity = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;

                        if (structuringElement[i, j] == 1)
                        {
                            max = Math.Max(max, intensity);
                        }
                    }
                }
            }

            return Color.FromArgb(max, max, max);
        }

        public Bitmap ApplyDilation(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            int offset = elementSize / 2;

            for (int x = offset; x < sourceImage.Width - offset; x++)
            {
                for (int y = offset; y < sourceImage.Height - offset; y++)
                {
                    // Проверяем, была ли запрошена отмена
                    if (worker.CancellationPending)
                    {
                        return null; // Возвращаем null, если отмена была запрошена
                    }

                    Color newColor = calculateNewPixelColor(sourceImage, x, y);
                    resultImage.SetPixel(x, y, newColor);
                }

                // Обновляем прогресс
                int progressPercentage = (int)((float)(x - offset) / (sourceImage.Width - 2 * offset) * 100);
                worker.ReportProgress(progressPercentage);
            }

            return resultImage;
        }
    }

    public class ErosionFilter : Filters
    {
        private int[,] structuringElement;
        private int elementSize;

        public ErosionFilter(int size = 3)
        {
            elementSize = size;
            structuringElement = new int[size, size];

            // Инициализация структурного элемента
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    structuringElement[i, j] = 1;
        }

        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int min = 255; // Инициализируем минимальное значение максимальным
            int offset = elementSize / 2;

            // Проходим по структурному элементу
            for (int i = 0; i < elementSize; i++)
            {
                for (int j = 0; j < elementSize; j++)
                {
                    int pixelX = x + i - offset;
                    int pixelY = y + j - offset;

                    // Проверка границ
                    if (pixelX >= 0 && pixelX < sourceImage.Width && pixelY >= 0 && pixelY < sourceImage.Height)
                    {
                        Color pixelColor = sourceImage.GetPixel(pixelX, pixelY);
                        int intensity = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;

                        // Обновление минимальной интенсивности
                        if (structuringElement[i, j] == 1)
                        {
                            min = Math.Min(min, intensity);
                        }
                    }
                }
            }

            return Color.FromArgb(min, min, min); // Возвращаем новый цвет пикселя
        }

        public Bitmap ApplyErosion(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            int offset = elementSize / 2;

            for (int x = offset; x < sourceImage.Width - offset; x++)
            {
                for (int y = offset; y < sourceImage.Height - offset; y++)
                {
                    // Проверяем, была ли запрошена отмена
                    if (worker.CancellationPending)
                    {
                        return null; // Возвращаем null, если отмена была запрошена
                    }

                    // Получаем новый цвет пикселя
                    Color newColor = calculateNewPixelColor(sourceImage, x, y);
                    resultImage.SetPixel(x, y, newColor);
                }

                // Обновляем прогресс
                int progressPercentage = (int)((float)(x - offset) / (sourceImage.Width - 2 * offset) * 100);
                worker.ReportProgress(progressPercentage);
            }

            return resultImage;
        }
    }

    public class OpeningFilter : Filters
    {
        private ErosionFilter erosion;
        private DilationFilter dilation;

        public OpeningFilter(int size = 3)
        {
            erosion = new ErosionFilter(size);
            dilation = new DilationFilter(size);
        }

        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            return sourceImage.GetPixel(x, y); // Возвращаем оригинальный цвет, так как обработка происходит в методах ApplyOpening
        }

        public Bitmap ApplyOpening(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap erodedImage = erosion.ApplyErosion(sourceImage, worker);
            if (erodedImage == null) return null; // Проверка на отмену

            Bitmap resultImage = dilation.ApplyDilation(erodedImage, worker);
            if (resultImage == null) return null; // Проверка на отмену

            return resultImage; // Возвращаем результирующее изображение
        }
    }
    public class ClosingFilter : Filters
    {
        private DilationFilter dilation;
        private ErosionFilter erosion;

        public ClosingFilter(int size = 3)
        {
            dilation = new DilationFilter(size);
            erosion = new ErosionFilter(size);
        }

        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            return sourceImage.GetPixel(x, y); // Этот метод не используется в процессе закрытия
        }

        public Bitmap ApplyClosing(Bitmap sourceImage, BackgroundWorker worker)
        {
            // Применяем дилатацию к исходному изображению
            Bitmap dilatedImage = dilation.ApplyDilation(sourceImage, worker);
            if (dilatedImage == null) return null; // Проверка на отмену

            // Применяем эрозию к результату дилатации
            Bitmap resultImage = erosion.ApplyErosion(dilatedImage, worker);
            if (resultImage == null) return null; // Проверка на отмену

            return resultImage; // Возвращаем результирующее изображение
        }
    }

    public class Gradient
    {
        private DilationFilter dilation;
        private ErosionFilter erosion;

        public Gradient(int size = 3)
        {
            dilation = new DilationFilter(size);
            erosion = new ErosionFilter(size);
        }

        public Bitmap ApplyGradient(Bitmap sourceImage, BackgroundWorker worker)
        {
            
            Bitmap dilated = dilation.ApplyDilation(sourceImage, worker);
            if (dilated == null) return null;

            Bitmap eroded = erosion.ApplyErosion(sourceImage, worker);
            if (eroded == null) return null;

            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            for (int x = 0; x < sourceImage.Width; x++)
            {
                for (int y = 0; y < sourceImage.Height; y++)
                {
                    if (worker.CancellationPending)
                    {
                        return null;
                    }

                    Color colorD = dilated.GetPixel(x, y);
                    Color colorE = eroded.GetPixel(x, y);

                    int r = Math.Max(0, colorD.R - colorE.R);
                    int g = Math.Max(0, colorD.G - colorE.G);
                    int b = Math.Max(0, colorD.B - colorE.B);

                    resultImage.SetPixel(x, y, Color.FromArgb(r, g, b));
                }

                worker.ReportProgress((int)((float)x / sourceImage.Width * 100));
            }

            return resultImage;
        }
    }

    public class MedianFilter : MatrixFilter
    {
        private int kernelSize;

        public MedianFilter(int size = 3) : base(CreateKernel(size))
        {
            kernelSize = size;
        }

        private static float[,] CreateKernel(int size)
        {
            // Создаем единичный (или другой) ядро для фильтрации
            float[,] kernel = new float[size, size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] = 1; // Можно настроить ядро по вашему усмотрению
            return kernel;
        }

        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radius = kernelSize / 2;
            List<int> R = new List<int>();
            List<int> G = new List<int>();
            List<int> B = new List<int>();

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    int idX = Clamp(x + i, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + j, 0, sourceImage.Height - 1);
                    Color pixel = sourceImage.GetPixel(idX, idY);
                    R.Add(pixel.R);
                    G.Add(pixel.G);
                    B.Add(pixel.B);
                }
            }

            R.Sort();
            G.Sort();
            B.Sort();
            int medianIndex = R.Count / 2;

            return Color.FromArgb(
                Clamp(R[medianIndex], 0, 255),
                Clamp(G[medianIndex], 0, 255),
                Clamp(B[medianIndex], 0, 255)
            );
        }

        public Bitmap ApplyMedianFilter(Bitmap sourceImage, BackgroundWorker worker)
        {
            return processImage(sourceImage, worker);
        }
    }

    class Reflector : Filters
    {
        protected int[] FindMaxValue(Bitmap sourceimage)
        {
            int maxR = sourceimage.GetPixel(0, 0).R, maxG = sourceimage.GetPixel(0, 0).G, maxB = sourceimage.GetPixel(0, 0).B;
            int[] result = new int[3];
            result[0] = maxR;
            result[1] = maxG;
            result[2] = maxB;
            return result;
        }
        public override Color calculateNewPixelColor(Bitmap sourceimage, int i, int j)
        {
            Color sourcecolor = sourceimage.GetPixel(i, j);
            int[] colors = new int[3];
            colors = FindMaxValue(sourceimage);
            double R = sourcecolor.R;
            double G = sourcecolor.G;
            double B = sourcecolor.B;
            Color resultcolor = Color.FromArgb(
                                               Clamp((int)(R * 255 / Math.Max((int)colors[0], 1)), 0, 255),
                                               Clamp((int)(G * 255 / Math.Max((int)colors[1], 1)), 0, 255),
                                               Clamp((int)(B * 255 / Math.Max((int)colors[2], 1)), 0, 255));
            return resultcolor;
        }

    }

    public class LinearStretching : Filters
    {
        private double meanR, meanG, meanB;
        private double stdDeviationR, stdDeviationG, stdDeviationB;

        public void CalculateMeanAndStdDev(Bitmap sourceImage)
        {
            long sumR = 0, sumG = 0, sumB = 0;
            int totalPixels = sourceImage.Width * sourceImage.Height;

            // средние значения
            for (int x = 0; x < sourceImage.Width; x++)
            {
                for (int y = 0; y < sourceImage.Height; y++)
                {
                    Color pixelColor = sourceImage.GetPixel(x, y);
                    sumR += pixelColor.R;
                    sumG += pixelColor.G;
                    sumB += pixelColor.B;
                }
            }

            meanR = sumR / (double)totalPixels;
            meanG = sumG / (double)totalPixels;
            meanB = sumB / (double)totalPixels;

            // стандартное отклонение
            double sumSquaredDiffR = 0, sumSquaredDiffG = 0, sumSquaredDiffB = 0;

            for (int x = 0; x < sourceImage.Width; x++)
            {
                for (int y = 0; y < sourceImage.Height; y++)
                {
                    Color pixelColor = sourceImage.GetPixel(x, y);
                    sumSquaredDiffR += Math.Pow(pixelColor.R - meanR, 2);
                    sumSquaredDiffG += Math.Pow(pixelColor.G - meanG, 2);
                    sumSquaredDiffB += Math.Pow(pixelColor.B - meanB, 2);
                }
            }

            stdDeviationR = Math.Sqrt(sumSquaredDiffR / totalPixels);
            stdDeviationG = Math.Sqrt(sumSquaredDiffG / totalPixels);
            stdDeviationB = Math.Sqrt(sumSquaredDiffB / totalPixels);
        }

        public override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            // Получаем цвет исходного пикселя
            Color sourceColor = sourceImage.GetPixel(x, y);

            // Применяем нормальное отклонение для растяжения
            int newR = Clamp((int)((sourceColor.R - meanR) / stdDeviationR * 128 + 128), 0, 255);
            int newG = Clamp((int)((sourceColor.G - meanG) / stdDeviationG * 128 + 128), 0, 255);
            int newB = Clamp((int)((sourceColor.B - meanB) / stdDeviationB * 128 + 128), 0, 255);

            return Color.FromArgb(newR, newG, newB);
        }

        public Bitmap ApplyLinearStretching(Bitmap sourceImage) //, BackgroundWorker worker)
        {
            // Находим средние значения и стандартные отклонения
            CalculateMeanAndStdDev(sourceImage);

            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            for (int x = 0; x < sourceImage.Width; x++)
            {
                for (int y = 0; y < sourceImage.Height; y++)
                {
                    //if (worker.CancellationPending)
                    //{
                    //    return null; // Возвращаем null, если отмена была запрошена
                    //}

                    Color newColor = calculateNewPixelColor(sourceImage, x, y);
                    resultImage.SetPixel(x, y, newColor);
                }

                int progressPercentage = (int)((float)x / sourceImage.Width * 100);
                //worker.ReportProgress(progressPercentage);
            }
            //worker.CancelAsync();

            return resultImage;
        }
    }


}

