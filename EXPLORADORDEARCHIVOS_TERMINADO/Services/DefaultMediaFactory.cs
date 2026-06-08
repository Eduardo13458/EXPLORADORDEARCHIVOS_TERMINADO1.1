using System;
using System.Windows.Forms;
using System.IO;
using EXPLORADORDEARCHIVOS_TERMINADO.Interfaces;
using EXPLORADORDEARCHIVOS_TERMINADO.Services;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Services
{
    /// <summary>
    /// Fábrica simple que devuelve handlers para video, audio e imagen.
    /// Implementa la lógica de creación centralizada; Form1 ya no crea formularios directamente.
    /// </summary>
    public sealed class DefaultMediaFactory
    {
        private readonly IServiceProvider _provider;

        public DefaultMediaFactory(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public IMediaHandler CreateHandlerFor(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower().TrimStart('.');

            // Usar el clasificador de tipos centralizado
            if (EXPLORADORDEARCHIVOS_TERMINADO.Processing.FileTypeClassifier.IsVideo(ext))
                return new VideoFormHandler(_provider);
            if (EXPLORADORDEARCHIVOS_TERMINADO.Processing.FileTypeClassifier.IsMusic(ext))
                return new AudioFormHandler(_provider);
            if (EXPLORADORDEARCHIVOS_TERMINADO.Processing.FileTypeClassifier.IsImage(ext))
                return new ImageFormHandler(_provider);

            return new DefaultFileHandler();
        }
    }

    // Handlers mínimos que envuelven a los formularios existentes
    internal class VideoFormHandler : IMediaHandler
    {
        private FormMP4? _form;
        private readonly IServiceProvider _provider;
        public VideoFormHandler(IServiceProvider provider) { _provider = provider; }
        public void Handle(string filePath)
        {
            if (_form == null || _form.IsDisposed)
            {
                _form = _provider.GetService(typeof(FormMP4)) as FormMP4 ?? Activator.CreateInstance<FormMP4>();
                _form.FormClosed += (s, e) => _form = null;
                _form.Show();
            }
            _form.CargarYReproducir(filePath);
            _form.BringToFront();
        }
        public void Close() { if (_form != null && !_form.IsDisposed) _form.Close(); }
    }

    internal class AudioFormHandler : IMediaHandler
    {
        private FormMP3? _form;
        private readonly IServiceProvider _provider;
        public AudioFormHandler(IServiceProvider provider) { _provider = provider; }
        public void Handle(string filePath)
        {
            if (_form == null || _form.IsDisposed)
            {
                _form = _provider.GetService(typeof(FormMP3)) as FormMP3
                        ?? Activator.CreateInstance(typeof(FormMP3), _provider.GetService(typeof(AudioService))) as FormMP3;
                _form.FormClosed += (s, e) => _form = null;
                _form.Show();
            }
            _form.AgregarYReproducir(filePath);
            _form.BringToFront();
        }
        public void Close() { if (_form != null && !_form.IsDisposed) _form.Close(); }
    }

    internal class ImageFormHandler : IMediaHandler
    {
        private FormEditarFotos? _form;
        private readonly IServiceProvider _provider;
        public ImageFormHandler(IServiceProvider provider) { _provider = provider; }
        public void Handle(string filePath)
        {
            if (_form == null || _form.IsDisposed)
            {
                _form = _provider.GetService(typeof(FormEditarFotos)) as FormEditarFotos ?? Activator.CreateInstance<FormEditarFotos>();
                _form.FormClosed += (s, e) => _form = null;
                _form.Show();
            }
            _form.AbrirImagenDesdeRuta(filePath);
            _form.BringToFront();
        }
        public void Close() { if (_form != null && !_form.IsDisposed) _form.Close(); }
    }

    internal class DefaultFileHandler : IMediaHandler
    {
        public void Handle(string filePath)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
        public void Close() { }
    }
}
