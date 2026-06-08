namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    partial class FormCorrector
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCorrector));
            btnSeleccionar = new Button();
            btnProcesar = new Button();
            btnAplicarFiltros = new Button();
            btnLimpiarFiltros = new Button();
            btnGuardarCorrecciones = new Button();
            btnExportar = new Button();
            btnLimpiarDatos = new Button();
            lblTiposError = new Label();
            saveFileDialog = new SaveFileDialog();
            txtArchivo = new TextBox();
            txtOrdenarPor = new TextBox();
            lblArchivo = new Label();
            lblOrdenarPor = new Label();
            lblFiltros = new Label();
            lblDatos = new Label();
            lblErrores = new Label();
            pnlFiltros = new FlowLayoutPanel();
            dgvDatos = new DataGridView();
            lstErrores = new ListBox();
            progressBar = new ProgressBar();
            openFileDialog = new OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)dgvDatos).BeginInit();
            SuspendLayout();
            // 
            // btnSeleccionar
            // 
            btnSeleccionar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSeleccionar.Location = new Point(872, 15);
            btnSeleccionar.Margin = new Padding(3, 4, 3, 4);
            btnSeleccionar.Name = "btnSeleccionar";
            btnSeleccionar.Size = new Size(114, 33);
            btnSeleccionar.TabIndex = 1;
            btnSeleccionar.Text = "Examinar…";
            btnSeleccionar.Click += BtnSeleccionar_Click;
            // 
            // btnProcesar
            // 
            btnProcesar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnProcesar.Location = new Point(872, 59);
            btnProcesar.Margin = new Padding(3, 4, 3, 4);
            btnProcesar.Name = "btnProcesar";
            btnProcesar.Size = new Size(114, 33);
            btnProcesar.TabIndex = 3;
            btnProcesar.Text = "Procesar";
            btnProcesar.Click += BtnProcesar_Click;
            // 
            // btnAplicarFiltros
            // 
            btnAplicarFiltros.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAplicarFiltros.Enabled = false;
            btnAplicarFiltros.Location = new Point(736, 309);
            btnAplicarFiltros.Margin = new Padding(3, 4, 3, 4);
            btnAplicarFiltros.Name = "btnAplicarFiltros";
            btnAplicarFiltros.Size = new Size(120, 33);
            btnAplicarFiltros.TabIndex = 5;
            btnAplicarFiltros.Text = "Aplicar filtros";
            btnAplicarFiltros.Click += BtnAplicarFiltros_Click;
            // 
            // btnLimpiarFiltros
            // 
            btnLimpiarFiltros.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLimpiarFiltros.Enabled = false;
            btnLimpiarFiltros.Location = new Point(865, 309);
            btnLimpiarFiltros.Margin = new Padding(3, 4, 3, 4);
            btnLimpiarFiltros.Name = "btnLimpiarFiltros";
            btnLimpiarFiltros.Size = new Size(120, 33);
            btnLimpiarFiltros.TabIndex = 6;
            btnLimpiarFiltros.Text = "Limpiar filtros";
            btnLimpiarFiltros.Click += BtnLimpiarFiltros_Click;
            // 
            // btnGuardarCorrecciones
            // 
            btnGuardarCorrecciones.Enabled = false;
            btnGuardarCorrecciones.Location = new Point(14, 309);
            btnGuardarCorrecciones.Margin = new Padding(3, 4, 3, 4);
            btnGuardarCorrecciones.Name = "btnGuardarCorrecciones";
            btnGuardarCorrecciones.Size = new Size(183, 33);
            btnGuardarCorrecciones.TabIndex = 9;
            btnGuardarCorrecciones.Text = "Guardar correcciones";
            btnGuardarCorrecciones.Click += BtnGuardarCorrecciones_Click;
            // 
            // btnExportar
            // 
            btnExportar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExportar.Enabled = false;
            btnExportar.Location = new Point(851, 345);
            btnExportar.Margin = new Padding(3, 4, 3, 4);
            btnExportar.Name = "btnExportar";
            btnExportar.Size = new Size(149, 33);
            btnExportar.TabIndex = 10;
            btnExportar.Text = "Exportar datos…";
            btnExportar.Click += BtnExportar_Click;
            // 
            // btnLimpiarDatos
            // 
            btnLimpiarDatos.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLimpiarDatos.Enabled = false;
            btnLimpiarDatos.Location = new Point(674, 345);
            btnLimpiarDatos.Margin = new Padding(3, 4, 3, 4);
            btnLimpiarDatos.Name = "btnLimpiarDatos";
            btnLimpiarDatos.Size = new Size(171, 33);
            btnLimpiarDatos.TabIndex = 11;
            btnLimpiarDatos.Text = "Limpiar datos auto…";
            btnLimpiarDatos.Click += BtnLimpiarDatos_Click;
            // 
            // lblTiposError
            // 
            lblTiposError.AutoSize = true;
            lblTiposError.ForeColor = Color.DarkOrange;
            lblTiposError.Location = new Point(206, 316);
            lblTiposError.Name = "lblTiposError";
            lblTiposError.Size = new Size(0, 20);
            lblTiposError.TabIndex = 12;
            // 
            // saveFileDialog
            // 
            saveFileDialog.DefaultExt = "csv";
            saveFileDialog.Filter = "CSV (*.csv)|*.csv|Texto/TSV (*.txt)|*.txt|JSON (*.json)|*.json|XML (*.xml)|*.xml";
            saveFileDialog.Title = "Exportar datos";
            // 
            // txtArchivo
            // 
            txtArchivo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtArchivo.Location = new Point(86, 16);
            txtArchivo.Margin = new Padding(3, 4, 3, 4);
            txtArchivo.Name = "txtArchivo";
            txtArchivo.ReadOnly = true;
            txtArchivo.Size = new Size(777, 27);
            txtArchivo.TabIndex = 0;
            // 
            // txtOrdenarPor
            // 
            txtOrdenarPor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtOrdenarPor.Location = new Point(109, 60);
            txtOrdenarPor.Margin = new Padding(3, 4, 3, 4);
            txtOrdenarPor.Name = "txtOrdenarPor";
            txtOrdenarPor.PlaceholderText = "Ej: Nombre ASC  (dejar vacío = sin orden)";
            txtOrdenarPor.Size = new Size(342, 27);
            txtOrdenarPor.TabIndex = 2;
            // 
            // lblArchivo
            // 
            lblArchivo.AutoSize = true;
            lblArchivo.Location = new Point(14, 20);
            lblArchivo.Name = "lblArchivo";
            lblArchivo.Size = new Size(62, 20);
            lblArchivo.TabIndex = 0;
            lblArchivo.Text = "Archivo:";
            // 
            // lblOrdenarPor
            // 
            lblOrdenarPor.AutoSize = true;
            lblOrdenarPor.Location = new Point(14, 64);
            lblOrdenarPor.Name = "lblOrdenarPor";
            lblOrdenarPor.Size = new Size(93, 20);
            lblOrdenarPor.TabIndex = 2;
            lblOrdenarPor.Text = "Ordenar por:";
            // 
            // lblFiltros
            // 
            lblFiltros.AutoSize = true;
            lblFiltros.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblFiltros.Location = new Point(14, 129);
            lblFiltros.Name = "lblFiltros";
            lblFiltros.Size = new Size(128, 20);
            lblFiltros.TabIndex = 5;
            lblFiltros.Text = "Filtros dinámicos";
            // 
            // lblDatos
            // 
            lblDatos.AutoSize = true;
            lblDatos.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblDatos.Location = new Point(14, 353);
            lblDatos.Name = "lblDatos";
            lblDatos.Size = new Size(103, 20);
            lblDatos.TabIndex = 13;
            lblDatos.Text = "Datos válidos";
            // 
            // lblErrores
            // 
            lblErrores.AutoSize = true;
            lblErrores.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblErrores.Location = new Point(14, 809);
            lblErrores.Name = "lblErrores";
            lblErrores.Size = new Size(154, 20);
            lblErrores.TabIndex = 14;
            lblErrores.Text = "Errores de validación";
            // 
            // pnlFiltros
            // 
            pnlFiltros.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlFiltros.AutoScroll = true;
            pnlFiltros.BorderStyle = BorderStyle.FixedSingle;
            pnlFiltros.Location = new Point(14, 153);
            pnlFiltros.Margin = new Padding(3, 4, 3, 4);
            pnlFiltros.Name = "pnlFiltros";
            pnlFiltros.Size = new Size(972, 146);
            pnlFiltros.TabIndex = 4;
            // 
            // dgvDatos
            // 
            dgvDatos.AllowUserToAddRows = false;
            dgvDatos.AllowUserToDeleteRows = false;
            dgvDatos.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvDatos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDatos.Location = new Point(14, 380);
            dgvDatos.Margin = new Padding(3, 4, 3, 4);
            dgvDatos.Name = "dgvDatos";
            dgvDatos.RowHeadersWidth = 51;
            dgvDatos.Size = new Size(973, 413);
            dgvDatos.TabIndex = 7;
            dgvDatos.CellEndEdit += DgvDatos_CellEndEdit;
            dgvDatos.CellFormatting += DgvDatos_CellFormatting;
            // 
            // lstErrores
            // 
            lstErrores.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstErrores.FormattingEnabled = true;
            lstErrores.Location = new Point(14, 836);
            lstErrores.Margin = new Padding(3, 4, 3, 4);
            lstErrores.Name = "lstErrores";
            lstErrores.Size = new Size(972, 144);
            lstErrores.TabIndex = 8;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(14, 104);
            progressBar.Margin = new Padding(3, 4, 3, 4);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(973, 13);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 4;
            progressBar.Visible = false;
            // 
            // openFileDialog
            // 
            openFileDialog.Filter = resources.GetString("openFileDialog.Filter");
            openFileDialog.Title = "Seleccionar archivo de datos";
            // 
            // FormCorrector
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1270, 1053);
            Controls.Add(lblArchivo);
            Controls.Add(txtArchivo);
            Controls.Add(btnSeleccionar);
            Controls.Add(lblOrdenarPor);
            Controls.Add(txtOrdenarPor);
            Controls.Add(btnProcesar);
            Controls.Add(progressBar);
            Controls.Add(lblFiltros);
            Controls.Add(pnlFiltros);
            Controls.Add(btnAplicarFiltros);
            Controls.Add(btnLimpiarFiltros);
            Controls.Add(btnGuardarCorrecciones);
            Controls.Add(btnExportar);
            Controls.Add(btnLimpiarDatos);
            Controls.Add(lblTiposError);
            Controls.Add(lblDatos);
            Controls.Add(dgvDatos);
            Controls.Add(lblErrores);
            Controls.Add(lstErrores);
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(1021, 1018);
            Name = "FormCorrector";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Limpieza de Datos";
            ((System.ComponentModel.ISupportInitialize)dgvDatos).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnSeleccionar;
        private Button btnProcesar;
        private Button btnAplicarFiltros;
        private Button btnLimpiarFiltros;
        private Button btnGuardarCorrecciones;
        private Button btnExportar;
        private Button btnLimpiarDatos;
        private Label lblTiposError;
        private SaveFileDialog saveFileDialog;
        private TextBox txtArchivo;
        private TextBox txtOrdenarPor;
        private Label lblArchivo;
        private Label lblOrdenarPor;
        private Label lblFiltros;
        private Label lblDatos;
        private Label lblErrores;
        private FlowLayoutPanel pnlFiltros;
        private DataGridView dgvDatos;
        private ListBox lstErrores;
        private ProgressBar progressBar;
        private OpenFileDialog openFileDialog;
    }

}