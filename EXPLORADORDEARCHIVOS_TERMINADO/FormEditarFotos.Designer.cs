namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    partial class FormEditarFotos
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tableLayoutPanel = new TableLayoutPanel();
            panelControls = new FlowLayoutPanel();
            btnOpen = new Button();
            btnSave = new Button();
            btnGray = new Button();
            btnSepia = new Button();
            btnRevert = new Button();
            btnInvert = new Button();
            btnCrop = new Button();
            btnBrush = new Button();
            btnColor = new Button();
            panelColorPreview = new Panel();
            groupAdjusts = new GroupBox();
            lblBrightness = new Label();
            trackBrightness = new TrackBar();
            lblContrast = new Label();
            trackContrast = new TrackBar();
            lblSaturation = new Label();
            trackSaturation = new TrackBar();
            lblBrushSize = new Label();
            lblBrushSizeValue = new Label();
            trackBrushSize = new TrackBar();
            canvas = new PictureBox();
            webMapa = new Microsoft.Web.WebView2.WinForms.WebView2();
            txtCoordenadas = new TextBox();
            btnMostrarUbicacion = new Button();
            btnVer = new Button();
            tableLayoutPanel.SuspendLayout();
            panelControls.SuspendLayout();
            groupAdjusts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBrightness).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackContrast).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackSaturation).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBrushSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)canvas).BeginInit();
            ((System.ComponentModel.ISupportInitialize)webMapa).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel.Controls.Add(panelControls, 1, 0);
            tableLayoutPanel.Controls.Add(canvas, 0, 0);
            tableLayoutPanel.Location = new Point(0, 0);
            tableLayoutPanel.Margin = new Padding(3, 4, 3, 4);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 1;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 765F));
            tableLayoutPanel.Size = new Size(1297, 784);
            tableLayoutPanel.TabIndex = 0;
            // 
            // panelControls
            // 
            panelControls.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            panelControls.AutoScroll = true;
            panelControls.Controls.Add(btnOpen);
            panelControls.Controls.Add(btnSave);
            panelControls.Controls.Add(btnGray);
            panelControls.Controls.Add(btnSepia);
            panelControls.Controls.Add(btnRevert);
            panelControls.Controls.Add(btnInvert);
            panelControls.Controls.Add(btnCrop);
            panelControls.Controls.Add(btnBrush);
            panelControls.Controls.Add(btnColor);
            panelControls.Controls.Add(panelColorPreview);
            panelControls.Controls.Add(groupAdjusts);
            panelControls.Controls.Add(lblBrushSize);
            panelControls.Controls.Add(lblBrushSizeValue);
            panelControls.Controls.Add(trackBrushSize);
            panelControls.FlowDirection = FlowDirection.TopDown;
            panelControls.Location = new Point(975, 4);
            panelControls.Margin = new Padding(3, 4, 3, 4);
            panelControls.Name = "panelControls";
            panelControls.Size = new Size(319, 776);
            panelControls.TabIndex = 1;
            // 
            // btnOpen
            // 
            btnOpen.AutoSize = true;
            btnOpen.Location = new Point(3, 4);
            btnOpen.Margin = new Padding(3, 4, 3, 4);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(86, 40);
            btnOpen.TabIndex = 0;
            btnOpen.Text = "Abrir";
            btnOpen.Click += btnOpen_Click;
            // 
            // btnSave
            // 
            btnSave.AutoSize = true;
            btnSave.Location = new Point(3, 52);
            btnSave.Margin = new Padding(3, 4, 3, 4);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(86, 40);
            btnSave.TabIndex = 1;
            btnSave.Text = "Guardar";
            btnSave.Click += btnSave_Click;
            // 
            // btnGray
            // 
            btnGray.AutoSize = true;
            btnGray.Location = new Point(3, 100);
            btnGray.Margin = new Padding(3, 4, 3, 4);
            btnGray.Name = "btnGray";
            btnGray.Size = new Size(141, 40);
            btnGray.TabIndex = 2;
            btnGray.Text = "Escala de grises";
            btnGray.Click += btnGray_Click;
            // 
            // btnSepia
            // 
            btnSepia.AutoSize = true;
            btnSepia.Location = new Point(3, 148);
            btnSepia.Margin = new Padding(3, 4, 3, 4);
            btnSepia.Name = "btnSepia";
            btnSepia.Size = new Size(86, 40);
            btnSepia.TabIndex = 3;
            btnSepia.Text = "Sepia";
            btnSepia.Click += btnSepia_Click;
            // 
            // btnRevert
            // 
            btnRevert.AutoSize = true;
            btnRevert.Location = new Point(3, 196);
            btnRevert.Margin = new Padding(3, 4, 3, 4);
            btnRevert.Name = "btnRevert";
            btnRevert.Size = new Size(86, 40);
            btnRevert.TabIndex = 4;
            btnRevert.Text = "Revertir";
            btnRevert.Click += btnRevert_Click;
            // 
            // btnInvert
            // 
            btnInvert.AutoSize = true;
            btnInvert.Location = new Point(3, 244);
            btnInvert.Margin = new Padding(3, 4, 3, 4);
            btnInvert.Name = "btnInvert";
            btnInvert.Size = new Size(86, 40);
            btnInvert.TabIndex = 5;
            btnInvert.Text = "Invertir";
            btnInvert.Click += btnInvert_Click;
            // 
            // btnCrop
            // 
            btnCrop.AutoSize = true;
            btnCrop.Location = new Point(3, 292);
            btnCrop.Margin = new Padding(3, 4, 3, 4);
            btnCrop.Name = "btnCrop";
            btnCrop.Size = new Size(86, 40);
            btnCrop.TabIndex = 6;
            btnCrop.Text = "Recortar";
            btnCrop.Click += btnCrop_Click;
            // 
            // btnBrush
            // 
            btnBrush.AutoSize = true;
            btnBrush.Location = new Point(3, 340);
            btnBrush.Margin = new Padding(3, 4, 3, 4);
            btnBrush.Name = "btnBrush";
            btnBrush.Size = new Size(86, 40);
            btnBrush.TabIndex = 7;
            btnBrush.Text = "Pincel";
            btnBrush.Click += btnBrush_Click;
            // 
            // btnColor
            // 
            btnColor.AutoSize = true;
            btnColor.Location = new Point(3, 388);
            btnColor.Margin = new Padding(3, 4, 3, 4);
            btnColor.Name = "btnColor";
            btnColor.Size = new Size(86, 40);
            btnColor.TabIndex = 8;
            btnColor.Text = "Color";
            btnColor.Click += btnColor_Click;
            // 
            // panelColorPreview
            // 
            panelColorPreview.BackColor = Color.Black;
            panelColorPreview.Location = new Point(3, 436);
            panelColorPreview.Margin = new Padding(3, 4, 3, 4);
            panelColorPreview.Name = "panelColorPreview";
            panelColorPreview.Size = new Size(34, 40);
            panelColorPreview.TabIndex = 9;
            // 
            // groupAdjusts
            // 
            groupAdjusts.Controls.Add(lblBrightness);
            groupAdjusts.Controls.Add(trackBrightness);
            groupAdjusts.Controls.Add(lblContrast);
            groupAdjusts.Controls.Add(trackContrast);
            groupAdjusts.Controls.Add(lblSaturation);
            groupAdjusts.Controls.Add(trackSaturation);
            groupAdjusts.Location = new Point(3, 484);
            groupAdjusts.Margin = new Padding(3, 4, 3, 4);
            groupAdjusts.Name = "groupAdjusts";
            groupAdjusts.Padding = new Padding(3, 4, 3, 4);
            groupAdjusts.Size = new Size(274, 193);
            groupAdjusts.TabIndex = 10;
            groupAdjusts.TabStop = false;
            groupAdjusts.Text = "Ajustes";
            // 
            // lblBrightness
            // 
            lblBrightness.Location = new Point(7, 76);
            lblBrightness.Name = "lblBrightness";
            lblBrightness.Size = new Size(79, 31);
            lblBrightness.TabIndex = 0;
            lblBrightness.Text = "Brillo";
            // 
            // trackBrightness
            // 
            trackBrightness.Location = new Point(78, 76);
            trackBrightness.Margin = new Padding(3, 4, 3, 4);
            trackBrightness.Maximum = 100;
            trackBrightness.Minimum = -100;
            trackBrightness.Name = "trackBrightness";
            trackBrightness.Size = new Size(197, 56);
            trackBrightness.TabIndex = 1;
            trackBrightness.TickFrequency = 10;
            trackBrightness.Scroll += TrackBar_Scroll;
            // 
            // lblContrast
            // 
            lblContrast.Location = new Point(7, 25);
            lblContrast.Name = "lblContrast";
            lblContrast.Size = new Size(64, 31);
            lblContrast.TabIndex = 2;
            lblContrast.Text = "Contraste";
            // 
            // trackContrast
            // 
            trackContrast.Location = new Point(78, 25);
            trackContrast.Margin = new Padding(3, 4, 3, 4);
            trackContrast.Maximum = 100;
            trackContrast.Minimum = -100;
            trackContrast.Name = "trackContrast";
            trackContrast.Size = new Size(197, 56);
            trackContrast.TabIndex = 3;
            trackContrast.TickFrequency = 10;
            trackContrast.Scroll += TrackBar_Scroll;
            // 
            // lblSaturation
            // 
            lblSaturation.Location = new Point(7, 140);
            lblSaturation.Name = "lblSaturation";
            lblSaturation.Size = new Size(79, 31);
            lblSaturation.TabIndex = 4;
            lblSaturation.Text = "Saturación";
            // 
            // trackSaturation
            // 
            trackSaturation.Location = new Point(78, 140);
            trackSaturation.Margin = new Padding(3, 4, 3, 4);
            trackSaturation.Maximum = 100;
            trackSaturation.Minimum = -100;
            trackSaturation.Name = "trackSaturation";
            trackSaturation.Size = new Size(197, 56);
            trackSaturation.TabIndex = 5;
            trackSaturation.TickFrequency = 10;
            trackSaturation.Scroll += TrackBar_Scroll;
            // 
            // lblBrushSize
            // 
            lblBrushSize.AutoSize = true;
            lblBrushSize.Location = new Point(3, 681);
            lblBrushSize.Name = "lblBrushSize";
            lblBrushSize.Size = new Size(105, 20);
            lblBrushSize.TabIndex = 11;
            lblBrushSize.Text = "Tamaño pincel";
            // 
            // lblBrushSizeValue
            // 
            lblBrushSizeValue.AutoSize = true;
            lblBrushSizeValue.Location = new Point(3, 701);
            lblBrushSizeValue.Name = "lblBrushSizeValue";
            lblBrushSizeValue.Size = new Size(17, 20);
            lblBrushSizeValue.TabIndex = 13;
            lblBrushSizeValue.Text = "5";
            // 
            // trackBrushSize
            // 
            trackBrushSize.Location = new Point(283, 4);
            trackBrushSize.Margin = new Padding(3, 4, 3, 4);
            trackBrushSize.Maximum = 50;
            trackBrushSize.Minimum = 1;
            trackBrushSize.Name = "trackBrushSize";
            trackBrushSize.Size = new Size(229, 56);
            trackBrushSize.TabIndex = 12;
            trackBrushSize.Value = 5;
            trackBrushSize.Scroll += trackBrushSize_Scroll;
            // 
            // canvas
            // 
            canvas.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            canvas.Location = new Point(3, 4);
            canvas.Margin = new Padding(3, 4, 3, 4);
            canvas.Name = "canvas";
            canvas.Size = new Size(966, 681);
            canvas.SizeMode = PictureBoxSizeMode.Zoom;
            canvas.TabIndex = 2;
            canvas.TabStop = false;
            canvas.Paint += canvas_Paint;
            canvas.MouseDown += canvas_MouseDown;
            canvas.MouseMove += canvas_MouseMove;
            canvas.MouseUp += canvas_MouseUp;
            // 
            // webMapa
            // 
            webMapa.AllowExternalDrop = true;
            webMapa.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            webMapa.CreationProperties = null;
            webMapa.DefaultBackgroundColor = Color.White;
            webMapa.Location = new Point(0, 693);
            webMapa.Margin = new Padding(3, 4, 3, 4);
            webMapa.Name = "webMapa";
            webMapa.Size = new Size(854, 240);
            webMapa.TabIndex = 1;
            webMapa.ZoomFactor = 2D;
            // 
            // txtCoordenadas
            // 
            txtCoordenadas.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            txtCoordenadas.Location = new Point(864, 792);
            txtCoordenadas.Margin = new Padding(3, 4, 3, 4);
            txtCoordenadas.Name = "txtCoordenadas";
            txtCoordenadas.Size = new Size(265, 27);
            txtCoordenadas.TabIndex = 2;
            // 
            // btnMostrarUbicacion
            // 
            btnMostrarUbicacion.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnMostrarUbicacion.Location = new Point(914, 843);
            btnMostrarUbicacion.Margin = new Padding(3, 4, 3, 4);
            btnMostrarUbicacion.Name = "btnMostrarUbicacion";
            btnMostrarUbicacion.Size = new Size(153, 32);
            btnMostrarUbicacion.TabIndex = 3;
            btnMostrarUbicacion.Text = "Mostrar Ubicacion";
            btnMostrarUbicacion.UseVisualStyleBackColor = true;
            btnMostrarUbicacion.Click += btnMostrarUbicacion_Click;
            // 
            // btnVer
            // 
            btnVer.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnVer.Location = new Point(864, 843);
            btnVer.Margin = new Padding(3, 4, 3, 4);
            btnVer.Name = "btnVer";
            btnVer.Size = new Size(43, 32);
            btnVer.TabIndex = 4;
            btnVer.Text = "Ver";
            btnVer.UseVisualStyleBackColor = true;
            btnVer.Click += btnVer_Click;
            // 
            // FormEditarFotos
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1358, 961);
            Controls.Add(btnVer);
            Controls.Add(btnMostrarUbicacion);
            Controls.Add(txtCoordenadas);
            Controls.Add(webMapa);
            Controls.Add(tableLayoutPanel);
            Margin = new Padding(3, 4, 3, 4);
            Name = "FormEditarFotos";
            Text = "Simple Photo Editor";
            FormClosing += FormEditarFotos_FormClosing;
            tableLayoutPanel.ResumeLayout(false);
            panelControls.ResumeLayout(false);
            panelControls.PerformLayout();
            groupAdjusts.ResumeLayout(false);
            groupAdjusts.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBrightness).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackContrast).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackSaturation).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBrushSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)canvas).EndInit();
            ((System.ComponentModel.ISupportInitialize)webMapa).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.FlowLayoutPanel panelControls;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnRevert;
        private System.Windows.Forms.Button btnGray;
        private System.Windows.Forms.Button btnSepia;
        private System.Windows.Forms.Button btnInvert;
        private System.Windows.Forms.Button btnCrop;
        private System.Windows.Forms.Button btnBrush;
        private System.Windows.Forms.Button btnColor;
        private System.Windows.Forms.Panel panelColorPreview;
        private System.Windows.Forms.GroupBox groupAdjusts;
        private System.Windows.Forms.Label lblBrightness;
        private System.Windows.Forms.TrackBar trackBrightness;
        private System.Windows.Forms.Label lblContrast;
        private System.Windows.Forms.TrackBar trackContrast;
        private System.Windows.Forms.Label lblSaturation;
        private System.Windows.Forms.TrackBar trackSaturation;
        private System.Windows.Forms.Label lblBrushSize;
        private System.Windows.Forms.TrackBar trackBrushSize;
        private System.Windows.Forms.Label lblBrushSizeValue;
        private Button btnVer;

        #endregion

        private PictureBox canvas;
        private Microsoft.Web.WebView2.WinForms.WebView2 webMapa;
        private TextBox txtCoordenadas;
        private Button btnMostrarUbicacion;
    }
}