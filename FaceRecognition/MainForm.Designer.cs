namespace FaceRecognition
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            picView = new PictureBox();
            grpSource = new GroupBox();
            btnCameraStart = new Button();
            btnCameraStop = new Button();
            btnSelectImage = new Button();
            grpTrain = new GroupBox();
            lblLabel = new Label();
            txtLabel = new TextBox();
            btnAddSample = new Button();
            lstLabels = new ListBox();
            btnDeleteLabel = new Button();
            btnTrain = new Button();
            lblTrainInfo = new Label();
            grpResult = new GroupBox();
            lblResult = new Label();
            lblConfidence = new Label();
            lblStatus = new Label();
            lblPrivacy = new Label();
            openFileDialog = new OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)picView).BeginInit();
            SuspendLayout();
            //
            // picView
            //
            picView.Name = "picView";
            picView.BackColor = Color.Black;
            picView.SizeMode = PictureBoxSizeMode.Zoom;
            picView.Location = new Point(12, 12);
            picView.Size = new Size(640, 480);
            //
            // grpSource
            //
            grpSource.Name = "grpSource";
            grpSource.Text = "Görüntü kaynağı";
            grpSource.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            grpSource.Location = new Point(668, 8);
            grpSource.Size = new Size(320, 108);
            //
            // btnCameraStart
            //
            btnCameraStart.Name = "btnCameraStart";
            btnCameraStart.Text = "📷 Kamera Başlat";
            btnCameraStart.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnCameraStart.BackColor = Color.FromArgb(46, 125, 50);
            btnCameraStart.ForeColor = Color.White;
            btnCameraStart.FlatStyle = FlatStyle.Flat;
            btnCameraStart.Location = new Point(12, 26);
            btnCameraStart.Size = new Size(144, 34);
            btnCameraStart.Click += BtnCameraStart_Click;
            grpSource.Controls.Add(btnCameraStart);
            //
            // btnCameraStop
            //
            btnCameraStop.Name = "btnCameraStop";
            btnCameraStop.Text = "■ Kamera Durdur";
            btnCameraStop.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnCameraStop.BackColor = Color.FromArgb(198, 40, 40);
            btnCameraStop.ForeColor = Color.White;
            btnCameraStop.FlatStyle = FlatStyle.Flat;
            btnCameraStop.Location = new Point(164, 26);
            btnCameraStop.Size = new Size(144, 34);
            btnCameraStop.Click += BtnCameraStop_Click;
            grpSource.Controls.Add(btnCameraStop);
            //
            // btnSelectImage
            //
            btnSelectImage.Name = "btnSelectImage";
            btnSelectImage.Text = "🖼 Görüntü Seç (dosyadan)";
            btnSelectImage.Font = new Font("Segoe UI", 9F);
            btnSelectImage.Location = new Point(12, 66);
            btnSelectImage.Size = new Size(296, 32);
            btnSelectImage.Click += BtnSelectImage_Click;
            grpSource.Controls.Add(btnSelectImage);
            //
            // grpTrain
            //
            grpTrain.Name = "grpTrain";
            grpTrain.Text = "Örnekler ve eğitim";
            grpTrain.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            grpTrain.Location = new Point(668, 122);
            grpTrain.Size = new Size(320, 270);
            //
            // lblLabel
            //
            lblLabel.Name = "lblLabel";
            lblLabel.Text = "Kişi adı:";
            lblLabel.Font = new Font("Segoe UI", 9F);
            lblLabel.AutoSize = true;
            lblLabel.Location = new Point(12, 30);
            grpTrain.Controls.Add(lblLabel);
            //
            // txtLabel
            //
            txtLabel.Name = "txtLabel";
            txtLabel.Font = new Font("Segoe UI", 9F);
            txtLabel.Location = new Point(76, 26);
            txtLabel.Size = new Size(232, 23);
            grpTrain.Controls.Add(txtLabel);
            //
            // btnAddSample
            //
            btnAddSample.Name = "btnAddSample";
            btnAddSample.Text = "➕ Görüntüdeki Yüzü Örnek Olarak Ekle";
            btnAddSample.Font = new Font("Segoe UI", 9F);
            btnAddSample.Location = new Point(12, 56);
            btnAddSample.Size = new Size(296, 32);
            btnAddSample.Click += BtnAddSample_Click;
            grpTrain.Controls.Add(btnAddSample);
            //
            // lstLabels
            //
            lstLabels.Name = "lstLabels";
            lstLabels.Font = new Font("Consolas", 9.75F);
            lstLabels.IntegralHeight = false;
            lstLabels.Location = new Point(12, 94);
            lstLabels.Size = new Size(296, 92);
            grpTrain.Controls.Add(lstLabels);
            //
            // btnDeleteLabel
            //
            btnDeleteLabel.Name = "btnDeleteLabel";
            btnDeleteLabel.Text = "Seçili Kişiyi Sil";
            btnDeleteLabel.Font = new Font("Segoe UI", 9F);
            btnDeleteLabel.Location = new Point(12, 192);
            btnDeleteLabel.Size = new Size(144, 30);
            btnDeleteLabel.Click += BtnDeleteLabel_Click;
            grpTrain.Controls.Add(btnDeleteLabel);
            //
            // btnTrain
            //
            btnTrain.Name = "btnTrain";
            btnTrain.Text = "🧠 Modeli Eğit";
            btnTrain.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnTrain.BackColor = Color.FromArgb(21, 101, 192);
            btnTrain.ForeColor = Color.White;
            btnTrain.FlatStyle = FlatStyle.Flat;
            btnTrain.Location = new Point(164, 192);
            btnTrain.Size = new Size(144, 30);
            btnTrain.Click += BtnTrain_Click;
            grpTrain.Controls.Add(btnTrain);
            //
            // lblTrainInfo
            //
            lblTrainInfo.Name = "lblTrainInfo";
            lblTrainInfo.Font = new Font("Segoe UI", 9F);
            lblTrainInfo.ForeColor = Color.DimGray;
            lblTrainInfo.Location = new Point(12, 228);
            lblTrainInfo.Size = new Size(296, 36);
            grpTrain.Controls.Add(lblTrainInfo);
            //
            // grpResult
            //
            grpResult.Name = "grpResult";
            grpResult.Text = "Tanıma sonucu";
            grpResult.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            grpResult.Location = new Point(668, 398);
            grpResult.Size = new Size(320, 94);
            //
            // lblResult
            //
            lblResult.Name = "lblResult";
            lblResult.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblResult.Location = new Point(12, 24);
            lblResult.Size = new Size(296, 34);
            lblResult.Text = "-";
            grpResult.Controls.Add(lblResult);
            //
            // lblConfidence
            //
            lblConfidence.Name = "lblConfidence";
            lblConfidence.Font = new Font("Segoe UI", 9F);
            lblConfidence.ForeColor = Color.DimGray;
            lblConfidence.Location = new Point(12, 62);
            lblConfidence.Size = new Size(296, 22);
            grpResult.Controls.Add(lblConfidence);
            //
            // lblStatus
            //
            lblStatus.Name = "lblStatus";
            lblStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblStatus.ForeColor = Color.White;
            lblStatus.BackColor = Color.Gray;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.Location = new Point(12, 500);
            lblStatus.Size = new Size(976, 30);
            //
            // lblPrivacy
            //
            lblPrivacy.Name = "lblPrivacy";
            lblPrivacy.Font = new Font("Segoe UI", 8.25F);
            lblPrivacy.ForeColor = Color.DimGray;
            lblPrivacy.Location = new Point(12, 534);
            lblPrivacy.Size = new Size(976, 18);
            lblPrivacy.Text = "Ders amaçlı demo: yalnızca gönüllülerin/kendi yüzünüzün örnekleri kullanılır, görüntüler yalnızca bu bilgisayarda (FaceData klasörü) saklanır.";
            //
            // openFileDialog
            //
            openFileDialog.Filter = "Resim dosyaları (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
            openFileDialog.Title = "Yüz içeren bir resim seçin";
            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 560);
            Controls.Add(picView);
            Controls.Add(grpSource);
            Controls.Add(grpTrain);
            Controls.Add(grpResult);
            Controls.Add(lblStatus);
            Controls.Add(lblPrivacy);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "OpenCV ile Yüz Algılama ve Tanıma - YMH219";
            FormClosing += MainForm_FormClosing;
            ((System.ComponentModel.ISupportInitialize)picView).EndInit();
            ResumeLayout(false);
        }

        private PictureBox picView;
        private GroupBox grpSource;
        private Button btnCameraStart;
        private Button btnCameraStop;
        private Button btnSelectImage;
        private GroupBox grpTrain;
        private Label lblLabel;
        private TextBox txtLabel;
        private Button btnAddSample;
        private ListBox lstLabels;
        private Button btnDeleteLabel;
        private Button btnTrain;
        private Label lblTrainInfo;
        private GroupBox grpResult;
        private Label lblResult;
        private Label lblConfidence;
        private Label lblStatus;
        private Label lblPrivacy;
        private OpenFileDialog openFileDialog;
    }
}
