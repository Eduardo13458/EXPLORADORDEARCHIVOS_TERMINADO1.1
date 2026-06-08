namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    partial class FormMP3
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMP3));
            dataGridView1 = new DataGridView();
            btnPropiedades = new Button();
            trackDuracion = new TrackBar();
            trackVolumen = new TrackBar();
            pictPause = new PictureBox();
            pictNext = new PictureBox();
            pictBack = new PictureBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            openFileDialog1 = new OpenFileDialog();
            timer1 = new System.Windows.Forms.Timer(components);
            pictureBoxAlbum = new PictureBox();
            lstLista = new ListBox();
            btnAbrirReproductor = new Button();
            btnDeleteList = new Button();
            pictSearch = new PictureBox();
            label5 = new Label();
            txtSearch = new TextBox();
            pictSearchTxt = new PictureBox();
            lblName = new Label();
            pictPass = new PictureBox();
            pictDelay = new PictureBox();
            pictReset = new PictureBox();
            pictLoop = new PictureBox();
            pictRandom = new PictureBox();
            btnEdit = new Button();
            btnSavePlaylist = new Button();
            btnEliminar = new Button();
            listBoxLetras = new ListBox();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackDuracion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackVolumen).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictPause).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictNext).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictBack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxAlbum).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictSearch).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictSearchTxt).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictPass).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictDelay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictReset).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictLoop).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictRandom).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.Location = new Point(1060, 445);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(291, 264);
            dataGridView1.TabIndex = 0;
            // 
            // btnPropiedades
            // 
            btnPropiedades.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnPropiedades.Location = new Point(160, 649);
            btnPropiedades.Name = "btnPropiedades";
            btnPropiedades.Size = new Size(138, 49);
            btnPropiedades.TabIndex = 2;
            btnPropiedades.Text = "Ver Propiedad de un archivo";
            btnPropiedades.UseVisualStyleBackColor = true;
            btnPropiedades.Click += btnPropiedades_Click;
            // 
            // trackDuracion
            // 
            trackDuracion.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            trackDuracion.Location = new Point(113, 456);
            trackDuracion.Name = "trackDuracion";
            trackDuracion.Size = new Size(837, 56);
            trackDuracion.TabIndex = 5;
            trackDuracion.Scroll += trackDuracion_Scroll;
            // 
            // trackVolumen
            // 
            trackVolumen.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            trackVolumen.Location = new Point(42, 75);
            trackVolumen.Maximum = 100;
            trackVolumen.Name = "trackVolumen";
            trackVolumen.Orientation = Orientation.Vertical;
            trackVolumen.Size = new Size(56, 267);
            trackVolumen.TabIndex = 6;
            trackVolumen.Value = 70;
            trackVolumen.Scroll += trackVolumen_Scroll;
            // 
            // pictPause
            // 
            pictPause.Anchor = AnchorStyles.Bottom;
            pictPause.Image = (Image)resources.GetObject("pictPause.Image");
            pictPause.Location = new Point(496, 502);
            pictPause.Name = "pictPause";
            pictPause.Size = new Size(68, 62);
            pictPause.SizeMode = PictureBoxSizeMode.Zoom;
            pictPause.TabIndex = 8;
            pictPause.TabStop = false;
            pictPause.Click += pictPause_Click;
            // 
            // pictNext
            // 
            pictNext.Anchor = AnchorStyles.Bottom;
            pictNext.Image = (Image)resources.GetObject("pictNext.Image");
            pictNext.Location = new Point(686, 502);
            pictNext.Name = "pictNext";
            pictNext.Size = new Size(68, 62);
            pictNext.SizeMode = PictureBoxSizeMode.Zoom;
            pictNext.TabIndex = 9;
            pictNext.TabStop = false;
            pictNext.Click += pictNext_Click;
            // 
            // pictBack
            // 
            pictBack.Anchor = AnchorStyles.Bottom;
            pictBack.Image = (Image)resources.GetObject("pictBack.Image");
            pictBack.Location = new Point(313, 502);
            pictBack.Name = "pictBack";
            pictBack.Size = new Size(68, 62);
            pictBack.SizeMode = PictureBoxSizeMode.Zoom;
            pictBack.TabIndex = 10;
            pictBack.TabStop = false;
            pictBack.Click += pictBack_Click;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.White;
            label1.Location = new Point(12, 344);
            label1.Name = "label1";
            label1.Size = new Size(95, 28);
            label1.TabIndex = 11;
            label1.Text = "Volumen";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.BackColor = Color.FromArgb(179, 179, 179);
            label2.Location = new Point(956, 456);
            label2.Name = "label2";
            label2.Size = new Size(44, 20);
            label2.TabIndex = 12;
            label2.Text = "00:00";
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label3.AutoSize = true;
            label3.BackColor = Color.FromArgb(179, 179, 179);
            label3.Location = new Point(53, 456);
            label3.Name = "label3";
            label3.Size = new Size(44, 20);
            label3.TabIndex = 13;
            label3.Text = "00:00";
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label4.AutoSize = true;
            label4.ForeColor = Color.White;
            label4.Location = new Point(1291, 9);
            label4.Name = "label4";
            label4.Size = new Size(50, 20);
            label4.TabIndex = 14;
            label4.Text = "0 de 0";
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            openFileDialog1.FileOk += openFileDialog1_FileOk;
            // 
            // timer1
            // 
            timer1.Tick += timer1_Tick;
            // 
            // pictureBoxAlbum
            // 
            pictureBoxAlbum.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pictureBoxAlbum.BackColor = Color.FromArgb(24, 24, 24);
            pictureBoxAlbum.Location = new Point(314, 37);
            pictureBoxAlbum.Name = "pictureBoxAlbum";
            pictureBoxAlbum.Size = new Size(440, 360);
            pictureBoxAlbum.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxAlbum.TabIndex = 16;
            pictureBoxAlbum.TabStop = false;
            // 
            // lstLista
            // 
            lstLista.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            lstLista.BackColor = Color.FromArgb(24, 24, 24);
            lstLista.ForeColor = Color.White;
            lstLista.FormattingEnabled = true;
            lstLista.Location = new Point(1060, 75);
            lstLista.Name = "lstLista";
            lstLista.Size = new Size(291, 364);
            lstLista.TabIndex = 17;
            lstLista.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            lstLista.DoubleClick += listBox1_DoubleClick;
            // 
            // btnAbrirReproductor
            // 
            btnAbrirReproductor.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAbrirReproductor.Location = new Point(12, 649);
            btnAbrirReproductor.Name = "btnAbrirReproductor";
            btnAbrirReproductor.Size = new Size(138, 49);
            btnAbrirReproductor.TabIndex = 18;
            btnAbrirReproductor.Text = "Abrir Reproductor";
            btnAbrirReproductor.UseVisualStyleBackColor = true;
            btnAbrirReproductor.Click += btnAbrirReproductor_Click;
            // 
            // btnDeleteList
            // 
            btnDeleteList.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDeleteList.Location = new Point(777, 649);
            btnDeleteList.Name = "btnDeleteList";
            btnDeleteList.Size = new Size(138, 49);
            btnDeleteList.TabIndex = 19;
            btnDeleteList.Text = "Eliminar Lista";
            btnDeleteList.UseVisualStyleBackColor = true;
            btnDeleteList.Click += btnDeleteList_Click;
            // 
            // pictSearch
            // 
            pictSearch.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            pictSearch.Image = (Image)resources.GetObject("pictSearch.Image");
            pictSearch.Location = new Point(12, 532);
            pictSearch.Name = "pictSearch";
            pictSearch.Size = new Size(86, 85);
            pictSearch.SizeMode = PictureBoxSizeMode.Zoom;
            pictSearch.TabIndex = 20;
            pictSearch.TabStop = false;
            pictSearch.Click += pictSearch_Click;
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label5.ForeColor = Color.White;
            label5.Location = new Point(1060, 9);
            label5.Name = "label5";
            label5.Size = new Size(61, 31);
            label5.TabIndex = 22;
            label5.Text = "Lista";
            // 
            // txtSearch
            // 
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtSearch.AutoCompleteMode = AutoCompleteMode.Append;
            txtSearch.Location = new Point(1060, 43);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(245, 27);
            txtSearch.TabIndex = 23;
            // 
            // pictSearchTxt
            // 
            pictSearchTxt.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pictSearchTxt.Image = (Image)resources.GetObject("pictSearchTxt.Image");
            pictSearchTxt.Location = new Point(1311, 37);
            pictSearchTxt.Name = "pictSearchTxt";
            pictSearchTxt.Size = new Size(40, 32);
            pictSearchTxt.SizeMode = PictureBoxSizeMode.Zoom;
            pictSearchTxt.TabIndex = 24;
            pictSearchTxt.TabStop = false;
            pictSearchTxt.Click += pictSearchTxt_Click;
            // 
            // lblName
            // 
            lblName.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblName.AutoSize = true;
            lblName.Font = new Font("Segoe UI", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblName.ForeColor = Color.White;
            lblName.Location = new Point(160, 408);
            lblName.Name = "lblName";
            lblName.Size = new Size(14, 31);
            lblName.TabIndex = 25;
            lblName.Text = "\r\n";
            // 
            // pictPass
            // 
            pictPass.Anchor = AnchorStyles.Bottom;
            pictPass.Image = (Image)resources.GetObject("pictPass.Image");
            pictPass.Location = new Point(593, 502);
            pictPass.Name = "pictPass";
            pictPass.Size = new Size(68, 62);
            pictPass.SizeMode = PictureBoxSizeMode.Zoom;
            pictPass.TabIndex = 26;
            pictPass.TabStop = false;
            pictPass.Click += pictPass_Click;
            // 
            // pictDelay
            // 
            pictDelay.Anchor = AnchorStyles.Bottom;
            pictDelay.Image = (Image)resources.GetObject("pictDelay.Image");
            pictDelay.Location = new Point(401, 502);
            pictDelay.Name = "pictDelay";
            pictDelay.Size = new Size(68, 62);
            pictDelay.SizeMode = PictureBoxSizeMode.Zoom;
            pictDelay.TabIndex = 27;
            pictDelay.TabStop = false;
            pictDelay.Click += pictDelay_Click;
            // 
            // pictReset
            // 
            pictReset.Anchor = AnchorStyles.Bottom;
            pictReset.Image = (Image)resources.GetObject("pictReset.Image");
            pictReset.Location = new Point(225, 502);
            pictReset.Name = "pictReset";
            pictReset.Size = new Size(68, 62);
            pictReset.SizeMode = PictureBoxSizeMode.Zoom;
            pictReset.TabIndex = 28;
            pictReset.TabStop = false;
            pictReset.Click += pictReset_Click;
            // 
            // pictLoop
            // 
            pictLoop.Anchor = AnchorStyles.Bottom;
            pictLoop.Image = (Image)resources.GetObject("pictLoop.Image");
            pictLoop.Location = new Point(858, 502);
            pictLoop.Name = "pictLoop";
            pictLoop.Size = new Size(68, 62);
            pictLoop.SizeMode = PictureBoxSizeMode.Zoom;
            pictLoop.TabIndex = 29;
            pictLoop.TabStop = false;
            pictLoop.Click += pictLoop_Click;
            // 
            // pictRandom
            // 
            pictRandom.Anchor = AnchorStyles.Bottom;
            pictRandom.Image = (Image)resources.GetObject("pictRandom.Image");
            pictRandom.Location = new Point(767, 502);
            pictRandom.Name = "pictRandom";
            pictRandom.Size = new Size(68, 62);
            pictRandom.SizeMode = PictureBoxSizeMode.Zoom;
            pictRandom.TabIndex = 30;
            pictRandom.TabStop = false;
            pictRandom.Click += pictRandom_Click;
            // 
            // btnEdit
            // 
            btnEdit.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEdit.Location = new Point(314, 649);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(138, 49);
            btnEdit.TabIndex = 32;
            btnEdit.Text = "Editar propiedades";
            btnEdit.UseVisualStyleBackColor = true;
            btnEdit.Click += btnEdit_Click;
            // 
            // btnSavePlaylist
            // 
            btnSavePlaylist.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSavePlaylist.Location = new Point(469, 649);
            btnSavePlaylist.Name = "btnSavePlaylist";
            btnSavePlaylist.Size = new Size(138, 49);
            btnSavePlaylist.TabIndex = 33;
            btnSavePlaylist.Text = "Guardar PlayList";
            btnSavePlaylist.UseVisualStyleBackColor = true;
            btnSavePlaylist.Click += btnSavePlaylist_Click;
            // 
            // btnEliminar
            // 
            btnEliminar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEliminar.Location = new Point(624, 649);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(138, 49);
            btnEliminar.TabIndex = 34;
            btnEliminar.Text = "Eliminar de Lista";
            btnEliminar.UseVisualStyleBackColor = true;
            btnEliminar.Click += btnEliminar_Click;
            // 
            // listBoxLetras
            // 
            listBoxLetras.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            listBoxLetras.BackColor = Color.FromArgb(24, 24, 24);
            listBoxLetras.ForeColor = Color.White;
            listBoxLetras.FormattingEnabled = true;
            listBoxLetras.Location = new Point(777, 75);
            listBoxLetras.Name = "listBoxLetras";
            listBoxLetras.Size = new Size(261, 304);
            listBoxLetras.TabIndex = 35;
            // 
            // FormMP3
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(18, 18, 18);
            ClientSize = new Size(1353, 710);
            Controls.Add(listBoxLetras);
            Controls.Add(btnEliminar);
            Controls.Add(btnSavePlaylist);
            Controls.Add(btnEdit);
            Controls.Add(pictRandom);
            Controls.Add(pictLoop);
            Controls.Add(pictReset);
            Controls.Add(pictDelay);
            Controls.Add(pictPass);
            Controls.Add(lblName);
            Controls.Add(pictSearchTxt);
            Controls.Add(txtSearch);
            Controls.Add(label5);
            Controls.Add(pictSearch);
            Controls.Add(btnDeleteList);
            Controls.Add(btnAbrirReproductor);
            Controls.Add(lstLista);
            Controls.Add(pictureBoxAlbum);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(pictBack);
            Controls.Add(pictNext);
            Controls.Add(pictPause);
            Controls.Add(trackVolumen);
            Controls.Add(trackDuracion);
            Controls.Add(btnPropiedades);
            Controls.Add(dataGridView1);
            Name = "FormMP3";
            Text = "FormMP3";
            FormClosing += FormMP3_FormClosing;
            Load += FormMP3_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackDuracion).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackVolumen).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictPause).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictNext).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictBack).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxAlbum).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictSearch).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictSearchTxt).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictPass).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictDelay).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictReset).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictLoop).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictRandom).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridView1;
        private Button btnPropiedades;
        private TrackBar trackDuracion;
        private TrackBar trackVolumen;
        private PictureBox pictPause;
        private PictureBox pictNext;
        private PictureBox pictBack;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Timer timer1;
        private PictureBox pictureBoxAlbum;
        private ListBox lstLista;
        private Button btnAbrirReproductor;
        private Button btnDeleteList;
        private PictureBox pictSearch;
        private Label label5;
        private TextBox txtSearch;
        private PictureBox pictSearchTxt;
        private Label lblName;
        private PictureBox pictPass;
        private PictureBox pictDelay;
        private PictureBox pictReset;
        private PictureBox pictLoop;
        private PictureBox pictRandom;
        private Button btnEdit;
        private Button btnSavePlaylist;
        private Button btnEliminar;
        private ListBox listBoxLetras;
    }
}