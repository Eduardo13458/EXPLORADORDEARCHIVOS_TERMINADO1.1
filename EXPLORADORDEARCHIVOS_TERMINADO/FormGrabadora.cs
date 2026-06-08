using AForge.Video;
using AForge.Video.DirectShow;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using FFMpegCore;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using EXPLORADORDEARCHIVOS_TERMINADO.Services;

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    public partial class FormGrabadora : Form
    {
        // Audio
        private WaveInEvent _waveInEvent;
        private WaveFileWriter _waveFileWriter;

        // Video
        private System.Windows.Forms.Timer _timerVideoCapture;
        private Bitmap _frameBuffer;
        private Graphics _graphics;
        private int _videoFrameCount;
        private List<Bitmap> _videoFrames;

        // AviWriter personalizado

        private System.Windows.Forms.Timer _timerRecordingTime;
        private string _recordingFilePath;
        private string _videoFilePath;
        private string _audioFilePath;
        private int _secondsRecorded = 0;
        private bool _isRecording = false;
        private bool _isAudioOnly = true;
        private bool _useCameraInsteadOfScreen = false;
        private System.Diagnostics.Stopwatch _stopwatch;

        private const int VIDEO_WIDTH = 1280;
        private const int VIDEO_HEIGHT = 720;
        private const int VIDEO_FPS = 30;
        private const int AUDIO_SAMPLE_RATE = 44100;

        private PictureBox _previewBox;
        private System.Windows.Forms.Timer _cameraTimer;
        private FilterInfoCollection _videoDevices;
        private VideoCaptureDevice _videoSource;

        private bool _isSavingVideo = false;
        private static string _recordingsFolder;
        private static readonly string _ffmpegPath = @"ffmpeg\bin\ffmpeg.exe";

        public FormGrabadora()
        {
            InitializeComponent();
            InitializeRecordingsFolder();
            InitializeRecorder();
            _previewBox = previewBox;
            InitializeCamera();
        }

        private void InitializeRecordingsFolder()
        {
            try
            {
                _recordingsFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Grabaciones"
                );

                if (!Directory.Exists(_recordingsFolder))
                {
                    Directory.CreateDirectory(_recordingsFolder);
                }

                System.Diagnostics.Debug.WriteLine($"Carpeta de grabaciones: {_recordingsFolder}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creando carpeta: {ex.Message}");
                _recordingsFolder = Path.Combine(Path.GetTempPath(), "Grabaciones");
                Directory.CreateDirectory(_recordingsFolder);
            }
        }

        

        private void InitializeCamera()
        {
            try
            {
                _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (_videoDevices.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Se encontraron {_videoDevices.Count} cámara(s)");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No se encontraron cámaras");
                }

                _cameraTimer = new System.Windows.Forms.Timer();
                _cameraTimer.Interval = 33;
                _cameraTimer.Tick += CameraTimer_Tick;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inicializando cámara: {ex.Message}");
            }
        }

        private void CameraTimer_Tick(object sender, EventArgs e)
        {
        }

        private void InitializeRecorder()
        {
            _stopwatch = new System.Diagnostics.Stopwatch();
            _videoFrames = new List<Bitmap>();

            _timerRecordingTime = new System.Windows.Forms.Timer();
            _timerRecordingTime.Interval = 1000;
            _timerRecordingTime.Tick += TimerRecordingTime_Tick;

            _timerVideoCapture = new System.Windows.Forms.Timer();
            _timerVideoCapture.Interval = 1000 / VIDEO_FPS;
            _timerVideoCapture.Tick += TimerVideoCapture_Tick;

            RefreshAudioDevices();
        }

        private void RefreshAudioDevices()
        {
            cmbAudioDevices.Items.Clear();

            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                WaveInCapabilities cap = WaveIn.GetCapabilities(i);
                cmbAudioDevices.Items.Add($"{cap.ProductName} ({cap.Channels}ch)");
            }

            if (cmbAudioDevices.Items.Count > 0)
            {
                cmbAudioDevices.SelectedIndex = 0;
                lblAudioStatus.Text = "Dispositivo de audio detectado";
            }
            else
            {
                lblAudioStatus.Text = "No se detectó dispositivo de audio";
            }
        }

        private void BtnStartRecording_Click(object sender, EventArgs e)
        {
            if (_isSavingVideo)
            {
                MessageBox.Show("Se está guardando un video. Por favor, espera a que termine.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_isAudioOnly)
                StartAudioRecording();
            else if (_useCameraInsteadOfScreen)
                StartCameraRecording();
            else
                StartVideoRecording();
        }

        private void StartAudioRecording()
        {
            try
            {
                if (_isRecording)
                {
                    MessageBox.Show("Ya hay una grabación en progreso.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string fileName = $"audio_{DateTime.Now:yyyyMMdd_HHmmss}.wav";
                _recordingFilePath = Path.Combine(_recordingsFolder, fileName);

                int deviceIndex = cmbAudioDevices.SelectedIndex >= 0 ? cmbAudioDevices.SelectedIndex : 0;

                _waveInEvent = new WaveInEvent
                {
                    DeviceNumber = deviceIndex,
                    WaveFormat = new WaveFormat(44100, 16, 2)
                };

                _waveInEvent.DataAvailable += WaveInEvent_DataAvailable;
                _waveInEvent.RecordingStopped += WaveInEvent_RecordingStopped;

                _waveFileWriter = new WaveFileWriter(_recordingFilePath, _waveInEvent.WaveFormat);

                _isRecording = true;
                _secondsRecorded = 0;
                _stopwatch.Restart();

                _waveInEvent.StartRecording();
                _timerRecordingTime.Start();

                btnStartRecording.Enabled = false;
                btnStopRecording.Enabled = true;
                lblRecordingStatus.Text = "GRABANDO AUDIO...";
                lblRecordingStatus.ForeColor = System.Drawing.Color.Red;
                lblRecordingTime.Text = "00:00:00";

                System.Diagnostics.Debug.WriteLine("Grabación de audio iniciada");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar grabación: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                CleanupRecording();
                ResetUI();
            }
        }

        private void StartVideoRecording()
        {
            try
            {
                if (_isRecording)
                {
                    MessageBox.Show("Ya hay una grabación en progreso.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ? Archivo temporal (será convertido a MP4)
                string fileName = $"video_pantalla_{DateTime.Now:yyyyMMdd_HHmmss}.tmp";
                _videoFilePath = Path.Combine(_recordingsFolder, fileName);

                _frameBuffer = new Bitmap(VIDEO_WIDTH, VIDEO_HEIGHT, PixelFormat.Format32bppRgb);
                _graphics = Graphics.FromImage(_frameBuffer);
                _videoFrameCount = 0;
                _videoFrames = new List<Bitmap>();

                _previewBox.Visible = true;
                pnlPreview.Visible = true;

                int deviceIndex = cmbAudioDevices.SelectedIndex >= 0 ? cmbAudioDevices.SelectedIndex : 0;

                _waveInEvent = new WaveInEvent
                {
                    DeviceNumber = deviceIndex,
                    WaveFormat = new WaveFormat(AUDIO_SAMPLE_RATE, 16, 2)
                };

                _waveInEvent.DataAvailable += WaveInEvent_DataAvailable;
                _waveInEvent.RecordingStopped += WaveInEvent_RecordingStopped;

                _recordingFilePath = Path.Combine(
                    Path.GetTempPath(),
                    $"temp_audio_{Guid.NewGuid().ToString().Substring(0, 8)}.wav"
                );

                _waveFileWriter = new WaveFileWriter(_recordingFilePath, _waveInEvent.WaveFormat);

                _isRecording = true;
                _secondsRecorded = 0;
                _stopwatch.Restart();

                _waveInEvent.StartRecording();
                _timerRecordingTime.Start();
                _timerVideoCapture.Start();

                btnStartRecording.Enabled = false;
                btnStopRecording.Enabled = true;
                lblRecordingStatus.Text = "GRABANDO VIDEO (PANTALLA) + AUDIO...";
                lblRecordingStatus.ForeColor = System.Drawing.Color.Magenta;
                lblRecordingTime.Text = "00:00:00";

                System.Diagnostics.Debug.WriteLine("? Grabación de video (pantalla) + audio iniciada");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar grabación de video: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                CleanupVideoRecording();
                ResetUI();
            }
        }

        private void StartCameraRecording()
        {
            try
            {
                if (_isRecording)
                {
                    MessageBox.Show("Ya hay una grabación en progreso.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_videoDevices.Count == 0)
                {
                    MessageBox.Show("No se encontró ninguna cámara en el equipo.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ? Archivo temporal (será convertido a MP4)
                string fileName = $"video_camara_{DateTime.Now:yyyyMMdd_HHmmss}.tmp";
                _videoFilePath = Path.Combine(_recordingsFolder, fileName);

                _videoSource = new VideoCaptureDevice(_videoDevices[0].MonikerString);
                _videoSource.VideoResolution = _videoSource.VideoCapabilities[0];
                _videoSource.NewFrame += VideoSource_NewFrame;

                _previewBox.Visible = true;
                pnlPreview.Visible = true;
                _videoFrames = new List<Bitmap>();

                int deviceIndex = cmbAudioDevices.SelectedIndex >= 0 ? cmbAudioDevices.SelectedIndex : 0;

                _waveInEvent = new WaveInEvent
                {
                    DeviceNumber = deviceIndex,
                    WaveFormat = new WaveFormat(AUDIO_SAMPLE_RATE, 16, 2)
                };

                _waveInEvent.DataAvailable += WaveInEvent_DataAvailable;
                _waveInEvent.RecordingStopped += WaveInEvent_RecordingStopped;

                _recordingFilePath = Path.Combine(
                    Path.GetTempPath(),
                    $"temp_audio_{Guid.NewGuid().ToString().Substring(0, 8)}.wav"
                );

                _waveFileWriter = new WaveFileWriter(_recordingFilePath, _waveInEvent.WaveFormat);

                _isRecording = true;
                _secondsRecorded = 0;
                _stopwatch.Restart();

                _videoSource.Start();
                _waveInEvent.StartRecording();
                _timerRecordingTime.Start();
                _cameraTimer.Start();

                btnStartRecording.Enabled = false;
                btnStopRecording.Enabled = true;
                lblRecordingStatus.Text = "? GRABANDO VIDEO (CÁMARA) + AUDIO...";
                lblRecordingStatus.ForeColor = System.Drawing.Color.Blue;
                lblRecordingTime.Text = "00:00:00";

                System.Diagnostics.Debug.WriteLine("? Grabación de cámara + audio iniciada");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar grabación de cámara: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                CleanupVideoRecording();
                ResetUI();
            }
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                Bitmap imgPreview = (Bitmap)eventArgs.Frame.Clone();

                if (_previewBox.InvokeRequired)
                {
                    _previewBox.Invoke(new Action(() =>
                    {
                        _previewBox.Image?.Dispose();
                        _previewBox.Image = imgPreview;
                    }));
                }
                else
                {
                    _previewBox.Image?.Dispose();
                    _previewBox.Image = imgPreview;
                }

                if (_isRecording && _videoFrames != null)
                {
                    Bitmap frameEstandar = new Bitmap(eventArgs.Frame.Width, eventArgs.Frame.Height, PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(frameEstandar))
                    {
                        g.DrawImage(eventArgs.Frame, 0, 0, eventArgs.Frame.Width, eventArgs.Frame.Height);
                    }

                    lock (_videoFrames)
                    {
                        _videoFrames.Add(frameEstandar);
                    }
                    _videoFrameCount++;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error procesando frame de cámara: {ex.Message}");
            }
        }

        private void TimerVideoCapture_Tick(object sender, EventArgs e)
        {
            if (_frameBuffer != null && _graphics != null)
            {
                try
                {
                    // ? CORRECCIÓN: Obtener los límites reales de todos los monitores
                    Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
                    
                    // Si necesitas capturar todos los monitores, usa esto:
                    // var allScreens = Screen.AllScreens;
                    // Rectangle screenBounds = Rectangle.Union(
                    //     allScreens[0].Bounds,
                    //     allScreens.Length > 1 ? allScreens[1].Bounds : allScreens[0].Bounds
                    // );

                    // Redimensionar el buffer si es necesario
                    if (_frameBuffer.Width != screenBounds.Width || _frameBuffer.Height != screenBounds.Height)
                    {
                        _frameBuffer?.Dispose();
                        _graphics?.Dispose();
                        _frameBuffer = new Bitmap(screenBounds.Width, screenBounds.Height, PixelFormat.Format32bppRgb);
                        _graphics = Graphics.FromImage(_frameBuffer);
                    }

                    // ? CORRECCIÓN: Usar las coordenadas reales de la pantalla
                    _graphics.CopyFromScreen(
                        screenBounds.X,      // Coordenada X real (puede ser negativa en multi-monitor)
                        screenBounds.Y,      // Coordenada Y real
                        0,                   // Destino X en el buffer
                        0,                   // Destino Y en el buffer
                        screenBounds.Size    // Tamaño real de la pantalla
                    );

                    Bitmap cloneFrame = new Bitmap(_frameBuffer.Width, _frameBuffer.Height, PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(cloneFrame))
                    {
                        g.DrawImage(_frameBuffer, 0, 0);
                    }

                    _previewBox.Image?.Dispose();
                    _previewBox.Image = new Bitmap(cloneFrame);

                    if (_isRecording && _videoFrames != null)
                    {
                        lock (_videoFrames)
                        {
                            _videoFrames.Add(cloneFrame);
                        }
                        _videoFrameCount++;
                    }
                    else
                    {
                        cloneFrame.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error capturando frame: {ex.Message}");
                }
            }
        }
        private void BtnStopRecording_Click(object sender, EventArgs e)
        {
            StopRecording();
        }

        private void StopRecording()
        {
            try
            {
                if (_waveInEvent == null)
                {
                    MessageBox.Show("No hay grabación activa.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _waveInEvent.StopRecording();
                _timerRecordingTime.Stop();
                _timerVideoCapture.Stop();
                _cameraTimer.Stop();
                _stopwatch.Stop();

                if (_videoSource != null && _videoSource.IsRunning)
                {
                    _videoSource.SignalToStop();
                }

                CleanupRecording();

                if (!_isAudioOnly)
                {
                    lblRecordingStatus.Text = "Guardando video con audio...";
                    lblRecordingStatus.ForeColor = System.Drawing.Color.Yellow;
                    btnStopRecording.Enabled = false;
                    btnStopRecording.Text = "Guardando...";
                    SaveVideoFileAsync();
                }
                else
                {
                    lblRecordingStatus.Text = "GRABACIÓN COMPLETADA";
                    lblRecordingStatus.ForeColor = System.Drawing.Color.Green;
                    lblRecordingTime.Text = $"Guardado en: {Path.GetFileName(_recordingFilePath)}";

                    System.Diagnostics.Debug.WriteLine($"Grabación guardada: {_recordingFilePath}");

                    FileInfo fileInfo = new FileInfo(_recordingFilePath);
                    long sizeInMB = fileInfo.Length / (1024 * 1024);

                    // Preguntar si desea reproducir la grabación de audio
                    DialogResult result = MessageBox.Show(
                        $"Grabación guardada exitosamente\n\n" +
                        $"Archivo: {Path.GetFileName(_recordingFilePath)}\n" +
                        $"Tamaño: {sizeInMB} MB\n" +
                        $"Ubicación: {_recordingsFolder}\n\n" +
                        $"¿Deseas reproducir la grabación?",
                        "Reproducir Audio",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        PlayAudioInMediaPlayer(_recordingFilePath);
                    }

                    ResetUI();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                MessageBox.Show($"Error al detener grabación: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetUI();
            }
        }

        private async void SaveVideoFileAsync()
        {
            _isSavingVideo = true;
            btnStartRecording.Enabled = false;

            try
            {
                await Task.Run(() => SaveVideoWithFFmpeg());

                if (InvokeRequired)
                {
                    Invoke(new Action(() => OnVideoSaveComplete()));
                }
                else
                {
                    OnVideoSaveComplete();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error en hilo de guardado: {ex.Message}");

                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        MessageBox.Show($"Error guardando video: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        CleanupVideoRecording();
                        ResetUI();
                    }));
                }
                else
                {
                    MessageBox.Show($"Error guardando video: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CleanupVideoRecording();
                    ResetUI();
                }
            }
            finally
            {
                _isSavingVideo = false;
            }
        }

        private void SaveVideoWithFFmpeg()
        {
            try
            {
                if (_videoFrames == null || _videoFrames.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No hay frames para guardar");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"? Guardando {_videoFrames.Count} frames como imágenes temporales...");

                // Crear carpeta temporal para frames
                string tempFramesFolder = Path.Combine(Path.GetTempPath(), $"frames_{Guid.NewGuid().ToString().Substring(0, 8)}");
                Directory.CreateDirectory(tempFramesFolder);

                try
                {
                    // Guardar frames como imágenes PNG (con paralelización)
                    int totalFrames = _videoFrames.Count;
                    List<Bitmap> framesCopy = new List<Bitmap>();

                    lock (_videoFrames)
                    {
                        framesCopy.AddRange(_videoFrames);
                    }

                    // Usar Parallel para guardar frames más rápido
                    System.Collections.Concurrent.ConcurrentBag<Exception> exceptions =
                        new System.Collections.Concurrent.ConcurrentBag<Exception>();

                    Parallel.For(0, totalFrames, new ParallelOptions { MaxDegreeOfParallelism = 4 }, i =>
                    {
                        try
                        {
                            if (framesCopy[i] != null)
                            {
                                string framePath = Path.Combine(tempFramesFolder, $"frame_{i:D6}.png");
                                framesCopy[i].Save(framePath, ImageFormat.Png);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"?? Error guardando frame {i}: {ex.Message}");
                            exceptions.Add(ex);
                        }

                        if ((i + 1) % 30 == 0)
                        {
                            int porcentaje = (int)(((i + 1.0) / totalFrames) * 100);
                            System.Diagnostics.Debug.WriteLine($"? Progreso frames: {porcentaje}%");
                        }
                    });

                    if (exceptions.Count > 0)
                    {
                        throw new AggregateException("Errores guardando frames", exceptions);
                    }

                    System.Diagnostics.Debug.WriteLine("Frames guardados temporalmente");

                    // Crear video MP4 con FFmpeg usando frames + audio
                    CreateMP4WithFFmpeg(tempFramesFolder);
                }
                finally
                {
                    // Limpiar carpeta de frames temporales
                    try
                    {
                        if (Directory.Exists(tempFramesFolder))
                        {
                            Directory.Delete(tempFramesFolder, true);
                            System.Diagnostics.Debug.WriteLine("Carpeta temporal de frames eliminada");
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error guardando video: {ex.Message}");
                throw;
            }
        }
        private void CreateMP4WithFFmpeg(string framesFolder)
        {
            try
            {
                // Buscar FFmpeg en la carpeta ffmpeg/bin del proyecto
                string ffmpegFullPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "ffmpeg", "bin", "ffmpeg.exe"
                );

                System.Diagnostics.Debug.WriteLine($"Buscando FFmpeg en: {ffmpegFullPath}");

                if (!File.Exists(ffmpegFullPath))
                {
                    System.Diagnostics.Debug.WriteLine($"FFmpeg no encontrado en: {ffmpegFullPath}");

                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            MessageBox.Show($"No se encontró FFmpeg en:\n{ffmpegFullPath}\n\nAsegúrate de que la carpeta 'ffmpeg/bin' contiene el archivo 'ffmpeg.exe'",
                                "Error de dependencias", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                    }
                    else
                    {
                        MessageBox.Show($"No se encontró FFmpeg en:\n{ffmpegFullPath}\n\nAsegúrate de que la carpeta 'ffmpeg/bin' contiene el archivo 'ffmpeg.exe'",
                            "Error de dependencias", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return;
                }

                // Cambiar extensión de video a .mp4
                string mp4FilePath = Path.Combine(
                    _recordingsFolder,
                    Path.GetFileNameWithoutExtension(_videoFilePath) + ".mp4"
                );

                string framesPattern = Path.Combine(framesFolder, "frame_%06d.png");

                // Calcular el framerate basado en los frames grabados
                double recordingDurationSeconds = _stopwatch.Elapsed.TotalSeconds;
                double actualFPS = _videoFrameCount / recordingDurationSeconds;

                System.Diagnostics.Debug.WriteLine($"Frames capturados: {_videoFrameCount}");
                System.Diagnostics.Debug.WriteLine($"Duración de grabación: {recordingDurationSeconds:F2}s");
                System.Diagnostics.Debug.WriteLine($"FPS actual: {actualFPS:F2}");

                string arguments;
                if (File.Exists(_recordingFilePath))
                {
                    // Usar el framerate actual en lugar del teórico
                    arguments = $"-framerate {actualFPS:F2} -i \"{framesPattern}\" -i \"{_recordingFilePath}\" " +
                                $"-c:v libx264 -pix_fmt yuv420p -c:a aac -shortest -y \"{mp4FilePath}\"";
                }
                else
                {
                    arguments = $"-framerate {actualFPS:F2} -i \"{framesPattern}\" " +
                                $"-c:v libx264 -pix_fmt yuv420p -y \"{mp4FilePath}\"";
                }

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = ffmpegFullPath,
                    Arguments = arguments,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                System.Diagnostics.Debug.WriteLine($"Iniciando FFmpeg con argumentos: {arguments}");

                // No cambiar cursor desde hilo de trabajo
                if (InvokeRequired)
                {
                    Invoke(new Action(() => this.Cursor = Cursors.WaitCursor));
                }
                else
                {
                    this.Cursor = Cursors.WaitCursor;
                }

                using (Process process = Process.Start(psi))
                {
                    string error = process.StandardError.ReadToEnd();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    System.Diagnostics.Debug.WriteLine($"FFmpeg salió con código: {process.ExitCode}");

                    if (process.ExitCode == 0)
                    {
                        if (File.Exists(mp4FilePath))
                        {
                            System.Diagnostics.Debug.WriteLine($"MP4 creado exitosamente: {mp4FilePath}");

                            _videoFilePath = mp4FilePath;

                            // Limpiar audio temporal
                            try
                            {
                                if (File.Exists(_recordingFilePath))
                                {
                                    File.Delete(_recordingFilePath);
                                    System.Diagnostics.Debug.WriteLine("Archivo de audio temporal eliminado");
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error eliminando audio temporal: {ex.Message}");
                            }
                        }
                        else
                        {
                            throw new Exception($"FFmpeg completó pero el archivo MP4 no se encontró en: {mp4FilePath}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Error de FFmpeg - Código: {process.ExitCode}");
                        System.Diagnostics.Debug.WriteLine($"Error output: {error}");
                        System.Diagnostics.Debug.WriteLine($"Standard output: {output}");
                        throw new Exception($"FFmpeg error (código {process.ExitCode}):\n{error}");
                    }
                }

                // Restaurar cursor desde hilo de trabajo
                if (InvokeRequired)
                {
                    Invoke(new Action(() => this.Cursor = Cursors.Default));
                }
                else
                {
                    this.Cursor = Cursors.Default;
                }
            }
            catch (Exception ex)
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        this.Cursor = Cursors.Default;
                        System.Diagnostics.Debug.WriteLine($"Excepción en CreateMP4WithFFmpeg: {ex.Message}");
                        MessageBox.Show($"Error creando MP4: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
                else
                {
                    this.Cursor = Cursors.Default;
                    System.Diagnostics.Debug.WriteLine($"Excepción en CreateMP4WithFFmpeg: {ex.Message}");
                    MessageBox.Show($"Error creando MP4: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void WaveInEvent_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (_waveFileWriter != null)
            {
                _waveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
                _waveFileWriter.Flush();
            }
        }

        private void WaveInEvent_RecordingStopped(object sender, StoppedEventArgs e)
        {
            CleanupRecording();
        }

        private void CleanupRecording()
        {
            try
            {
                _waveInEvent?.StopRecording();
                _waveInEvent?.Dispose();
                _waveInEvent = null;

                _waveFileWriter?.Dispose();
                _waveFileWriter = null;

                _timerRecordingTime?.Stop();
                _isRecording = false;
            }
            catch { }
        }

        private void CleanupVideoRecording()
        {
            try
            {
                _timerVideoCapture?.Stop();
                _cameraTimer?.Stop();

                _graphics?.Dispose();
                _graphics = null;

                _frameBuffer?.Dispose();
                _frameBuffer = null;

                if (_videoFrames != null)
                {
                    foreach (var frame in _videoFrames)
                    {
                        frame?.Dispose();
                    }
                    _videoFrames.Clear();
                    _videoFrames = null;
                }

                if (_videoSource != null)
                {
                    if (_videoSource.IsRunning)
                    {
                        _videoSource.SignalToStop();
                    }
                    _videoSource = null;
                }

                _previewBox.Visible = false;
                _previewBox.Image?.Dispose();
                _previewBox.Image = null;
            }
            catch { }
        }

        private void TimerRecordingTime_Tick(object sender, EventArgs e)
        {
            _secondsRecorded = (int)_stopwatch.Elapsed.TotalSeconds;
            int hours = _secondsRecorded / 3600;
            int minutes = (_secondsRecorded % 3600) / 60;
            int seconds = _secondsRecorded % 60;

            lblRecordingTime.Text = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }

        private void BtnToggleMode_Click(object sender, EventArgs e)
        {
            if (_isRecording || _isSavingVideo)
            {
                MessageBox.Show("No puedes cambiar el modo mientras estés grabando o guardando.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _isAudioOnly = !_isAudioOnly;

            if (_isAudioOnly)
            {
                btnToggleMode.Text = "Cambiar a Grabación de Video";
                lblModeStatus.Text = "Modo: SOLO AUDIO";
                lblModeStatus.ForeColor = System.Drawing.Color.Cyan;
                pictureModeIcon.BackColor = System.Drawing.Color.FromArgb(0, 0, 150);
                pnlPreview.Visible = false;
            }
            else
            {
                btnToggleMode.Text = "Cambiar a Grabación de Audio";
                lblModeStatus.Text = "Modo: VIDEO";
                lblModeStatus.ForeColor = System.Drawing.Color.Magenta;
                pictureModeIcon.BackColor = System.Drawing.Color.FromArgb(150, 0, 150);
                pnlPreview.Visible = true;
            }
        }

        private void BtnToggleCameraMode_Click(object sender, EventArgs e)
        {
            if (_isRecording || _isSavingVideo)
            {
                MessageBox.Show("No puedes cambiar el modo mientras estés grabando o guardando.",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _useCameraInsteadOfScreen = !_useCameraInsteadOfScreen;

            if (_useCameraInsteadOfScreen)
            {
                btnToggleMode.Text = "Cambiar a Grabación de Pantalla";
                lblModeStatus.Text = "Modo: VIDEO (CÁMARA)";
                lblModeStatus.ForeColor = System.Drawing.Color.Blue;
            }
            else
            {
                btnToggleMode.Text = "Cambiar a Grabación de Cámara";
                lblModeStatus.Text = "Modo: VIDEO (PANTALLA)";
                lblModeStatus.ForeColor = System.Drawing.Color.Magenta;
            }
        }

        private void BtnOpenRecordings_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(_recordingsFolder))
            {
                Directory.CreateDirectory(_recordingsFolder);
            }

            System.Diagnostics.Process.Start("explorer.exe", _recordingsFolder);
        }

        private void ResetUI()
        {
            btnStartRecording.Enabled = true;
            btnStartRecording.Text = "Iniciar Grabación";
            btnStopRecording.Enabled = false;
            btnStopRecording.Text = "Detener";
            _secondsRecorded = 0;
            lblRecordingTime.Text = "00:00:00";
            lblRecordingStatus.Text = "Listo para grabar";
            lblRecordingStatus.ForeColor = System.Drawing.Color.White;
            pnlPreview.Visible = false;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isSavingVideo)
            {
                DialogResult result = MessageBox.Show(
                    "El video aún se está guardando. ¿Esperar a que termine?",
                    "Grabación en progreso", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }

                int timeout = 0;
                while (_isSavingVideo && timeout < 300)
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(100);
                    timeout++;
                }
            }

            if (_isRecording || _waveInEvent != null)
            {
                DialogResult result = MessageBox.Show(
                    "Hay una grabación en progreso. ¿Descartar y cerrar?",
                    "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }

                StopRecording();
            }

            CleanupRecording();
            CleanupVideoRecording();
            _timerRecordingTime?.Dispose();
            _timerVideoCapture?.Dispose();
            _cameraTimer?.Dispose();
            _previewBox?.Dispose();

            base.OnFormClosing(e);
        }

        private void OnVideoSaveComplete()
        {
            try
            {
                // Verificar que el archivo MP4 se guardó correctamente
                if (!File.Exists(_videoFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Error: El archivo no se guardó en: {_videoFilePath}");

                    MessageBox.Show(
                        $"Error: No se pudo guardar el video.\n\nRuta esperada:\n{_videoFilePath}",
                        "Error al guardar",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    CleanupVideoRecording();
                    ResetUI();
                    return;
                }

                FileInfo fileInfo = new FileInfo(_videoFilePath);
                long sizeInMB = fileInfo.Length / (1024 * 1024);

                lblRecordingStatus.Text = "GRABACIÓN COMPLETADA";
                lblRecordingStatus.ForeColor = System.Drawing.Color.Green;
                lblRecordingTime.Text = $"Guardado en: {Path.GetFileName(_videoFilePath)}";

                System.Diagnostics.Debug.WriteLine($"Video MP4 guardado: {_videoFilePath}");
                System.Diagnostics.Debug.WriteLine($"Tamaño: {sizeInMB} MB");

                // Preguntar si desea reproducir el video
                DialogResult result = MessageBox.Show(
                    $"Grabación guardada exitosamente\n\n" +
                    $"Archivo: {Path.GetFileName(_videoFilePath)}\n" +
                    $"Tamaño: {sizeInMB} MB\n" +
                    $"Ubicación: {_recordingsFolder}\n\n" +
                    $"¿Deseas reproducir el video?",
                    "Reproducir Video",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    PlayVideoInMediaPlayer(_videoFilePath);
                }

                CleanupVideoRecording();
                ResetUI();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en OnVideoSaveComplete: {ex.Message}");
                MessageBox.Show($"Error finalizando guardado: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetUI();
            }
        }

        private void PlayVideoInMediaPlayer(string videoPath)
        {
            try
            {
                // Usar el mismo patrón que Form1 para abrir FormMP4
                FormMP4 formMP4 = new FormMP4();
                formMP4.FormClosed += (s, e) => formMP4 = null;
                formMP4.Show();
                
                // Dar tiempo para que el formulario se inicialice completamente
                formMP4.Invoke(new Action(() =>
                {
                    formMP4.CargarYReproducir(videoPath);
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al reproducir video: {ex.Message}");
                MessageBox.Show($"Error al reproducir el video:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PlayAudioInMediaPlayer(string audioPath)
        {
            try
            {
                // Usar el mismo patrón que Form1 para abrir FormMP3
                AudioService audioService = new AudioService();
                FormMP3 formMP3 = new FormMP3(audioService);
                formMP3.FormClosed += (s, e) => formMP3 = null;
                formMP3.Show();
                
                // Dar tiempo para que el formulario se inicialice completamente
                formMP3.Invoke(new Action(() =>
                {
                    formMP3.AgregarYReproducir(audioPath);
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al reproducir audio: {ex.Message}");
                MessageBox.Show($"Error al reproducir el audio:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}