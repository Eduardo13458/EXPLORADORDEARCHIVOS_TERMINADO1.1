using GeniusLyricsAPI.Models;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using EXPLORADORDEARCHIVOS_TERMINADO.Processing;
using EXPLORADORDEARCHIVOS_TERMINADO.Services;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    public partial class FormMP4 : Form, IMessageFilter
    {
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private VideoView videoView;
        private System.Windows.Forms.Timer _timerProgreso;
        private System.Windows.Forms.Timer _timerOcultarControles;
        private bool _isSeeking = false;
        private int _repeatMode = 0;
        private bool _isInitialized = false;
        private const int TRACKBAR_MAX = 10000;
        private long _lastSeekTime = 0;
        private const long SEEK_DEBOUNCE_MS = 100;
        private FormBorderStyle _originalFormStyle;
        private FormWindowState _originalFormState;
        private bool _isLoadingVideo = false;
        private bool _isClosing = false;
        private CancellationTokenSource _closingCts;
        private bool _isFullScreen = false;
        private Control _pnlVideoParent;
        private DockStyle _pnlVideoDockStyle;
        private Rectangle _pnlVideoBounds;
        private const int FULLSCREEN_CONTROLS_HEIGHT = 160;
        private const int HIDE_CONTROLES_DELAY_MS = 3000;
        private bool _controlsVisible = true;
        private Point _lastMousePosition = Point.Empty;

        public FormMP4()
        {
            InitializeComponent();
            this.FormClosing += FormMP4_FormClosing;
            _closingCts = new CancellationTokenSource();
            Application.AddMessageFilter(this);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InitializeVideoView();
            InitializeVLC();
            InitializeTimer();
            InitializeHideControlsTimer();
            ConfigurarTrackbar();
            ConfigurarPictureBoxes();
        }

        private void ConfigurarTrackbar()
        {
            trkProgreso.Minimum = 0;
            trkProgreso.Maximum = TRACKBAR_MAX;
            trkProgreso.Value = 0;
            trkProgreso.TickFrequency = TRACKBAR_MAX / 10;
        }

        private void ConfigurarPictureBoxes()
        {
            trkVolumen.Minimum = 0;
            trkVolumen.Maximum = 100;
            trkVolumen.Value = 100;
        }

        private void InitializeVideoView()
        {
            try
            {
                videoView = new VideoView();
                videoView.Dock = DockStyle.Fill;
                pnlVideo.Controls.Add(videoView);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear VideoView: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeVLC()
        {
            try
            {
                Core.Initialize();
                string[] opciones = new string[] { "--no-mouse-events", "--no-keyboard-events" };
                _libVLC = new LibVLC(opciones);

                _mediaPlayer = new MediaPlayer(_libVLC);
                videoView.MediaPlayer = _mediaPlayer;

                _mediaPlayer.EndReached += MediaPlayer_EndReached;
                _mediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
                _mediaPlayer.LengthChanged += MediaPlayer_LengthChanged;

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar VLC: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeTimer()
        {
            _timerProgreso = new System.Windows.Forms.Timer();
            _timerProgreso.Interval = 100;
            _timerProgreso.Tick += TimerProgreso_Tick;
            _timerProgreso.Start();
        }

        private void InitializeHideControlsTimer()
        {
            _timerOcultarControles = new System.Windows.Forms.Timer();
            _timerOcultarControles.Interval = HIDE_CONTROLES_DELAY_MS;
            _timerOcultarControles.Tick += TimerOcultarControles_Tick;
        }

        private void TimerOcultarControles_Tick(object sender, EventArgs e)
        {
            if (_isFullScreen && _controlsVisible && !_isClosing)
            {
                OcultarControlsPanelEnFullScreen();
                _controlsVisible = false;
                _timerOcultarControles.Stop();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            DetectarMovimientoRaton();
            base.OnMouseMove(e);
        }

        public bool PreFilterMessage(ref Message m)
        {
            const int WM_MOUSEMOVE = 0x0200;

            if (m.Msg == WM_MOUSEMOVE && _isFullScreen && !_isClosing)
            {
                Point currentMousePos = Control.MousePosition;

                if (_lastMousePosition != currentMousePos)
                {
                    _lastMousePosition = currentMousePos;
                    DetectarMovimientoRaton();
                }
            }

            return false;
        }

        private void DetectarMovimientoRaton()
        {
            if (_isFullScreen && !_isClosing)
            {
                if (!_controlsVisible)
                {
                    MostrarControlsPanelEnFullScreen();
                    _controlsVisible = true;
                    ReiniciarTimerOcultarControles();
                }
                else if (_controlsVisible)
                {
                    ReiniciarTimerOcultarControles();
                }
            }
        }

        private void ReiniciarTimerOcultarControles()
        {
            _timerOcultarControles.Stop();
            _timerOcultarControles.Start();
        }

        private void OcultarControlsPanelEnFullScreen()
        {
            if (pnlControls != null)
            {
                pnlControls.Hide();
            }
        }

        private void MostrarControlsPanelEnFullScreen()
        {
            if (pnlControls != null)
            {
                pnlControls.Show();
                pnlControls.BringToFront();
            }
        }

        // ================= MÉTODO DE PANTALLA COMPLETA ==================
        private void PicFullScreen_Click(object sender, EventArgs e)
        {
            ToggleFullScreen();
        }

        private void ToggleFullScreen()
        {
            if (_isClosing || !_isInitialized) return;

            this.SuspendLayout();

            if (!_isFullScreen)
            {
                _originalFormStyle = this.FormBorderStyle;
                _originalFormState = this.WindowState;
                _pnlVideoParent = pnlVideo.Parent;
                _pnlVideoBounds = pnlVideo.Bounds;
                _pnlVideoDockStyle = pnlVideo.Dock;

                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;

                if (_pnlVideoParent != this)
                {
                    _pnlVideoParent.Controls.Remove(pnlVideo);
                    this.Controls.Add(pnlVideo);
                }

                pnlVideo.Dock = DockStyle.Top;
                pnlVideo.Height = this.ClientSize.Height - FULLSCREEN_CONTROLS_HEIGHT;

                pnlControls.Dock = DockStyle.Bottom;
                pnlControls.Height = FULLSCREEN_CONTROLS_HEIGHT;
                pnlControls.BringToFront();

                _isFullScreen = true;
                _controlsVisible = true;
                _lastMousePosition = Control.MousePosition;
                ReiniciarTimerOcultarControles();
            }
            else
            {
                _timerOcultarControles.Stop();

                if (pnlControls != null)
                {
                    pnlControls.Show();
                    pnlControls.Dock = DockStyle.Bottom;
                    pnlControls.Height = 160;
                }

                this.FormBorderStyle = _originalFormStyle;
                this.WindowState = _originalFormState;

                if (_pnlVideoParent != this && _pnlVideoParent != null)
                {
                    this.Controls.Remove(pnlVideo);
                    _pnlVideoParent.Controls.Add(pnlVideo);
                }

                pnlVideo.Dock = _pnlVideoDockStyle;
                pnlVideo.Bounds = _pnlVideoBounds;

                _isFullScreen = false;
                _controlsVisible = true;
            }

            this.ResumeLayout(true);

            Task.Run(() =>
            {
                System.Threading.Thread.Sleep(50);
                this.BeginInvoke(new Action(() => {
                    pnlVideo.Refresh();
                }));
            });
        }

        // ================= MÉTODO DE CIERRE ==================
        private void FormMP4_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isClosing)
            {
                e.Cancel = false;
                return;
            }

            _isClosing = true;
            _closingCts.Cancel();

            if (_isFullScreen)
            {
                ToggleFullScreen();
            }

            DesuscribirseDeEventos();
            DetenerTimer();

            Task.Run(() => LimpiarVideoEnBackground());
        }

        private void LimpiarVideoEnBackground()
        {
            try
            {
                if (_mediaPlayer != null)
                {
                    try { _mediaPlayer.Stop(); } catch { }
                }

                if (_libVLC != null)
                {
                    try { _mediaPlayer?.Dispose(); } catch { }
                    try { _libVLC?.Dispose(); } catch { }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en limpieza background: {ex.Message}");
            }
        }

        private void DetenerTimer()
        {
            try
            {
                if (_timerProgreso != null)
                {
                    _timerProgreso.Stop();
                    _timerProgreso.Tick -= TimerProgreso_Tick;
                }

                if (_timerOcultarControles != null)
                {
                    _timerOcultarControles.Stop();
                    _timerOcultarControles.Tick -= TimerOcultarControles_Tick;
                }
            }
            catch { }
        }

        private void DesuscribirseDeEventos()
        {
            if (_mediaPlayer == null)
                return;

            try
            {
                _mediaPlayer.EndReached -= MediaPlayer_EndReached;
                _mediaPlayer.TimeChanged -= MediaPlayer_TimeChanged;
                _mediaPlayer.LengthChanged -= MediaPlayer_LengthChanged;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al desuscribirse: {ex.Message}");
            }
        }

        // ================= MÉTODO PÚBLICO PARA REPRODUCCIÓN AUTOMÁTICA ==================
        public void CargarYReproducir(string rutaArchivo)
        {
            if (!_isInitialized)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(500);
                    await CargarVideoAsyncAwaitable(rutaArchivo);
                });
            }
            else
            {
                CargarVideoAsync(rutaArchivo);
            }
        }

        private async Task CargarVideoAsyncAwaitable(string rutaArchivo)
        {
            await Task.Run(() => CargarVideoAsync(rutaArchivo));
        }

        private void PicAbrir_Click(object sender, EventArgs e)
        {
            if (!_isInitialized) return;

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.webm;*.m4v|Archivos MP4|*.mp4|Todos los archivos|*.*";
                ofd.Title = "Selecciona un archivo de video";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    CargarVideoAsync(ofd.FileName);
                }
            }
        }

        private async void CargarVideoAsync(string rutaArchivo)
        {
            if (!_isInitialized || _isClosing) return;

            try
            {
                _isLoadingVideo = true;
                _isSeeking = true;

                if (_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.Stop();
                    await Task.Delay(100);
                }

                _mediaPlayer.Media?.Dispose();

                var media = new LibVLCSharp.Shared.Media(_libVLC, rutaArchivo, FromType.FromPath);
                await media.Parse(MediaParseOptions.ParseLocal);

                _mediaPlayer.Play(media);

                _mediaPlayer.Mute = false;
                _mediaPlayer.Volume = trkVolumen.Value;

                lblNombreArchivo.Text = System.IO.Path.GetFileName(rutaArchivo);

                await Task.Delay(300);

                trkProgreso.Value = 0;
                lblTiempoActual.Text = "00:00";
                lblTiempoTotal.Text = "00:00";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el video: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isLoadingVideo = false;
                _isSeeking = false;
            }
        }

        private void PicPlayPause_Click(object sender, EventArgs e)
        {
            if (!_isInitialized || _mediaPlayer == null || _isLoadingVideo || _isClosing) return;

            try
            {
                if (_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.Pause();
                    PicPlayPause.Image = Properties.Resources.PlayMP4;
                }
                else
                {
                    _mediaPlayer.Play();
                    PicPlayPause.Image = Properties.Resources.PauseMP4;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en Play/Pause: {ex.Message}");
            }
        }

        private void PicStop_Click(object sender, EventArgs e)
        {
            if (!_isInitialized || _mediaPlayer == null || _isLoadingVideo || _isClosing) return;

            try
            {
                _mediaPlayer.Stop();
                trkProgreso.Value = 0;
                lblTiempoActual.Text = "00:00";
                lblTiempoTotal.Text = "00:00";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en Stop: {ex.Message}");
            }
        }

        private void PicAdelante_Click(object sender, EventArgs e)
        {
            if (!_isInitialized || _mediaPlayer == null || _isLoadingVideo || _isClosing) return;
            RealizarSeekAsync(_mediaPlayer.Time + 5000);
        }

        private void PicAtras_Click(object sender, EventArgs e)
        {
            if (!_isInitialized || _mediaPlayer == null || _isLoadingVideo || _isClosing) return;
            long nuevoTiempo = Math.Max(0, _mediaPlayer.Time - 5000);
            RealizarSeekAsync(nuevoTiempo);
        }

        private void PicRepeat_Click(object sender, EventArgs e)
        {
            if (_isClosing) return;
            _repeatMode = (_repeatMode + 1) % 3;
            ActualizarVisualsRepeat();
        }

        private void PicMute_Click(object sender, EventArgs e)
        {
            if (!_isInitialized || _mediaPlayer == null || _isClosing) return;

            try
            {
                _mediaPlayer.Mute = !_mediaPlayer.Mute;

                if (_mediaPlayer.Mute)
                {
                    picMute.BackColor = Color.FromArgb(200, 50, 50);
                }
                else
                {
                    picMute.BackColor = Color.Black;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en Mute: {ex.Message}");
            }
        }

        private void TrkVolumen_Scroll(object sender, EventArgs e)
        {
            if (!_isInitialized || _mediaPlayer == null || _isClosing) return;
            try { _mediaPlayer.Volume = trkVolumen.Value; } catch { }
        }

        private void TrkProgreso_Scroll(object sender, EventArgs e)
        {
            if (!_isInitialized || _mediaPlayer?.Media == null || _isLoadingVideo || _isClosing) return;

            long duracion = _mediaPlayer.Length;
            long nuevoTiempo = (long)((trkProgreso.Value / (double)TRACKBAR_MAX) * duracion);

            RealizarSeekAsync(nuevoTiempo);
        }

        private async void RealizarSeekAsync(long nuevoTiempo)
        {
            if (_isLoadingVideo || _isClosing)
                return;

            long ahora = DateTime.UtcNow.Ticks / 10000;
            if (ahora - _lastSeekTime < SEEK_DEBOUNCE_MS)
                return;

            _lastSeekTime = ahora;
            _isSeeking = true;

            try
            {
                await Task.Run(() =>
                {
                    if (_mediaPlayer?.Media != null && !_isClosing)
                    {
                        _mediaPlayer.Time = Math.Max(0, nuevoTiempo);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en seek: {ex.Message}");
            }
            finally
            {
                _isSeeking = false;
            }
        }

        private void MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            if (_isClosing) return;
            if (!_isSeeking && !_isLoadingVideo)
            {
                ActualizarBarraProgreso();
            }
        }

        private void MediaPlayer_LengthChanged(object sender, MediaPlayerLengthChangedEventArgs e)
        {
            if (_isClosing) return;
            if (!_isLoadingVideo)
            {
                ActualizarBarraProgreso();
            }
        }

        private void TimerProgreso_Tick(object sender, EventArgs e)
        {
            if (_isClosing) return;
            if (_mediaPlayer?.Media != null && !_isSeeking && !_isLoadingVideo)
            {
                ActualizarBarraProgreso();
            }
        }

        private void ActualizarVisualsRepeat()
        {
            switch (_repeatMode)
            {
                case 0:
                    picRepeat.BackColor = Color.Black;
                    break;
                case 1:
                    picRepeat.BackColor = Color.FromArgb(255, 100, 200);
                    break;
                case 2:
                    picRepeat.BackColor = Color.FromArgb(100, 200, 255);
                    break;
            }
        }

        private void ActualizarBarraProgreso()
        {
            try
            {
                if (_mediaPlayer?.Media == null || _isClosing) return;

                long duracion = _mediaPlayer.Length;
                long tiempoActual = _mediaPlayer.Time;

                if (duracion <= 0 || tiempoActual < 0)
                    return;

                int nuevoValor = (int)((tiempoActual / (double)duracion) * TRACKBAR_MAX);
                nuevoValor = Math.Clamp(nuevoValor, 0, TRACKBAR_MAX);

                if (trkProgreso.InvokeRequired)
                {
                    trkProgreso.BeginInvoke(new Action(() =>
                    {
                        if (!_isSeeking && !_isLoadingVideo && trkProgreso.Value != nuevoValor)
                            trkProgreso.Value = nuevoValor;
                    }));
                }
                else
                {
                    if (!_isSeeking && !_isLoadingVideo && trkProgreso.Value != nuevoValor)
                        trkProgreso.Value = nuevoValor;
                }

                string tiempoAct = FormatearTiempo(tiempoActual);
                string tiempoTot = FormatearTiempo(duracion);

                if (lblTiempoActual.InvokeRequired)
                {
                    lblTiempoActual.BeginInvoke(new Action(() => lblTiempoActual.Text = tiempoAct));
                }
                else
                {
                    lblTiempoActual.Text = tiempoAct;
                }

                if (lblTiempoTotal.InvokeRequired)
                {
                    lblTiempoTotal.BeginInvoke(new Action(() => lblTiempoTotal.Text = tiempoTot));
                }
                else
                {
                    lblTiempoTotal.Text = tiempoTot;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando barra: {ex.Message}");
            }
        }

        private void MediaPlayer_EndReached(object sender, EventArgs e)
        {
            if (_isClosing) return;
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => GestionarFinVideo()));
                }
                else
                {
                    GestionarFinVideo();
                }
            }
            catch { }
        }

        private void GestionarFinVideo()
        {
            if (_mediaPlayer == null || _isLoadingVideo || _isClosing)
                return;

            switch (_repeatMode)
            {
                case 0:
                    try { _mediaPlayer.Stop(); } catch { }
                    ActualizarUIAlInicio();
                    break;
                case 1:
                case 2:
                    ReiniciarVideoAsync();
                    break;
            }
        }

        private void ReiniciarVideoAsync()
        {
            if (_isClosing) return;
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action(ReiniciarVideoAsync)); } catch { }
                return;
            }

            try
            {
                _mediaPlayer.Stop();
                _mediaPlayer.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al reiniciar: {ex.Message}");
            }
        }

        private void ActualizarUIAlInicio()
        {
            if (_isClosing) return;
            try
            {
                if (trkProgreso.InvokeRequired)
                {
                    trkProgreso.BeginInvoke(new Action(() =>
                    {
                        trkProgreso.Value = 0;
                        lblTiempoActual.Text = "00:00";
                    }));
                }
                else
                {
                    trkProgreso.Value = 0;
                    lblTiempoActual.Text = "00:00";
                }
            }
            catch { }
        }

        private string FormatearTiempo(long milisegundos)
        {
            if (milisegundos < 0)
                return "00:00";

            long horas = milisegundos / 3600000;
            long minutos = (milisegundos % 3600000) / 60000;
            long segundos = (milisegundos % 60000) / 1000;

            return horas > 0
                ? $"{horas:D2}:{minutos:D2}:{segundos:D2}"
                : $"{minutos:D2}:{segundos:D2}";
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                Application.RemoveMessageFilter(this);
                _timerProgreso?.Dispose();
                _timerOcultarControles?.Dispose();
                _closingCts?.Dispose();
            }
            catch { }

            base.OnClosed(e);
        }
    }
}