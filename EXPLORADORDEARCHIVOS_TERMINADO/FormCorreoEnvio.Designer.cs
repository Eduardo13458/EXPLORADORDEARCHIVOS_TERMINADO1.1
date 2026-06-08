using System.Windows.Forms;

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    partial class FormCorreoEnvio
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
            lblArchivoNombre = new Label();
            pnlCuerpo = new Panel();
            lblDestinatario = new Label();
            txtDestinatario = new TextBox();
            lblRemitenteInfo = new Label();
            lblAsunto = new Label();
            txtAsunto = new TextBox();
            lblMensaje = new Label();
            txtMensaje = new TextBox();
            pnlBotones = new Panel();
            btnEnviar = new Button();
            btnCancelar = new Button();
            progressBar = new ProgressBar();

            pnlTop.SuspendLayout();
            pnlCuerpo.SuspendLayout();
            pnlBotones.SuspendLayout();
            SuspendLayout();

            // ── pnlTop ──────────────────────────────────────────────────────
            pnlTop.BackColor = System.Drawing.Color.FromArgb(30, 30, 50);
            pnlTop.Controls.Add(lblTitulo);
            pnlTop.Controls.Add(lblArchivoNombre);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Height = 72;

            lblTitulo.Text = "✉  Enviar archivo por correo";
            lblTitulo.ForeColor = System.Drawing.Color.White;
            lblTitulo.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new System.Drawing.Point(14, 8);

            lblArchivoNombre.Text = "";
            lblArchivoNombre.ForeColor = System.Drawing.Color.FromArgb(150, 210, 255);
            lblArchivoNombre.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblArchivoNombre.AutoSize = true;
            lblArchivoNombre.Location = new System.Drawing.Point(16, 40);

            // ── pnlCuerpo ───────────────────────────────────────────────────
            pnlCuerpo.Dock = DockStyle.Fill;
            pnlCuerpo.BackColor = System.Drawing.Color.FromArgb(40, 40, 60);
            pnlCuerpo.Padding = new Padding(16, 10, 16, 6);

            int x = 14, y = 12, lblW = 120, ctrlX = 138, ctrlW = 330;

            SetLabel(lblDestinatario, "Para (destinatario):", x, y, lblW);
            SetTxt(txtDestinatario, ctrlX, y, ctrlW);
            txtDestinatario.PlaceholderText = "correo@ejemplo.com";
            y += 36;

            // Remitente fijo — solo informativo
            lblRemitenteInfo.Text = "De: eduardosalazarmartinez360@gmail.com";
            lblRemitenteInfo.ForeColor = System.Drawing.Color.FromArgb(120, 210, 120);
            lblRemitenteInfo.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Italic);
            lblRemitenteInfo.Location = new System.Drawing.Point(x, y + 2);
            lblRemitenteInfo.Size = new System.Drawing.Size(ctrlX + ctrlW - x, 18);
            y += 28;

            SetLabel(lblAsunto, "Asunto:", x, y, lblW);
            SetTxt(txtAsunto, ctrlX, y, ctrlW);
            y += 36;

            SetLabel(lblMensaje, "Mensaje:", x, y, lblW);
            txtMensaje.Location = new System.Drawing.Point(ctrlX, y);
            txtMensaje.Size = new System.Drawing.Size(ctrlW, 80);
            txtMensaje.Multiline = true;
            txtMensaje.ScrollBars = ScrollBars.Vertical;
            EstiloTxt(txtMensaje);

            pnlCuerpo.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblDestinatario, txtDestinatario,
                lblRemitenteInfo,
                lblAsunto,       txtAsunto,
                lblMensaje,      txtMensaje
            });

            // ── pnlBotones ──────────────────────────────────────────────────
            pnlBotones.BackColor = System.Drawing.Color.FromArgb(30, 30, 50);
            pnlBotones.Dock = DockStyle.Bottom;
            pnlBotones.Height = 56;
            pnlBotones.Controls.Add(btnEnviar);
            pnlBotones.Controls.Add(btnCancelar);
            pnlBotones.Controls.Add(progressBar);

            btnEnviar.Text = "  Enviar correo";
            btnEnviar.BackColor = System.Drawing.Color.FromArgb(16, 137, 62);
            btnEnviar.ForeColor = System.Drawing.Color.White;
            btnEnviar.FlatStyle = FlatStyle.Flat;
            btnEnviar.Size = new System.Drawing.Size(150, 34);
            btnEnviar.Location = new System.Drawing.Point(10, 11);
            btnEnviar.Click += btnEnviar_Click;

            btnCancelar.Text = "Cancelar";
            btnCancelar.BackColor = System.Drawing.Color.FromArgb(170, 50, 50);
            btnCancelar.ForeColor = System.Drawing.Color.White;
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Size = new System.Drawing.Size(100, 34);
            btnCancelar.Location = new System.Drawing.Point(170, 11);
            btnCancelar.Click += (s, e) => Close();

            progressBar.Location = new System.Drawing.Point(280, 18);
            progressBar.Size = new System.Drawing.Size(180, 20);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.Visible = false;

            // ── FormCorreoEnvio ─────────────────────────────────────────────
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(496, 420);
            BackColor = System.Drawing.Color.FromArgb(40, 40, 60);
            Controls.Add(pnlCuerpo);
            Controls.Add(pnlBotones);
            Controls.Add(pnlTop);
            Name = "FormCorreoEnvio";
            Text = "Enviar por correo";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            pnlCuerpo.ResumeLayout(false);
            pnlCuerpo.PerformLayout();
            pnlBotones.ResumeLayout(false);
            ResumeLayout(false);
        }

        private static void SetLabel(Label l, string t, int x, int y, int w)
        {
            l.Text = t; l.AutoSize = false;
            l.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            l.Font = new System.Drawing.Font("Segoe UI", 9F);
            l.Location = new System.Drawing.Point(x, y + 4);
            l.Size = new System.Drawing.Size(w, 20);
        }

        private static void SetTxt(TextBox tb, int x, int y, int w)
        {
            tb.Location = new System.Drawing.Point(x, y);
            tb.Size = new System.Drawing.Size(w, 26);
            EstiloTxt(tb);
        }

        private static void EstiloTxt(TextBox tb)
        {
            tb.BackColor = System.Drawing.Color.FromArgb(55, 55, 80);
            tb.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.Font = new System.Drawing.Font("Segoe UI", 9.5F);
        }

        private Panel pnlTop;
        private Label lblTitulo;
        private Label lblArchivoNombre;
        private Panel pnlCuerpo;
        private Label lblDestinatario;
        private TextBox txtDestinatario;
        private Label lblRemitenteInfo;
        private Label lblAsunto;
        private TextBox txtAsunto;
        private Label lblMensaje;
        private TextBox txtMensaje;
        private Panel pnlBotones;
        private Button btnEnviar;
        private Button btnCancelar;
        private ProgressBar progressBar;
    }
}
