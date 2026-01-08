using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NeuralNetwork1;

namespace NeuralNetworkZodiac
{
    public class NeuralNetworksStand : Form
    {
        private GenerateImage generator = new GenerateImage();
        private BaseNetwork currentNetwork = null;
        private SamplesSet trainingSet = new SamplesSet();
        private SamplesSet testSet = new SamplesSet();
        private SamplesSet generatedSet = new SamplesSet();

        // Элементы управления
        private ComboBox networkTypeComboBox;
        private Button trainButton;
        private Button testButton;
        private Button generateTrainingSetButton;
        private Button generateTestSetButton;
        private Button openDrawFormBtn;
        private Button openCameraFormBtn;
        private TextBox epochsTextBox;
        private TextBox errorTextBox;
        private ProgressBar progressBar;
        private Label statusLabel;
        private PictureBox samplePictureBox;
        private ListBox resultsListBox;
        private Label trainingSetLabel;
        private Label testSetLabel;
        private Label generatedSetLabel;
        private Label totalLabel;
        private Button saveTrainingSetBtn;
        private Button loadTrainingSetBtn;
        private Button clearGeneratedSetBtn;
        private Button loadDatasetFromFolderBtn;
        private Button augmentDatasetBtn;
        private Button analyzePerformanceBtn;
        private Label accuracyLabel;
        private Button exportResultsBtn;
        private ToolTip toolTip; // Добавляем ToolTip

        private Dictionary<string, Func<int[], BaseNetwork>> networkConstructors;
        private CameraAndDrawForm drawForm;
        private CameraCaptureForm cameraForm;

        public NeuralNetworksStand(Dictionary<string, Func<int[], BaseNetwork>> networks)
        {
            networkConstructors = networks;
            toolTip = new ToolTip(); // Инициализируем ToolTip
            InitializeComponent();
            InitializeNetworkControls();
            CreateDatasetFolder();
        }

        private void InitializeComponent()
        {
            this.Text = "Нейросеть для распознавания знаков зодиака";
            this.Size = new Size(1400, 950);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9);
            this.BackColor = Color.White;

            // Панель управления сетью
            Panel controlPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(1360, 130),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(5)
            };

            // Выбор типа сети
            Label networkLabel = new Label
            {
                Text = "Тип нейросети:",
                Location = new Point(10, 15),
                Size = new Size(100, 25),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            networkTypeComboBox = new ComboBox
            {
                Location = new Point(115, 15),
                Size = new Size(180, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };

            // Добавляем элементы в комбобокс
            if (networkConstructors != null && networkConstructors.Count > 0)
            {
                foreach (var key in networkConstructors.Keys)
                {
                    networkTypeComboBox.Items.Add(key);
                }
                networkTypeComboBox.SelectedIndex = 0;
            }
            else
            {
                networkTypeComboBox.Items.Add("Accord.Net");
                networkTypeComboBox.Items.Add("Student Network");
                networkTypeComboBox.SelectedIndex = 0;
            }

            networkTypeComboBox.SelectedIndexChanged += NetworkTypeComboBox_SelectedIndexChanged;

            // Параметры обучения
            Label epochsLabel = new Label
            {
                Text = "Эпох обучения:",
                Location = new Point(310, 15),
                Size = new Size(100, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            epochsTextBox = new TextBox
            {
                Location = new Point(415, 15),
                Size = new Size(70, 25),
                Text = "100",
                Font = new Font("Segoe UI", 9)
            };

            Label errorLabel = new Label
            {
                Text = "Целевая ошибка:",
                Location = new Point(500, 15),
                Size = new Size(110, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            errorTextBox = new TextBox
            {
                Location = new Point(615, 15),
                Size = new Size(80, 25),
                Text = "0.01",
                Font = new Font("Segoe UI", 9)
            };

            // Кнопки обучения и тестирования
            trainButton = new Button
            {
                Text = "ОБУЧИТЬ СЕТЬ",
                Location = new Point(710, 15),
                Size = new Size(140, 25),
                BackColor = Color.FromArgb(76, 175, 80),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            trainButton.FlatAppearance.BorderSize = 0;
            trainButton.Click += TrainButton_Click;

            testButton = new Button
            {
                Text = "ТЕСТИРОВАТЬ",
                Location = new Point(860, 15),
                Size = new Size(140, 25),
                BackColor = Color.FromArgb(33, 150, 243),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            testButton.FlatAppearance.BorderSize = 0;
            testButton.Click += TestButton_Click;

            analyzePerformanceBtn = new Button
            {
                Text = "АНАЛИЗ ТОЧНОСТИ",
                Location = new Point(1010, 15),
                Size = new Size(140, 25),
                BackColor = Color.FromArgb(255, 193, 7),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            analyzePerformanceBtn.FlatAppearance.BorderSize = 0;
            analyzePerformanceBtn.Click += AnalyzePerformanceBtn_Click;

            // Кнопки для работы с данными
            openDrawFormBtn = new Button
            {
                Text = "📝 РИСОВАНИЕ",
                Location = new Point(10, 55),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(156, 39, 176),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            openDrawFormBtn.FlatAppearance.BorderSize = 0;
            openDrawFormBtn.Click += OpenDrawFormBtn_Click;

            openCameraFormBtn = new Button
            {
                Text = "📷 КАМЕРА",
                Location = new Point(170, 55),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(156, 39, 176),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            openCameraFormBtn.FlatAppearance.BorderSize = 0;
            openCameraFormBtn.Click += OpenCameraFormBtn_Click;

            generateTrainingSetButton = new Button
            {
                Text = "СГЕНЕРИРОВАТЬ ОБУЧАЮЩУЮ",
                Location = new Point(330, 55),
                Size = new Size(200, 35),
                BackColor = Color.FromArgb(0, 150, 136),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            generateTrainingSetButton.FlatAppearance.BorderSize = 0;
            generateTrainingSetButton.Click += GenerateTrainingSetButton_Click;

            generateTestSetButton = new Button
            {
                Text = "СГЕНЕРИРОВАТЬ ТЕСТОВУЮ",
                Location = new Point(540, 55),
                Size = new Size(200, 35),
                BackColor = Color.FromArgb(0, 150, 136),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            generateTestSetButton.FlatAppearance.BorderSize = 0;
            generateTestSetButton.Click += GenerateTestSetButton_Click;

            loadDatasetFromFolderBtn = new Button
            {
                Text = "ЗАГРУЗИТЬ ИЗ ПАПКИ",
                Location = new Point(750, 55),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(121, 85, 72),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            loadDatasetFromFolderBtn.FlatAppearance.BorderSize = 0;
            loadDatasetFromFolderBtn.Click += LoadDatasetFromFolderBtn_Click;

            // Кнопки сохранения/загрузки
            saveTrainingSetBtn = new Button
            {
                Text = "СОХРАНИТЬ ВЫБОРКУ",
                Location = new Point(940, 55),
                Size = new Size(170, 35),
                BackColor = Color.FromArgb(96, 125, 139),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            saveTrainingSetBtn.FlatAppearance.BorderSize = 0;
            saveTrainingSetBtn.Click += SaveTrainingSetBtn_Click;

            loadTrainingSetBtn = new Button
            {
                Text = "ЗАГРУЗИТЬ ВЫБОРКУ",
                Location = new Point(1120, 55),
                Size = new Size(170, 35),
                BackColor = Color.FromArgb(96, 125, 139),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            loadTrainingSetBtn.FlatAppearance.BorderSize = 0;
            loadTrainingSetBtn.Click += LoadTrainingSetBtn_Click;

            // Второй ряд кнопок
            augmentDatasetBtn = new Button
            {
                Text = "АУГМЕНТИРОВАТЬ ВЫБОРКУ",
                Location = new Point(10, 95),
                Size = new Size(200, 35),
                BackColor = Color.FromArgb(255, 87, 34),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            augmentDatasetBtn.FlatAppearance.BorderSize = 0;
            augmentDatasetBtn.Click += AugmentDatasetBtn_Click;

            clearGeneratedSetBtn = new Button
            {
                Text = "ОЧИСТИТЬ СГЕНЕРИРОВАННУЮ",
                Location = new Point(220, 95),
                Size = new Size(220, 35),
                BackColor = Color.FromArgb(244, 67, 54),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            clearGeneratedSetBtn.FlatAppearance.BorderSize = 0;
            clearGeneratedSetBtn.Click += ClearGeneratedSetBtn_Click;

            exportResultsBtn = new Button
            {
                Text = "ЭКСПОРТИРОВАТЬ РЕЗУЛЬТАТЫ",
                Location = new Point(450, 95),
                Size = new Size(220, 35),
                BackColor = Color.FromArgb(63, 81, 181),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            exportResultsBtn.FlatAppearance.BorderSize = 0;
            exportResultsBtn.Click += ExportResultsBtn_Click;

            // Добавляем элементы на панель управления
            controlPanel.Controls.AddRange(new Control[]
            {
                networkLabel, networkTypeComboBox, epochsLabel, epochsTextBox,
                errorLabel, errorTextBox, trainButton, testButton, analyzePerformanceBtn,
                openDrawFormBtn, openCameraFormBtn, generateTrainingSetButton,
                generateTestSetButton, loadDatasetFromFolderBtn,
                saveTrainingSetBtn, loadTrainingSetBtn, augmentDatasetBtn,
                clearGeneratedSetBtn, exportResultsBtn
            });

            // Прогресс бар
            progressBar = new ProgressBar
            {
                Location = new Point(10, 150),
                Size = new Size(1360, 30),
                Style = ProgressBarStyle.Continuous,
                ForeColor = Color.FromArgb(76, 175, 80)
            };

            // Статус
            statusLabel = new Label
            {
                Location = new Point(10, 185),
                Size = new Size(1360, 35),
                Text = "Готов к работе. Выберите тип нейросети и начните обучение.",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Padding = new Padding(10, 0, 0, 0)
            };

            // Панель с информацией о выборках
            Panel datasetPanel = new Panel
            {
                Location = new Point(10, 230),
                Size = new Size(1360, 90),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.AliceBlue,
                Padding = new Padding(10)
            };

            trainingSetLabel = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(450, 25),
                Text = "Обучающая выборка (ручная): 0 примеров",
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft
            };

            generatedSetLabel = new Label
            {
                Location = new Point(10, 40),
                Size = new Size(450, 25),
                Text = "Сгенерированная выборка: 0 примеров",
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft
            };

            testSetLabel = new Label
            {
                Location = new Point(470, 10),
                Size = new Size(450, 25),
                Text = "Тестовая выборка: 0 примеров",
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft
            };

            totalLabel = new Label
            {
                Location = new Point(470, 40),
                Size = new Size(450, 25),
                Text = "Всего для обучения: 0 примеров",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.DarkGreen
            };

            accuracyLabel = new Label
            {
                Location = new Point(930, 10),
                Size = new Size(410, 60),
                Text = "Точность: не тестировалась",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightYellow,
                ForeColor = Color.DarkRed
            };

            datasetPanel.Controls.AddRange(new Control[]
            {
                trainingSetLabel, generatedSetLabel, testSetLabel, totalLabel, accuracyLabel
            });

            // Панель с изображением и результатами
            Panel resultPanel = new Panel
            {
                Location = new Point(10, 330),
                Size = new Size(1360, 580),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            // PictureBox для отображения сгенерированного образа
            Panel samplePanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(350, 380),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke
            };

            samplePictureBox = new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(330, 330),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            Label sampleLabel = new Label
            {
                Text = "Сгенерированный образ знака зодиака",
                Location = new Point(10, 345),
                Size = new Size(330, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            samplePanel.Controls.AddRange(new Control[] { samplePictureBox, sampleLabel });

            // ListBox для результатов
            Panel resultsPanel = new Panel
            {
                Location = new Point(370, 10),
                Size = new Size(980, 560),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            Label resultsTitle = new Label
            {
                Text = "РЕЗУЛЬТАТЫ И ЛОГИ",
                Location = new Point(10, 10),
                Size = new Size(960, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            resultsListBox = new ListBox
            {
                Location = new Point(10, 45),
                Size = new Size(960, 505),
                Font = new Font("Consolas", 9),
                ScrollAlwaysVisible = true,
                BackColor = Color.Black,
                ForeColor = Color.Lime
            };

            resultsPanel.Controls.AddRange(new Control[] { resultsTitle, resultsListBox });
            resultPanel.Controls.AddRange(new Control[] { samplePanel, resultsPanel });

            // Информационная панель внизу
            Panel infoPanel = new Panel
            {
                Location = new Point(10, 920),
                Size = new Size(1360, 30),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightYellow
            };

            Label infoLabel = new Label
            {
                Text = "💡 Подсказка: Аугментация выборки - создание дополнительных вариантов изображений с поворотами, шумом и искажениями для улучшения обучения нейросети",
                Location = new Point(10, 5),
                Size = new Size(1340, 20),
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleLeft
            };

            infoPanel.Controls.Add(infoLabel);

            // Добавляем все панели на форму
            this.Controls.AddRange(new Control[]
            {
                controlPanel, progressBar, statusLabel, datasetPanel,
                resultPanel, infoPanel
            });

            // Добавляем ToolTip для кнопок
            toolTip.SetToolTip(augmentDatasetBtn, "Аугментация выборки - создание дополнительных вариантов изображений\n" +
                                                 "с поворотами (±45°), добавлением шума, изменением яркости и контраста.\n" +
                                                 "Это помогает нейросети лучше обобщать и повышает точность распознавания.");
            toolTip.SetToolTip(generateTrainingSetButton, "Сгенерировать искусственные изображения знаков зодиака\n" +
                                                          "для обучения нейросети. Будет создано по 100 примеров каждого знака.");
            toolTip.SetToolTip(openCameraFormBtn, "Открыть окно для захвата изображений с веб-камеры.\n" +
                                                  "Нарисуйте знак зодиака на бумаге и наведите камеру.");
        }

        private void InitializeNetworkControls()
        {
            UpdateCurrentNetwork();
            UpdateDatasetLabels();
        }

        private void UpdateCurrentNetwork()
        {
            try
            {
                if (networkTypeComboBox == null || networkTypeComboBox.SelectedItem == null)
                {
                    statusLabel.Text = "Ошибка: Не выбран тип сети";
                    statusLabel.ForeColor = Color.Red;
                    return;
                }

                string networkType = networkTypeComboBox.SelectedItem.ToString();

                if (networkConstructors == null || !networkConstructors.ContainsKey(networkType))
                {
                    statusLabel.Text = $"Ошибка: Конструктор для сети '{networkType}' не найден";
                    statusLabel.ForeColor = Color.Red;
                    return;
                }

                int[] structure = new int[] { 400, 200, 100, 12 };

                try
                {
                    currentNetwork = networkConstructors[networkType](structure);
                    currentNetwork.TrainProgress += Network_TrainProgress;
                    statusLabel.Text = $"Сеть '{networkType}' инициализирована. Структура: {string.Join("-", structure)}";
                    statusLabel.ForeColor = Color.DarkBlue;
                }
                catch (Exception ex)
                {
                    statusLabel.Text = $"Ошибка создания сети: {ex.Message}";
                    statusLabel.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Ошибка при обновлении сети: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
            }
        }

        private void UpdateDatasetLabels()
        {
            try
            {
                trainingSetLabel.Text = $"Обучающая выборка (ручная): {trainingSet?.Count ?? 0} примеров";
                generatedSetLabel.Text = $"Сгенерированная выборка: {generatedSet?.Count ?? 0} примеров";
                testSetLabel.Text = $"Тестовая выборка: {testSet?.Count ?? 0} примеров";
                int total = (trainingSet?.Count ?? 0) + (generatedSet?.Count ?? 0);
                totalLabel.Text = $"Всего для обучения: {total} примеров";

                // Обновляем цвет в зависимости от количества данных
                if (total < 50)
                {
                    totalLabel.ForeColor = Color.Red;
                    totalLabel.Text += " (мало данных!)";
                }
                else if (total < 200)
                    totalLabel.ForeColor = Color.Orange;
                else if (total < 500)
                    totalLabel.ForeColor = Color.Green;
                else
                    totalLabel.ForeColor = Color.DarkGreen;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления меток датасета: {ex.Message}");
            }
        }

        private void Network_TrainProgress(double progress, double error, TimeSpan time)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Network_TrainProgress(progress, error, time)));
                return;
            }

            try
            {
                progressBar.Value = Math.Min(100, (int)(progress * 100));
                statusLabel.Text = $"Обучение... Прогресс: {progress:P2}, Ошибка: {error:F6}, Время: {time:hh\\:mm\\:ss}";
                statusLabel.ForeColor = Color.DarkGreen;

                // Обновляем прогресс в результатах каждые 10%
                if ((int)(progress * 100) % 10 == 0 && progress > 0 && progress < 1)
                {
                    resultsListBox.Items.Add($"Прогресс обучения: {progress:P2}, Ошибка: {error:F6}");
                    resultsListBox.TopIndex = resultsListBox.Items.Count - 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в Network_TrainProgress: {ex.Message}");
            }
        }

        private void NetworkTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCurrentNetwork();
        }

        private void TrainButton_Click(object sender, EventArgs e)
        {
            if (currentNetwork == null)
            {
                MessageBox.Show("Сначала выберите и инициализируйте тип нейросети!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Объединяем ручную и сгенерированную выборки
            SamplesSet combinedSet = new SamplesSet();
            if (trainingSet != null)
                foreach (Sample sample in trainingSet.samples)
                    combinedSet.AddSample(sample);
            if (generatedSet != null)
                foreach (Sample sample in generatedSet.samples)
                    combinedSet.AddSample(sample);

            if (combinedSet.Count == 0)
            {
                MessageBox.Show("Нет данных для обучения! Добавьте или сгенерируйте выборку.", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int epochs = 100;
                double acceptableError = 0.01;

                if (!int.TryParse(epochsTextBox.Text, out epochs))
                {
                    MessageBox.Show("Неверный формат числа эпох. Используется значение по умолчанию: 100");
                    epochs = 100;
                }

                if (!double.TryParse(errorTextBox.Text, out acceptableError))
                {
                    MessageBox.Show("Неверный формат целевой ошибки. Используется значение по умолчанию: 0.01");
                    acceptableError = 0.01;
                }

                trainButton.Enabled = false;
                testButton.Enabled = false;
                progressBar.Value = 0;

                resultsListBox.Items.Add("=== НАЧАЛО ОБУЧЕНИЯ ===");
                resultsListBox.Items.Add($"Тип сети: {networkTypeComboBox.SelectedItem}");
                resultsListBox.Items.Add($"Эпох: {epochs}, Целевая ошибка: {acceptableError}");
                resultsListBox.Items.Add($"Всего примеров: {combinedSet.Count} (ручных: {trainingSet?.Count ?? 0}, сгенерированных: {generatedSet?.Count ?? 0})");
                resultsListBox.Items.Add("");

                // Запускаем обучение в отдельном потоке
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        double error = currentNetwork.TrainOnDataSet(combinedSet, epochs, acceptableError, true);

                        Invoke(new Action(() =>
                        {
                            trainButton.Enabled = true;
                            testButton.Enabled = true;

                            resultsListBox.Items.Add("");
                            resultsListBox.Items.Add("=== ОБУЧЕНИЕ ЗАВЕРШЕНО ===");
                            resultsListBox.Items.Add($"Финальная ошибка: {error:F6}");
                            resultsListBox.Items.Add($"Примеров использовано: {combinedSet.Count}");
                            resultsListBox.TopIndex = resultsListBox.Items.Count - 1;

                            string result = $"Обучение завершено успешно!\n\n" +
                                          $"Финальная ошибка: {error:F6}\n" +
                                          $"Всего примеров: {combinedSet.Count}\n" +
                                          $"Ручных: {trainingSet?.Count ?? 0}, Сгенерированных: {generatedSet?.Count ?? 0}";

                            MessageBox.Show(result, "Обучение завершено",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }));
                    }
                    catch (Exception ex)
                    {
                        Invoke(new Action(() =>
                        {
                            trainButton.Enabled = true;
                            testButton.Enabled = true;
                            resultsListBox.Items.Add($"ОШИБКА при обучении: {ex.Message}");
                            resultsListBox.TopIndex = resultsListBox.Items.Count - 1;

                            MessageBox.Show($"Ошибка при обучении:\n{ex.Message}", "Ошибка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                trainButton.Enabled = true;
                testButton.Enabled = true;
            }
        }

        private void TestButton_Click(object sender, EventArgs e)
        {
            if (currentNetwork == null)
            {
                MessageBox.Show("Сначала выберите и обучите нейросеть!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (testSet == null || testSet.Count == 0)
            {
                MessageBox.Show("Тестовая выборка пуста! Сгенерируйте тестовую выборку.", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            resultsListBox.Items.Clear();
            resultsListBox.Items.Add("=== НАЧАЛО ТЕСТИРОВАНИЯ ===");
            resultsListBox.Items.Add($"Тестовая выборка: {testSet.Count} примеров");
            resultsListBox.Items.Add("");

            try
            {
                double accuracy = testSet.TestNeuralNetwork(currentNetwork);

                resultsListBox.Items.Add($"Общая точность: {accuracy:P2}");
                resultsListBox.Items.Add("");
                resultsListBox.Items.Add("Результаты по знакам зодиака:");
                resultsListBox.Items.Add(new string('=', 70));

                int[] correctPerClass = new int[12];
                int[] totalPerClass = new int[12];

                foreach (Sample sample in testSet.samples)
                {
                    var predicted = currentNetwork.Predict(sample);
                    totalPerClass[(int)sample.actualClass]++;

                    if (sample.actualClass == predicted)
                    {
                        correctPerClass[(int)sample.actualClass]++;
                    }
                }

                for (int i = 0; i < 12; i++)
                {
                    if (totalPerClass[i] > 0)
                    {
                        double classAccuracy = (double)correctPerClass[i] / totalPerClass[i];
                        string className = ((ZodiacSign)i).ToRussianString().PadRight(12);
                        string result = $"{className}: {correctPerClass[i]}/{totalPerClass[i]} ({classAccuracy:P2})";

                        // Цветовое кодирование
                        if (classAccuracy >= 0.9)
                            result += " [ОТЛИЧНО]";
                        else if (classAccuracy >= 0.7)
                            result += " [ХОРОШО]";
                        else if (classAccuracy >= 0.5)
                            result += " [УДОВЛЕТВОРИТЕЛЬНО]";
                        else
                            result += " [ПЛОХО]";

                        resultsListBox.Items.Add(result);
                    }
                }

                // Обновляем метку точности
                accuracyLabel.Text = $"ТОЧНОСТЬ: {accuracy:P2}";
                if (accuracy >= 0.9)
                {
                    accuracyLabel.BackColor = Color.LightGreen;
                    accuracyLabel.ForeColor = Color.DarkGreen;
                }
                else if (accuracy >= 0.7)
                {
                    accuracyLabel.BackColor = Color.LightYellow;
                    accuracyLabel.ForeColor = Color.Orange;
                }
                else
                {
                    accuracyLabel.BackColor = Color.LightCoral;
                    accuracyLabel.ForeColor = Color.DarkRed;
                }

                resultsListBox.Items.Add("");
                resultsListBox.Items.Add("=== ТЕСТИРОВАНИЕ ЗАВЕРШЕНО ===");
                resultsListBox.TopIndex = resultsListBox.Items.Count - 1;

                statusLabel.Text = $"Тестирование завершено. Точность: {accuracy:P2}";
                statusLabel.ForeColor = accuracy >= 0.8 ? Color.DarkGreen : Color.DarkRed;
            }
            catch (Exception ex)
            {
                resultsListBox.Items.Add($"ОШИБКА при тестировании: {ex.Message}");
                resultsListBox.TopIndex = resultsListBox.Items.Count - 1;
                MessageBox.Show($"Ошибка при тестировании:\n{ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AnalyzePerformanceBtn_Click(object sender, EventArgs e)
        {
            if (currentNetwork == null || testSet == null || testSet.Count == 0)
            {
                MessageBox.Show("Сначала обучите сеть и создайте тестовую выборку!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            resultsListBox.Items.Add("=== АНАЛИЗ ПРОИЗВОДИТЕЛЬНОСТИ ===");

            try
            {
                // Измеряем время предсказания
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                int testCount = Math.Min(100, testSet.Count);
                int correctCount = 0;

                for (int i = 0; i < testCount; i++)
                {
                    var predicted = currentNetwork.Predict(testSet[i]);
                    if (predicted == testSet[i].actualClass)
                        correctCount++;
                }

                stopwatch.Stop();
                double avgTime = stopwatch.ElapsedMilliseconds / (double)testCount;
                double accuracy = (double)correctCount / testCount;

                resultsListBox.Items.Add($"Среднее время предсказания: {avgTime:F2} мс");
                resultsListBox.Items.Add($"Тестов за секунду: {1000 / avgTime:F0}");
                resultsListBox.Items.Add($"Точность на {testCount} тестах: {accuracy:P2}");
                resultsListBox.Items.Add($"Потребление памяти: {GC.GetTotalMemory(false) / 1024 / 1024:F2} MB");
                resultsListBox.Items.Add("");

                resultsListBox.TopIndex = resultsListBox.Items.Count - 1;
            }
            catch (Exception ex)
            {
                resultsListBox.Items.Add($"ОШИБКА анализа: {ex.Message}");
                resultsListBox.TopIndex = resultsListBox.Items.Count - 1;
            }
        }

        private void GenerateTrainingSetButton_Click(object sender, EventArgs e)
        {
            generatedSet = new SamplesSet();
            int samplesPerClass = 100;

            resultsListBox.Items.Add("=== ГЕНЕРАЦИЯ ОБУЧАЮЩЕЙ ВЫБОРКИ ===");

            for (int sign = 0; sign < 12; sign++)
            {
                for (int i = 0; i < samplesPerClass; i++)
                {
                    generator.FigureCount = 12;
                    generator.generate_figure((ZodiacSign)sign);
                    var sample = generator.GenerateFigure();
                    generatedSet.AddSample(sample);
                }
                resultsListBox.Items.Add($"Сгенерировано {samplesPerClass} примеров для {((ZodiacSign)sign).ToRussianString()}");
            }

            UpdateDatasetLabels();
            if (generatedSet.Count > 0)
            {
                generator.FigureCount = 12;
                generator.generate_figure((ZodiacSign)0);
                samplePictureBox.Image = generator.GenBitmap();
            }

            statusLabel.Text = $"Сгенерирована обучающая выборка: {generatedSet.Count} примеров";
            statusLabel.ForeColor = Color.DarkBlue;

            resultsListBox.Items.Add($"ИТОГО: {generatedSet.Count} примеров");
            resultsListBox.Items.Add("");
            resultsListBox.TopIndex = resultsListBox.Items.Count - 1;
        }

        private void GenerateTestSetButton_Click(object sender, EventArgs e)
        {
            testSet = new SamplesSet();
            int samplesPerClass = 20;

            resultsListBox.Items.Add("=== ГЕНЕРАЦИЯ ТЕСТОВОЙ ВЫБОРКИ ===");

            for (int sign = 0; sign < 12; sign++)
            {
                for (int i = 0; i < samplesPerClass; i++)
                {
                    generator.FigureCount = 12;
                    generator.generate_figure((ZodiacSign)sign);
                    var sample = generator.GenerateFigure();
                    testSet.AddSample(sample);
                }
                resultsListBox.Items.Add($"Сгенерировано {samplesPerClass} тестовых примеров для {((ZodiacSign)sign).ToRussianString()}");
            }

            UpdateDatasetLabels();
            if (testSet.Count > 0)
            {
                generator.FigureCount = 12;
                generator.generate_figure((ZodiacSign)0);
                samplePictureBox.Image = generator.GenBitmap();
            }

            statusLabel.Text = $"Сгенерирована тестовая выборка: {testSet.Count} примеров";
            statusLabel.ForeColor = Color.DarkBlue;

            resultsListBox.Items.Add($"ИТОГО: {testSet.Count} тестовых примеров");
            resultsListBox.Items.Add("");
            resultsListBox.TopIndex = resultsListBox.Items.Count - 1;
        }

        private void OpenDrawFormBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (drawForm == null || drawForm.IsDisposed)
                {
                    drawForm = new CameraAndDrawForm();
                    drawForm.OnImageCaptured += DrawForm_OnImageCaptured;
                    drawForm.OnImageForRecognition += DrawForm_OnImageForRecognition;
                }
                drawForm.Show();
                drawForm.BringToFront();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы рисования: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenCameraFormBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (cameraForm == null || cameraForm.IsDisposed)
                {
                    cameraForm = new CameraCaptureForm();
                    cameraForm.OnImageCaptured += CameraForm_OnImageCaptured;
                    cameraForm.OnImageForRecognition += CameraForm_OnImageForRecognition;
                }
                cameraForm.Show();
                cameraForm.BringToFront();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы камеры: {ex.Message}\n\nУбедитесь, что веб-камера подключена и доступна.",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DrawForm_OnImageCaptured(Bitmap image, ZodiacSign sign)
        {
            try
            {
                string datasetPath = Path.Combine(Application.StartupPath, "Dataset");
                string classFolder = Path.Combine(datasetPath, sign.ToRussianString());

                if (!Directory.Exists(classFolder))
                    Directory.CreateDirectory(classFolder);

                // Сохраняем оригинальное изображение
                string filename = $"{sign.ToRussianString()}_{DateTime.Now:yyyyMMdd_HHmmss}_original.png";
                string fullPath = Path.Combine(classFolder, filename);
                image.Save(fullPath, ImageFormat.Png);

                // Создаем аугментированные версии
                List<Bitmap> augmentedImages = CreateAugmentedImages(image, 5); // 5 аугментированных версий

                // Добавляем в выборку оригинал и все аугментированные версии
                double[] originalInput = ConvertImageToVector(image);
                trainingSet.AddSample(new Sample(originalInput, 12, sign));

                foreach (var augImage in augmentedImages)
                {
                    double[] augInput = ConvertImageToVector(augImage);
                    trainingSet.AddSample(new Sample(augInput, 12, sign));

                    // Сохраняем аугментированную версию
                    string augFilename = $"{sign.ToRussianString()}_{DateTime.Now:yyyyMMdd_HHmmss}_aug{Guid.NewGuid().ToString().Substring(0, 4)}.png";
                    string augPath = Path.Combine(classFolder, augFilename);
                    augImage.Save(augPath, ImageFormat.Png);
                    augImage.Dispose();
                }

                Invoke(new Action(() =>
                {
                    UpdateDatasetLabels();
                    samplePictureBox.Image = image;
                    statusLabel.Text = $"Добавлен образ '{sign.ToRussianString()}'. Всего ручных образов: {trainingSet.Count}";
                    statusLabel.ForeColor = Color.DarkGreen;

                    resultsListBox.Items.Add($"Добавлен образ: {sign.ToRussianString()} (+{augmentedImages.Count} аугментированных)");
                    resultsListBox.Items.Add($"Сохранено в: {classFolder}");
                    resultsListBox.TopIndex = resultsListBox.Items.Count - 1;
                    if (resultsListBox.Items.Count > 100) resultsListBox.Items.RemoveAt(0);
                }));
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show($"Ошибка при сохранении изображения: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
        }

        private List<Bitmap> CreateAugmentedImages(Bitmap original, int count)
        {
            List<Bitmap> augmented = new List<Bitmap>();
            Random rand = new Random();

            for (int i = 0; i < count; i++)
            {
                Bitmap augImage = new Bitmap(original);

                // Случайный поворот от -45 до +45 градусов
                float angle = (float)(rand.NextDouble() * 90 - 45);
                augImage = RotateImage(augImage, angle);

                // Случайное добавление шума
                augImage = AddNoise(augImage, rand.Next(5, 20));

                // Случайное изменение яркости/контраста
                augImage = AdjustBrightnessContrast(augImage,
                    (float)(rand.NextDouble() * 0.4 + 0.8), // brightness
                    (float)(rand.NextDouble() * 0.4 + 0.8)); // contrast

                augmented.Add(augImage);
            }

            return augmented;
        }

        private Bitmap RotateImage(Bitmap original, float angle)
        {
            Bitmap rotated = new Bitmap(original.Width, original.Height);
            using (Graphics g = Graphics.FromImage(rotated))
            {
                g.TranslateTransform(original.Width / 2, original.Height / 2);
                g.RotateTransform(angle);
                g.TranslateTransform(-original.Width / 2, -original.Height / 2);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
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
                if (rand.NextDouble() > 0.5)
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

        private void DrawForm_OnImageForRecognition(Bitmap image)
        {
            if (currentNetwork == null)
            {
                MessageBox.Show("Сначала обучите нейросеть!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                double[] input = ConvertImageToVector(image);
                Sample sample = new Sample(input, 12, ZodiacSign.Undef);
                ZodiacSign predicted = currentNetwork.Predict(sample);

                Invoke(new Action(() =>
                {
                    drawForm.UpdateRecognitionResult(predicted.ToRussianString());
                    resultsListBox.Items.Add($"Распознано (рисование): {predicted.ToRussianString()}");
                    resultsListBox.TopIndex = resultsListBox.Items.Count - 1;
                    if (resultsListBox.Items.Count > 100) resultsListBox.Items.RemoveAt(0);
                }));
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    resultsListBox.Items.Add($"Ошибка распознавания: {ex.Message}");
                    resultsListBox.TopIndex = resultsListBox.Items.Count - 1;
                }));
            }
        }

        private void CameraForm_OnImageCaptured(Bitmap image, ZodiacSign sign)
        {
            DrawForm_OnImageCaptured(image, sign);
        }

        private void CameraForm_OnImageForRecognition(Bitmap image)
        {
            if (currentNetwork == null)
            {
                MessageBox.Show("Сначала обучите нейросеть!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                double[] input = ConvertImageToVector(image);
                Sample sample = new Sample(input, 12, ZodiacSign.Undef);
                ZodiacSign predicted = currentNetwork.Predict(sample);

                Invoke(new Action(() =>
                {
                    cameraForm.UpdateRecognitionResult(predicted.ToRussianString());
                    resultsListBox.Items.Add($"Распознано (камера): {predicted.ToRussianString()}");
                    resultsListBox.TopIndex = resultsListBox.Items.Count - 1;
                    if (resultsListBox.Items.Count > 100) resultsListBox.Items.RemoveAt(0);
                }));
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    resultsListBox.Items.Add($"Ошибка распознавания с камеры: {ex.Message}");
                    resultsListBox.TopIndex = resultsListBox.Items.Count - 1;
                }));
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

        private void SaveTrainingSetBtn_Click(object sender, EventArgs e)
        {
            if (trainingSet == null || trainingSet.Count == 0)
            {
                MessageBox.Show("Нет данных для сохранения!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Training Set (*.trn)|*.trn|All files (*.*)|*.*",
                Title = "Сохранить обучающую выборку",
                InitialDirectory = Application.StartupPath,
                FileName = $"zodiac_dataset_{DateTime.Now:yyyyMMdd_HHmmss}.trn",
                DefaultExt = "trn"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(saveDialog.FileName))
                    {
                        writer.WriteLine(trainingSet.Count);
                        foreach (Sample sample in trainingSet.samples)
                        {
                            writer.Write($"{(int)sample.actualClass}");
                            for (int i = 0; i < sample.input.Length; i++)
                            {
                                writer.Write($" {sample.input[i]:F6}");
                            }
                            writer.WriteLine();
                        }
                    }

                    resultsListBox.Items.Add($"Сохранена выборка: {trainingSet.Count} примеров");
                    resultsListBox.Items.Add($"Файл: {Path.GetFileName(saveDialog.FileName)}");
                    resultsListBox.TopIndex = resultsListBox.Items.Count - 1;

                    MessageBox.Show($"Ручная выборка сохранена!\n\n" +
                                  $"Примеров: {trainingSet.Count}\n" +
                                  $"Файл: {Path.GetFileName(saveDialog.FileName)}\n" +
                                  $"Путь: {saveDialog.FileName}",
                                  "Сохранение завершено",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении:\n{ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadTrainingSetBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog
            {
                Filter = "Training Set (*.trn)|*.trn|All files (*.*)|*.*",
                Title = "Загрузить обучающую выборку",
                InitialDirectory = Application.StartupPath,
                Multiselect = false
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    trainingSet = new SamplesSet();
                    using (StreamReader reader = new StreamReader(openDialog.FileName))
                    {
                        int count = int.Parse(reader.ReadLine());
                        for (int i = 0; i < count; i++)
                        {
                            string line = reader.ReadLine();
                            if (string.IsNullOrEmpty(line)) continue;

                            string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            if (parts.Length < 2) continue;

                            int classId = int.Parse(parts[0]);
                            double[] input = new double[parts.Length - 1];

                            for (int j = 1; j < parts.Length; j++)
                            {
                                input[j - 1] = double.Parse(parts[j]);
                            }

                            Sample sample = new Sample(input, 12, (ZodiacSign)classId);
                            trainingSet.AddSample(sample);
                        }
                    }

                    UpdateDatasetLabels();

                    resultsListBox.Items.Add($"Загружена выборка: {trainingSet.Count} примеров");
                    resultsListBox.Items.Add($"Файл: {Path.GetFileName(openDialog.FileName)}");
                    resultsListBox.TopIndex = resultsListBox.Items.Count - 1;

                    MessageBox.Show($"Ручная выборка загружена успешно!\n\n" +
                                  $"Примеров: {trainingSet.Count}\n" +
                                  $"Файл: {Path.GetFileName(openDialog.FileName)}",
                                  "Загрузка завершена",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке:\n{ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearGeneratedSetBtn_Click(object sender, EventArgs e)
        {
            if (generatedSet == null || generatedSet.Count == 0)
            {
                MessageBox.Show("Сгенерированная выборка уже пуста!", "Информация",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Вы действительно хотите очистить сгенерированную выборку?\n\n" +
                              $"Будет удалено {generatedSet.Count} примеров.",
                              "Подтверждение очистки",
                              MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                generatedSet = new SamplesSet();
                UpdateDatasetLabels();
                statusLabel.Text = "Сгенерированная выборка очищена";
                resultsListBox.Items.Add("Сгенерированная выборка очищена");
                resultsListBox.TopIndex = resultsListBox.Items.Count - 1;
            }
        }

        private void LoadDatasetFromFolderBtn_Click(object sender, EventArgs e)
        {
            string datasetPath = Path.Combine(Application.StartupPath, "Dataset");

            if (!Directory.Exists(datasetPath))
            {
                if (MessageBox.Show("Папка Dataset не найдена! Создать её?", "Папка не найдена",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    CreateDatasetFolder();
                }
                return;
            }

            int loadedCount = 0;
            trainingSet = new SamplesSet();

            resultsListBox.Items.Add("=== ЗАГРУЗКА ИЗ ПАПКИ DATASET ===");

            string[] classFolders = Directory.GetDirectories(datasetPath);
            if (classFolders.Length == 0)
            {
                MessageBox.Show("В папке Dataset нет подпапок с изображениями!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (string folder in classFolders)
            {
                string className = Path.GetFileName(folder);
                ZodiacSign sign = ZodiacSignHelper.FromRussianString(className);

                if (sign == ZodiacSign.Undef)
                {
                    resultsListBox.Items.Add($"Пропущена папка: {className} (не распознан знак зодиака)");
                    continue;
                }

                string[] imageFiles = Directory.GetFiles(folder, "*.png");
                string[] imageFilesJpg = Directory.GetFiles(folder, "*.jpg");
                string[] imageFilesBmp = Directory.GetFiles(folder, "*.bmp");

                List<string> allImageFiles = new List<string>();
                allImageFiles.AddRange(imageFiles);
                allImageFiles.AddRange(imageFilesJpg);
                allImageFiles.AddRange(imageFilesBmp);

                resultsListBox.Items.Add($"{className}: {allImageFiles.Count} изображений");

                foreach (string imageFile in allImageFiles)
                {
                    try
                    {
                        using (Bitmap image = new Bitmap(imageFile))
                        {
                            // Конвертируем в 200x200 если нужно
                            Bitmap processed = image;
                            if (image.Width != 200 || image.Height != 200)
                            {
                                processed = new Bitmap(200, 200);
                                using (Graphics g = Graphics.FromImage(processed))
                                {
                                    g.Clear(Color.White);
                                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                    g.DrawImage(image, 0, 0, 200, 200);
                                }
                            }

                            double[] input = ConvertImageToVector(processed);
                            Sample sample = new Sample(input, 12, sign);
                            trainingSet.AddSample(sample);
                            loadedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        resultsListBox.Items.Add($"Ошибка загрузки {Path.GetFileName(imageFile)}: {ex.Message}");
                    }
                }
            }

            UpdateDatasetLabels();
            statusLabel.Text = $"Загружено {loadedCount} изображений из папки Dataset";
            resultsListBox.Items.Add($"ИТОГО загружено: {loadedCount} изображений из {classFolders.Length} классов");
            resultsListBox.Items.Add("");
            resultsListBox.TopIndex = resultsListBox.Items.Count - 1;
        }

        private void AugmentDatasetBtn_Click(object sender, EventArgs e)
        {
            if (trainingSet == null || trainingSet.Count == 0)
            {
                MessageBox.Show("Нет данных для аугментации! Сначала загрузите или создайте выборку.",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int originalCount = trainingSet.Count;
            var augmentedSet = new SamplesSet();

            resultsListBox.Items.Add("=== АУГМЕНТАЦИЯ ВЫБОРКИ ===");
            resultsListBox.Items.Add($"Исходное количество: {originalCount} примеров");
            resultsListBox.Items.Add("Создание аугментированных версий...");

            int augmentationFactor = 3; // Создаем 3 аугментированные версии для каждого образца

            foreach (Sample sample in trainingSet.samples)
            {
                // Добавляем оригинальный образ
                augmentedSet.AddSample(sample);

                // Создаем аугментированные версии
                for (int i = 0; i < augmentationFactor; i++)
                {
                    double[] augmentedInput = ApplyAugmentation(sample.input);
                    augmentedSet.AddSample(new Sample(augmentedInput, 12, sample.actualClass));
                }
            }

            trainingSet = augmentedSet;
            UpdateDatasetLabels();
            statusLabel.Text = $"Датасет аугментирован: {originalCount} → {trainingSet.Count} примеров";

            resultsListBox.Items.Add($"После аугментации: {trainingSet.Count} примеров");
            resultsListBox.Items.Add($"Коэффициент аугментации: {(double)trainingSet.Count / originalCount:F1}x");
            resultsListBox.Items.Add("✓ Аугментация завершена успешно");
            resultsListBox.Items.Add("");
            resultsListBox.TopIndex = resultsListBox.Items.Count - 1;

            MessageBox.Show($"Аугментация выборки завершена успешно!\n\n" +
                          $"Исходное количество: {originalCount} примеров\n" +
                          $"После аугментации: {trainingSet.Count} примеров\n" +
                          $"Увеличение в {(double)trainingSet.Count / originalCount:F1} раз",
                          "Аугментация завершена",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private double[] ApplyAugmentation(double[] original)
        {
            double[] augmented = new double[original.Length];
            Random rand = new Random();

            for (int i = 0; i < original.Length; i++)
            {
                // Добавляем случайный шум и небольшие искажения
                double noise = (rand.NextDouble() - 0.5) * 0.25; // Увеличили шум до 25%
                double shift = (rand.NextDouble() - 0.5) * 0.15; // Увеличили смещение до 15%
                augmented[i] = Math.Max(0, Math.Min(1, original[i] + noise + shift));
            }

            return augmented;
        }

        private void ExportResultsBtn_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|CSV file (*.csv)|*.csv|All files (*.*)|*.*",
                Title = "Экспортировать результаты",
                InitialDirectory = Application.StartupPath,
                FileName = $"results_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                DefaultExt = "txt"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(saveDialog.FileName))
                    {
                        writer.WriteLine("=== РЕЗУЛЬТАТЫ РАБОТЫ НЕЙРОСЕТИ ДЛЯ РАСПОЗНАВАНИЯ ЗНАКОВ ЗОДИАКА ===");
                        writer.WriteLine($"Дата экспорта: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        writer.WriteLine();
                        writer.WriteLine("ИНФОРМАЦИЯ О ВЫБОРКАХ:");
                        writer.WriteLine($"Обучающая выборка (ручная): {trainingSet?.Count ?? 0} примеров");
                        writer.WriteLine($"Сгенерированная выборка: {generatedSet?.Count ?? 0} примеров");
                        writer.WriteLine($"Тестовая выборка: {testSet?.Count ?? 0} примеров");
                        writer.WriteLine($"Всего для обучения: {(trainingSet?.Count ?? 0) + (generatedSet?.Count ?? 0)} примеров");
                        writer.WriteLine();
                        writer.WriteLine("ЛОГ РАБОТЫ:");
                        writer.WriteLine(new string('=', 80));

                        foreach (var item in resultsListBox.Items)
                        {
                            writer.WriteLine(item.ToString());
                        }
                    }

                    MessageBox.Show($"Результаты успешно экспортированы в файл:\n{saveDialog.FileName}",
                                  "Экспорт завершен", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте:\n{ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CreateDatasetFolder()
        {
            string datasetPath = Path.Combine(Application.StartupPath, "Dataset");
            if (!Directory.Exists(datasetPath))
            {
                Directory.CreateDirectory(datasetPath);
                // Создаем подпапки для каждого знака
                foreach (string signName in ZodiacSignHelper.GetAllRussianNames())
                {
                    Directory.CreateDirectory(Path.Combine(datasetPath, signName));
                }
                resultsListBox.Items.Add("✓ Создана папка Dataset со структурой подпапок для знаков зодиака");
                resultsListBox.Items.Add("");
                resultsListBox.TopIndex = resultsListBox.Items.Count - 1;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            try
            {
                if (drawForm != null && !drawForm.IsDisposed)
                    drawForm.Close();

                if (cameraForm != null && !cameraForm.IsDisposed)
                    cameraForm.Close();
            }
            catch { }
        }
    }
}