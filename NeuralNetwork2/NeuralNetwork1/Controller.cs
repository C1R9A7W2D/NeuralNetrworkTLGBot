using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using AForge.Imaging;
using AForge.Imaging.Filters;
using NeuralNetwork1;

namespace NeuralNetworkZodiac
{
    public delegate void FormUpdateDelegate();

    public class Controller
    {
        public FormUpdateDelegate updateDelegate;
        public Settings settings = new Settings();
        public bool Ready { get; set; } = true;

        private Bitmap originalImage;
        private Bitmap processedImage;
        private Bitmap displayImage;
        private Stopwatch stopwatch = new Stopwatch();
        private int frameCounter = 0;
        private const int PROCESS_EVERY_N_FRAME = 2;
        private bool isDisposed = false;

        public Controller(FormUpdateDelegate updateDelegate)
        {
            this.updateDelegate = updateDelegate;
        }

        public void ProcessImage(Bitmap bitmap)
        {
            if (!Ready || isDisposed) return;

            try
            {
                frameCounter++;
                if (frameCounter % PROCESS_EVERY_N_FRAME != 0)
                {
                    Ready = true;
                    return;
                }

                Ready = false;
                stopwatch.Restart();

                if (bitmap == null)
                {
                    Ready = true;
                    return;
                }

                // Используем фиксированный размер для обработки
                int targetWidth = 640;
                int targetHeight = 480;

                Bitmap scaledBitmap = null;
                Bitmap croppedBitmap = null;
                Bitmap tempDisplayImage = null;

                try
                {
                    // Масштабирование изображения
                    float scaleX = (float)targetWidth / bitmap.Width;
                    float scaleY = (float)targetHeight / bitmap.Height;
                    float scale = Math.Min(scaleX, scaleY);

                    if (scale < 1.0f)
                    {
                        int newWidth = (int)(bitmap.Width * scale);
                        int newHeight = (int)(bitmap.Height * scale);
                        scaledBitmap = new Bitmap(newWidth, newHeight);
                        using (Graphics g = Graphics.FromImage(scaledBitmap))
                        {
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
                            g.DrawImage(bitmap, 0, 0, newWidth, newHeight);
                        }
                    }
                    else
                    {
                        scaledBitmap = new Bitmap(bitmap);
                    }

                    int width = scaledBitmap.Width;
                    int height = scaledBitmap.Height;
                    int minSide = Math.Min(width, height);

                    if (minSide < 100)
                    {
                        Ready = true;
                        return;
                    }

                    // Вычисляем область обрезки с защитой от выхода за границы
                    int border = Math.Max(1, Math.Min(50, settings.border));
                    int side = Math.Max(100, minSide - 2 * border);

                    // Ограничиваем перемещение
                    int maxOffset = Math.Max(0, (minSide - side) / 2);
                    settings.left = Math.Max(-maxOffset, Math.Min(maxOffset, settings.left));
                    settings.top = Math.Max(-maxOffset, Math.Min(maxOffset, settings.top));

                    // Вычисляем координаты обрезки
                    int centerX = width / 2;
                    int centerY = height / 2;
                    int startX = centerX - side / 2 + settings.left;
                    int startY = centerY - side / 2 + settings.top;

                    // Обеспечиваем, чтобы область обрезки была внутри изображения
                    startX = Math.Max(0, Math.Min(width - side, startX));
                    startY = Math.Max(0, Math.Min(height - side, startY));

                    // Проверяем корректность координат
                    if (side <= 0 || startX < 0 || startY < 0 || startX + side > width || startY + side > height)
                    {
                        Ready = true;
                        return;
                    }

                    // Создаем обрезанное изображение
                    Rectangle cropRect = new Rectangle(startX, startY, side, side);
                    croppedBitmap = new Bitmap(side, side);
                    using (Graphics g = Graphics.FromImage(croppedBitmap))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(scaledBitmap, 0, 0, cropRect, GraphicsUnit.Pixel);
                    }

                    // Обновляем оригинальное изображение
                    if (originalImage != null)
                    {
                        originalImage.Dispose();
                    }
                    originalImage = new Bitmap(croppedBitmap);

                    // Создаем черно-белое изображение для отображения
                    tempDisplayImage = CreateDisplayImageSafe(croppedBitmap);

                    // Обновляем displayImage с блокировкой
                    lock (this)
                    {
                        if (displayImage != null)
                        {
                            displayImage.Dispose();
                        }
                        displayImage = tempDisplayImage;
                        tempDisplayImage = null; // Предотвращаем двойное освобождение
                    }

                    stopwatch.Stop();
                    updateDelegate?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка обработки изображения: {ex.Message}");
                }
                finally
                {
                    // Освобождаем временные ресурсы
                    scaledBitmap?.Dispose();
                    croppedBitmap?.Dispose();
                    tempDisplayImage?.Dispose();
                    Ready = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка в ProcessImage: {ex.Message}");
                Ready = true;
            }
        }

        private Bitmap CreateDisplayImageSafe(Bitmap source)
        {
            if (source == null) return new Bitmap(200, 200);

            Bitmap result = new Bitmap(source.Width, source.Height);
            byte threshold = settings.threshold;

            // Используем GetPixel/SetPixel вместо unsafe кода
            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    Color pixel = source.GetPixel(x, y);

                    // Конвертируем в градации серого
                    int gray = (int)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114);

                    // Применяем порог
                    Color newColor = gray < threshold ? Color.Black : Color.White;
                    result.SetPixel(x, y, newColor);
                }
            }

            // Рисуем сетку поверх черно-белого изображения
            DrawGridOnDisplayImageSafe(result);
            return result;
        }

        private void DrawGridOnDisplayImageSafe(Bitmap image)
        {
            if (image == null) return;

            using (Graphics g = Graphics.FromImage(image))
            {
                // Рисуем сетку
                using (Pen gridPen = new Pen(Color.FromArgb(200, 255, 0, 0), 1))
                {
                    int blockWidth = image.Width / settings.blocksCount;
                    int blockHeight = image.Height / settings.blocksCount;

                    for (int r = 0; r <= settings.blocksCount; r++)
                    {
                        for (int c = 0; c <= settings.blocksCount; c++)
                        {
                            g.DrawRectangle(gridPen, c * blockWidth, r * blockHeight, blockWidth, blockHeight);
                        }
                    }
                }

                // Рисуем красную рамку
                using (Pen borderPen = new Pen(Color.Red, 2))
                {
                    g.DrawRectangle(borderPen, 0, 0, image.Width - 1, image.Height - 1);
                }

                // Отображаем информацию
                using (Font font = new Font("Arial", 10))
                {
                    string info = $"Порог: {settings.threshold}";
                    g.DrawString(info, font, Brushes.Red, 5, 5);
                }
            }
        }

        public Bitmap ProcessForNeuralNetwork()
        {
            if (originalImage == null || isDisposed)
            {
                return CreateEmptyImage(200, 200);
            }

            try
            {
                Bitmap result = new Bitmap(200, 200);

                using (Graphics g = Graphics.FromImage(result))
                {
                    g.Clear(Color.White);

                    if (originalImage != null)
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(originalImage, 0, 0, 200, 200);
                    }
                }

                // Применяем порог
                ApplyThresholdSafe(result, settings.threshold);
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка обработки для нейросети: {ex.Message}");
                return CreateEmptyImage(200, 200);
            }
        }

        private void ApplyThresholdSafe(Bitmap image, byte threshold)
        {
            if (image == null) return;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    int gray = (int)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114);
                    Color newColor = gray < threshold ? Color.Black : Color.White;
                    image.SetPixel(x, y, newColor);
                }
            }
        }

        public Bitmap GetOriginalImage()
        {
            lock (this)
            {
                if (displayImage == null || isDisposed)
                {
                    return CreateInfoImage(400, 300, "Ожидание изображения...", settings.threshold);
                }

                // Масштабируем для отображения
                Bitmap scaled = new Bitmap(400, 300);
                using (Graphics g = Graphics.FromImage(scaled))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g.DrawImage(displayImage, 0, 0, 400, 300);
                }
                return scaled;
            }
        }

        public Bitmap GetProcessedImage()
        {
            if (processedImage == null && !isDisposed)
            {
                processedImage = ProcessForNeuralNetwork();
            }
            return processedImage;
        }

        public void UpdateProcessedImage()
        {
            if (isDisposed) return;

            lock (this)
            {
                if (processedImage != null)
                {
                    processedImage.Dispose();
                    processedImage = null;
                }
                processedImage = ProcessForNeuralNetwork();
            }
        }

        public double[] ConvertProcessedImageToVector()
        {
            if (processedImage == null || isDisposed)
                return new double[400];

            return ConvertImageToVectorSafe(processedImage);
        }

        private double[] ConvertImageToVectorSafe(Bitmap image)
        {
            double[] vector = new double[400];

            for (int i = 0; i < 200; i++)
            {
                int rowSum = 0;
                int colSum = 0;

                for (int j = 0; j < 200; j++)
                {
                    Color pixelRow = image.GetPixel(i, j);
                    Color pixelCol = image.GetPixel(j, i);

                    if (pixelRow.R < 128) rowSum++;
                    if (pixelCol.R < 128) colSum++;
                }

                vector[i] = (double)rowSum / 200.0;
                vector[200 + i] = (double)colSum / 200.0;
            }

            return vector;
        }

        private Bitmap CreateEmptyImage(int width, int height)
        {
            Bitmap empty = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(empty))
            {
                g.Clear(Color.White);
            }
            return empty;
        }

        private Bitmap CreateInfoImage(int width, int height, string message, byte threshold)
        {
            Bitmap info = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(info))
            {
                g.Clear(Color.White);
                using (Pen borderPen = new Pen(Color.Red, 2))
                {
                    g.DrawRectangle(borderPen, 0, 0, width - 1, height - 1);
                }
                using (Font font = new Font("Arial", 14))
                {
                    g.DrawString(message, font, Brushes.Red, width / 2 - 70, height / 2 - 30);
                    g.DrawString($"Порог: {threshold}", font, Brushes.Blue, width / 2 - 50, height / 2);
                }
            }
            return info;
        }

        public long GetProcessingTime() => stopwatch.ElapsedMilliseconds;

        public void Dispose()
        {
            isDisposed = true;

            lock (this)
            {
                originalImage?.Dispose();
                processedImage?.Dispose();
                displayImage?.Dispose();

                originalImage = null;
                processedImage = null;
                displayImage = null;
            }
        }
    }

    public class Settings
    {
        private int _border = 10;
        public int border
        {
            get => _border;
            set
            {
                _border = Math.Max(1, Math.Min(50, value));
                // Корректируем top и left если нужно
                top = Math.Max(-2 * _border, Math.Min(2 * _border, top));
                left = Math.Max(-2 * _border, Math.Min(2 * _border, left));
            }
        }

        public int blocksCount = 8;
        public Size processedDesiredSize = new Size(200, 200);

        private int _top = 0;
        public int top
        {
            get => _top;
            set
            {
                _top = Math.Max(-2 * border, Math.Min(2 * border, value));
            }
        }

        private int _left = 0;
        public int left
        {
            get => _left;
            set
            {
                _left = Math.Max(-2 * border, Math.Min(2 * border, value));
            }
        }

        public bool processImg = true;
        public byte threshold = 128;
        public float differenceLim = 0.1f;

        public void incTop() { top++; }
        public void decTop() { top--; }
        public void incLeft() { left++; }
        public void decLeft() { left--; }
    }
}