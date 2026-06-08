using System;
using System.IO;
using System.Text.Json;


namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    internal static class EmailSettings
    {
        private static readonly string _path = Path.Combine(
            Application.UserAppDataPath, "email_settings.json");

        private sealed class SettingsData
        {
            public string Remitente { get; set; } = string.Empty;
        }

        /// <summary>Devuelve el correo remitente guardado, o cadena vacía si no hay ninguno.</summary>
        public static string CargarRemitente()
        {
            try
            {
                if (!File.Exists(_path)) return string.Empty;
                var json = File.ReadAllText(_path);
                var data = JsonSerializer.Deserialize<SettingsData>(json);
                return data?.Remitente ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>Guarda el correo remitente para futuras sesiones.</summary>
        public static void GuardarRemitente(string correo)
        {
            try
            {
                var data = new SettingsData { Remitente = correo };
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
                File.WriteAllText(_path, json);
            }
            catch { /* No interrumpir el flujo si falla el guardado */ }
        }
    }
}