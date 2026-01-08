using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using NeuralNetwork1;

namespace NeuralNetworkZodiac
{
    public class CameraCaptureForm : Form
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private PictureBox videoBox;
        private PictureBox processedBox;
        private PictureBox originalBox;
        private Button startBtn;
        private Button stopBtn;
        private Button captureBtn;
        private Button recognizeBtn;
        private ComboBox cameraCombo;
        private ComboBox zodiacCombo;
        private ComboBox resolutionsBox;
        private Label resultLabel;
        private Button saveImageBtn;
        private Bitmap currentFrame;
        private Controller controller;
        private Stopwatch sw = new Stopwatch();
        private Label ticksLabel;
        private TrackBar thresholdTrackBar;
        private TrackBar borderTrackBar;
        private Panel controlPanel;
        private CheckBox processImgCheckBox;
        private Label thresholdLabel;
        private Label borderLabel;
        private Label timeLabel;
        private Label topLabel;
        private Label leftLabel;
        private Label infoLabel;
        private Button resetSettingsBtn;
        private System.Threading.Timer updateTimer;
        private Button autoAugmentBtn;
        private NumericUpDown augmentCountBox;
        private Label augmentLabel;
        private ToolTip toolTip; // Добавляем ToolTip

        public event Action<Bitmap, ZodiacSign> OnImageCaptured;
        public event Action<Bitmap> OnImageForRecognition;

        public CameraCaptureForm()
        {
            toolTip = new ToolTip(); // Инициализируем ToolTip
            InitializeComponent();
            controller = new Controller(UpdateFormFields);
            InitializeCameraList();

            // Таймер для обновления информации
            updateTimer = new System.Threading.Timer(UpdateInfo, null, 0, 500);
        }

        private void InitializeComponent()
        {
            this.Text = "Камера - Распознавание знаков зодиака";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9);
            this.KeyPreview = true;
            this.KeyDown += CameraCaptureForm_KeyDown;

            // Верхняя панель с камерами и кнопками
            Panel topPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(1380, 60),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(5)
            };

            // Выбор камеры
            Label cameraLabel = new Label
            {
                Text = "Камера:",
                Location = new Point(10, 15),
                Size = new Size(70, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };

            cameraCombo = new ComboBox
            {
                Location = new Point(85, 15),
                Size = new Size(220, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cameraCombo.SelectionChangeCommitted += CameraCombo_SelectionChangeCommitted;

            // Разрешения
            Label resolutionLabel = new Label
            {
                Text = "Разрешение:",
                Location = new Point(315, 15),
                Size = new Size(90, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };

            resolutionsBox = new ComboBox
            {
                Location = new Point(410, 15),
                Size = new Size(180, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };

            // Кнопки управления камерой
            startBtn = new Button
            {
                Text = "СТАРТ",
                Location = new Point(600, 15),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(76, 175, 80),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            startBtn.FlatAppearance.BorderSize = 0;
            startBtn.Click += StartBtn_Click;

            stopBtn = new Button
            {
                Text = "СТОП",
                Location = new Point(710, 15),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(244, 67, 54),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            stopBtn.FlatAppearance.BorderSize = 0;
            stopBtn.Click += StopBtn_Click;

            // Выбор знака зодиака
            Label signLabel = new Label
            {
                Text = "Знак:",
                Location = new Point(820, 15),
                Size = new Size(50, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };

            zodiacCombo = new ComboBox
            {
                Location = new Point(875, 15),
                Size = new Size(150, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            zodiacCombo.Items.AddRange(ZodiacSignHelper.GetAllRussianNames());
            zodiacCombo.SelectedIndex = 0;

            // Кнопка сброса настроек
            resetSettingsBtn = new Button
            {
                Text = "СБРОС",
                Location = new Point(1035, 15),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(255, 193, 7),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            resetSettingsBtn.FlatAppearance.BorderSize = 0;
            resetSettingsBtn.Click += ResetSettingsBtn_Click;

            // Кнопка авто-аугментации
            autoAugmentBtn = new Button
            {
                Text = "АУГМЕНТИРОВАТЬ",
                Location = new Point(1145, 15),
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(156, 39, 176),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            autoAugmentBtn.FlatAppearance.BorderSize = 0;
            autoAugmentBtn.Click += AutoAugmentBtn_Click;

            // Настройка количества аугментаций
            augmentLabel = new Label
            {
                Text = "Кол-во:",
                Location = new Point(1305, 15),
                Size = new Size(50, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };

            augmentCountBox = new NumericUpDown
            {
                Location = new Point(1360, 15),
                Size = new Size(60, 30),
                Minimum = 1,
                Maximum = 10,
                Value = 5,
                Font = new Font("Segoe UI", 9)
            };

            topPanel.Controls.AddRange(new Control[]
            {
                cameraLabel, cameraCombo, resolutionLabel, resolutionsBox,
                startBtn, stopBtn, signLabel, zodiacCombo, resetSettingsBtn,
                autoAugmentBtn, augmentLabel, augmentCountBox
            });

            // Панель с изображениями
            Panel imagePanel = new Panel
            {
                Location = new Point(10, 80),
                Size = new Size(1380, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            // Видео с камеры
            videoBox = new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(500, 480),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Black,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            // Оригинальное изображение с обработкой
            originalBox = new PictureBox
            {
                Location = new Point(520, 10),
                Size = new Size(500, 480),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            // Обработанное изображение для нейросети
            processedBox = new PictureBox
            {
                Location = new Point(1030, 10),
                Size = new Size(200, 200),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            Label videoLabel = new Label
            {
                Text = "ВИДЕО С КАМЕРЫ",
                Location = new Point(10, 495),
                Size = new Size(500, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            Label originalLabel = new Label
            {
                Text = "ОБРАБОТАННОЕ ИЗОБРАЖЕНИЕ",
                Location = new Point(520, 495),
                Size = new Size(500, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            Label processedLabel = new Label
            {
                Text = "ДЛЯ НЕЙРОСЕТИ (200x200)",
                Location = new Point(1030, 215),
                Size = new Size(200, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            // Метка для времени обработки
            timeLabel = new Label
            {
                Location = new Point(1030, 240),
                Size = new Size(200, 30),
                Text = "Время: 0 мс",
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Информация о положении
            infoLabel = new Label
            {
                Location = new Point(1030, 275),
                Size = new Size(340, 60),
                Text = "Положение: Top=20, Left=20\nГраница: 10\nУправление: WASD, Q/E",
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightYellow,
                Padding = new Padding(5)
            };

            // Кнопки управления под обработанным изображением
            captureBtn = new Button
            {
                Text = "ЗАХВАТИТЬ И ДОБАВИТЬ",
                Location = new Point(1030, 340),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(33, 150, 243),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            captureBtn.FlatAppearance.BorderSize = 0;
            captureBtn.Click += CaptureBtn_Click;

            recognizeBtn = new Button
            {
                Text = "РАСПОЗНАТЬ",
                Location = new Point(1030, 390),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(76, 175, 80),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            recognizeBtn.FlatAppearance.BorderSize = 0;
            recognizeBtn.Click += RecognizeBtn_Click;

            saveImageBtn = new Button
            {
                Text = "СОХРАНИТЬ",
                Location = new Point(1030, 440),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(121, 85, 72),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            saveImageBtn.FlatAppearance.BorderSize = 0;
            saveImageBtn.Click += SaveImageBtn_Click;

            imagePanel.Controls.AddRange(new Control[]
            {
                videoBox, originalBox, processedBox,
                videoLabel, originalLabel, processedLabel,
                timeLabel, infoLabel, captureBtn,
                recognizeBtn, saveImageBtn
            });

            // Панель управления обработкой
            controlPanel = new Panel
            {
                Location = new Point(10, 590),
                Size = new Size(1380, 260),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.AliceBlue,
                Padding = new Padding(10)
            };

            // Настройки обработки
            thresholdLabel = new Label
            {
                Text = "ПОРОГ (1-255):",
                Location = new Point(10, 20),
                Size = new Size(120, 30),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            thresholdTrackBar = new TrackBar
            {
                Location = new Point(135, 20),
                Size = new Size(250, 30),
                Minimum = 1,
                Maximum = 255,
                Value = 60, // Уменьшен минимальный порог
                TickFrequency = 10,
                LargeChange = 20,
                SmallChange = 5
            };
            thresholdTrackBar.ValueChanged += ThresholdTrackBar_ValueChanged;

            Label thresholdValueLabel = new Label
            {
                Text = "60",
                Location = new Point(390, 20),
                Size = new Size(50, 30),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };
            thresholdTrackBar.ValueChanged += (s, e) =>
                thresholdValueLabel.Text = thresholdTrackBar.Value.ToString();

            borderLabel = new Label
            {
                Text = "ГРАНИЦА (1-50):",
                Location = new Point(450, 20),
                Size = new Size(130, 30),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            borderTrackBar = new TrackBar
            {
                Location = new Point(585, 20),
                Size = new Size(250, 30),
                Minimum = 1,
                Maximum = 50, // Уменьшен максимальный размер
                Value = 10, // Уменьшено значение по умолчанию
                TickFrequency = 5,
                LargeChange = 10,
                SmallChange = 2
            };
            borderTrackBar.ValueChanged += BorderTrackBar_ValueChanged;

            Label borderValueLabel = new Label
            {
                Text = "10",
                Location = new Point(840, 20),
                Size = new Size(50, 30),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };
            borderTrackBar.ValueChanged += (s, e) =>
                borderValueLabel.Text = borderTrackBar.Value.ToString();

            // Положение области
            Panel positionPanel = new Panel
            {
                Location = new Point(10, 70),
                Size = new Size(400, 60),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightYellow
            };

            Label positionTitle = new Label
            {
                Text = "ПОЛОЖЕНИЕ ОБЛАСТИ:",
                Location = new Point(10, 5),
                Size = new Size(380, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            topLabel = new Label
            {
                Text = "Верх:",
                Location = new Point(50, 30),
                Size = new Size(60, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label topValueLabel = new Label
            {
                Text = "20",
                Location = new Point(115, 30),
                Size = new Size(40, 25),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            leftLabel = new Label
            {
                Text = "Лево:",
                Location = new Point(170, 30),
                Size = new Size(60, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label leftValueLabel = new Label
            {
                Text = "20",
                Location = new Point(235, 30),
                Size = new Size(40, 25),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            Button resetPositionBtn = new Button
            {
                Text = "Сброс",
                Location = new Point(290, 28),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(255, 193, 7),
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat
            };
            resetPositionBtn.FlatAppearance.BorderSize = 0;
            resetPositionBtn.Click += (s, e) =>
            {
                controller.settings.top = 20;
                controller.settings.left = 20;
            };

            positionPanel.Controls.AddRange(new Control[]
            {
                positionTitle, topLabel, topValueLabel,
                leftLabel, leftValueLabel, resetPositionBtn
            });

            // CheckBox для включения/выключения обработки
            processImgCheckBox = new CheckBox
            {
                Text = "ВКЛЮЧИТЬ ОБРАБОТКУ ИЗОБРАЖЕНИЯ",
                Location = new Point(420, 70),
                Size = new Size(250, 30),
                Checked = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            processImgCheckBox.CheckedChanged += ProcessImgCheckBox_CheckedChanged;

            // Метка результата
            resultLabel = new Label
            {
                Location = new Point(10, 140),
                Size = new Size(1360, 60),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Text = "РЕЗУЛЬТАТ: ОЖИДАНИЕ...",
                TextAlign = ContentAlignment.MiddleLeft,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                ForeColor = Color.DarkBlue,
                Padding = new Padding(10)
            };

            // Инструкция
            Panel instructionPanel = new Panel
            {
                Location = new Point(10, 210),
                Size = new Size(1360, 40),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightCyan
            };

            Label instructionLabel = new Label
            {
                Text = "УПРАВЛЕНИЕ: W/S - двигать область вверх/вниз • A/D - влево/вправо • Q/E - увеличить/уменьшить границу • Нарисуйте знак зодиака на бумаге и наведите камеру",
                Location = new Point(10, 10),
                Size = new Size(1340, 20),
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleLeft
            };

            instructionPanel.Controls.Add(instructionLabel);

            controlPanel.Controls.AddRange(new Control[]
            {
                thresholdLabel, thresholdTrackBar, thresholdValueLabel,
                borderLabel, borderTrackBar, borderValueLabel,
                positionPanel, processImgCheckBox, resultLabel, instructionPanel
            });

            this.Controls.AddRange(new Control[]
            {
                topPanel, imagePanel, controlPanel
            });

            this.FormClosing += CameraCaptureForm_FormClosing;

            // Добавляем ToolTip для кнопки авто-аугментации
            toolTip.SetToolTip(autoAugmentBtn, "Автоматически создать аугментированные версии изображения\n" +
                                               "с поворотами, шумом и изменением яркости/контраста");

            // Обновление значений положения
            System.Windows.Forms.Timer updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = 100;
            updateTimer.Tick += (s, e) =>
            {
                topValueLabel.Text = controller.settings.top.ToString();
                leftValueLabel.Text = controller.settings.left.ToString();
            };
            updateTimer.Start();
        }

        private void InitializeCameraList()
        {
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count == 0)
                {
                    MessageBox.Show("Камеры не найдены! Подключите веб-камеру.", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                cameraCombo.Items.Clear();
                foreach (FilterInfo device in videoDevices)
                {
                    cameraCombo.Items.Add(device.Name);
                }
                cameraCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске камер: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CameraCombo_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cameraCombo.SelectedItem == null) return;

            try
            {
                var vcd = new VideoCaptureDevice(videoDevices[cameraCombo.SelectedIndex].MonikerString);
                resolutionsBox.Items.Clear();

                if (vcd.VideoCapabilities.Length > 0)
                {
                    for (int i = 0; i < vcd.VideoCapabilities.Length; i++)
                    {
                        var cap = vcd.VideoCapabilities[i];
                        resolutionsBox.Items.Add($"{cap.FrameSize.Width}x{cap.FrameSize.Height} ({cap.MaximumFrameRate} fps)");
                    }
                    resolutionsBox.SelectedIndex = Math.Min(2, vcd.VideoCapabilities.Length - 1);
                }
                else
                {
                    resolutionsBox.Items.Add("По умолчанию");
                    resolutionsBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки разрешений: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            if (cameraCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите камеру!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                videoSource = new VideoCaptureDevice(videoDevices[cameraCombo.SelectedIndex].MonikerString);

                // Устанавливаем разрешение, если выбрано
                if (resolutionsBox.SelectedIndex >= 0 &&
                    resolutionsBox.SelectedItem.ToString() != "По умолчанию" &&
                    resolutionsBox.SelectedIndex < videoSource.VideoCapabilities.Length)
                {
                    videoSource.VideoResolution = videoSource.VideoCapabilities[resolutionsBox.SelectedIndex];
                }

                videoSource.NewFrame += VideoSource_NewFrame;
                videoSource.Start();

                startBtn.Enabled = false;
                stopBtn.Enabled = true;
                controlPanel.Enabled = true;
                cameraCombo.Enabled = false;
                resolutionsBox.Enabled = false;

                resultLabel.Text = "КАМЕРА ЗАПУЩЕНА. НАВЕДИТЕ НА ЗНАК ЗОДИАКА...";
                resultLabel.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска камеры: {ex.Message}\n\nУбедитесь, что камера не занята другим приложением.",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StopBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (videoSource != null && videoSource.IsRunning)
                {
                    videoSource.SignalToStop();
                    videoSource.WaitForStop();
                    videoSource.NewFrame -= VideoSource_NewFrame;

                    startBtn.Enabled = true;
                    stopBtn.Enabled = false;
                    controlPanel.Enabled = false;
                    cameraCombo.Enabled = true;
                    resolutionsBox.Enabled = true;

                    resultLabel.Text = "КАМЕРА ОСТАНОВЛЕНА";
                    resultLabel.ForeColor = Color.Red;

                    // Очищаем изображения
                    videoBox.Image = null;
                    originalBox.Image = null;
                    processedBox.Image = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка остановки камеры: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                // Получаем кадр
                Bitmap frame = (Bitmap)eventArgs.Frame.Clone();

                // Обновляем видео на форме (цветное изображение)
                if (videoBox.InvokeRequired)
                {
                    videoBox.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            if (videoBox.Image != null)
                                videoBox.Image.Dispose();
                            videoBox.Image = frame;
                        }
                        catch { }
                    }));
                }
                else
                {
                    if (videoBox.Image != null)
                        videoBox.Image.Dispose();
                    videoBox.Image = frame;
                }

                // Передаем кадр в контроллер для обработки (только если контроллер готов)
                if (controller.Ready)
                {
                    // Используем отдельную копию для обработки
                    Bitmap processingFrame = (Bitmap)frame.Clone();
                    controller.ProcessImage(processingFrame);
                    // Не освобождаем здесь, контроллер сам освободит при необходимости
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка получения кадра: {ex.Message}");
            }
        }

        private void UpdateFormFields()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateFormFields));
                return;
            }

            try
            {
                // Обновляем оригинальное изображение с сеткой
                if (originalBox.Image != null)
                    originalBox.Image.Dispose();
                originalBox.Image = controller.GetOriginalImage();

                // Обновляем обработанное изображение
                if (processedBox.Image != null)
                    processedBox.Image.Dispose();
                processedBox.Image = controller.GetProcessedImage();

                // Обновляем время обработки
                sw.Stop();
                timeLabel.Text = $"Обработка: {sw.ElapsedMilliseconds} мс";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка обновления формы: {ex.Message}");
            }
        }

        private void UpdateInfo(object state)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object>(UpdateInfo), state);
                return;
            }

            try
            {
                infoLabel.Text = $"Положение области:\n" +
                                $"Top: {controller.settings.top}, Left: {controller.settings.left}\n" +
                                $"Граница: {controller.settings.border}\n" +
                                $"Порог: {controller.settings.threshold}";
            }
            catch { }
        }

        private void CaptureBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (controller.GetProcessedImage() == null)
                {
                    MessageBox.Show("Нет обработанного изображения! Запустите камеру.", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (zodiacCombo.SelectedItem == null)
                {
                    MessageBox.Show("Выберите знак зодиака!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string selected = zodiacCombo.SelectedItem.ToString();
                ZodiacSign sign = ZodiacSignHelper.FromRussianString(selected);

                if (sign != ZodiacSign.Undef)
                {
                    Bitmap processed = controller.GetProcessedImage();

                    // Сохраняем в папку Dataset
                    string datasetPath = Path.Combine(Application.StartupPath, "Dataset");
                    string classFolder = Path.Combine(datasetPath, sign.ToRussianString());

                    if (!Directory.Exists(classFolder))
                        Directory.CreateDirectory(classFolder);

                    // Сохраняем оригинал
                    string filename = $"{sign.ToRussianString()}_{DateTime.Now:yyyyMMdd_HHmmss}_original.png";
                    string fullPath = Path.Combine(classFolder, filename);
                    processed.Save(fullPath, ImageFormat.Png);

                    // Создаем аугментированные версии
                    int augmentCount = (int)augmentCountBox.Value;
                    Random rand = new Random();

                    for (int i = 0; i < augmentCount; i++)
                    {
                        Bitmap augImage = CreateAugmentedImage(processed, rand);
                        string augFilename = $"{sign.ToRussianString()}_{DateTime.Now:yyyyMMdd_HHmmss}_aug{i + 1}.png";
                        string augPath = Path.Combine(classFolder, augFilename);
                        augImage.Save(augPath, ImageFormat.Png);
                        augImage.Dispose();
                    }

                    OnImageCaptured?.Invoke(processed, sign);
                    resultLabel.Text = $"ДОБАВЛЕНО: {selected} (оригинал + {augmentCount} аугментированных)";
                    resultLabel.ForeColor = Color.Blue;

                    MessageBox.Show($"Изображение знака '{selected}' добавлено в обучающую выборку!\n\n" +
                                  $"Сохранено в: {classFolder}\n" +
                                  $"Файлов создано: {augmentCount + 1}",
                                  "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при захвате: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Bitmap CreateAugmentedImage(Bitmap original, Random rand)
        {
            Bitmap augImage = new Bitmap(original);

            // Случайный поворот от -45 до +45 градусов
            float angle = (float)(rand.NextDouble() * 90 - 45);
            augImage = RotateImage(augImage, angle);

            // Случайное добавление шума
            augImage = AddNoise(augImage, rand.Next(10, 30));

            // Случайное изменение яркости/контраста
            float brightness = (float)(rand.NextDouble() * 0.5 + 0.75); // 0.75-1.25
            float contrast = (float)(rand.NextDouble() * 0.6 + 0.7); // 0.7-1.3
            augImage = AdjustBrightnessContrast(augImage, brightness, contrast);

            return augImage;
        }

        private Bitmap RotateImage(Bitmap original, float angle)
        {
            Bitmap rotated = new Bitmap(original.Width, original.Height);
            using (Graphics g = Graphics.FromImage(rotated))
            {
                g.TranslateTransform(original.Width / 2, original.Height / 2);
                g.RotateTransform(angle);
                g.TranslateTransform(-original.Width / 2, -original.Height / 2);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(original, new Point(0, 0));
            }
            return rotated;
        }

        private Bitmap AddNoise(Bitmap original, int noiseLevel)
        {
            Bitmap noisy = new Bitmap(original);
            Random rand = new Random();

            for (int i = 0; i < noiseLevel; i++)
            {
                int x = rand.Next(original.Width);
                int y = rand.Next(original.Height);
                Color pixel = original.GetPixel(x, y);

                // Инвертируем случайные пиксели
                if (rand.NextDouble() > 0.7)
                {
                    noisy.SetPixel(x, y, Color.FromArgb(255 - pixel.R, 255 - pixel.G, 255 - pixel.B));
                }
            }

            return noisy;
        }

        private Bitmap AdjustBrightnessContrast(Bitmap original, float brightness, float contrast)
        {
            Bitmap adjusted = new Bitmap(original.Width, original.Height);

            for (int x = 0; x < original.Width; x++)
            {
                for (int y = 0; y < original.Height; y++)
                {
                    Color pixel = original.GetPixel(x, y);

                    // Применяем яркость
                    int r = (int)(pixel.R * brightness);
                    int g = (int)(pixel.G * brightness);
                    int b = (int)(pixel.B * brightness);

                    // Применяем контраст
                    r = (int)(((r - 128) * contrast) + 128);
                    g = (int)(((g - 128) * contrast) + 128);
                    b = (int)(((b - 128) * contrast) + 128);

                    // Ограничиваем значения
                    r = Math.Max(0, Math.Min(255, r));
                    g = Math.Max(0, Math.Min(255, g));
                    b = Math.Max(0, Math.Min(255, b));

                    adjusted.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

            return adjusted;
        }

        private void AutoAugmentBtn_Click(object sender, EventArgs e)
        {
            if (controller.GetProcessedImage() == null)
            {
                MessageBox.Show("Нет изображения для аугментации!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (zodiacCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите знак зодиака!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selected = zodiacCombo.SelectedItem.ToString();
            ZodiacSign sign = ZodiacSignHelper.FromRussianString(selected);

            if (sign != ZodiacSign.Undef)
            {
                Bitmap processed = controller.GetProcessedImage();
                int augmentCount = (int)augmentCountBox.Value;

                // Создаем аугментированные версии и сразу добавляем в выборку
                Random rand = new Random();
                int addedCount = 0;

                for (int i = 0; i < augmentCount; i++)
                {
                    Bitmap augImage = CreateAugmentedImage(processed, rand);

                    // Добавляем в выборку
                    double[] augInput = ConvertImageToVector(augImage);
                    Sample sample = new Sample(augInput, 12, sign);

                    // Здесь нужно добавить в trainingSet главной формы
                    // Для этого вызовем событие OnImageCaptured с аугментированным изображением
                    OnImageCaptured?.Invoke(augImage, sign);
                    addedCount++;

                    augImage.Dispose();
                }

                resultLabel.Text = $"СОЗДАНО {addedCount} АУГМЕНТИРОВАННЫХ ВЕРСИЙ: {selected}";
                resultLabel.ForeColor = Color.DarkGreen;

                MessageBox.Show($"Создано {addedCount} аугментированных версий знака '{selected}'!\n" +
                              "Все версии добавлены в обучающую выборку.",
                              "Аугментация завершена", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private double[] ConvertImageToVector(Bitmap image)
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

        private void RecognizeBtn_Click(object sender, EventArgs e)
        {
            if (controller.GetProcessedImage() == null)
            {
                MessageBox.Show("Нет обработанного изображения!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Bitmap processed = controller.GetProcessedImage();
            OnImageForRecognition?.Invoke(processed);
        }

        private void SaveImageBtn_Click(object sender, EventArgs e)
        {
            if (processedBox.Image == null)
            {
                MessageBox.Show("Нет изображения для сохранения!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp",
                Title = "Сохранить изображение",
                FileName = $"zodiac_{DateTime.Now:yyyyMMdd_HHmmss}.png",
                DefaultExt = "png"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string extension = Path.GetExtension(saveDialog.FileName).ToLower();
                    ImageFormat format = ImageFormat.Png;

                    if (extension == ".jpg" || extension == ".jpeg")
                        format = ImageFormat.Jpeg;
                    else if (extension == ".bmp")
                        format = ImageFormat.Bmp;

                    processedBox.Image.Save(saveDialog.FileName, format);

                    resultLabel.Text = $"СОХРАНЕНО: {Path.GetFileName(saveDialog.FileName)}";
                    resultLabel.ForeColor = Color.DarkBlue;

                    MessageBox.Show($"Изображение сохранено как\n{saveDialog.FileName}",
                                  "Сохранено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CameraCaptureForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopBtn_Click(null, null);
            updateTimer?.Dispose();
        }

        public void UpdateRecognitionResult(string result)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateRecognitionResult), result);
                return;
            }

            resultLabel.Text = $"РАСПОЗНАНО: {result}";
            resultLabel.ForeColor = Color.DarkGreen;
        }

        // Обработчики изменения настроек
        private void ThresholdTrackBar_ValueChanged(object sender, EventArgs e)
        {
            controller.settings.threshold = (byte)thresholdTrackBar.Value;
            controller.settings.differenceLim = Math.Max(0.01f, (float)thresholdTrackBar.Value / 255.0f);

            // Принудительно обновляем отображение с новым порогом
            if (controller != null && currentFrame != null)
            {
                Bitmap frameCopy = (Bitmap)currentFrame.Clone();
                controller.ProcessImage(frameCopy);
            }
        }

        private void BorderTrackBar_ValueChanged(object sender, EventArgs e)
        {
            controller.settings.border = borderTrackBar.Value;
        }

        private void ProcessImgCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            controller.settings.processImg = processImgCheckBox.Checked;
        }

        private void ResetSettingsBtn_Click(object sender, EventArgs e)
        {
            controller.settings.top = 20;
            controller.settings.left = 20;
            controller.settings.border = 10;
            controller.settings.threshold = 60;
            controller.settings.differenceLim = 0.05f;

            thresholdTrackBar.Value = 60;
            borderTrackBar.Value = 10;

            resultLabel.Text = "НАСТРОЙКИ СБРОШЕНЫ К ЗНАЧЕНИЯМ ПО УМОЛЧАНИЮ";
            resultLabel.ForeColor = Color.Orange;

            MessageBox.Show("Настройки обработки сброшены к значениям по умолчанию:\n\n" +
                          "• Положение: Top=20, Left=20\n" +
                          "• Граница: 10\n" +
                          "• Порог: 60\n" +
                          "• Difference Limit: 0.05",
                          "Сброс настроек", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CameraCaptureForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W: controller.settings.decTop(); break;
                case Keys.S: controller.settings.incTop(); break;
                case Keys.A: controller.settings.decLeft(); break;
                case Keys.D: controller.settings.incLeft(); break;
                case Keys.Q:
                    controller.settings.border = Math.Max(1, controller.settings.border - 1);
                    borderTrackBar.Value = controller.settings.border;
                    break;
                case Keys.E:
                    controller.settings.border = Math.Min(50, controller.settings.border + 1);
                    borderTrackBar.Value = controller.settings.border;
                    break;
            }
        }
    }
}