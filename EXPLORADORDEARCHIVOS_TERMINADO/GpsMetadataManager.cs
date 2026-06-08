using System;
using System.Drawing;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    public static class GpsMetadataManager
    {
        public static (double latitude, double longitude)? ExtractCoordinatesFromImage(Image image)
        {
            try
            {
                var imagePath = image.Tag as string;
                if (string.IsNullOrEmpty(imagePath))
                    return null;

                var directories = ImageMetadataReader.ReadMetadata(imagePath);
                var gpsDirectory = directories.OfType<GpsDirectory>().FirstOrDefault();

                if (gpsDirectory == null)
                    return null;

                var location = gpsDirectory.GetGeoLocation();
                if (location == null)
                    return null;

                return (location.Latitude, location.Longitude);
            }
            catch
            {
                return null;
            }
        }

        public static bool TryParseCoordinates(string input, out double latitude, out double longitude)
        {
            latitude = 0;
            longitude = 0;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (TryParseWithDirectionalSymbols(input, out latitude, out longitude))
                return true;

            return TryParseSimpleFormat(input, out latitude, out longitude);
        }

        private static bool TryParseSimpleFormat(string input, out double latitude, out double longitude)
        {
            latitude = 0;
            longitude = 0;

            var parts = input.Split(',');
            if (parts.Length != 2)
                return false;

            if (!double.TryParse(parts[0].Trim(), out latitude) ||
                !double.TryParse(parts[1].Trim(), out longitude))
                return false;

            return IsValidCoordinateRange(latitude, longitude);
        }

        private static bool TryParseWithDirectionalSymbols(string input, out double latitude, out double longitude)
        {
            latitude = 0;
            longitude = 0;

            try
            {
                var parts = input.Split(',');
                if (parts.Length != 2)
                    return false;

                if (!ExtractCoordinate(parts[0].Trim(), out latitude, out string latDir) ||
                    !ExtractCoordinate(parts[1].Trim(), out longitude, out string lonDir))
                    return false;

                ApplyDirectionSign(ref latitude, latDir);
                ApplyDirectionSign(ref longitude, lonDir, isLongitude: true);

                return IsValidCoordinateRange(latitude, longitude);
            }
            catch
            {
                return false;
            }
        }

        private static bool ExtractCoordinate(string input, out double value, out string direction)
        {
            value = 0;
            direction = "";

            input = input.Replace(" ", "").ToUpper();

            char[] separators = { '°', '°', '°' };
            var parts = input.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 1 || !double.TryParse(parts[0], out value))
                return false;

            direction = parts.Length > 1 ? parts[1] : "N";
            if (direction.Length > 0)
                direction = direction[0].ToString();

            return true;
        }

        private static void ApplyDirectionSign(ref double coordinate, string direction, bool isLongitude = false)
        {
            if (direction.Equals("S", StringComparison.OrdinalIgnoreCase))
                coordinate = -coordinate;
            else if ((direction.Equals("W", StringComparison.OrdinalIgnoreCase) ||
                      direction.Equals("O", StringComparison.OrdinalIgnoreCase)) && isLongitude)
                coordinate = -coordinate;
        }

        private static bool IsValidCoordinateRange(double latitude, double longitude)
        {
            return latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180;
        }

        /// <summary>
        /// Convierte coordenadas a formato EXIF (3 RATIONAL de 8 bytes cada uno).
        ///  CRÍTICO: Usa BIG-ENDIAN (requerido por EXIF/PNG), no little-endian de BitConverter.
        /// </summary>
        public static byte[] ConvertCoordinateToExifFormat(double coordinate)
        {
            byte[] result = new byte[24];

            int degrees = (int)coordinate;
            double remainder = (coordinate - degrees) * 60;
            int minutes = (int)remainder;
            double seconds = (remainder - minutes) * 60;

            // Convertir usando BitConverter estándar (compatible nativamente con GDI+ / PropertyItem)
            BitConverter.GetBytes((uint)degrees).CopyTo(result, 0);
            BitConverter.GetBytes((uint)1).CopyTo(result, 4);

            BitConverter.GetBytes((uint)minutes).CopyTo(result, 8);
            BitConverter.GetBytes((uint)1).CopyTo(result, 12);

            // Multiplicamos por 1000 para rescatar precisión de milisegundos de arco en los segundos
            BitConverter.GetBytes((uint)(seconds * 1000)).CopyTo(result, 16);
            BitConverter.GetBytes((uint)1000).CopyTo(result, 20);

            return result;
        }

        /// <summary>Escribe un uint en big-endian en un array de bytes.</summary>
        private static void WriteBigEndianUint(byte[] buffer, int offset, uint value)
        {
            buffer[offset]     = (byte)((value >> 24) & 0xFF);
            buffer[offset + 1] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 3] = (byte)(value & 0xFF);
        }
    }
}