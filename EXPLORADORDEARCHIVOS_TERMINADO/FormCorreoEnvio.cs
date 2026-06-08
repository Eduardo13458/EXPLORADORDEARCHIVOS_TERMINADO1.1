using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    public partial class FormCorreoEnvio : Form
    {
        private readonly string _archivoAdjunto;

        // ── Cuenta remitente fija ────────────────────────────────────────────
        private const string RemitenteFijo = "eduardosalazarmartinez360@gmail.com";
        private const string PasswordFija = "pzrm zoxc wzva jujc";
        private const string SmtpHost = "smtp.gmail.com";
        private const int SmtpPort = 587;

        public FormCorreoEnvio(string archivoAdjunto)
        {
            InitializeComponent();
            _archivoAdjunto = archivoAdjunto;

            lblArchivoNombre.Text = $" Adjunto: {Path.GetFileName(archivoAdjunto)}";
            txtAsunto.Text = $"Archivo exportado: {Path.GetFileName(archivoAdjunto)}";
            txtMensaje.Text = "Hola,\n\nTe envío el archivo exportado desde el Explorador de Archivos.\n\nSaludos.";
            txtDestinatario.Focus();
        }


        // ── Enviar ───────────────────────────────────────────────────────────
        private async void btnEnviar_Click(object? sender, EventArgs e)
        {
            if (!Validar()) return;

            SetEnviando(true);

            try
            {
                // Construir mensaje
                var msg = new MimeMessage();
                msg.From.Add(MailboxAddress.Parse(RemitenteFijo));
                msg.To.Add(MailboxAddress.Parse(txtDestinatario.Text.Trim()));
                msg.Subject = txtAsunto.Text.Trim();

                var builder = new BodyBuilder { TextBody = txtMensaje.Text };
                builder.Attachments.Add(_archivoAdjunto);
                msg.Body = builder.ToMessageBody();

                // Enviar
                await Task.Run(async () =>
                {
                    using var smtp = new SmtpClient();
                    await smtp.ConnectAsync(SmtpHost, SmtpPort, SecureSocketOptions.StartTls);
                    await smtp.AuthenticateAsync(RemitenteFijo, PasswordFija);
                    await smtp.SendAsync(msg);
                    await smtp.DisconnectAsync(true);
                });

                SetEnviando(false);
                MessageBox.Show(
                    $"¡Correo enviado correctamente!\n\nDestinatario: {txtDestinatario.Text.Trim()}",
                    "Enviado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (MailKit.Security.AuthenticationException)
            {
                SetEnviando(false);
                MessageBox.Show(
                    "Error de autenticación.\n\nVerifique que la contraseña de aplicación sea correcta.",
                    "Autenticación fallida", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                SetEnviando(false);
                MessageBox.Show(
                    $" Error al enviar:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private bool Validar()
        {
            if (string.IsNullOrWhiteSpace(txtDestinatario.Text) || ObtenerDominio(txtDestinatario.Text) == null)
            { Alerta("Ingresa un correo destinatario válido."); txtDestinatario.Focus(); return false; }

            if (!File.Exists(_archivoAdjunto))
            { Alerta($"No se encontró el archivo adjunto:\n{_archivoAdjunto}"); return false; }

            return true;
        }

        private void SetEnviando(bool enviando)
        {
            btnEnviar.Enabled = !enviando;
            btnCancelar.Enabled = !enviando;
            progressBar.Visible = enviando;
            btnEnviar.Text = enviando ? "  Enviando…" : "  Enviar correo";
        }

        private static string? ObtenerDominio(string correo)
        {
            int at = correo.IndexOf('@');
            if (at < 0 || at == correo.Length - 1) return null;
            return correo[(at + 1)..].ToLower();
        }

        private static void Alerta(string msg)
            => MessageBox.Show(msg, "Dato requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
