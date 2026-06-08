namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    partial class FormGrabadora
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pnlMain = new Panel();
            pnlButtons = new Panel();
            btnOpenRecordings = new Button();
            btnStopRecording = new Button();
            btnStartRecording = new Button();
            pnlPreview = new GroupBox();
            previewBox = new PictureBox();
            pnlStatus = new GroupBox();
            lblRecordingTime = new Label();
            lblRecordingStatus = new Label();
            pnlDevices = new GroupBox();
            lblAudioStatus = new Label();
            cmbAudioDevices = new ComboBox();
            lblDevice = new Label();
            pnlMode = new GroupBox();
            pictureModeIcon = new PictureBox();
            btnToggleCameraMode = new Button();
            btnToggleMode = new Button();
            lblModeStatus = new Label();
            lblTitle = new Label();
            pnlMain.SuspendLayout();
            pnlButtons.SuspendLayout();
            pnlPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)previewBox).BeginInit();
            pnlStatus.SuspendLayout();
            pnlDevices.SuspendLayout();
            pnlMode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureModeIcon).BeginInit();
            SuspendLayout();
            // 
            // pnlMain
            // 
            pnlMain.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlMain.Controls.Add(pnlButtons);
            pnlMain.Controls.Add(pnlPreview);
            pnlMain.Controls.Add(pnlStatus);
            pnlMain.Controls.Add(pnlDevices);
            pnlMain.Controls.Add(pnlMode);
            pnlMain.Controls.Add(lblTitle);
            pnlMain.Location = new Point(0, -1);
            pnlMain.Name = "pnlMain";
            pnlMain.Padding = new Padding(15);
            pnlMain.Size = new Size(1033, 648);
            pnlMain.TabIndex = 0;
            // 
            // pnlButtons
            // 
            pnlButtons.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlButtons.Controls.Add(btnOpenRecordings);
            pnlButtons.Controls.Add(btnStopRecording);
            pnlButtons.Controls.Add(btnStartRecording);
            pnlButtons.Location = new Point(15, 498);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(972, 85);
            pnlButtons.TabIndex = 5;
            // 
            // btnOpenRecordings
            // 
            btnOpenRecordings.BackColor = Color.FromArgb(33, 150, 243);
            btnOpenRecordings.FlatAppearance.BorderSize = 0;
            btnOpenRecordings.FlatStyle = FlatStyle.Flat;
            btnOpenRecordings.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnOpenRecordings.ForeColor = Color.White;
            btnOpenRecordings.Location = new Point(360, 20);
            btnOpenRecordings.Name = "btnOpenRecordings";
            btnOpenRecordings.Size = new Size(150, 45);
            btnOpenRecordings.TabIndex = 2;
            btnOpenRecordings.Text = "Carpeta";
            btnOpenRecordings.UseVisualStyleBackColor = false;
            btnOpenRecordings.Click += BtnOpenRecordings_Click;
            // 
            // btnStopRecording
            // 
            btnStopRecording.BackColor = Color.FromArgb(244, 67, 54);
            btnStopRecording.Enabled = false;
            btnStopRecording.FlatAppearance.BorderSize = 0;
            btnStopRecording.FlatStyle = FlatStyle.Flat;
            btnStopRecording.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnStopRecording.ForeColor = Color.White;
            btnStopRecording.Location = new Point(190, 20);
            btnStopRecording.Name = "btnStopRecording";
            btnStopRecording.Size = new Size(150, 45);
            btnStopRecording.TabIndex = 1;
            btnStopRecording.Text = "Detener";
            btnStopRecording.UseVisualStyleBackColor = false;
            btnStopRecording.Click += BtnStopRecording_Click;
            // 
            // btnStartRecording
            // 
            btnStartRecording.BackColor = Color.FromArgb(76, 175, 80);
            btnStartRecording.FlatAppearance.BorderSize = 0;
            btnStartRecording.FlatStyle = FlatStyle.Flat;
            btnStartRecording.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnStartRecording.ForeColor = Color.White;
            btnStartRecording.Location = new Point(20, 20);
            btnStartRecording.Name = "btnStartRecording";
            btnStartRecording.Size = new Size(150, 45);
            btnStartRecording.TabIndex = 0;
            btnStartRecording.Text = "Iniciar";
            btnStartRecording.UseVisualStyleBackColor = false;
            btnStartRecording.Click += BtnStartRecording_Click;
            // 
            // pnlPreview
            // 
            pnlPreview.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlPreview.Controls.Add(previewBox);
            pnlPreview.ForeColor = Color.White;
            pnlPreview.Location = new Point(420, 60);
            pnlPreview.Name = "pnlPreview";
            pnlPreview.Padding = new Padding(10);
            pnlPreview.Size = new Size(590, 422);
            pnlPreview.TabIndex = 2;
            pnlPreview.TabStop = false;
            pnlPreview.Text = "Vista Previa";
            pnlPreview.Visible = false;
            // 
            // previewBox
            // 
            previewBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            previewBox.BackColor = Color.Black;
            previewBox.BorderStyle = BorderStyle.FixedSingle;
            previewBox.Location = new Point(10, 30);
            previewBox.Name = "previewBox";
            previewBox.Size = new Size(570, 382);
            previewBox.SizeMode = PictureBoxSizeMode.Zoom;
            previewBox.TabIndex = 0;
            previewBox.TabStop = false;
            // 
            // pnlStatus
            // 
            pnlStatus.Controls.Add(lblRecordingTime);
            pnlStatus.Controls.Add(lblRecordingStatus);
            pnlStatus.ForeColor = Color.White;
            pnlStatus.Location = new Point(12, 363);
            pnlStatus.Name = "pnlStatus";
            pnlStatus.Padding = new Padding(10);
            pnlStatus.Size = new Size(389, 100);
            pnlStatus.TabIndex = 4;
            pnlStatus.TabStop = false;
            pnlStatus.Text = "Estado de Grabación";
            // 
            // lblRecordingTime
            // 
            lblRecordingTime.AutoSize = true;
            lblRecordingTime.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblRecordingTime.ForeColor = Color.White;
            lblRecordingTime.Location = new Point(20, 60);
            lblRecordingTime.Name = "lblRecordingTime";
            lblRecordingTime.Size = new Size(112, 32);
            lblRecordingTime.TabIndex = 1;
            lblRecordingTime.Text = "00:00:00";
            // 
            // lblRecordingStatus
            // 
            lblRecordingStatus.AutoSize = true;
            lblRecordingStatus.ForeColor = Color.White;
            lblRecordingStatus.Location = new Point(20, 30);
            lblRecordingStatus.Name = "lblRecordingStatus";
            lblRecordingStatus.Size = new Size(140, 20);
            lblRecordingStatus.TabIndex = 0;
            lblRecordingStatus.Text = "Estado: Esperando...";
            // 
            // pnlDevices
            // 
            pnlDevices.Controls.Add(lblAudioStatus);
            pnlDevices.Controls.Add(cmbAudioDevices);
            pnlDevices.Controls.Add(lblDevice);
            pnlDevices.ForeColor = Color.White;
            pnlDevices.Location = new Point(12, 214);
            pnlDevices.Name = "pnlDevices";
            pnlDevices.Padding = new Padding(10);
            pnlDevices.Size = new Size(389, 129);
            pnlDevices.TabIndex = 3;
            pnlDevices.TabStop = false;
            pnlDevices.Text = "Dispositivos";
            // 
            // lblAudioStatus
            // 
            lblAudioStatus.AutoSize = true;
            lblAudioStatus.ForeColor = Color.LimeGreen;
            lblAudioStatus.Location = new Point(19, 89);
            lblAudioStatus.Name = "lblAudioStatus";
            lblAudioStatus.Size = new Size(157, 20);
            lblAudioStatus.TabIndex = 2;
            lblAudioStatus.Text = "Dispositivo disponible";
            // 
            // cmbAudioDevices
            // 
            cmbAudioDevices.BackColor = Color.FromArgb(45, 45, 45);
            cmbAudioDevices.ForeColor = Color.White;
            cmbAudioDevices.Location = new Point(20, 50);
            cmbAudioDevices.Name = "cmbAudioDevices";
            cmbAudioDevices.Size = new Size(360, 28);
            cmbAudioDevices.TabIndex = 1;
            // 
            // lblDevice
            // 
            lblDevice.AutoSize = true;
            lblDevice.ForeColor = Color.White;
            lblDevice.Location = new Point(19, 27);
            lblDevice.Name = "lblDevice";
            lblDevice.Size = new Size(151, 20);
            lblDevice.TabIndex = 0;
            lblDevice.Text = "Dispositivo de Audio:";
            // 
            // pnlMode
            // 
            pnlMode.Controls.Add(pictureModeIcon);
            pnlMode.Controls.Add(btnToggleCameraMode);
            pnlMode.Controls.Add(btnToggleMode);
            pnlMode.Controls.Add(lblModeStatus);
            pnlMode.ForeColor = Color.White;
            pnlMode.Location = new Point(12, 90);
            pnlMode.Name = "pnlMode";
            pnlMode.Padding = new Padding(10);
            pnlMode.Size = new Size(389, 100);
            pnlMode.TabIndex = 1;
            pnlMode.TabStop = false;
            pnlMode.Text = "Modo";
            // 
            // pictureModeIcon
            // 
            pictureModeIcon.Location = new Point(13, 27);
            pictureModeIcon.Name = "pictureModeIcon";
            pictureModeIcon.Size = new Size(100, 60);
            pictureModeIcon.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureModeIcon.TabIndex = 2;
            pictureModeIcon.TabStop = false;
            // 
            // btnToggleCameraMode
            // 
            btnToggleCameraMode.BackColor = Color.FromArgb(45, 45, 45);
            btnToggleCameraMode.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            btnToggleCameraMode.FlatStyle = FlatStyle.Flat;
            btnToggleCameraMode.ForeColor = Color.White;
            btnToggleCameraMode.Location = new Point(260, 55);
            btnToggleCameraMode.Name = "btnToggleCameraMode";
            btnToggleCameraMode.Size = new Size(120, 35);
            btnToggleCameraMode.TabIndex = 2;
            btnToggleCameraMode.Text = "Cámara";
            btnToggleCameraMode.UseVisualStyleBackColor = false;
            btnToggleCameraMode.Click += BtnToggleCameraMode_Click;
            // 
            // btnToggleMode
            // 
            btnToggleMode.BackColor = Color.FromArgb(45, 45, 45);
            btnToggleMode.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            btnToggleMode.FlatStyle = FlatStyle.Flat;
            btnToggleMode.ForeColor = Color.White;
            btnToggleMode.Location = new Point(130, 55);
            btnToggleMode.Name = "btnToggleMode";
            btnToggleMode.Size = new Size(120, 35);
            btnToggleMode.TabIndex = 1;
            btnToggleMode.Text = "Cambiar Modo";
            btnToggleMode.UseVisualStyleBackColor = false;
            btnToggleMode.Click += BtnToggleMode_Click;
            // 
            // lblModeStatus
            // 
            lblModeStatus.AutoSize = true;
            lblModeStatus.ForeColor = Color.White;
            lblModeStatus.Location = new Point(130, 30);
            lblModeStatus.Name = "lblModeStatus";
            lblModeStatus.Size = new Size(140, 20);
            lblModeStatus.TabIndex = 0;
            lblModeStatus.Text = "Modo actual: Audio";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(15, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(303, 41);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Grabadora de Audio";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // FormGrabadora
            // 
            BackColor = Color.FromArgb(24, 24, 24);
            ClientSize = new Size(1032, 647);
            Controls.Add(pnlMain);
            Name = "FormGrabadora";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Grabadora";
            pnlMain.ResumeLayout(false);
            pnlMain.PerformLayout();
            pnlButtons.ResumeLayout(false);
            pnlPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)previewBox).EndInit();
            pnlStatus.ResumeLayout(false);
            pnlStatus.PerformLayout();
            pnlDevices.ResumeLayout(false);
            pnlDevices.PerformLayout();
            pnlMode.ResumeLayout(false);
            pnlMode.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureModeIcon).EndInit();
            ResumeLayout(false);
        }

        private Button btnToggleMode;
        private Label lblModeStatus;
        private PictureBox pictureModeIcon;
        private ComboBox cmbAudioDevices;
        private Label lblAudioStatus;
        private Label lblRecordingStatus;
        private Label lblRecordingTime;
        private Button btnStartRecording;
        private Button btnStopRecording;
        private Button btnOpenRecordings;
        private Button btnToggleCameraMode;
        private Panel pnlMain;
        private Label lblTitle;
        private GroupBox pnlMode;
        private GroupBox pnlDevices;
        private Label lblDevice;
        private GroupBox pnlStatus;
        private Panel pnlButtons;
        private GroupBox pnlPreview;
        private PictureBox previewBox;
    }
}