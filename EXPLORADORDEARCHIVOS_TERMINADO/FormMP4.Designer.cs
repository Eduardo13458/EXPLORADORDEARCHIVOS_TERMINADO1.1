namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    partial class FormMP4
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMP4));
            pnlVideo = new Panel();
            pnlControls = new Panel();
            PicPlayPause = new PictureBox();
            trkProgreso = new TrackBar();
            lblNombreArchivo = new Label();
            lblTiempoActual = new Label();
            lblTiempoTotal = new Label();
            picAbrir = new PictureBox();
            picStop = new PictureBox();
            picAtras = new PictureBox();
            picAdelante = new PictureBox();
            picRepeat = new PictureBox();
            picMute = new PictureBox();
            picFullScreen = new PictureBox();
            trkVolumen = new TrackBar();
            pnlControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PicPlayPause).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkProgreso).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picAbrir).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picStop).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picAtras).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picAdelante).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picRepeat).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picMute).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picFullScreen).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkVolumen).BeginInit();
            SuspendLayout();
            // 
            // pnlVideo
            // 
            pnlVideo.BackColor = Color.Black;
            pnlVideo.Dock = DockStyle.Fill;
            pnlVideo.Location = new Point(0, 0);
            pnlVideo.Margin = new Padding(3, 4, 3, 4);
            pnlVideo.Name = "pnlVideo";
            pnlVideo.Size = new Size(1463, 800);
            pnlVideo.TabIndex = 0;
            // 
            // pnlControls
            // 
            pnlControls.BackColor = Color.FromArgb(30, 0, 50);
            pnlControls.Controls.Add(PicPlayPause);
            pnlControls.Controls.Add(trkProgreso);
            pnlControls.Controls.Add(lblNombreArchivo);
            pnlControls.Controls.Add(lblTiempoActual);
            pnlControls.Controls.Add(lblTiempoTotal);
            pnlControls.Controls.Add(picAbrir);
            pnlControls.Controls.Add(picStop);
            pnlControls.Controls.Add(picAtras);
            pnlControls.Controls.Add(picAdelante);
            pnlControls.Controls.Add(picRepeat);
            pnlControls.Controls.Add(picMute);
            pnlControls.Controls.Add(picFullScreen);
            pnlControls.Controls.Add(trkVolumen);
            pnlControls.Dock = DockStyle.Bottom;
            pnlControls.Location = new Point(0, 800);
            pnlControls.Margin = new Padding(3, 4, 3, 4);
            pnlControls.Name = "pnlControls";
            pnlControls.Size = new Size(1463, 160);
            pnlControls.TabIndex = 1;
            // 
            // PicPlayPause
            // 
            PicPlayPause.Anchor = AnchorStyles.Left;
            PicPlayPause.BackColor = Color.Black;
            PicPlayPause.Cursor = Cursors.Hand;
            PicPlayPause.Image = (Image)resources.GetObject("PicPlayPause.Image");
            PicPlayPause.Location = new Point(74, 116);
            PicPlayPause.Margin = new Padding(3, 4, 3, 4);
            PicPlayPause.Name = "PicPlayPause";
            PicPlayPause.Size = new Size(57, 37);
            PicPlayPause.SizeMode = PictureBoxSizeMode.Zoom;
            PicPlayPause.TabIndex = 13;
            PicPlayPause.TabStop = false;
            PicPlayPause.Click += PicPlayPause_Click;
            // 
            // trkProgreso
            // 
            trkProgreso.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            trkProgreso.Location = new Point(11, 31);
            trkProgreso.Margin = new Padding(3, 4, 3, 4);
            trkProgreso.Name = "trkProgreso";
            trkProgreso.Size = new Size(1440, 56);
            trkProgreso.TabIndex = 1;
            trkProgreso.TickStyle = TickStyle.None;
            trkProgreso.Scroll += TrkProgreso_Scroll;
            // 
            // lblNombreArchivo
            // 
            lblNombreArchivo.AutoSize = true;
            lblNombreArchivo.ForeColor = Color.White;
            lblNombreArchivo.Location = new Point(11, 7);
            lblNombreArchivo.Name = "lblNombreArchivo";
            lblNombreArchivo.Size = new Size(90, 20);
            lblNombreArchivo.TabIndex = 0;
            lblNombreArchivo.Text = "Sin archivo...";
            // 
            // lblTiempoActual
            // 
            lblTiempoActual.AutoSize = true;
            lblTiempoActual.ForeColor = Color.LightGray;
            lblTiempoActual.Location = new Point(11, 92);
            lblTiempoActual.Name = "lblTiempoActual";
            lblTiempoActual.Size = new Size(44, 20);
            lblTiempoActual.TabIndex = 2;
            lblTiempoActual.Text = "00:00";
            // 
            // lblTiempoTotal
            // 
            lblTiempoTotal.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTiempoTotal.AutoSize = true;
            lblTiempoTotal.ForeColor = Color.LightGray;
            lblTiempoTotal.Location = new Point(1378, 91);
            lblTiempoTotal.Name = "lblTiempoTotal";
            lblTiempoTotal.Size = new Size(44, 20);
            lblTiempoTotal.TabIndex = 3;
            lblTiempoTotal.Text = "00:00";
            lblTiempoTotal.TextAlign = ContentAlignment.TopRight;
            // 
            // picAbrir
            // 
            picAbrir.Anchor = AnchorStyles.Left;
            picAbrir.BackColor = Color.Black;
            picAbrir.Cursor = Cursors.Hand;
            picAbrir.Image = (Image)resources.GetObject("picAbrir.Image");
            picAbrir.Location = new Point(11, 116);
            picAbrir.Margin = new Padding(3, 4, 3, 4);
            picAbrir.Name = "picAbrir";
            picAbrir.Size = new Size(57, 37);
            picAbrir.SizeMode = PictureBoxSizeMode.Zoom;
            picAbrir.TabIndex = 4;
            picAbrir.TabStop = false;
            picAbrir.Click += PicAbrir_Click;
            // 
            // picStop
            // 
            picStop.Anchor = AnchorStyles.Left;
            picStop.BackColor = Color.Black;
            picStop.Cursor = Cursors.Hand;
            picStop.Image = (Image)resources.GetObject("picStop.Image");
            picStop.Location = new Point(134, 116);
            picStop.Margin = new Padding(3, 4, 3, 4);
            picStop.Name = "picStop";
            picStop.Size = new Size(57, 37);
            picStop.SizeMode = PictureBoxSizeMode.Zoom;
            picStop.TabIndex = 7;
            picStop.TabStop = false;
            picStop.Click += PicStop_Click;
            // 
            // picAtras
            // 
            picAtras.Anchor = AnchorStyles.Left;
            picAtras.BackColor = Color.Black;
            picAtras.Cursor = Cursors.Hand;
            picAtras.Image = (Image)resources.GetObject("picAtras.Image");
            picAtras.Location = new Point(197, 116);
            picAtras.Margin = new Padding(3, 4, 3, 4);
            picAtras.Name = "picAtras";
            picAtras.Size = new Size(57, 37);
            picAtras.SizeMode = PictureBoxSizeMode.Zoom;
            picAtras.TabIndex = 8;
            picAtras.TabStop = false;
            picAtras.Click += PicAtras_Click;
            // 
            // picAdelante
            // 
            picAdelante.Anchor = AnchorStyles.Left;
            picAdelante.BackColor = Color.Black;
            picAdelante.Cursor = Cursors.Hand;
            picAdelante.Image = (Image)resources.GetObject("picAdelante.Image");
            picAdelante.Location = new Point(260, 116);
            picAdelante.Margin = new Padding(3, 4, 3, 4);
            picAdelante.Name = "picAdelante";
            picAdelante.Size = new Size(57, 37);
            picAdelante.SizeMode = PictureBoxSizeMode.Zoom;
            picAdelante.TabIndex = 9;
            picAdelante.TabStop = false;
            picAdelante.Click += PicAdelante_Click;
            // 
            // picRepeat
            // 
            picRepeat.Anchor = AnchorStyles.Left;
            picRepeat.BackColor = Color.Black;
            picRepeat.Cursor = Cursors.Hand;
            picRepeat.Image = (Image)resources.GetObject("picRepeat.Image");
            picRepeat.Location = new Point(323, 116);
            picRepeat.Margin = new Padding(3, 4, 3, 4);
            picRepeat.Name = "picRepeat";
            picRepeat.Size = new Size(57, 37);
            picRepeat.SizeMode = PictureBoxSizeMode.Zoom;
            picRepeat.TabIndex = 10;
            picRepeat.TabStop = false;
            picRepeat.Click += PicRepeat_Click;
            // 
            // picMute
            // 
            picMute.Anchor = AnchorStyles.Left;
            picMute.BackColor = Color.Black;
            picMute.Cursor = Cursors.Hand;
            picMute.Image = (Image)resources.GetObject("picMute.Image");
            picMute.Location = new Point(385, 116);
            picMute.Margin = new Padding(3, 4, 3, 4);
            picMute.Name = "picMute";
            picMute.Size = new Size(57, 37);
            picMute.SizeMode = PictureBoxSizeMode.Zoom;
            picMute.TabIndex = 11;
            picMute.TabStop = false;
            picMute.Click += PicMute_Click;
            // 
            // picFullScreen
            // 
            picFullScreen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            picFullScreen.BackColor = Color.Black;
            picFullScreen.Cursor = Cursors.Hand;
            picFullScreen.Image = (Image)resources.GetObject("picFullScreen.Image");
            picFullScreen.Location = new Point(1395, 116);
            picFullScreen.Margin = new Padding(3, 4, 3, 4);
            picFullScreen.Name = "picFullScreen";
            picFullScreen.Size = new Size(57, 37);
            picFullScreen.SizeMode = PictureBoxSizeMode.Zoom;
            picFullScreen.TabIndex = 14;
            picFullScreen.TabStop = false;
            picFullScreen.Click += PicFullScreen_Click;
            // 
            // trkVolumen
            // 
            trkVolumen.Anchor = AnchorStyles.Left;
            trkVolumen.Location = new Point(448, 116);
            trkVolumen.Margin = new Padding(3, 4, 3, 4);
            trkVolumen.Maximum = 100;
            trkVolumen.Name = "trkVolumen";
            trkVolumen.Size = new Size(171, 56);
            trkVolumen.TabIndex = 12;
            trkVolumen.TickStyle = TickStyle.None;
            trkVolumen.Value = 50;
            trkVolumen.Scroll += TrkVolumen_Scroll;
            // 
            // FormMP4
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(1463, 960);
            Controls.Add(pnlVideo);
            Controls.Add(pnlControls);
            Margin = new Padding(3, 4, 3, 4);
            Name = "FormMP4";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Reproductor de Video VLC";
            pnlControls.ResumeLayout(false);
            pnlControls.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PicPlayPause).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkProgreso).EndInit();
            ((System.ComponentModel.ISupportInitialize)picAbrir).EndInit();
            ((System.ComponentModel.ISupportInitialize)picStop).EndInit();
            ((System.ComponentModel.ISupportInitialize)picAtras).EndInit();
            ((System.ComponentModel.ISupportInitialize)picAdelante).EndInit();
            ((System.ComponentModel.ISupportInitialize)picRepeat).EndInit();
            ((System.ComponentModel.ISupportInitialize)picMute).EndInit();
            ((System.ComponentModel.ISupportInitialize)picFullScreen).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkVolumen).EndInit();
            ResumeLayout(false);
        }


        private Panel pnlVideo;
        private Panel pnlControls;
        private PictureBox picAbrir;
        private PictureBox picStop;
        private PictureBox picAdelante;
        private PictureBox picAtras;
        private PictureBox picRepeat;
        private PictureBox picMute;
        private PictureBox picFullScreen;
        private TrackBar trkVolumen;
        private TrackBar trkProgreso;
        private Label lblTiempoActual;
        private Label lblTiempoTotal;
        private Label lblNombreArchivo;
        private PictureBox PicPlayPause;
    }

}