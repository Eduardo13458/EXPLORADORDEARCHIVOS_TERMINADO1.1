using System;
using System.Windows.Forms;
using EXPLORADORDEARCHIVOS_TERMINADO.Interfaces;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Services
{
    public class FormManager : IFormManager
    {
        private readonly IServiceProvider _provider;

        public FormManager(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public T GetOrCreate<T>(T current) where T : Form
        {
            if (current != null && !current.IsDisposed)
            {
                current.BringToFront();
                current.WindowState = FormWindowState.Normal;
                return current;
            }

            // Intentar resolver desde el contenedor
            var resolved = _provider.GetService(typeof(T)) as T;
            if (resolved != null)
                return resolved;

            // Fallback a reflection si no está registrado
            return Activator.CreateInstance<T>();
        }
    }
}
