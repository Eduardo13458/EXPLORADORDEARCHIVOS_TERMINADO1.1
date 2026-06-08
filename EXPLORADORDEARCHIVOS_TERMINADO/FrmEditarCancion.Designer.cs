namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    partial class FrmEditarCancion
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;

        private System.Windows.Forms.TextBox txtTitulo;
        private System.Windows.Forms.TextBox txtArtista;
        private System.Windows.Forms.TextBox txtAlbum;
        private System.Windows.Forms.TextBox txtAnio;
        private System.Windows.Forms.TextBox txtLetra;

        private System.Windows.Forms.PictureBox pictureBoxPortada;
        private System.Windows.Forms.Button btnCambiarPortada;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            txtTitulo = new TextBox();
            txtArtista = new TextBox();
            txtAlbum = new TextBox();
            txtAnio = new TextBox();
            txtLetra = new TextBox();
            pictureBoxPortada = new PictureBox();
            btnCambiarPortada = new Button();
            btnGuardar = new Button();
            btnCancelar = new Button();
            openFileDialog1 = new OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPortada).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.ForeColor = Color.White;
            label1.Location = new Point(20, 20);
            label1.Name = "label1";
            label1.Size = new Size(64, 23);
            label1.TabIndex = 0;
            label1.Text = "Título:";
            // 
            // label2
            // 
            label2.ForeColor = Color.White;
            label2.Location = new Point(20, 60);
            label2.Name = "label2";
            label2.Size = new Size(64, 23);
            label2.TabIndex = 1;
            label2.Text = "Artista:";
            // 
            // label3
            // 
            label3.ForeColor = Color.White;
            label3.Location = new Point(20, 100);
            label3.Name = "label3";
            label3.Size = new Size(64, 23);
            label3.TabIndex = 2;
            label3.Text = "Álbum:";
            // 
            // label4
            // 
            label4.ForeColor = Color.White;
            label4.Location = new Point(20, 140);
            label4.Name = "label4";
            label4.Size = new Size(64, 23);
            label4.TabIndex = 3;
            label4.Text = "Año:";
            // 
            // label5
            // 
            label5.ForeColor = Color.White;
            label5.Location = new Point(20, 180);
            label5.Name = "label5";
            label5.Size = new Size(100, 23);
            label5.TabIndex = 4;
            label5.Text = "Letra:";
            // 
            // txtTitulo
            // 
            txtTitulo.Location = new Point(90, 17);
            txtTitulo.Name = "txtTitulo";
            txtTitulo.Size = new Size(323, 27);
            txtTitulo.TabIndex = 5;
            // 
            // txtArtista
            // 
            txtArtista.Location = new Point(90, 57);
            txtArtista.Name = "txtArtista";
            txtArtista.Size = new Size(323, 27);
            txtArtista.TabIndex = 6;
            // 
            // txtAlbum
            // 
            txtAlbum.Location = new Point(90, 97);
            txtAlbum.Name = "txtAlbum";
            txtAlbum.Size = new Size(323, 27);
            txtAlbum.TabIndex = 7;
            // 
            // txtAnio
            // 
            txtAnio.Location = new Point(90, 137);
            txtAnio.Name = "txtAnio";
            txtAnio.Size = new Size(323, 27);
            txtAnio.TabIndex = 8;
            // 
            // txtLetra
            // 
            txtLetra.Location = new Point(20, 206);
            txtLetra.Multiline = true;
            txtLetra.Name = "txtLetra";
            txtLetra.ScrollBars = ScrollBars.Vertical;
            txtLetra.Size = new Size(510, 120);
            txtLetra.TabIndex = 9;
            // 
            // pictureBoxPortada
            // 
            pictureBoxPortada.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxPortada.Location = new Point(538, 17);
            pictureBoxPortada.Name = "pictureBoxPortada";
            pictureBoxPortada.Size = new Size(160, 160);
            pictureBoxPortada.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxPortada.TabIndex = 10;
            pictureBoxPortada.TabStop = false;
            // 
            // btnCambiarPortada
            // 
            btnCambiarPortada.ForeColor = Color.White;
            btnCambiarPortada.Location = new Point(538, 183);
            btnCambiarPortada.Name = "btnCambiarPortada";
            btnCambiarPortada.Size = new Size(160, 30);
            btnCambiarPortada.TabIndex = 11;
            btnCambiarPortada.Text = "Cambiar portada";
            btnCambiarPortada.Click += btnCambiarPortada_Click;
            // 
            // btnGuardar
            // 
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Location = new Point(350, 340);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(80, 30);
            btnGuardar.TabIndex = 12;
            btnGuardar.Text = "Guardar";
            btnGuardar.Click += btnGuardar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.ForeColor = SystemColors.ControlLightLight;
            btnCancelar.Location = new Point(450, 340);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(80, 30);
            btnCancelar.TabIndex = 13;
            btnCancelar.Text = "Cancelar";
            btnCancelar.Click += btnCancelar_Click;
            // 
            // openFileDialog1
            // 
            openFileDialog1.Filter = "Imágenes|*.jpg;*.jpeg;*.png";
            // 
            // FrmEditarCancion
            // 
            BackColor = Color.Black;
            ClientSize = new Size(781, 456);
            Controls.Add(label1);
            Controls.Add(label2);
            Controls.Add(label3);
            Controls.Add(label4);
            Controls.Add(label5);
            Controls.Add(txtTitulo);
            Controls.Add(txtArtista);
            Controls.Add(txtAlbum);
            Controls.Add(txtAnio);
            Controls.Add(txtLetra);
            Controls.Add(pictureBoxPortada);
            Controls.Add(btnCambiarPortada);
            Controls.Add(btnGuardar);
            Controls.Add(btnCancelar);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmEditarCancion";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Editar Canción";
            ((System.ComponentModel.ISupportInitialize)pictureBoxPortada).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}