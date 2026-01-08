using System;
using System.Drawing;
using NeuralNetwork1;

namespace NeuralNetworkZodiac
{
    public class GenerateImage
    {
        public bool[,] img = new bool[200, 200];
        private Random rand = new Random();

        public ZodiacSign currentFigure = ZodiacSign.Undef;
        public int FigureCount { get; set; } = 12;
        public int FigureCenterGitter { get; set; } = 40;
        public int FigureSizeGitter { get; set; } = 40;
        public int FigureSize { get; set; } = 80;

        public void ClearImage()
        {
            for (int i = 0; i < 200; ++i)
                for (int j = 0; j < 200; ++j)
                    img[i, j] = false;
        }

        public Sample GenerateFigure()
        {
            generate_figure();
            double[] input = new double[400];

            // Улучшенный метод создания вектора - сумма по строкам и столбцам
            for (int i = 0; i < 200; i++)
            {
                int rowSum = 0;
                int colSum = 0;
                for (int j = 0; j < 200; j++)
                {
                    if (img[i, j]) rowSum++;
                    if (img[j, i]) colSum++;
                }
                input[i] = (double)rowSum / 200.0;
                input[200 + i] = (double)colSum / 200.0;
            }

            return new Sample(input, FigureCount, currentFigure);
        }

        private Point GetCenterPoint()
        {
            return new Point(100 + rand.Next(-20, 21), 100 + rand.Next(-20, 21));
        }

        private void DrawLine(int x1, int y1, int x2, int y2, int thickness = 3)
        {
            // Алгоритм Брезенхема с толщиной
            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int sx = (x1 < x2) ? 1 : -1;
            int sy = (y1 < y2) ? 1 : -1;
            int err = dx - dy;

            for (int t = 0; t < thickness; t++)
            {
                int cx1 = x1;
                int cy1 = y1;
                int offsetX = t - thickness / 2;
                int offsetY = t - thickness / 2;

                while (true)
                {
                    int px = cx1 + offsetX;
                    int py = cy1 + offsetY;

                    if (px >= 0 && px < 200 && py >= 0 && py < 200)
                        img[px, py] = true;

                    if (cx1 == x2 && cy1 == y2) break;

                    int e2 = 2 * err;
                    if (e2 > -dy) { err -= dy; cx1 += sx; }
                    if (e2 < dx) { err += dx; cy1 += sy; }
                }
            }
        }

        private void DrawCircle(int cx, int cy, int radius, bool filled = false)
        {
            if (filled)
            {
                for (double angle = 0; angle < 2 * Math.PI; angle += 0.05)
                {
                    for (int r = 0; r <= radius; r++)
                    {
                        int x = (int)(cx + r * Math.Cos(angle));
                        int y = (int)(cy + r * Math.Sin(angle));
                        if (x >= 0 && x < 200 && y >= 0 && y < 200)
                            img[x, y] = true;
                    }
                }
            }
            else
            {
                for (double angle = 0; angle < 2 * Math.PI; angle += 0.02)
                {
                    int x = (int)(cx + radius * Math.Cos(angle));
                    int y = (int)(cy + radius * Math.Sin(angle));
                    if (x >= 0 && x < 200 && y >= 0 && y < 200)
                        img[x, y] = true;
                }
            }
        }

        // Улучшенные символы для знаков зодиака
        public void create_aries() // Овен
        {
            currentFigure = ZodiacSign.Aries;
            Point center = GetCenterPoint();
            // Рога барана
            DrawLine(center.X - 30, center.Y + 20, center.X - 10, center.Y - 20);
            DrawLine(center.X - 10, center.Y - 20, center.X + 10, center.Y);
            DrawLine(center.X + 10, center.Y, center.X + 30, center.Y + 20);
            // Голова
            DrawCircle(center.X, center.Y + 10, 15);
        }

        public void create_taurus() // Телец
        {
            currentFigure = ZodiacSign.Taurus;
            Point center = GetCenterPoint();
            // Голова быка
            DrawCircle(center.X, center.Y, 25);
            // Рога
            DrawLine(center.X - 15, center.Y - 20, center.X - 30, center.Y - 40);
            DrawLine(center.X + 15, center.Y - 20, center.X + 30, center.Y - 40);
        }

        public void create_gemini() // Близнецы
        {
            currentFigure = ZodiacSign.Gemini;
            Point center = GetCenterPoint();
            // Две вертикальные линии
            for (int i = -25; i <= 25; i += 2)
            {
                int y = center.Y + i;
                if (center.X - 20 >= 0 && y >= 0 && y < 200)
                    img[center.X - 20, y] = true;
                if (center.X + 20 >= 0 && y >= 0 && y < 200)
                    img[center.X + 20, y] = true;
            }
            // Соединяющие линии
            DrawLine(center.X - 20, center.Y - 25, center.X + 20, center.Y - 25);
            DrawLine(center.X - 20, center.Y, center.X + 20, center.Y);
            DrawLine(center.X - 20, center.Y + 25, center.X + 20, center.Y + 25);
        }

        public void create_cancer() // Рак
        {
            currentFigure = ZodiacSign.Cancer;
            Point center = GetCenterPoint();
            // Клешни рака
            DrawCircle(center.X - 25, center.Y, 15, true);
            DrawCircle(center.X + 25, center.Y, 15, true);
            // Тело
            DrawCircle(center.X, center.Y, 20, false);
        }

        public void create_leo() // Лев
        {
            currentFigure = ZodiacSign.Leo;
            Point center = GetCenterPoint();
            // Голова льва
            DrawCircle(center.X, center.Y, 25, false);
            // Грива
            for (double angle = 0; angle < 2 * Math.PI; angle += Math.PI / 8)
            {
                int x1 = (int)(center.X + 25 * Math.Cos(angle));
                int y1 = (int)(center.Y + 25 * Math.Sin(angle));
                int x2 = (int)(center.X + 40 * Math.Cos(angle + 0.1));
                int y2 = (int)(center.Y + 40 * Math.Sin(angle + 0.1));
                DrawLine(x1, y1, x2, y2);
            }
        }

        public void create_virgo() // Дева
        {
            currentFigure = ZodiacSign.Virgo;
            Point center = GetCenterPoint();
            // Фигура девы (буква M)
            DrawLine(center.X - 25, center.Y + 25, center.X, center.Y - 25);
            DrawLine(center.X, center.Y - 25, center.X + 25, center.Y + 25);
            // Круг внизу
            DrawCircle(center.X, center.Y + 15, 10, true);
        }

        public void create_libra() // Весы
        {
            currentFigure = ZodiacSign.Libra;
            Point center = GetCenterPoint();
            // Поперечная планка
            DrawLine(center.X - 30, center.Y, center.X + 30, center.Y);
            // Вертикальная стойка
            DrawLine(center.X, center.Y - 30, center.X, center.Y + 30);
            // Чаши весов
            DrawCircle(center.X - 20, center.Y + 20, 12, false);
            DrawCircle(center.X + 20, center.Y + 20, 12, false);
        }

        public void create_scorpio() // Скорпион
        {
            currentFigure = ZodiacSign.Scorpio;
            Point center = GetCenterPoint();
            // Тело скорпиона (изогнутая линия)
            for (int i = -15; i <= 15; i++)
            {
                int x = center.X + i;
                int y = center.Y + (int)(10 * Math.Sin(i * 0.2));
                if (x >= 0 && x < 200 && y >= 0 && y < 200) img[x, y] = true;
            }
            // Хвост
            DrawLine(center.X + 15, center.Y, center.X + 30, center.Y - 15);
            DrawLine(center.X + 30, center.Y - 15, center.X + 40, center.Y);
        }

        public void create_sagittarius() // Стрелец
        {
            currentFigure = ZodiacSign.Sagittarius;
            Point center = GetCenterPoint();
            // Стрела
            DrawLine(center.X - 30, center.Y, center.X + 30, center.Y);
            DrawLine(center.X + 30, center.Y, center.X + 20, center.Y - 10);
            DrawLine(center.X + 30, center.Y, center.X + 20, center.Y + 10);
            // Лук (дуга)
            for (int i = -20; i <= 20; i++)
            {
                int x = center.X - 20 + i;
                int y = center.Y + (int)(Math.Sqrt(400 - i * i) * 0.5);
                if (x >= 0 && x < 200 && y >= 0 && y < 200) img[x, y] = true;
            }
        }

        public void create_capricorn() // Козерог
        {
            currentFigure = ZodiacSign.Capricorn;
            Point center = GetCenterPoint();
            // Рога козерога
            DrawLine(center.X, center.Y - 30, center.X - 20, center.Y);
            DrawLine(center.X, center.Y - 30, center.X + 20, center.Y);
            // Тело
            DrawLine(center.X - 20, center.Y, center.X, center.Y + 30);
            DrawLine(center.X + 20, center.Y, center.X, center.Y + 30);
        }

        public void create_aquarius() // Водолей
        {
            currentFigure = ZodiacSign.Aquarius;
            Point center = GetCenterPoint();
            // Волны воды
            for (int i = -25; i <= 25; i += 5)
            {
                int x = center.X + i;
                int y1 = center.Y - 15 + (int)(10 * Math.Sin(i * 0.3));
                int y2 = center.Y + 15 + (int)(10 * Math.Cos(i * 0.3));

                if (x >= 0 && x < 200 && y1 >= 0 && y1 < 200) img[x, y1] = true;
                if (x >= 0 && x < 200 && y2 >= 0 && y2 < 200) img[x, y2] = true;
            }
        }

        public void create_pisces() // Рыбы
        {
            currentFigure = ZodiacSign.Pisces;
            Point center = GetCenterPoint();
            // Две рыбы
            DrawCircle(center.X - 25, center.Y, 18, false);
            DrawCircle(center.X + 25, center.Y, 18, false);
            // Соединяющая линия
            DrawLine(center.X - 7, center.Y, center.X + 7, center.Y);
            // Хвосты
            DrawLine(center.X - 43, center.Y, center.X - 32, center.Y - 10);
            DrawLine(center.X - 43, center.Y, center.X - 32, center.Y + 10);
            DrawLine(center.X + 43, center.Y, center.X + 32, center.Y - 10);
            DrawLine(center.X + 43, center.Y, center.X + 32, center.Y + 10);
        }

        public void generate_figure(ZodiacSign type = ZodiacSign.Undef)
        {
            ClearImage();

            if (type == ZodiacSign.Undef || (int)type >= FigureCount)
                type = (ZodiacSign)rand.Next(FigureCount);

            switch (type)
            {
                case ZodiacSign.Aries: create_aries(); break;
                case ZodiacSign.Taurus: create_taurus(); break;
                case ZodiacSign.Gemini: create_gemini(); break;
                case ZodiacSign.Cancer: create_cancer(); break;
                case ZodiacSign.Leo: create_leo(); break;
                case ZodiacSign.Virgo: create_virgo(); break;
                case ZodiacSign.Libra: create_libra(); break;
                case ZodiacSign.Scorpio: create_scorpio(); break;
                case ZodiacSign.Sagittarius: create_sagittarius(); break;
                case ZodiacSign.Capricorn: create_capricorn(); break;
                case ZodiacSign.Aquarius: create_aquarius(); break;
                case ZodiacSign.Pisces: create_pisces(); break;
                default: break;
            }

            // Добавляем случайные искажения для аугментации
            if (rand.NextDouble() < 0.4)
            {
                // Случайные точки шума
                int noiseCount = rand.Next(5, 20);
                for (int i = 0; i < noiseCount; i++)
                {
                    int x = rand.Next(200);
                    int y = rand.Next(200);
                    img[x, y] = !img[x, y];
                }
            }

            // Случайное изменение толщины линий
            if (rand.NextDouble() < 0.3)
            {
                // Легкое размытие/утолщение
                bool[,] thickened = new bool[200, 200];
                for (int x = 1; x < 199; x++)
                {
                    for (int y = 1; y < 199; y++)
                    {
                        if (img[x, y])
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                for (int dy = -1; dy <= 1; dy++)
                                {
                                    if (rand.NextDouble() < 0.7)
                                    {
                                        thickened[x + dx, y + dy] = true;
                                    }
                                }
                            }
                        }
                    }
                }
                img = thickened;
            }
        }

        public Bitmap GenBitmap()
        {
            Bitmap drawArea = new Bitmap(200, 200);
            for (int i = 0; i < 200; ++i)
                for (int j = 0; j < 200; ++j)
                    if (img[i, j])
                        drawArea.SetPixel(i, j, Color.Black);
                    else
                        drawArea.SetPixel(i, j, Color.White);

            return drawArea;
        }
    }
}