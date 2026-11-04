using OpenCvSharp.Extensions;
using Mat = OpenCvSharp.Mat;
using Rect = OpenCvSharp.Rect;
using Cv2 = OpenCvSharp.Cv2;
using Scalar = OpenCvSharp.Scalar;
using CvPoint = OpenCvSharp.Point;

namespace FaceRecognition
{
    /// <summary>
    /// Ana pencere. Kamera veya dosyadan gelen görüntüde yüzleri bulur, dikdörtgenle işaretler
    /// ve model eğitildiyse kim olduğunu tahmin eder.
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly CameraService camera = new CameraService();
        private readonly FaceRecognizerService recognizer = new FaceRecognizerService();
        private readonly FaceDataset dataset = new FaceDataset(Path.Combine(AppContext.BaseDirectory, "FaceData"));
        private readonly object processLock = new object();   // kamera iş parçacığı ile arayüz aynı anda modeli kullanmasın
        private FaceDetector detector;
        private Mat currentFrame;                              // örnek eklemek için son görüntü

        public MainForm()
        {
            InitializeComponent();
            camera.FrameReady += Camera_FrameReady;
            camera.Error += (s, msg) => RunOnUi(() =>
            {
                MessageBox.Show(msg, "Kamera hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                UpdateButtons();
            });

            try
            {
                detector = new FaceDetector(Path.Combine(AppContext.BaseDirectory, "Data", "haarcascade_frontalface_default.xml"));
                SetStatus("Hazır. Kamerayı başlatın veya bir görüntü seçin.", Color.FromArgb(96, 96, 96));
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Model dosyası eksik", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Yüz algılama modeli bulunamadı; algılama yapılamaz.", Color.FromArgb(198, 40, 40));
            }

            RefreshLabels();
            UpdateButtons();
        }

        // ---------- Görüntü kaynağı ----------

        private void BtnCameraStart_Click(object sender, EventArgs e)
        {
            try
            {
                camera.Start();
                SetStatus("📷 Kamera açık — yüzler algılanıyor.", Color.FromArgb(46, 125, 50));
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Kamera açılamadı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            UpdateButtons();
        }

        private void BtnCameraStop_Click(object sender, EventArgs e)
        {
            camera.Stop();
            SetStatus("Kamera durduruldu.", Color.FromArgb(96, 96, 96));
            UpdateButtons();
        }

        private void BtnSelectImage_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                return;
            camera.Stop();
            Mat image = Cv2.ImRead(openFileDialog.FileName);
            if (image.Empty())
            {
                image.Dispose();
                MessageBox.Show("Seçilen dosya resim olarak okunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var (bitmap, result) = ProcessFrame(image);
            ShowFrame(bitmap, result);
            SetStatus($"🖼 Görüntü: {Path.GetFileName(openFileDialog.FileName)} — {result.FaceCount} yüz bulundu.", Color.FromArgb(21, 101, 192));
            UpdateButtons();
        }

        /// <summary>Kameradan yeni kare geldiğinde (arka plan iş parçacığında) çalışır.</summary>
        private void Camera_FrameReady(object sender, Mat frame)
        {
            var (bitmap, result) = ProcessFrame(frame);
            RunOnUi(() => ShowFrame(bitmap, result));
        }

        // ---------- Algılama ve tanıma ----------

        /// <summary>Bir karede yüzleri bulur, işaretler, tanır. Karenin sahipliğini alır.</summary>
        private (Bitmap Bitmap, FrameResult Result) ProcessFrame(Mat frame)
        {
            var result = new FrameResult();
            lock (processLock)
            {
                // Örnek eklemek için karenin işaretlenmemiş kopyasını sakla
                currentFrame?.Dispose();
                currentFrame = frame.Clone();

                if (detector != null)
                {
                    Rect[] faces = detector.Detect(frame);
                    result.FaceCount = faces.Length;
                    for (int i = 0; i < faces.Length; i++)
                    {
                        Rect face = faces[i];
                        string text = "Yuz";
                        Scalar color = new Scalar(0, 200, 255);   // turuncu: tanınmadı / model yok

                        if (recognizer.IsTrained)
                        {
                            using Mat prepared = FaceDetector.PrepareFace(frame, face);
                            Prediction p = recognizer.Predict(prepared);
                            text = p.IsKnown ? $"{p.Label} %{p.ConfidencePercent}" : "Bilinmiyor";
                            color = p.IsKnown ? new Scalar(80, 200, 0) : new Scalar(0, 0, 230);
                            if (i == 0) result.Best = p;   // en büyük yüz
                        }

                        Cv2.Rectangle(frame, face, color, 2);
                        Cv2.PutText(frame, text, new CvPoint(face.X, Math.Max(face.Y - 8, 15)),
                            OpenCvSharp.HersheyFonts.HersheySimplex, 0.7, color, 2);
                    }
                }
            }
            Bitmap bmp = BitmapConverter.ToBitmap(frame);
            frame.Dispose();
            return (bmp, result);
        }

        private void ShowFrame(Bitmap bitmap, FrameResult result)
        {
            if (IsDisposed) { bitmap.Dispose(); return; }
            Image old = picView.Image;
            picView.Image = bitmap;
            old?.Dispose();

            if (result.FaceCount == 0)
            {
                lblResult.Text = "Yüz yok";
                lblResult.ForeColor = Color.Gray;
                lblConfidence.Text = "";
            }
            else if (!recognizer.IsTrained)
            {
                lblResult.Text = $"{result.FaceCount} yüz algılandı";
                lblResult.ForeColor = Color.DarkOrange;
                lblConfidence.Text = "Tanıma için önce örnek ekleyip modeli eğitin.";
            }
            else if (result.Best != null)
            {
                lblResult.Text = result.Best.Label;
                lblResult.ForeColor = result.Best.IsKnown ? Color.FromArgb(46, 125, 50) : Color.FromArgb(198, 40, 40);
                lblConfidence.Text = $"Güven: %{result.Best.ConfidencePercent}   (LBPH uzaklık: {result.Best.Distance:0.0})";
            }
        }

        // ---------- Örnekler ve eğitim ----------

        private void BtnAddSample_Click(object sender, EventArgs e)
        {
            string label = txtLabel.Text.Trim();
            if (label.Length == 0)
            {
                MessageBox.Show("Önce kişi adını (etiket) yazın.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            lock (processLock)
            {
                if (currentFrame == null || detector == null)
                {
                    MessageBox.Show("Önce kamerayı başlatın veya bir görüntü seçin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                Rect[] faces = detector.Detect(currentFrame);
                if (faces.Length == 0)
                {
                    MessageBox.Show("Görüntüde yüz bulunamadı. Kameraya düz bakın veya başka bir resim seçin.", "Yüz yok",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                using Mat prepared = FaceDetector.PrepareFace(currentFrame, faces[0]);
                dataset.AddSample(label, prepared);
            }
            RefreshLabels();
            SetStatus($"'{label}' için yüz örneği eklendi. Farklı açılardan birkaç örnek ekleyip modeli eğitin.", Color.FromArgb(21, 101, 192));
        }

        private void BtnTrain_Click(object sender, EventArgs e)
        {
            try
            {
                int count;
                Cursor = Cursors.WaitCursor;
                lock (processLock) count = recognizer.Train(dataset);
                SetStatus($"🧠 Model {recognizer.LabelCount} kişi ve {count} örnekle eğitildi. Tanıma aktif.", Color.FromArgb(46, 125, 50));
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is OpenCvSharp.OpenCVException)
            {
                MessageBox.Show(ex.Message, "Eğitim yapılamadı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
            RefreshLabels();
        }

        private void BtnDeleteLabel_Click(object sender, EventArgs e)
        {
            if (lstLabels.SelectedItem is not string item) return;
            string label = item[..item.LastIndexOf(" (")];
            if (MessageBox.Show($"'{label}' etiketinin tüm örnekleri silinsin mi?", "Onay",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            dataset.DeleteLabel(label);
            RefreshLabels();
        }

        // ---------- Yardımcılar ----------

        private void RefreshLabels()
        {
            lstLabels.Items.Clear();
            foreach (var pair in dataset.GetLabelCounts().OrderBy(p => p.Key))
                lstLabels.Items.Add($"{pair.Key} ({pair.Value} örnek)");
            lblTrainInfo.Text = recognizer.IsTrained
                ? $"Model eğitildi: {recognizer.LabelCount} kişi."
                : "Model henüz eğitilmedi (en az 2 kişi gerekir).";
        }

        private void UpdateButtons()
        {
            bool running = camera.IsRunning;
            btnCameraStart.Enabled = !running;
            btnCameraStop.Enabled = running;
            btnCameraStart.BackColor = running ? Color.Silver : Color.FromArgb(46, 125, 50);
            btnCameraStop.BackColor = running ? Color.FromArgb(198, 40, 40) : Color.Silver;
        }

        private void SetStatus(string text, Color color)
        {
            lblStatus.Text = "  " + text;
            lblStatus.BackColor = color;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            camera.FrameReady -= Camera_FrameReady;
            camera.Dispose();
            recognizer.Dispose();
            detector?.Dispose();
            currentFrame?.Dispose();
        }

        private void RunOnUi(Action action)
        {
            if (IsDisposed || Disposing) return;
            try
            {
                if (InvokeRequired) BeginInvoke(action);
                else action();
            }
            catch (InvalidOperationException) { }   // form kapanırken
        }

        /// <summary>Bir karenin işlenme sonucu.</summary>
        private class FrameResult
        {
            public int FaceCount { get; set; }
            public Prediction Best { get; set; }
        }
    }
}
