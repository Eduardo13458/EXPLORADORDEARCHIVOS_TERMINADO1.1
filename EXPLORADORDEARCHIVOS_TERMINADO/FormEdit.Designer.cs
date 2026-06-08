using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    partial class FormEdit
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
            pnlTop = new Panel();
            lblTitulo = new Label();
            lblEstado = new Label();
            pnlControles = new Panel();
            btnCargar = new Button();
            btnGuardar = new Button();
            splitter = new SplitContainer();
            dgvDatos = new DataGridView();
            rtbContenido = new RichTextBox();
            webViewPdf = new WebView2();
            pnlTop.SuspendLayout();
            pnlControles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitter).BeginInit();
            splitter.Panel1.SuspendLayout();
            splitter.Panel2.SuspendLayout();
            splitter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDatos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)webViewPdf).BeginInit();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.BackColor = Color.FromArgb(30, 30, 50);
            pnlTop.Controls.Add(lblTitulo);
            pnlTop.Controls.Add(lblEstado);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Padding = new Padding(10);
            pnlTop.Size = new Size(1732, 60);
            pnlTop.TabIndex = 2;
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(10, 10);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(224, 32);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Editor de Archivos";
            // 
            // lblEstado
            // 
            lblEstado.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblEstado.AutoSize = true;
            lblEstado.Font = new Font("Segoe UI", 9F);
            lblEstado.ForeColor = Color.LightGreen;
            lblEstado.Location = new Point(1932, 15);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new Size(40, 20);
            lblEstado.TabIndex = 1;
            lblEstado.Text = "Listo";
            // 
            // pnlControles
            // 
            pnlControles.BackColor = Color.FromArgb(45, 45, 68);
            pnlControles.Controls.Add(btnCargar);
            pnlControles.Controls.Add(btnGuardar);
            pnlControles.Dock = DockStyle.Top;
            pnlControles.Location = new Point(0, 60);
            pnlControles.Name = "pnlControles";
            pnlControles.Padding = new Padding(10);
            pnlControles.Size = new Size(1732, 50);
            pnlControles.TabIndex = 1;
            // 
            // btnCargar
            // 
            btnCargar.BackColor = Color.FromArgb(0, 120, 215);
            btnCargar.FlatStyle = FlatStyle.Flat;
            btnCargar.ForeColor = Color.White;
            btnCargar.Location = new Point(10, 9);
            btnCargar.Name = "btnCargar";
            btnCargar.Size = new Size(150, 32);
            btnCargar.TabIndex = 0;
            btnCargar.Text = " Cargar Archivo";
            btnCargar.UseVisualStyleBackColor = false;
            btnCargar.Click += btnCargar_Click;
            // 
            // btnGuardar
            // 
            btnGuardar.BackColor = Color.FromArgb(16, 137, 62);
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Location = new Point(170, 9);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(150, 32);
            btnGuardar.TabIndex = 1;
            btnGuardar.Text = " Guardar Cambios";
            btnGuardar.UseVisualStyleBackColor = false;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // splitter
            // 
            splitter.Dock = DockStyle.Fill;
            splitter.Location = new Point(0, 110);
            splitter.Name = "splitter";
            // 
            // splitter.Panel1
            // 
            splitter.Panel1.Controls.Add(dgvDatos);
            // 
            // splitter.Panel2
            // 
            splitter.Panel2.Controls.Add(rtbContenido);
            splitter.Panel2.Controls.Add(webViewPdf);
            splitter.Size = new Size(1732, 590);
            splitter.SplitterDistance = 883;
            splitter.TabIndex = 0;
            // 
            // dgvDatos
            // 
            dgvDatos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvDatos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDatos.Dock = DockStyle.Fill;
            dgvDatos.Location = new Point(0, 0);
            dgvDatos.Name = "dgvDatos";
            dgvDatos.RowHeadersWidth = 51;
            dgvDatos.Size = new Size(883, 590);
            dgvDatos.TabIndex = 0;
            // 
            // rtbContenido
            // 
            rtbContenido.BackColor = Color.FromArgb(30, 30, 30);
            rtbContenido.BorderStyle = BorderStyle.None;
            rtbContenido.Dock = DockStyle.Fill;
            rtbContenido.Font = new Font("Consolas", 11F);
            rtbContenido.ForeColor = Color.FromArgb(220, 220, 220);
            rtbContenido.Location = new Point(0, 0);
            rtbContenido.Name = "rtbContenido";
            rtbContenido.Size = new Size(845, 590);
            rtbContenido.TabIndex = 0;
            rtbContenido.Text = "";
            // 
            // webViewPdf
            // 
            webViewPdf.AllowExternalDrop = true;
            webViewPdf.CreationProperties = null;
            webViewPdf.DefaultBackgroundColor = Color.White;
            webViewPdf.Dock = DockStyle.Fill;
            webViewPdf.Location = new Point(0, 0);
            webViewPdf.Name = "webViewPdf";
            webViewPdf.Size = new Size(845, 590);
            webViewPdf.TabIndex = 1;
            webViewPdf.Visible = false;
            webViewPdf.ZoomFactor = 1D;
            // 
            // FormEdit
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1732, 700);
            Controls.Add(splitter);
            Controls.Add(pnlControles);
            Controls.Add(pnlTop);
            Name = "FormEdit";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Editor de Archivos";
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            pnlControles.ResumeLayout(false);
            splitter.Panel1.ResumeLayout(false);
            splitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitter).EndInit();
            splitter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvDatos).EndInit();
            ((System.ComponentModel.ISupportInitialize)webViewPdf).EndInit();
            ResumeLayout(false);
        }

        private Panel pnlTop;
        private Label lblTitulo;
        private Label lblEstado;
        private Panel pnlControles;
        private Button btnCargar;
        private Button btnGuardar;
        private SplitContainer splitter;
        public DataGridView dgvDatos;
        public RichTextBox rtbContenido;
        public WebView2 webViewPdf;
    }
}