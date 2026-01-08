using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NeuralNetwork1;

namespace NeuralNetworkZodiac
{
    public class CameraAndDrawForm : Form
    {
        private PictureBox pictureBox;
        private Bitmap drawBitmap;
        private Graphics drawGraphics;
        private Point lastPoint;
        private bool isDrawing = false;
        private Button captureBtn;
        private Button clearBtn;
        private Button recognizeBtn;
        private ComboBox zodiacCombo;
        private PictureBox processedBox;
        private Label resultLabel;
        private Button saveImageBtn;
        private Pen drawingPen;
        private Label instructionLabel;
        private Button processImageBtn;
        private CheckBox gridCheckBox;
        private TrackBar brushSizeTrackBar;
        private Label brushSizeLabel;

        public event Action<Bitmap, ZodiacSign> OnImageCaptured;
        public event Action<Bitmap> OnImageForRecognition;

        public CameraAndDrawForm()
        {
            InitializeComponent();
            drawingPen = new Pen(Color.Black, 3);
            drawingPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            drawingPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
        }

        private void InitializeComponent()
        {
            this.Text = "Рисование знаков зодиака";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9);

            // Левая панель - рисование
            Panel drawPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(500, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke
            };

            pictureBox = new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(480, 480),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Cursor = Cursors.Cross
            };
            pictureBox.MouseDown += PictureBox_MouseDown;
            pictureBox.MouseMove += PictureBox_MouseMove;
            pictureBox.MouseUp += PictureBox_MouseUp;

            // Настройки рисования
            Panel drawSettingsPanel = new Panel
            {
                Location = new Point(10, 495),
                Size = new Size(480, 40),
                BorderStyle = BorderStyle.None
            };

            brushSizeLabel = new Label
            {
                Text = "Размер кисти:",
                Location = new Point(5, 10),
                Size = new Size(80, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            brushSizeTrackBar = new TrackBar
            {
                Location = new Point(90, 10),
                Size = new Size(150, 25),
                Minimum = 1,
                Maximum = 20,
                Value = 3,
                TickFrequency = 2
            };
            brushSizeTrackBar.ValueChanged += BrushSizeTrackBar_ValueChanged;

            gridCheckBox = new CheckBox
            {
                Text = "Показать сетку",
                Location = new Point(250, 10),
                Size = new Size(120, 25),
                Checked = false
            };
            gridCheckBox.CheckedChanged += GridCheckBox_CheckedChanged;

            drawSettingsPanel.Controls.AddRange(new Control[] { brushSizeLabel, brushSizeTrackBar, gridCheckBox });

            drawPanel.Controls.AddRange(new Control[] { pictureBox, drawSettingsPanel });

            // Правая панель - управление
            Panel controlPanel = new Panel
            {
                Location = new Point(520, 10),
                Size = new Size(460, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.AliceBlue
            };

            // Обработанное изображение
            processedBox = new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(200, 200),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            Label processedLabel = new Label
            {
                Text = "Обработанное изображение (200x200):",
                Location = new Point(10, 215),
                Size = new Size(200, 30),
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Кнопка обработки
            processImageBtn = new Button
            {
                Text = "Обработать изображение",
                Location = new Point(220, 80),
                Size = new Size(150, 35),
                BackColor = Color.LightGray,
                Font = new Font("Segoe UI", 9)
            };
            processImageBtn.Click += ProcessImageBtn_Click;

            // Выбор знака зодиака
            Label signLabel = new Label
            {
                Text = "Выберите знак зодиака:",
                Location = new Point(10, 250),
                Size = new Size(200, 30),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            zodiacCombo = new ComboBox
            {
                Location = new Point(10, 285),
                Size = new Size(200, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            zodiacCombo.Items.AddRange(ZodiacSignHelper.GetAllRussianNames());
            zodiacCombo.SelectedIndex = 0;

            // Кнопки
            captureBtn = new Button
            {
                Text = "Добавить в обучающую выборку",
                Location = new Point(10, 330),
                Size = new Size(200, 40),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.LightGreen
            };
            captureBtn.Click += CaptureBtn_Click;

            recognizeBtn = new Button
            {
                Text = "Распознать",
                Location = new Point(10, 380),
                Size = new Size(200, 40),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.LightBlue
            };
            recognizeBtn.Click += RecognizeBtn_Click;

            clearBtn = new Button
            {
                Text = "Очистить холст",
                Location = new Point(220, 330),
                Size = new Size(100, 40),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.LightCoral
            };
            clearBtn.Click += ClearBtn_Click;

            saveImageBtn = new Button
            {
                Text = "Сохранить",
                Location = new Point(220, 380),
                Size = new Size(100, 40),
                Font = new Font("Segoe UI", 10)
            };
            saveImageBtn.Click += SaveImageBtn_Click;

            // Результат
            resultLabel = new Label
            {
                Location = new Point(10, 430),
                Size = new Size(440, 60),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Text = "Результат: ожидание...",
                TextAlign = ContentAlignment.MiddleLeft,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            controlPanel.Controls.AddRange(new Control[]
            {
                processedBox, processedLabel, processImageBtn,
                signLabel, zodiacCombo, captureBtn, recognizeBtn,
                clearBtn, saveImageBtn, resultLabel
            });

            // Инструкция внизу
            instructionLabel = new Label
            {
                Location = new Point(10, 520),
                Size = new Size(970, 100),
                Font = new Font("Segoe UI", 9),
                Text = "ИНСТРУКЦИЯ:\n" +
                      "1. Нарисуйте знак зодиака черным цветом на белом фоне (используйте мышь)\n" +
                      "2. Постарайтесь рисовать в центре области, один знак за раз\n" +
                      "3. Настройте размер кисти при необходимости\n" +
                      "4. Нажмите 'Обработать изображение' для предварительного просмотра\n" +
                      "5. Выберите соответствующий знак зодиака из списка\n" +
                      "6. Нажмите 'Добавить в обучающую выборку' для сохранения\n" +
                      "7. Или 'Распознать' для тестирования обученной нейросети",
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightYellow,
                Padding = new Padding(5)
            };

            // Инициализация поверхности рисования
            drawBitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
            drawGraphics = Graphics.FromImage(drawBitmap);
            drawGraphics.Clear(Color.White);
            drawGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            pictureBox.Image = drawBitmap;

            this.Controls.AddRange(new Control[]
            {
                drawPanel, controlPanel, instructionLabel
            });
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDrawing = true;
                lastPoint = e.Location;
            }
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                drawingPen.Width = brushSizeTrackBar.Value;
                drawGraphics.DrawLine(drawingPen, lastPoint, e.Location);
                lastPoint = e.Location;
                pictureBox.Invalidate();
            }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            isDrawing = false;
        }

        private void BrushSizeTrackBar_ValueChanged(object sender, EventArgs e)
        {
            drawingPen.Width = brushSizeTrackBar.Value;
        }

        private void GridCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            RedrawCanvas();
        }

        private void RedrawCanvas()
        {
            drawGraphics.Clear(Color.White);

            if (gridCheckBox.Checked)
            {
                Pen gridPen = new Pen(Color.LightGray, 1);
                int gridSize = 50;

                for (int x = 0; x < pictureBox.Width; x += gridSize)
                {
                    drawGraphics.DrawLine(gridPen, x, 0, x, pictureBox.Height);
                }

                for (int y = 0; y < pictureBox.Height; y += gridSize)
                {
                    drawGraphics.DrawLine(gridPen, 0, y, pictureBox.Width, y);
                }

                // Центральные линии
                gridPen.Color = Color.Red;
                gridPen.Width = 2;
                drawGraphics.DrawLine(gridPen, pictureBox.Width / 2, 0, pictureBox.Width / 2, pictureBox.Height);
                drawGraphics.DrawLine(gridPen, 0, pictureBox.Height / 2, pictureBox.Width, pictureBox.Height / 2);

                gridPen.Dispose();
            }

            pictureBox.Invalidate();
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            RedrawCanvas();
            processedBox.Image = null;
            resultLabel.Text = "Результат: ожидание...";
        }

        private void ProcessImageBtn_Click(object sender, EventArgs e)
        {
            Bitmap processed = PreprocessImage(drawBitmap);
            processedBox.Image = processed;
            resultLabel.Text = "Изображение обработано. Выберите действие.";
        }

        private void CaptureBtn_Click(object sender, EventArgs e)
        {
            if (zodiacCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите знак зодиака!");
                return;
            }

            if (processedBox.Image == null)
            {
                MessageBox.Show("Сначала обработайте изображение!");
                return;
            }

            Bitmap processed = (Bitmap)processedBox.Image;

            string selected = zodiacCombo.SelectedItem.ToString();
            ZodiacSign sign = ZodiacSignHelper.FromRussianString(selected);

            if (sign != ZodiacSign.Undef)
            {
                OnImageCaptured?.Invoke(processed, sign);
                resultLabel.Text = $"Добавлен: {selected}";
                resultLabel.ForeColor = Color.Green;

                MessageBox.Show($"Образ знака '{selected}' добавлен в обучающую выборку и сохранен в папку Dataset!",
                              "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void RecognizeBtn_Click(object sender, EventArgs e)
        {
            if (processedBox.Image == null)
            {
                MessageBox.Show("Сначала обработайте изображение!");
                return;
            }

            Bitmap processed = (Bitmap)processedBox.Image;
            OnImageForRecognition?.Invoke(processed);
        }

        private void SaveImageBtn_Click(object sender, EventArgs e)
        {
            if (processedBox.Image == null)
            {
                MessageBox.Show("Сначала обработайте изображение!");
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
            saveDialog.Title = "Сохранить изображение";
            saveDialog.FileName = $"zodiac_drawing_{DateTime.Now:yyyyMMdd_HHmmss}.png";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string extension = Path.GetExtension(saveDialog.FileName).ToLower();
                    System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;

                    if (extension == ".jpg" || extension == ".jpeg")
                        format = System.Drawing.Imaging.ImageFormat.Jpeg;
                    else if (extension == ".bmp")
                        format = System.Drawing.Imaging.ImageFormat.Bmp;

                    processedBox.Image.Save(saveDialog.FileName, format);
                    MessageBox.Show($"Изображение сохранено как {Path.GetFileName(saveDialog.FileName)}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
                }
            }
        }

        private Bitmap PreprocessImage(Bitmap original)
        {
            // Создаем копию оригинального изображения
            Bitmap source = new Bitmap(original);

            // Конвертируем в градации серого
            Bitmap grayscale = new Bitmap(source.Width, source.Height);
            for (int x = 0; x < source.Width; x++)
            {
                for (int y = 0; y < source.Height; y++)
                {
                    Color pixel = source.GetPixel(x, y);
                    int gray = (int)(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11);
                    grayscale.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }

            // Бинаризация с адаптивным порогом
            Bitmap binary = new Bitmap(grayscale.Width, grayscale.Height);
            int[] histogram = new int[256];

            // Строим гистограмму
            for (int x = 0; x < grayscale.Width; x++)
                for (int y = 0; y < grayscale.Height; y++)
                    histogram[grayscale.GetPixel(x, y).R]++;

            // Метод Оцу для определения порога
            int total = grayscale.Width * grayscale.Height;
            float sum = 0;
            for (int i = 0; i < 256; i++) sum += i * histogram[i];

            float sumB = 0;
            int wB = 0;
            int wF = 0;
            float varMax = 0;
            int threshold = 0;

            for (int i = 0; i < 256; i++)
            {
                wB += histogram[i];
                if (wB == 0) continue;

                wF = total - wB;
                if (wF == 0) break;

                sumB += (float)(i * histogram[i]);
                float mB = sumB / wB;
                float mF = (sum - sumB) / wF;

                float varBetween = (float)wB * (float)wF * (mB - mF) * (mB - mF);

                if (varBetween > varMax)
                {
                    varMax = varBetween;
                    threshold = i;
                }
            }

            // Применяем порог
            for (int x = 0; x < grayscale.Width; x++)
            {
                for (int y = 0; y < grayscale.Height; y++)
                {
                    Color pixel = grayscale.GetPixel(x, y);
                    binary.SetPixel(x, y, pixel.R < threshold ? Color.Black : Color.White);
                }
            }

            // Обрезка пустых областей
            Rectangle bounds = GetContentBounds(binary);
            if (bounds.Width > 0 && bounds.Height > 0)
            {
                Bitmap cropped = new Bitmap(bounds.Width, bounds.Height);
                using (Graphics g = Graphics.FromImage(cropped))
                {
                    g.DrawImage(binary, new Rectangle(0, 0, bounds.Width, bounds.Height),
                               bounds, GraphicsUnit.Pixel);
                }

                // Масштабирование до 200x200 с сохранением пропорций
                Bitmap resized = new Bitmap(200, 200);
                using (Graphics g = Graphics.FromImage(resized))
                {
                    g.Clear(Color.White);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                    float scale = Math.Min(180f / bounds.Width, 180f / bounds.Height);
                    int newWidth = (int)(bounds.Width * scale);
                    int newHeight = (int)(bounds.Height * scale);
                    int offsetX = (200 - newWidth) / 2;
                    int offsetY = (200 - newHeight) / 2;

                    g.DrawImage(cropped, offsetX, offsetY, newWidth, newHeight);
                }
                return resized;
            }

            return new Bitmap(200, 200);
        }

        private Rectangle GetContentBounds(Bitmap image)
        {
            int minX = image.Width, minY = image.Height;
            int maxX = 0, maxY = 0;
            bool found = false;

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixel = image.GetPixel(x, y);
                    if (pixel.R < 128) // Черный пиксель
                    {
                        found = true;
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            if (!found)
                return Rectangle.Empty;

            // Добавляем отступ 5%
            int paddingX = (int)((maxX - minX) * 0.05);
            int paddingY = (int)((maxY - minY) * 0.05);
            paddingX = Math.Max(10, Math.Min(20, paddingX));
            paddingY = Math.Max(10, Math.Min(20, paddingY));

            minX = Math.Max(0, minX - paddingX);
            minY = Math.Max(0, minY - paddingY);
            maxX = Math.Min(image.Width - 1, maxX + paddingX);
            maxY = Math.Min(image.Height - 1, maxY + paddingY);

            return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        public void UpdateRecognitionResult(string result)
        {
            resultLabel.Text = $"Распознано как: {result}";
            resultLabel.ForeColor = Color.Blue;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                drawingPen?.Dispose();
                drawGraphics?.Dispose();
                drawBitmap?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}